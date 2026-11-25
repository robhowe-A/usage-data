
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace UsageData.Application.Exceptions;

internal sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var request = httpContext.Request;
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        if ((exception is OperationCanceledException || exception.InnerException is OperationCanceledException) && httpContext.RequestAborted.IsCancellationRequested)
        {   //The client disconnected
            LogRequestAborted(logger, request.Method, request.Path, traceId);
            httpContext.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            return true;
        }

        var (status, title) = Map(exception);

        // Log the path only, no PII or tokens.
        LogUnhandledException(logger, exception, status, request.Method, request.Path, traceId);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = status >= 500
                ? "https://tools.ietf.org/html/rfc9110#section-15.6.1"
                : "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Instance = $"{request.Method} {request.Path}",
            Extensions = { ["traceId"] = traceId }
        };

        if (environment.IsDevelopment())
        {
            problem.Detail = exception.Message;
            problem.Extensions["exceptionType"] = exception.GetType().FullName;
        }

        httpContext.Response.StatusCode = status;

        var written = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });

        // Fallback: TryWriteAsync returns false when the Accept header rules out JSON.
        if (!written)
        {
            await httpContext.Response.WriteAsJsonAsync(
                problem, options: null, contentType: "application/problem+json", cancellationToken);
        }

        return true;
    }

    private static (int Status, string Title) Map(Exception exception) => exception switch
    {
        BadHttpRequestException bad => (bad.StatusCode, "Bad request."),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
    };

    [LoggerMessage(EventId = 5000, Level = LogLevel.Error,
        Message = "Unhandled exception. Status={StatusCode} Method={Method} Path={Path} TraceId={TraceId}")]
    private static partial void LogUnhandledException(
        ILogger logger, Exception exception, int statusCode, string method, string path, string traceId);

    [LoggerMessage(EventId = 5001, Level = LogLevel.Information,
        Message = "Request aborted by client. Method={Method} Path={Path} TraceId={TraceId}")]
    private static partial void LogRequestAborted(ILogger logger, string method, string path, string traceId);
}