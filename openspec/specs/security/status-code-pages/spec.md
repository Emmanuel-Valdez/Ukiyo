# security/status-code-pages Specification

## Purpose
Replace bare HTTP status responses for 404/500-class errors with branded, localized pages that preserve site navigation and do not leak exception details in production.

## Requirements

### Requirement: Status code errors show a friendly page
The system SHALL register a status code handler that re-executes a shared error page for `404 Not Found` and `500 Internal Server Error`, returning the original status code with a user-friendly body.

#### Scenario: Unknown route shows 404 page
- **WHEN** a visitor requests a route that does not exist
- **THEN** the response status is `404` and the body shows the shared error page with navigation and localized copy

#### Scenario: Server error shows 500 page
- **WHEN** an unhandled exception occurs during a request
- **THEN** the response status is `500` and the body shows the shared error page without exception details in production

### Requirement: Error pages preserve brand and localization
The system SHALL render error pages using the same layout, theme, and localization resources as the rest of the site.

#### Scenario: Error page in Spanish
- **WHEN** the request culture is `es-AR` and a `404` occurs
- **THEN** the error page displays Spanish copy and keeps the active theme

### Requirement: Error pages do not leak details
The system SHALL not render stack traces, exception messages, or internal paths on the shared error page outside the development environment.

#### Scenario: Production error page is safe
- **WHEN** a `500` error occurs in a non-development environment
- **THEN** the response body contains only a generic message and support link
