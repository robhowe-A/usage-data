namespace UsageData.Application.Tests.Infrastructure;

public sealed class UsageDataApiClientTest : IDisposable
{
    private TestAppFactory Factory { get; } = new("Development");

    public HttpClient CreateClient() => Factory.CreateClient();

    public void Dispose() => Factory.Dispose();
}