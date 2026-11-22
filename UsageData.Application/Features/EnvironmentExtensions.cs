using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Microsoft.EntityFrameworkCore;
using UsageData.Data;

namespace UsageData.Application.Features;

internal static class EnvironmentExtensions
{
    internal static WebApplicationBuilder AddEnvironmentLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        if (builder.Environment.IsDevelopment())
        {
            builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information); // SQL logging
            builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Information);
        }
        else if (builder.Environment.IsStaging())
        {
            builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning); // warnings/errors only
        }
        else // Production
        {
            builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Error); // minimal EF noise
        }

        return builder;
    }

    extension(WebApplication app)
    {
        internal async Task<WebApplication> AddApplicationMigrations(WebApplicationBuilder builder)
        {
            var autoMigrate = builder.Configuration.GetValue<bool>("Database:AutoMigrate");

            if (autoMigrate && (builder.Environment.IsDevelopment() || builder.Environment.IsStaging()))
            {
                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<UserDataContext>();
                await db.Database.MigrateAsync();
            }

            return app;
        }

        internal WebApplication LogEnvironmentMetadata()
        {
            using var scope = app.Services.CreateScope();
            var db  = scope.ServiceProvider.GetRequiredService<UserDataContext>();
            var asm = Assembly.GetEntryAssembly()!;

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

            return app;

            string? Meta(string key) => asm.GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(a => a.Key == key)?.Value;

        }
    }
}
