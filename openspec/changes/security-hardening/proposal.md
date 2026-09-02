## Why

The application currently ships with default Identity lockout disabled, no global rate limiting configured, bare 404 responses, and no health endpoint. These gaps create avoidable operational and abuse risk before launch: brute-force login attempts, accidental or malicious endpoint overload, poor error UX, and invisible failures in deployments. This change closes the four smallest high-value security gaps identified in the Phase 11 backlog.

## What Changes

- **Rate limiting:** Add ASP.NET Core rate limiting with a global fixed-window policy and a stricter login-specific policy applied to Identity account pages. Configure per-client rejection behavior, on-rejected logging, and response status `429`.
- **Identity lockout:** Enable account lockout on password failure (`lockoutOnFailure: true`) in the login page, configure lockout duration and max failed attempts via existing options, and ensure failed login attempts increment correctly.
- **Status code pages:** Register `UseStatusCodePages` with a re-executing path to a shared error page so bare 404/500 responses become user-friendly, localized, and on-brand.
- **Health checks:** Add `AddHealthChecks` with liveness/readiness-style endpoints (`/health/live`, `/health/ready`) and basic checks for the database and MinIO/S3 image storage.

## Capabilities

### New Capabilities

- `security/rate-limiting`: Global and login-scoped request rate limiting with configurable windows, reject logging, and 429 responses.
- `security/identity-lockout`: Identity account lockout enabled on password failure with configurable thresholds and duration.
- `security/status-code-pages`: Friendly, re-executed status code pages for 404/500-class responses.
- `security/health-checks`: Liveness and readiness health endpoints with database and MinIO checks.

### Modified Capabilities

_(none — these are additive operational-security capabilities that do not change existing business rules)_

## Impact

- **Files:** `Program.cs`, `Login.cshtml.cs`, shared error view/controller or existing error handling, plus small additions to `appsettings.json` and environment example files.
- **Dependencies:** No new NuGet packages — uses ASP.NET Core built-in `System.Threading.RateLimiting`, `Microsoft.AspNetCore.Diagnostics`, `Microsoft.Extensions.Diagnostics.HealthChecks`, and Identity APIs already referenced.
- **Risk:** Low-to-medium — touches authentication middleware ordering and 404 behavior. Lockout misconfiguration could lock out legitimate users; rate limits must not break legitimate checkout or webhook traffic.
