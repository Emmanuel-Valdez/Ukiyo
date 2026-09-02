## Purpose

Protect the application from accidental or malicious request floods by enforcing per-client request limits, with stricter limits on authentication endpoints that are common brute-force targets.

## ADDED Requirements

### Requirement: Global request rate limit is enforced
The system SHALL apply a global per-client rate limit to all HTTP requests with a configurable fixed window, permit count, and queue limit, and SHALL reject exceeded requests with HTTP status `429 Too Many Requests`.

#### Scenario: Normal traffic allowed
- **WHEN** a client makes requests within the configured global limit
- **THEN** all requests are processed normally

#### Scenario: Excessive global traffic rejected
- **WHEN** a client exceeds the configured global request limit within a window
- **THEN** subsequent requests receive `429 Too Many Requests` until the window resets

### Requirement: Login endpoints have a stricter rate limit
The system SHALL apply a separate, stricter rate-limiting policy to Identity login and password-related endpoints, so that brute-force credential guessing is slowed independently of the global limit.

#### Scenario: Repeated login attempts throttled
- **WHEN** a client submits many login requests in quick succession
- **THEN** requests beyond the login policy limit receive `429 Too Many Requests`

### Requirement: Rate-limit rejections are observable
The system SHALL log each rate-limited rejection with the client identifier, endpoint path, and configured policy name.

#### Scenario: Rejected request logged
- **WHEN** a request is rejected by rate limiting
- **THEN** a warning/log entry records the policy, path, and partitioned client key

### Requirement: Rate limits are configurable without code changes
The system SHALL read rate-limiting window size, permit count, and queue limit from configuration so values can differ between local development and production.

#### Scenario: Production config overrides defaults
- **WHEN** configuration supplies custom rate-limiting values
- **THEN** those values are used instead of the compiled defaults
