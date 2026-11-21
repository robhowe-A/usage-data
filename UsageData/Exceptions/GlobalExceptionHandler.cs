using Microsoft.AspNetCore.Mvc;

namespace UsageData.Exceptions;

internal sealed class GlobalExceptionHandler(
    RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("The client cancelled the request.");
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = ex switch
            {
                ApplicationException => StatusCodes.Status400BadRequest,
                BadHttpRequestException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            await context.Response.WriteAsJsonAsync(
                new ProblemDetails { 
                    Type = ex.GetType().Name,
                    Title = "An error occurred.",
                    Detail = ex.Message
                });
        }
    }
}
