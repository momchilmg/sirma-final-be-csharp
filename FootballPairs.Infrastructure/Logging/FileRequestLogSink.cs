using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using FootballPairs.Application.Logging;
using FootballPairs.Application.Logging.Models;

namespace FootballPairs.Infrastructure.Logging;

public sealed class FileRequestLogSink : IFallbackRequestLogSink
{
    private static readonly SemaphoreSlim FileLock = new(1, 1);
    private const int MaxWriteAttempts = 5;
    private const int BaseRetryDelayMilliseconds = 20;
    private static readonly TimeSpan MutexTimeout = TimeSpan.FromSeconds(5);

    public async Task WriteAsync(RequestLogEntry entry, CancellationToken cancellationToken)
    {
        var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        Directory.CreateDirectory(logsDirectory);

        var dailyFilePath = Path.Combine(logsDirectory, $"requests-{entry.Date:yyyy-MM-dd}.log");
        await AppendLineAsync(dailyFilePath, entry.Data, cancellationToken);
    }

    private static async Task AppendLineAsync(string filePath, string entryJson, CancellationToken cancellationToken)
    {
        await FileLock.WaitAsync(cancellationToken);
        try
        {
            using var mutex = new Mutex(false, BuildMutexName(filePath));
            var hasMutex = WaitForMutex(mutex, cancellationToken);
            Exception? lastException = null;
            try
            {
                for (var attempt = 1; attempt <= MaxWriteAttempts; attempt++)
                {
                    try
                    {
                        await using var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                        await using var writer = new StreamWriter(stream);
                        await writer.WriteLineAsync(entryJson.AsMemory(), cancellationToken);
                        return;
                    }
                    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                    {
                        lastException = exception;
                        if (attempt == MaxWriteAttempts)
                        {
                            break;
                        }

                        await Task.Delay(BaseRetryDelayMilliseconds * attempt, cancellationToken);
                    }
                }

                throw lastException ?? new IOException("Failed to append log line.");
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
            FileLock.Release();
        }
    }

    private static bool WaitForMutex(Mutex mutex, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < MutexTimeout)
        {
            cancellationToken.ThrowIfCancellationRequested();
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

        throw new IOException("Timed out acquiring cross-process log file lock.");
    }

    private static string BuildMutexName(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath).ToUpperInvariant();
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(fullPath));
        var hash = Convert.ToHexString(bytes);
        return $"Local\\FootballPairsLog_{hash}";
    }
}
