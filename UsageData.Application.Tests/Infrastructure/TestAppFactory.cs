using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace UsageData.Application.Tests.Infrastructure;

public sealed class TestAppFactory(string environmentName) : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment(environmentName);

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Testing:EnableThrowEndpoint"] = "true",
                ["Testing:SkipMigrations"] = "true"
            });
        });

        return base.CreateHost(builder);
    }
}
