using Microsoft.EntityFrameworkCore;
using UsageData.Application.Features;
using UsageData.Application.Features.Usage;
using UsageData.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationProblemDetails();

builder.Services.AddDbContext<UserDataContext>(dbContextOptionsBuilder =>
{
    var connectionString = builder.Configuration["MYSQL_CONNECTIONSTRING"]
                           ?? throw new InvalidOperationException("Missing connection string: MYSQL_CONNECTIONSTRING");
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

var app = builder.Build();

app.UseApplicationProblemDetails();

var skipMigrationsForTests = builder.Configuration.GetValue<bool>("Testing:SkipMigrations");
if (!skipMigrationsForTests)
{
    await app.AddApplicationMigrations(builder);
}

app.UseHttpsRedirection();
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
