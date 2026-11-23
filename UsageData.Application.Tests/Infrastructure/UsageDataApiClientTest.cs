using System.Net;

namespace UsageData.Application.Tests.Infrastructure;

public sealed class UsageDataApiClientTest : IDisposable
{
    private TestAppFactory Factory { get; } = new("Development");

    public HttpClient CreateClient()
    {
        var client = Factory.CreateClient();
        
        client.DefaultRequestHeaders.UserAgent.ParseAdd("UsageData Application Test | Robert Howell");
        client.DefaultRequestVersion = HttpVersion.Version20;
        client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
        
        return Factory.CreateClient();
    }

    public void Dispose() => Factory.Dispose();
}