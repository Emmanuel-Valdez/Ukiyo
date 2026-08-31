# Product Detail Quantity Specification

## Purpose

Controls the quantity control on the product detail page: a minus/plus stepper around a centered number input, equal-height with the Add-to-Cart row, with browser-safe minimum of 1.

## Requirements

### Requirement: Quantity stepper layout
The product detail page quantity section SHALL render a stepper group (minus button, number input, plus button) with no visible label. The stepper group and the number input SHALL expose localized `aria-label`s, and the minus/plus buttons localized `aria-label`s.

#### Scenario: Desktop layout
- **WHEN** user views the product detail page on a desktop viewport
- **THEN** the stepper and the Add to Cart (and favorite) buttons share one row with equal height (2.75rem), and the number inside the stepper is centered

#### Scenario: Mobile layout
- **WHEN** user views the product detail page on a narrow (mobile) viewport
- **THEN** the action buttons wrap below the stepper, the stepper keeps its internal minus/number/plus alignment and equal heights, and the number remains centered

#### Scenario: Native spinners hidden
- **WHEN** the number input is rendered
- **THEN** the browser's native increment/decrement spinners are hidden (replaced by the stepper buttons)

### Requirement: Stepper behavior
The stepper buttons SHALL adjust the quantity by 1 per click while never allowing the value to drop below the configured minimum.

#### Scenario: Increment
- **WHEN** user clicks the plus button
- **THEN** the quantity increases by 1 and never exceeds the maximum valid quantity

#### Scenario: Decrement at minimum
- **WHEN** the quantity is `1` and the user clicks the minus button
- **THEN** the value stays `1` (never below the minimum) and the minus button is disabled
- **AND** the `input` event fires so client-side validation reacts

### Requirement: Minimum quantity constraint
The quantity input SHALL reject values less than 1 at the browser level using the native `min` attribute.

#### Scenario: User enters zero
- **WHEN** user types `0` in the quantity input and attempts to submit
- **THEN** the browser prevents submission and highlights the input as invalid

#### Scenario: User enters negative number
- **WHEN** user types `-5` in the quantity input and attempts to submit
- **THEN** the browser prevents submission and highlights the input as invalid

#### Scenario: User enters valid quantity
- **WHEN** user enters `1` or greater in the quantity input
- **THEN** the value is accepted and the form submits normally