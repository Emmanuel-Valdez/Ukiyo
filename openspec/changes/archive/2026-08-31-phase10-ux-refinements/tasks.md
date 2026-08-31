## 1. Product Detail Quantity

- [x] 1.1 Replace the labeled quantity row with a `product-detail__stepper` group: minus button, centered number input with native spinners hidden, plus button; no visible label; localized `aria-label` on group + input
- [x] 1.2 Layout: stepper, Add-to-Cart, and favorite buttons share one equal-height row (`min-height: 2.75rem`, flex stretch) on desktop; actions wrap below on narrow viewports while the stepper keeps internal alignment and centered value
- [x] 1.3 Keep `min="1"`/`step="1"`; add inline JavaScript that clamps stepper clicks to the minimum, disables the minus button at the minimum, and dispatches an `input` event for client-side validation
- [x] 1.4 Add localized `IncreaseQuantity`/`DecreaseQuantity` labels (en/es) to `Details.{en,es}.resx` for the stepper buttons
- [x] 1.5 Style stepper, step buttons, and hover/active/disabled/focus-visible states in light and dark mode (`[data-bs-theme=dark]` overrides)

## 2. Site + Description Typography

- [x] 2.1 Remove Georgia at the root: replace the global `p { font-family: Georgia, Cambria, ... }` rule in the vendored `bootstrap.css` with the theme's sans-serif stack (fixes numbers site-wide, not just descriptions); drop the Georgia fallback from `.storefront-display-heading`
- [x] 2.2 `.product-detail__description`: `font-variant-numeric: lining-nums tabular-nums` for uniform digit height/width; keep word-wrap/overflow handling for long unbreakable measurement strings
- [x] 2.3 Verify descriptions and site text render correctly in light and dark mode

> Deviation note: the first implementation patched typography with family/size/line-height overrides inside `.product-detail__description`. The user's "letra queda arreglada pero las cifras" follow-up traced the real cause to the Litera theme's Georgia on the global `p` rule, so those redundant overrides were removed and the fix moved to the root (vendored `bootstrap.css`) + `font-variant-numeric`. The vendored stylesheet has no pipeline (no libman.json/package.json), so the edit is durable but must be reapplied if the vendor file is ever re-downloaded.

## 3. Password Visibility Toggle — Partial + JS

- [x] 3.1 Create a shared `_PasswordToggle.cshtml` partial (inline SVG eye/eye-slash button, `aria-label`, `aria-pressed`) in `Views/Shared/` and verify it renders a visible toggle button
- [x] 3.2 Create `wwwroot/js/passwordToggle.js` with vanilla JS that initializes all `.password-toggle` containers, toggles `type` between `password`/`text`, and swaps icon/aria-label on click
- [x] 3.3 Add `passwordToggle.js` to `_Layout.cshtml` scripts (or `_ValidationScriptsPartial`) and verify it loads on all Identity pages

## 4. Password Toggle — Identity Pages

- [x] 4.1 Add toggle to `Login.cshtml` (`Input.Password`) and verify show/hide works in light and dark mode
- [x] 4.2 Add toggle to `Register.cshtml` (`Input.Password`, `Input.ConfirmPassword`) and verify both fields have working toggles
- [x] 4.3 Add toggle to `ResetPassword.cshtml` (`Input.Password`, `Input.ConfirmPassword`) and verify both fields have working toggles
- [x] 4.4 Add toggle to `ChangePassword.cshtml` (`Input.OldPassword`, `Input.NewPassword`, `Input.ConfirmPassword`) and verify all three fields have working toggles
- [x] 4.5 Add toggle to `SetPassword.cshtml` (`Input.NewPassword`, `Input.ConfirmPassword`) and verify both fields have working toggles

## 5. Localization

- [x] 5.1 Add en-US aria-label strings ("Show password", "Hide password") and verify English labels appear
- [x] 5.2 Add es-AR aria-label strings ("Mostrar contraseña", "Ocultar contraseña") and verify Spanish labels appear

> Deviation note: labels live in the partial's own resource files `Resources/Views/Shared/_PasswordToggle.{en,es}.resx` (following the existing `_ThemeToggle`/`_Pager` partial convention) instead of the page-level Identity resx. `IViewLocalizer` resolves each partial's resources by its view path, so the labels still localize per culture.

## 6. Verification

- [x] 6.1 Run `dotnet test VaultShop.sln` and verify all tests pass
- [x] 6.2 Verify product detail page: stepper layout/behavior, min=1 enforcement, description typography in light/dark mode
- [x] 6.3 Verify all five Identity password pages: toggle works, accessible via keyboard, localized in en/es, light/dark mode

> Verification notes: `dotnet test VaultShop.sln` -> 149 passed, 0 warnings (Razor views compiled).
> Toggle behavior verified with a Playwright harness against the real `passwordToggle.js`, `site.css`,
> `bootstrap.css`, and the exact `_PasswordToggle.cshtml` markup: 20/20 checks pass (type toggle,
> aria-pressed/label swap, Enter/Space keyboard, 40px input padding, icon contrast in light + dark,
> focus-visible outline, es-AR labels, no JS errors).
> Quantity stepper verified with a second Playwright harness using the real `bootstrap.css`, `site.css`,
> and the exact stepper markup + inline JS: 13/13 checks pass (stepper == Add-to-Cart == favorite button at
> 44px, value centered, native spinners hidden, minus disabled at 1, no value below 1, `input` event fires,
> dark-mode stepper/step colors, no JS errors).
> Recommended final smoke check once the Docker stack runs: visual pass on a real product detail page and
> the five Identity pages in both languages and both themes.
