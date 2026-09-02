using System.Net;

namespace VaultShop.Web.Tests;

public class HealthChecksTests
{
    private static HttpClient CreateClient() =>
        new CustomWebApplicationFactory().CreateClient();

    [Fact]
    public async Task Live_Returns200_WithoutDependencyChecks()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("\"database\"", body);
        Assert.DoesNotContain("\"storage\"", body);
    }

    [Fact]
    public async Task Ready_Returns200_WithDatabaseAndStorageChecks()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"database\"", body);
        Assert.Contains("\"storage\"", body);
        Assert.Contains("\"Healthy\"", body);
    }

    [Fact]
    public async Task HealthResponses_DoNotExposeSecrets()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var readyBody = await (await client.GetAsync("/health/ready")).Content.ReadAsStringAsync();
        var liveBody = await (await client.GetAsync("/health/live")).Content.ReadAsStringAsync();

        var forbidden = new[] { "Password=", "Data Source=", "SecretKey", "AccessKey", "sk_test" };
        foreach (var token in forbidden)
        {
            Assert.DoesNotContain(token, readyBody);
            Assert.DoesNotContain(token, liveBody);
        }
    }
}
