using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using FootballPairs.Application.Common.Errors;
using Microsoft.AspNetCore.Mvc;

namespace FootballPairs.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException exception) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation(exception, "Request was canceled by the client.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception during request processing.");
            await WriteProblemDetailsAsync(context, exception);
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        if (exception is ValidationException validationException)
        {
            var validationProblemDetails = new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
                Type = GetTypeUri(StatusCodes.Status400BadRequest),
                Instance = context.Request.Path
            };
            validationProblemDetails.Errors.Add(string.Empty, [validationException.Message]);
            validationProblemDetails.Extensions["traceId"] = traceId;
            validationProblemDetails.Extensions["errorCode"] = ErrorCodes.ValidationFailed;
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(validationProblemDetails);
            return;
        }

        var (statusCode, title, errorCode) = MapException(exception);
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = GetTypeUri(statusCode),
            Instance = context.Request.Path
        };
        problemDetails.Extensions["traceId"] = traceId;
        problemDetails.Extensions["errorCode"] = errorCode;
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static (int StatusCode, string Title, string ErrorCode) MapException(Exception exception)
    {
        return exception switch
        {
            UnauthenticatedException => (StatusCodes.Status401Unauthorized, "Authentication is required.", ErrorCodes.Unauthenticated),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Access is forbidden.", ErrorCodes.Forbidden),
            NotFoundException => (StatusCodes.Status404NotFound, "The requested resource was not found.", ErrorCodes.NotFound),
            ConflictException => (StatusCodes.Status409Conflict, "The request conflicts with the current state.", ErrorCodes.Conflict),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", ErrorCodes.UnhandledError)
        };
    }

    private static string GetTypeUri(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            StatusCodes.Status401Unauthorized => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
            StatusCodes.Status403Forbidden => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            StatusCodes.Status409Conflict => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
            _ => "https://tools.ietf.org/html/rfc9110#section-15.6.1"
        };
    }
}
