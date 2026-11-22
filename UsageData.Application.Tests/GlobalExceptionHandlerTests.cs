using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UsageData.Application.Tests.Infrastructure;
using Xunit;

namespace UsageData.Application.Tests;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task Development_Returns500_ProblemJson_NoStackTrace_AndTraceId()
    {
        var (statusCode, contentType, body) = await CallThrowEndpointAsync("Development");

        Assert.Equal(HttpStatusCode.InternalServerError, statusCode);
        Assert.StartsWith("application/problem+json", contentType, StringComparison.OrdinalIgnoreCase);
        AssertNoStackTrace(body);
        AssertTraceIdPresent(body);
    }

    [Fact]
    public async Task Staging_Returns500_ProblemJson_NoStackTrace_AndTraceId()
    {
        var (statusCode, contentType, body) = await CallThrowEndpointAsync("Staging");

        Assert.Equal(HttpStatusCode.InternalServerError, statusCode);
        Assert.StartsWith("application/problem+json", contentType, StringComparison.OrdinalIgnoreCase);
        AssertNoStackTrace(body);
        AssertTraceIdPresent(body);
    }

    [Fact]
    public async Task Development_And_Staging_Responses_Differ()
    {
        var (_, _, devBody) = await CallThrowEndpointAsync("Development");
        var (_, _, prodBody) = await CallThrowEndpointAsync("Staging");

        Assert.NotEqual(devBody, prodBody);
    }

    private static async Task<(HttpStatusCode StatusCode, string ContentType, string Body)> CallThrowEndpointAsync(
        string environmentName)
    {
        using var factory = new TestAppFactory(environmentName);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using var response = await client.GetAsync("/__test/throw");
        var body = await response.Content.ReadAsStringAsync();
        var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

        return (response.StatusCode, contentType, body);
    }

    private static void AssertNoStackTrace(string body)
    {
        Assert.DoesNotContain("StackTrace", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at ", body, StringComparison.Ordinal);
    }

    private static void AssertTraceIdPresent(string body)
    {
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        string? traceId = null;

        if (root.TryGetProperty("traceId", out var traceIdElement) &&
            traceIdElement.ValueKind == JsonValueKind.String)
        {
            traceId = traceIdElement.GetString();
        }

        Assert.False(string.IsNullOrWhiteSpace(traceId));
    }
}