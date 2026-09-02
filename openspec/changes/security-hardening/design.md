## Context

The application is an ASP.NET Core 8 MVC monolith using ASP.NET Core Identity. The request pipeline currently has no global rate limiting, lockout is disabled on password login, `UseStatusCodePages` is not registered, and `AddHealthChecks` is absent. All four capabilities can be implemented with built-in framework features and minimal configuration, no new dependencies.

## Goals / Non-Goals

**Goals:**
- Add global and login-scoped rate limiting driven from configuration.
- Enable Identity lockout on the password login path with configurable thresholds.
- Add a shared, localized status-code error page for 404/500.
- Add `/health/live` and `/health/ready` endpoints with database and MinIO checks.

**Non-Goals:**
- WAF-level DDoS protection, CAPTCHA, bot detection, or per-IP firewall rules.
- Email notifications to admins on lockout or health failure.
- Detailed health response bodies beyond `Healthy`/`Unhealthy` status per check.
- Changing Identity password reset/registration lockout behavior.

## Decisions

- **Rate limiting:** Use `AddRateLimiter` with `FixedWindowLimiter`. Global policy applies to all requests; a `Login` policy is applied via `[EnableRateLimiting("Login")]` or endpoint convention to Identity account pages. Client partition key is `Connection.RemoteIpAddress` (default). Rejection logged via `OnRejected`.
- **Lockout:** Set `options.Lockout.AllowedForNewUsers = true` and `lockoutOnFailure: true` in `LoginModel.OnPostAsync`. Existing `SignInManager.PasswordSignInAsync` call already accepts the parameter; current code passes `false`.
- **Status code pages:** Use `UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}")`. `HomeController.Error(int? statusCode)` already exists in the Customer area; reuse it. Ensure the middleware is registered before exception handling.
- **Health checks:** Use `AddHealthChecks().AddDbContextCheck<ApplicationDbContext>()` and a custom `IHealthCheck` that calls `IImageStorageService` / MinIO to verify bucket reachability. Map endpoints with `RequireAuthorization` disabled; keep responses small.

## Risks / Trade-offs

- **Rate limiting on shared IPs:** Users behind NAT may share a partition key → mitigation: tune permit count generously; no user-specific key yet.
- **Lockout locking out admins:** Admin lockout could block store management → mitigation: keep max attempts moderate (5) and duration short (5 min); document manual unlock via DB or Identity admin UI.
- **Health endpoint exposes structure:** `/health/ready` reveals dependency count → mitigation: response only lists check names and status, no connection strings.
- **Middleware order:** `UseStatusCodePagesWithReExecute` must be placed so it catches pipeline errors but does not swallow exception-page details in dev → keep `UseDeveloperExceptionPage` before it in development.

## Migration Plan

1. Deploy code with new settings defaults (rate limits permissive, lockout enabled).
2. Verify `/health/live` and `/health/ready` respond.
3. Verify a non-existent route returns branded 404.
4. Monitor logs for `RateLimiter` rejections and `Identity` lockout events.
5. Rollback: revert commit; defaults restore previous behavior.
