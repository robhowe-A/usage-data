using System.Net;
using System.Net.Http.Json;
using UsageData.Application.Features.Usage;
using UsageData.Application.Tests.Infrastructure;
using Xunit;

namespace UsageData.Application.Tests;

public sealed class UsageDataControllerTests(UsageDataApiClientTest clientTest)
    : IClassFixture<UsageDataApiClientTest>
{
    private const string JsonMediaType = "application/json";

    private const int KnownUserAgentId = 1;
    private const int KnownAnonymousUserId = 172;
    private const int KnownAccessRecordAnonymousUserId = 3;
    private const int KnownAccessRecordUserAgentId = 179;

    private readonly HttpClient _client = clientTest.CreateClient();

    [Theory]
    [InlineData("/v1/user-agents")]
    [InlineData("/v1/user-agents/1")]
    [InlineData("/v1/anonymous-users")]
    [InlineData("/v1/anonymous-users/172")]
    [InlineData("/v1/access-records")]
    [InlineData("/v1/access-records/3/179")]
    public async Task Endpoint_ReturnsOk_WithJsonMediaType(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.ParseAdd(JsonMediaType);

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(JsonMediaType, response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetAccessRecordById_ReturnsAccessRecord()
    {
        var result = await GetOkJsonAsync<AccessedRecordDto>(
            $"/v1/access-records/{KnownAccessRecordAnonymousUserId}/{KnownAccessRecordUserAgentId}");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetAccessRecords_ReturnsNonEmptyList()
    {
        var result = await GetOkJsonAsync<List<AccessedRecordDto>>("/v1/access-records");

        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetAnonymousUserById_ReturnsAnonymousUser()
    {
        var result = await GetOkJsonAsync<AnonymousUserDto>($"/v1/anonymous-users/{KnownAnonymousUserId}");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetAnonymousUsers_ReturnsNonEmptyList()
    {
        var result = await GetOkJsonAsync<List<AnonymousUserDto>>("/v1/anonymous-users");

        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetUserAgentById_ReturnsUserAgent()
    {
        var result = await GetOkJsonAsync<UserAgentDto>($"/v1/user-agents/{KnownUserAgentId}");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetUserAgentById_UnknownId_ReturnsNotFound()
    {
        using var response = await _client.GetAsync($"/v1/user-agents/{int.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserAgents_ReturnsNonEmptyList()
    {
        var result = await GetOkJsonAsync<List<UserAgentDto>>("/v1/user-agents");

        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData("/v1/a")]
    [InlineData("/v1/%20")]
    [InlineData("/v1/user-agents/not-an-int")]
    public async Task UnknownRoute_ReturnsNotFound(string url)
    {
        using var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<T> GetOkJsonAsync<T>(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.ParseAdd(JsonMediaType);

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(JsonMediaType, response.Content.Headers.ContentType?.MediaType);

        // Payload has the expected DTO type from deserialization
        var body = await response.Content.ReadFromJsonAsync<T>();
        Assert.NotNull(body);
        return body;
    }
}