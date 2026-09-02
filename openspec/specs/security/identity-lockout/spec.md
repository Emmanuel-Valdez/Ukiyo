# security/identity-lockout Specification

## Purpose
Reduce the risk of credential brute-force attacks by enabling and enforcing ASP.NET Core Identity account lockout after repeated failed password attempts.

## Requirements

### Requirement: Failed password login attempts increment lockout
The system SHALL enable Identity lockout on the password login path and SHALL increment the failed access count and lock the account after the configured number of consecutive failures.

#### Scenario: Successful login does not lock account
- **WHEN** a user enters correct credentials
- **THEN** the user is signed in and the failed access count is reset

#### Scenario: Repeated failures lock account
- **WHEN** a user exceeds the configured failed login attempts within the lockout window
- **THEN** the account becomes locked for the configured duration and login is denied

### Requirement: Locked-out users see a friendly message
The system SHALL not disclose whether the account exists; for both non-existent users and locked-out users the login page SHALL show the same generic failure message.

#### Scenario: Locked account gets generic failure
- **WHEN** a locked-out user attempts to log in
- **THEN** the login page shows the same invalid-credential message as a non-existent user

### Requirement: Lockout settings are configurable
The system SHALL read maximum failed attempts, lockout duration, and default lockout enabled state from configuration.

#### Scenario: Production config sets stricter lockout
- **WHEN** configuration supplies custom lockout values
- **THEN** those values are used instead of compiled defaults

### Requirement: Lockout does not break legitimate flows
The system SHALL ensure external login, password reset, and registration flows continue to function while lockout is enabled.

#### Scenario: External login not affected by password lockout
- **WHEN** a user signs in via an external login provider
- **THEN** the password lockout counter is not consulted and the sign-in succeeds if the provider validates the account
