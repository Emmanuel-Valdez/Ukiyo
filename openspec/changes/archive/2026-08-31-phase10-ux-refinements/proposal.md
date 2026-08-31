## Why

Product detail and Identity pages have accumulated three UX issues that hurt perceived quality: the quantity input layout is misaligned, product descriptions rendered from Quill produce inconsistent number typography, and all password fields lack show/hide toggles. These are small, independent CSS/JS fixes that should ship together before moving to the Phase 11 backlog.

## What Changes

- **Product detail quantity control:** Replace the "Cantidad" label and native number spinners with a minus/plus stepper (centered value, native spinners hidden, `min="1"` preserved plus a JS clamp that disables the minus button at 1). Stepper, Add-to-Cart, and favorite buttons share one equal-height row (2.75rem) on desktop, wrapping below the stepper on narrow viewports. All stepper controls expose localized `aria-label`s.
- **Site + description typography:** Remove Georgia at the root — the Litera theme's global `p { font-family: Georgia, ... }` rule in the vendored `bootstrap.css` becomes the theme's sans-serif stack, fixing uneven old-style figures site-wide. `.product-detail__description` adds `font-variant-numeric: lining-nums tabular-nums` for uniform digit height and width and keeps word-wrap for long unbreakable strings.
- **Password visibility toggles:** Add a show/hide button (eye icon) to every `type="password"` input across Login, Register, ResetPassword, ChangePassword, and SetPassword pages. Pure vanilla JS, accessible (`aria-label`, `aria-pressed`), works in light/dark mode.

## Capabilities

### New Capabilities

- `ux/product-detail-quantity`: Quantity input alignment and browser-side min-value constraint on the product detail page.
- `ux/password-visibility-toggle`: Accessible show/hide password toggle button on all Identity password fields.
- `ux/rich-text-typography`: CSS normalization for Quill-rendered product descriptions to ensure consistent number and text typography.

### Modified Capabilities

_(none — these are additive CSS/JS changes with no spec-level behavior changes to existing capabilities)_

## Impact

- **Files:** `site.css`, `Details.cshtml` (+ inline stepper JS), vendored `bootstrap.css` (Georgia removal), `_PasswordToggle.cshtml` partial + `passwordToggle.js`, 5 Identity `.cshtml` files (Login, Register, ResetPassword, ChangePassword, SetPassword), localization `.resx` files for toggle and stepper labels (`_PasswordToggle.{en,es}.resx`, `Details.{en,es}.resx`).
- **Dependencies:** No new packages. Uses existing Bootstrap utilities, Bootstrap Icons, and vanilla JS.
- **Risk:** Low — CSS-only and client-side JS changes. No backend, model, or migration changes.
