# Rich Text Typography Specification

## Purpose

Normalizes site and product-description typography so numbers render uniformly. Root cause: the Bootstrap Litera theme styled every `<p>` with a Georgia fallback stack, and Georgia's old-style figures make digit heights uneven. Fixed at the root (no Georgia anywhere in app styles) plus numeric figure control on the description.

## Requirements

### Requirement: Consistent site typography
Site paragraph and body text SHALL use the application's sans-serif stack (no Georgia), so digits and letters render with consistent sizing across the whole site.

#### Scenario: Numbers in any paragraph
- **WHEN** any page renders paragraph text containing digits
- **THEN** the digits render in the same sans-serif stack and size as the surrounding text (no old-style Georgia figures)

#### Scenario: Theme fallback
- **WHEN** the page loads in light or dark mode
- **THEN** the sans-serif stack applies in both themes

### Requirement: Consistent description typography
The `.product-detail__description` container SHALL render digits with uniform height and width via `font-variant-numeric: lining-nums tabular-nums`, and inline content SHALL inherit the container's font-size, line-height, and font-family.

#### Scenario: Mixed text and numbers
- **WHEN** a product description contains text mixed with measurements like "38cmx28cmx14cm"
- **THEN** all characters (letters and digits) render at the same font-size with consistent line-height, and digits share uniform height and width

#### Scenario: Quill-generated HTML
- **WHEN** Quill produces `<p>`, `<strong>`, `<em>`, or `<u>` tags in the description
- **THEN** the rendered output inherits the description container's typography normalization

#### Scenario: Dark mode
- **WHEN** page is rendered in dark mode
- **THEN** the description typography normalization still applies with the dark theme's text color

### Requirement: Description content overflow
Long product descriptions SHALL wrap naturally without horizontal overflow or unexpected line breaks on any viewport width.

#### Scenario: Long measurement string
- **WHEN** a description contains a long unbreakable string (e.g., "38cmx28cmx14cm")
- **THEN** the text wraps at the container boundary without causing horizontal scroll