## Purpose

Adds an accessible show/hide toggle button to every password input across Identity pages so users can verify what they typed.

## ADDED Requirements

### Requirement: Password visibility toggle button
Every `type="password"` input in Identity pages (Login, Register, ResetPassword, ChangePassword, SetPassword) SHALL include a toggle button that switches the input between `type="password"` and `type="text"`.

#### Scenario: Toggle reveals password
- **WHEN** user clicks the show/hide toggle on a password field currently masked
- **THEN** the input type changes to `text` and the password characters become visible
- **AND** the toggle icon switches from eye (closed) to eye-slash (open)
- **AND** the toggle's `aria-label` updates to indicate "Hide password"

#### Scenario: Toggle hides password
- **WHEN** user clicks the show/hide toggle on a password field currently visible
- **THEN** the input type changes to `password` and the characters are masked again
- **AND** the toggle icon switches from eye-slash (open) to eye (closed)
- **AND** the toggle's `aria-label` updates to indicate "Show password"

#### Scenario: Keyboard accessibility
- **WHEN** user presses Enter or Space while the toggle button is focused
- **THEN** the toggle activates (same as click)

### Requirement: Toggle visual design
The toggle button SHALL be positioned inside the `form-floating` container, aligned to the right edge of the input, and visible in both light and dark modes.

#### Scenario: Light mode visibility
- **WHEN** page is rendered in light mode
- **THEN** the toggle icon is visible with sufficient contrast against the input background

#### Scenario: Dark mode visibility
- **WHEN** page is rendered in dark mode
- **THEN** the toggle icon is visible with sufficient contrast against the dark input background

### Requirement: Localization support
The toggle button `aria-label` values SHALL be localized for both en-US and es-AR cultures.

#### Scenario: English label
- **WHEN** user's culture is en-US
- **THEN** the toggle aria-label reads "Show password" / "Hide password"

#### Scenario: Spanish label
- **WHEN** user's culture is es-AR
- **THEN** the toggle aria-label reads "Mostrar contraseña" / "Ocultar contraseña"
