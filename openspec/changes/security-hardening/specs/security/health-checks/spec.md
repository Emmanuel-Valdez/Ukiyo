## Purpose

Expose lightweight endpoints that orchestrators and load balancers can use to verify the application process is alive and that critical dependencies are reachable.

## ADDED Requirements

### Requirement: Liveness endpoint reports process health
The system SHALL expose a `/health/live` endpoint that returns HTTP `200 OK` when the application process is running, requiring no dependency checks.

#### Scenario: Running app responds to liveness probe
- **WHEN** a monitoring system calls `/health/live`
- **THEN** the response is `200 OK` with a healthy status

### Requirement: Readiness endpoint checks critical dependencies
The system SHALL expose a `/health/ready` endpoint that checks the database and the configured MinIO/S3 image storage; it returns `200 OK` only when both are reachable and `503 Service Unavailable` otherwise.

#### Scenario: All dependencies healthy
- **WHEN** the database and MinIO/S3 storage are reachable
- **THEN** `/health/ready` returns `200 OK`

#### Scenario: Database unavailable
- **WHEN** the database is unreachable
- **THEN** `/health/ready` returns `503 Service Unavailable` and marks the database check unhealthy

#### Scenario: Storage unavailable
- **WHEN** MinIO/S3 storage is unreachable
- **THEN** `/health/ready` returns `503 Service Unavailable` and marks the storage check unhealthy

### Requirement: Health endpoints do not require authentication
The system SHALL allow anonymous access to `/health/live` and `/health/ready` so orchestrators and load balancers can probe them.

#### Scenario: Anonymous probe succeeds
- **WHEN** an unauthenticated caller requests `/health/live`
- **THEN** the request succeeds without a challenge

### Requirement: Health responses are concise
The system SHALL return a small JSON response from health endpoints and SHALL not include sensitive connection strings or credentials.

#### Scenario: Health response contains no secrets
- **WHEN** `/health/ready` is called
- **THEN** the response body does not contain connection strings, keys, or bucket credentials
