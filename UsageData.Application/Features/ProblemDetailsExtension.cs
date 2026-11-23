using System.Diagnostics;
using UsageData.Application.Exceptions;

namespace UsageData.Application.Features;

internal static class ProblemDetailsExtensions
{
    extension(WebApplicationBuilder builder)
    {
        internal WebApplicationBuilder AddApplicationProblemDetails()
        {
            builder.Services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = ctx =>
                {
                    var http = ctx.HttpContext;
                    ctx.ProblemDetails.Instance ??= $"{http.Request.Method} {http.Request.Path}";
                    ctx.ProblemDetails.Extensions.TryAdd("traceId", Activity.Current?.Id ?? http.TraceIdentifier);
                };
            });

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            return builder;
        }
    }

    extension(WebApplication app)
    {
        internal WebApplication UseApplicationProblemDetails()
        {
            app.UseExceptionHandler();
            return app;
        }
    }
}