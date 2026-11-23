using Microsoft.EntityFrameworkCore;
using UsageData.Application.Features;
using UsageData.Data;
using UsageData.Domain.Entities;

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
});

builder.AddEnvironmentLogging();

var app = builder.Build();

app.UseApplicationProblemDetails();

var skipMigrationsForTests = builder.Configuration.GetValue<bool>("Testing:SkipMigrations");
if (!skipMigrationsForTests)
{
    await app.AddApplicationMigrations(builder);
}

app.UseHttpsRedirection();

app.LogEnvironmentMetadata();

#region MinimalApi

var enableThrowEndpointForTests = builder.Configuration.GetValue<bool>("Testing:EnableThrowEndpoint");
if (enableThrowEndpointForTests)
{
    app.MapGet("/__test/throw", () =>
    {
        throw new InvalidOperationException("Intentional test exception");
    });
}


app.MapGet("/usage", async (UserDataContext dbContext, CancellationToken cancellationToken) =>
{
    var usage = await dbContext.UserAgents
        .AsNoTracking()
        .ToListAsync(cancellationToken);

    return Results.Ok(usage);
});

app.MapGet("/usage/{uid:int}", async (int uid, UserDataContext dbContext, CancellationToken cancellationToken) =>
{
    var usage = await dbContext.Set<UserAgent>()
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.AgentId == uid, cancellationToken);

    if (usage is null)
        return Results.NotFound();

    return Results.Ok(usage);
});

app.MapGet("/usage/anonymous-users", async (UserDataContext dbContext, CancellationToken cancellationToken) =>
{
    var users = await dbContext.AnonymousUsers
        .AsNoTracking()
        .ToListAsync(cancellationToken);

    return Results.Ok(users);
});

app.MapGet("/usage/anonymous-users/{anonymousUserId:int}", async (
    int anonymousUserId,
    UserDataContext dbContext,
    CancellationToken cancellationToken) =>
{
    var user = await dbContext.AnonymousUsers
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.AnonymousUserId == anonymousUserId, cancellationToken);

    if (user is null)
        return Results.NotFound();

    return Results.Ok(user);
});

app.MapGet("/usage/accessrecords", async (UserDataContext dbContext, CancellationToken cancellationToken) =>
{
    var records = await dbContext.AccessRecords
        .AsNoTracking()
        .ToListAsync(cancellationToken);

    return Results.Ok(records);
});

app.MapGet("/usage/accessrecords/{anonymousUserId:int}/{userAgentId:int}", async (
    int anonymousUserId,
    int userAgentId,
    UserDataContext dbContext,
    CancellationToken cancellationToken) =>
{
    var record = await dbContext.AccessRecords
        .AsNoTracking()
        .FirstOrDefaultAsync(
            r => r.AnonymousUserId == anonymousUserId && r.UserAgentId == userAgentId,
            cancellationToken);

    if (record is null)
        return Results.NotFound();

    return Results.Ok(record);
});
#endregion

app.Run();
