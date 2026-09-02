using System.Net;
using Microsoft.AspNetCore.Localization;

namespace VaultShop.Web.Tests;

public class StatusCodePagesTests
{
    [Theory]
    [InlineData("en-US", "en-US", "Page not found")]
    [InlineData("es-AR", "es-AR", "P&#xE1;gina no encontrada")]
    public async Task NonExistentRoute_Returns404_WithLocalizedErrorView(string cookieCulture, string routeCulture, string expectedPhrase)
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Simulate a visitor who selected a language (persisted culture cookie), so the re-executed
        // /Home/Error page resolves the same culture even though the 404 URL has no culture segment.
        client.DefaultRequestHeaders.Add("Cookie",
            $"{CookieRequestCultureProvider.DefaultCookieName}={CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(cookieCulture))}");

        var response = await client.GetAsync($"/{routeCulture}/this-route-does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains(expectedPhrase, body);
    }
}
