using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace VaultShop.Web.Tests;

public class RateLimitingTests
{
    [Fact]
    public async Task Login_RateLimiting_Returns429AfterExceedingLoginPolicy()
    {
        // Override Login limit to 5 so the test runs fast and is isolated from default config changes.
        using var factory = new CustomWebApplicationFactory().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["RateLimiting:LoginPermitLimit"] = "5",
                    ["RateLimiting:LoginWindowSeconds"] = "60",
                    ["RateLimiting:LoginQueueLimit"] = "0",
                    ["RateLimiting:GlobalPermitLimit"] = "100",
                    ["RateLimiting:GlobalWindowSeconds"] = "60",
                });
            });
        });
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // First 5 requests within the Login window should succeed (200).
        for (var i = 0; i < 5; i++)
        {
            var ok = await client.GetAsync("/Identity/Account/Login");
            Assert.NotEqual(HttpStatusCode.TooManyRequests, ok.StatusCode);
            Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        }

        // 6th request exceeds the Login policy -> 429.
        var rejected = await client.GetAsync("/Identity/Account/Login");
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
    }

    [Fact]
    public async Task Login_RateLimiting_NormalResponsesBeforeLimit()
    {
        using var factory = new CustomWebApplicationFactory().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["RateLimiting:LoginPermitLimit"] = "10",
                    ["RateLimiting:LoginWindowSeconds"] = "60",
                    ["RateLimiting:LoginQueueLimit"] = "0",
                });
            });
        });
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // 3 requests well under the limit should all succeed normally.
        for (var i = 0; i < 3; i++)
        {
            var response = await client.GetAsync("/Identity/Account/Login");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
