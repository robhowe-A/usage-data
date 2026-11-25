using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using UsageData.Application.Features;
using UsageData.Application.Features.Usage;
using UsageData.Application.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationProblemDetails();

builder.Services.AddDbContext<UserDataContext>(dbContextOptionsBuilder =>
{
    var connectionString = builder.Configuration["MYSQL_CONNECTIONSTRING"]
                           ?? throw new InvalidOperationException("Missing connection string: MYSQL_CONNECTIONSTRING")
        ;
    dbContextOptionsBuilder.UseMySQL(connectionString, 
        mySqlOptionsAction =>
        {
            if (!builder.Environment.IsStaging() && !builder.Environment.IsProduction()) return;

            mySqlOptionsAction.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            mySqlOptionsAction.CommandTimeout(30);
        });
    if (builder.Environment.IsDevelopment())
    {
        dbContextOptionsBuilder.EnableDetailedErrors();
        dbContextOptionsBuilder.EnableSensitiveDataLogging();
    }
    else
    {
        dbContextOptionsBuilder.EnableDetailedErrors(false);
        dbContextOptionsBuilder.EnableSensitiveDataLogging(false);
    }
},
ServiceLifetime.Scoped, 
ServiceLifetime.Scoped);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateOnBuild = true;
    options.ValidateScopes = true;
});

builder.AddEnvironmentLogging();
builder.Services.AddControllers();
builder.Services.AddScoped<IUsageService, UsageService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseApplicationProblemDetails();

var skipMigrationsForTests = builder.Configuration.GetValue<bool>("Testing:SkipMigrations");
if (!skipMigrationsForTests)
{
    await app.AddApplicationMigrations(builder);
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var requestMethod = context.Request.Method;
    var requestPath = context.Request.Path;
    var responseStatus = context.Response;
    var responseStatusCode = context.Response?.StatusCode;
    
    try
    {
        logger.LogInformation($"{DateTime.Now.ToLongDateString()} \"{requestMethod} {requestPath}\" {responseStatusCode} {traceId}");
        await next(context);
    }
    // Intercept client-aborted cancellations before ExceptionHandlerMiddleware swallows them
    catch (OperationCanceledException except) when (context.RequestAborted.IsCancellationRequested)
    {
        logger.LogInformation(
            $"Client request cancel exception handled: \"{requestMethod} {requestPath}\" {responseStatusCode}");

        if (!responseStatus.HasStarted)
        {
            context.Response?.StatusCode = StatusCodes.Status499ClientClosedRequest;
        }
        var exception = new Exception("exception...", except);

        exception.Data.Add("traceId", traceId);
        throw exception;
    }
});

app.UseSwagger(opt =>
{
    opt.RouteTemplate = "sandbox/{documentName}/openapi.json";
});
app.UseSwaggerUI(opt =>
{
    opt.RoutePrefix = "sandbox";
    opt.SwaggerEndpoint("/sandbox/v1/openapi.json", "API Demo v1");
    opt.EnableTryItOutByDefault(); 
});

app.MapControllers();

app.LogEnvironmentMetadata();

var enableThrowEndpointForTests = builder.Configuration.GetValue<bool>("Testing:EnableThrowEndpoint");
if (enableThrowEndpointForTests)
{
    app.MapGet("/__test/throw", () =>
    {
        throw new InvalidOperationException("Intentional test exception");
    });
}

app.Run();
