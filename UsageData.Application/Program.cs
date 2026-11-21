using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Microsoft.EntityFrameworkCore;
using UsageData.Data;
using UsageData.Domain.Entities;
using UsageData.Exceptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UserDataContext>(dbContextOptionsBuilder =>
{
    var connectionString = builder.Configuration.GetConnectionString("MYSQL_CONNECTIONSTRING");
    dbContextOptionsBuilder.UseMySQL(connectionString!, mySqlOptionsAction =>
    {
        mySqlOptionsAction.EnableRetryOnFailure(5);
        mySqlOptionsAction.CommandTimeout(30);
    });
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandler>();
app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var db  = scope.ServiceProvider.GetRequiredService<UserDataContext>();
    var asm = Assembly.GetEntryAssembly()!;
    string? Meta(string key) => asm.GetCustomAttributes<AssemblyMetadataAttribute>()
        .FirstOrDefault(a => a.Key == key)?.Value;

    var sb = new StringBuilder();
    sb.AppendLine($"Project:      {app.Environment.ApplicationName}");
    sb.AppendLine($"Version:      {asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");
    sb.AppendLine($"Author:       {asm.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company}");
    sb.AppendLine($"Copyright:    {asm.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright}");
    sb.AppendLine($"Created:      {Meta("CreateDate") ?? "unknown"}");
    sb.AppendLine($"Built:        {Meta("BuildDate")} UTC");
    sb.AppendLine($"Target:       {asm.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName}");
    sb.AppendLine($"Runtime:      {RuntimeInformation.FrameworkDescription}");
    sb.AppendLine($"EF Provider:  {db.Database.ProviderName}");

    try
    {
        db.Database.OpenConnection();
        sb.AppendLine($"DB Server:    {db.Database.GetDbConnection().ServerVersion}");
    }
    catch (Exception ex) { sb.AppendLine($"DB Server:    unavailable ({ex.GetType().Name})"); }
    finally { db.Database.CloseConnection(); }

    sb.AppendLine("Dependencies:");
    foreach (var r in asm.GetReferencedAssemblies()
                 .Where(r => !r.Name!.StartsWith("System") && r.Name != "netstandard")
                 .OrderBy(r => r.Name))
        sb.AppendLine($"  {r.Name} {r.Version}");

    File.WriteAllText("metadata.txt", sb.ToString());
}

#region MinimalApi
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
