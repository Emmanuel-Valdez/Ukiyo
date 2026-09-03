using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VaultShop.Models;

namespace VaultShop.Web.Tests;

public class IdentityLockoutTests
{
    [Fact]
    public async Task RepeatedFailedLogins_LockAccount_AndReturnsGenericMessageForLockedAndNonExistentUsers()
    {
        using var factory = new CustomWebApplicationFactory().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Identity:Lockout:AllowedForNewUsers"] = "true",
                    ["Identity:Lockout:MaxFailedAccessAttempts"] = "3",
                    ["Identity:Lockout:DefaultLockoutTimeSpanMinutes"] = "30",
                });
            });
        });
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        TestAuthHelper.SetRequestCulture(client, "en-US");

        // Use a fresh email to avoid cross-test state issues.
        var email = "lockout.tests@vaultshop.local";
        using (var scope = factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Name = "Lockout Test User",
                    EmailConfirmed = true,
                };
                await userManager.CreateAsync(user, "Test123!");
                await userManager.AddToRoleAsync(user, Utility.SD.Role_Customer);
            }
        }

        // Fail login MaxFailedAccessAttempts times with a wrong password.
        for (var i = 0; i < 3; i++)
        {
            var token = await TestAuthHelper.GetAntiforgeryTokenAsync(client, "/Identity/Account/Login");
            var form = new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = "WrongPassword123!",
                ["__RequestVerificationToken"] = token,
            };
            var response = await client.PostAsync("/Identity/Account/Login", new FormUrlEncodedContent(form));

            if (i < 2)
            {
                // Before lockout, failed login returns the login page with a generic error.
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var body = await response.Content.ReadAsStringAsync();
                Assert.Contains("Invalid login attempt", body);
            }
            else
            {
                // On the final failure the account locks and redirects to Lockout page.
                Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
                Assert.EndsWith("/Identity/Account/Lockout", response.Headers.Location!.OriginalString);
            }
        }

        // A subsequent valid password attempt while locked should still redirect to Lockout, not sign in.
        {
            var token = await TestAuthHelper.GetAntiforgeryTokenAsync(client, "/Identity/Account/Login");
            var form = new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = "Test123!",
                ["__RequestVerificationToken"] = token,
            };
            var response = await client.PostAsync("/Identity/Account/Login", new FormUrlEncodedContent(form));
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.EndsWith("/Identity/Account/Lockout", response.Headers.Location!.OriginalString);
        }

        // Non-existent user must return the same generic error message, not reveal absence.
        {
            var token = await TestAuthHelper.GetAntiforgeryTokenAsync(client, "/Identity/Account/Login");
            var form = new Dictionary<string, string>
            {
                ["Input.Email"] = "does.not.exist@vaultshop.local",
                ["Input.Password"] = "AnyPassword123!",
                ["__RequestVerificationToken"] = token,
            };
            var response = await client.PostAsync("/Identity/Account/Login", new FormUrlEncodedContent(form));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("Invalid login attempt", body);
            Assert.DoesNotContain("does not exist", body, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task Lockout_DisabledForNewUsers_DoesNotLockAccount()
    {
        using var factory = new CustomWebApplicationFactory().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Identity:Lockout:AllowedForNewUsers"] = "false",
                    ["Identity:Lockout:MaxFailedAccessAttempts"] = "3",
                    ["Identity:Lockout:DefaultLockoutTimeSpanMinutes"] = "30",
                });
            });
        });
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        TestAuthHelper.SetRequestCulture(client, "en-US");

        var email = "nolockout.tests@vaultshop.local";
        using (var scope = factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Name = "No Lockout Test User",
                    EmailConfirmed = true,
                };
                await userManager.CreateAsync(user, "Test123!");
                await userManager.AddToRoleAsync(user, Utility.SD.Role_Customer);
            }
        }

        // Fail more than MaxFailedAccessAttempts times; lockout is disabled, so account should not lock.
        for (var i = 0; i < 4; i++)
        {
            var token = await TestAuthHelper.GetAntiforgeryTokenAsync(client, "/Identity/Account/Login");
            var form = new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = "WrongPassword123!",
                ["__RequestVerificationToken"] = token,
            };
            var response = await client.PostAsync("/Identity/Account/Login", new FormUrlEncodedContent(form));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("Invalid login attempt", body);
            Assert.DoesNotContain("/Identity/Account/Lockout", body);
        }
    }

    [Fact]
    public async Task RegisterAndPasswordReset_FlowsStillWork_WithLockoutEnabled()
    {
        using var factory = new CustomWebApplicationFactory().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Identity:Lockout:AllowedForNewUsers"] = "true",
                    ["Identity:Lockout:MaxFailedAccessAttempts"] = "3",
                    ["Identity:Lockout:DefaultLockoutTimeSpanMinutes"] = "30",
                });
            });
        });
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Register a new user: should succeed and redirect to RegisterConfirmation (confirmed account required).
        var registerToken = await TestAuthHelper.GetAntiforgeryTokenAsync(client, "/Identity/Account/Register");
        var registerResponse = await client.PostAsync("/Identity/Account/Register", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["Input.Email"] = "register.lockout.tests@vaultshop.local",
                ["Input.Password"] = "Test123!",
                ["Input.ConfirmPassword"] = "Test123!",
                ["Input.Name"] = "Register Lockout Test",
                ["__RequestVerificationToken"] = registerToken,
            }));

        // Registration succeeds: with confirmed accounts required it redirects to confirmation; otherwise it may sign-in and redirect home.
        Assert.Equal(HttpStatusCode.Redirect, registerResponse.StatusCode);
        var registerLocation = registerResponse.Headers.Location!.OriginalString;
        Assert.True(
            registerLocation.Contains("/Identity/Account/RegisterConfirmation") || registerLocation == "/",
            $"Expected redirect to RegisterConfirmation or root, got: {registerLocation}");

        // Forgot password flow: should always redirect to confirmation regardless of user existence.
        var forgotToken = await TestAuthHelper.GetAntiforgeryTokenAsync(client, "/Identity/Account/ForgotPassword");
        var forgotResponse = await client.PostAsync("/Identity/Account/ForgotPassword", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["Input.Email"] = "register.lockout.tests@vaultshop.local",
                ["__RequestVerificationToken"] = forgotToken,
            }));
        Assert.Equal(HttpStatusCode.Redirect, forgotResponse.StatusCode);
        Assert.Contains("/Identity/Account/ForgotPasswordConfirmation", forgotResponse.Headers.Location!.OriginalString);
    }
}
