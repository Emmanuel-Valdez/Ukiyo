## 1. Rate Limiting

- [x] 1.1 Add `RateLimiterOptions` POCO and bind to configuration section `RateLimiting` with sensible defaults (global: 100 req/1 min, login: 10 req/1 min) and verify values resolve via `IOptions<RateLimiterOptions>`
- [x] 1.2 Register `AddRateLimiter` in `Program.cs` with a global fixed-window policy and a stricter `Login` policy, and verify the app starts without exception
- [x] 1.3 Apply `[EnableRateLimiting("Login")]` to Identity account endpoints and verify login requests hit the login policy
- [x] 1.4 Configure `OnRejected` to log policy name, path, and client partition key, and verify a 429 response is logged
- [x] 1.5 Add a test that asserts a 429 is returned after exceeding the login policy limit and normal responses before the limit

## 2. Identity Lockout

- [x] 2.1 Configure Identity lockout options (`AllowedForNewUsers`, `MaxFailedAccessAttempts`, `DefaultLockoutTimeSpan`) via `IdentityOptions` and bind from configuration section `Identity:Lockout`, then verify options values at runtime
- [x] 2.2 Change `LoginModel.OnPostAsync` to pass `lockoutOnFailure: true` to `PasswordSignInAsync` and verify the generated lockout counter increments on failed attempts
- [x] 2.3 Verify external login, password reset, and register flows still work when lockout is enabled
- [x] 2.4 Add a test that asserts repeated failed logins lock the account and the same generic message is returned for locked and non-existent users

## 3. Status Code Pages

- [x] 3.1 Register `UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}")` in the correct pipeline order (before routing/after exception handling) and verify unknown routes return the shared error view with status 404
- [x] 3.2 Add localized error title/body strings to existing shared resources or `Home` resources for `es-AR` and `en-US`, and verify the 404 page renders in both cultures
- [x] 3.3 Ensure the error view does not render exception details outside development, and verify the response body contains no stack trace in a non-dev environment
- [x] 3.4 Add a test that asserts a non-existent route returns 404 with the shared view and a known localized phrase

## 4. Health Checks

- [x] 4.1 Register `AddHealthChecks` in `Program.cs` with `AddDbContextCheck<ApplicationDbContext>` and a custom MinIO/S3 storage check, and verify the app starts
- [x] 4.2 Map `/health/live` (no checks) and `/health/ready` (database + storage checks) with `AllowAnonymous` and verify both respond with the expected status codes
- [x] 4.3 Ensure health response JSON contains no connection strings, keys, or bucket credentials, and verify by inspecting response body
- [x] 4.4 Add tests that assert `/health/live` returns 200 and `/health/ready` reflects database/storage health

## 5. Configuration & Documentation

- [ ] 5.1 Add `RateLimiting` and `Identity:Lockout` sections to `appsettings.json` and `.env.compose.example`/`.env.platform.example` if applicable, and verify no secrets are committed
- [ ] 5.2 Update `.local/context/core.md` or `plans/vaultshop-product-readiness-roadmap.md` Phase 11 status to note security hardening in progress
- [ ] 5.3 Run `dotnet test VaultShop.sln` green and run a manual smoke check of 404 page, health endpoints, and a lockout sequence in the browser
