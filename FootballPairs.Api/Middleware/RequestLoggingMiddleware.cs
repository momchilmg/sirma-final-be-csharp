using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FootballPairs.Application.Logging;
using FootballPairs.Application.Logging.Models;
using Microsoft.Extensions.Primitives;

namespace FootballPairs.Api.Middleware;

public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    private static readonly SemaphoreSlim FailureAuditLock = new(1, 1);
    private static readonly TimeSpan LogWriteTimeout = TimeSpan.FromMilliseconds(400);
    private static readonly TimeSpan FailureAuditMutexTimeout = TimeSpan.FromSeconds(5);
    private const int MaxFailureAuditWriteAttempts = 5;
    private const int BaseFailureAuditRetryDelayMilliseconds = 20;
    private static readonly HashSet<string> AllowedHeaderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Accept",
        "Content-Type",
        "Content-Length",
        "User-Agent",
        "Host",
        "X-Request-Id",
        "X-Correlation-Id"
    };

    public async Task InvokeAsync(
        HttpContext context,
        IRequestLogWriter requestLogWriter,
        ILogDataSanitizer logDataSanitizer)
    {
        try
        {
            await next(context);
        }
        finally
        {
            var payload = BuildPayload(context);
            var sanitizedPayload = logDataSanitizer.Sanitize(payload);
            var entry = new RequestLogEntry(
                DateTime.UtcNow,
                context.Response.StatusCode,
                GetUsername(context.User),
                sanitizedPayload);

            using var writeTimeoutCts = new CancellationTokenSource(LogWriteTimeout);
            try
            {
                await requestLogWriter.WriteAsync(entry, writeTimeoutCts.Token);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Request log persistence failed.");
                _ = WriteLoggingFailureAuditAsync(context, exception);
            }
        }
    }

    private static string BuildPayload(HttpContext context)
    {
        var headers = context.Request.Headers.ToDictionary(
            static header => header.Key,
            static header => ConvertHeaderValue(header.Value),
            StringComparer.OrdinalIgnoreCase);
        var safeHeaders = headers
            .Where(header => AllowedHeaderNames.Contains(header.Key))
            .ToDictionary(
                static header => header.Key,
                static header => header.Value,
                StringComparer.OrdinalIgnoreCase);
        var query = context.Request.Query.ToDictionary(
            static item => item.Key,
            static _ => "***REDACTED***",
            StringComparer.OrdinalIgnoreCase);
        var payload = new
        {
            timestampUtc = DateTime.UtcNow,
            method = context.Request.Method,
            path = context.Request.Path.Value ?? string.Empty,
            query,
            username = GetUsername(context.User),
            statusCode = context.Response.StatusCode,
            headers = safeHeaders,
            requestMetadata = new
            {
                contentType = context.Request.ContentType ?? string.Empty,
                contentLength = context.Request.ContentLength,
                hasBody = context.Request.ContentLength is > 0
            },
            responseMetadata = new
            {
                contentType = context.Response.ContentType ?? string.Empty,
                contentLength = context.Response.ContentLength
            }
        };

        return JsonSerializer.Serialize(payload);
    }

    private static string ConvertHeaderValue(StringValues values)
    {
        if (values.Count == 0)
        {
            return string.Empty;
        }

        return values.Count == 1 ? values[0] ?? string.Empty : string.Join(",", values.ToArray());
    }

    private static string? GetUsername(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return user.FindFirstValue(ClaimTypes.Name)
            ?? user.FindFirstValue("unique_name")
            ?? user.Identity?.Name;
    }

    private async Task WriteLoggingFailureAuditAsync(HttpContext context, Exception exception)
    {
        try
        {
            var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            Directory.CreateDirectory(logsDirectory);
            var filePath = Path.Combine(logsDirectory, $"logging-failures-{DateTime.UtcNow:yyyy-MM-dd}.log");
            var payload = new
            {
                timestampUtc = DateTime.UtcNow,
                traceId = context.TraceIdentifier,
                method = context.Request.Method,
                path = context.Request.Path.Value ?? string.Empty,
                statusCode = context.Response.StatusCode,
                errorType = exception.GetType().FullName ?? "UnknownException"
            };
            var line = JsonSerializer.Serialize(payload);

            await FailureAuditLock.WaitAsync(CancellationToken.None);
            try
            {
                using var mutex = new Mutex(false, BuildMutexName(filePath));
                var hasMutex = WaitForMutex(mutex);
                Exception? lastException = null;
                try
                {
                    for (var attempt = 1; attempt <= MaxFailureAuditWriteAttempts; attempt++)
                    {
                        try
                        {
                            await using var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                            await using var writer = new StreamWriter(stream);
                            await writer.WriteLineAsync(line.AsMemory(), CancellationToken.None);
                            return;
                        }
                        catch (Exception writeException) when (writeException is IOException or UnauthorizedAccessException)
                        {
                            lastException = writeException;
                            if (attempt == MaxFailureAuditWriteAttempts)
                            {
                                break;
                            }

                            await Task.Delay(BaseFailureAuditRetryDelayMilliseconds * attempt, CancellationToken.None);
                        }
                    }

                    throw lastException ?? new IOException("Failed to persist logging-failure audit entry.");
                }
                finally
                {
                    if (hasMutex)
                    {
                        mutex.ReleaseMutex();
                    }
                }
            }
            finally
            {
                FailureAuditLock.Release();
            }
        }
        catch (Exception auditException)
        {
            logger.LogError(auditException, "Failed to persist logging-failure audit entry.");
        }
    }

    private static bool WaitForMutex(Mutex mutex)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < FailureAuditMutexTimeout)
        {
            try
            {
                if (mutex.WaitOne(100))
                {
                    return true;
                }
            }
            catch (AbandonedMutexException)
            {
                return true;
            }
        }

        throw new IOException("Timed out acquiring cross-process audit log lock.");
    }

    private static string BuildMutexName(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath).ToUpperInvariant();
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(fullPath));
        var hash = Convert.ToHexString(bytes);
        return $"Local\\FootballPairsAudit_{hash}";
    }
}
