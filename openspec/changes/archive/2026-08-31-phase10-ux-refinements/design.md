## Context

The product detail page (`Details.cshtml`) uses `row g-3 align-items-end` with a "Cantidad" label and a native number spinner for the quantity/button row — the label was dropped and the native spinners replaced at the user's direction with a minus/plus stepper (same height as the Add-to-Cart row, centered value). Product descriptions are rendered via `@Html.Raw()` from Quill editor output and, alongside all site paragraphs, inherited the Bootstrap Litera theme's Georgia fallback — Georgia's old-style figures make digit heights uneven. All five Identity password pages use plain `type="password"` inputs with no visibility toggle.

## Goals / Non-Goals

**Goals:**
- Replace the quantity label + native spinner with an aligned minus/plus stepper (centered, equal height with the CTA row, no value below 1)
- Fix number typography at the root (remove Georgia site-wide) and make description digits uniform
- Normalize Quill-rendered description typography so numbers and text are consistent
- Add accessible, localized show/hide password toggles to all Identity password fields

**Non-Goals:**
- No changes to the Quill editor toolbar or configuration (admin-side)
- No backend validation changes (existing server-side validation remains)
- No new JS frameworks or dependencies
- No changes to the confirmation email or order summary views

## Decisions

### 1. Quantity control: minus/plus stepper instead of label + native spinner

**Choice:** Replace the "Cantidad" label and the native number spinners with a semantic stepper group: a minus button, a borderless centered number input (spinners hidden via `appearance`), and a plus button. The stepper and the action row sit in a flex layout (`product-detail__purchase`) with `align-items: stretch` and `min-height: 2.75rem`, so stepper, Add-to-Cart, and favorite buttons are the same height and the value stays centered.

**Alternatives considered:**
- Keep label + native spinners with `align-items-end` fix (the earlier CSS-Grid patch): the user rejected it on sight — the label eats vertical space and the native arrows look inconsistent across browsers.
- Server-side plus/minus POST (cart pattern): adds a round-trip per tap for a purely local UI concern; rejected.

**Rationale:** Markup stays one label-less group; no native-spinner inconsistency; no server round-trips; the flex stretch gives the equal-height/measured 44px result on mobile and desktop.

### 2. Quantity min-value: `min="1"` + JS clamp

**Choice:** Keep `min="1"`/`step="1"` on the `<input type="number">` for browser-submit validation, add a small inline script that clamps stepper clicks to the minimum, disables the minus button at the minimum, and dispatches an `input` event so `asp-validation-for` reacts.

**Alternatives considered:**
- JS validation only: loses browser-native submit rejection.
- Server-side only: confusing error after submit.

**Rationale:** Native `min` is the safety net; the click handler only prevents the stepper from producing an invalid value and gives visual feedback (minus disabled).

### 3. Typography: root-cause fix (remove Georgia) + numeric figures on description

**Choice:** Two-layer fix. First, remove Georgia at the source: the vendored Bootstrap Litera `bootstrap.css` styles every global `p` with `Georgia, Cambria, "Times New Roman", Times, serif` — replace with the theme's sans-serif stack (the same one Litera uses on `.lead`). This fixes every number on the site (cart, checkout, admin), not just product descriptions. The `.storefront-display-heading` Playfair stack keeps the display font but drops the Georgia fallback. Second, `.product-detail__description` gets `font-variant-numeric: lining-nums tabular-nums` so digits render with uniform height and width, and keeps the word-wrap/overflow handling for long unbreakable measurement strings.

**Alternatives considered:**
- CSS-only normalization inside `.product-detail__description` (first attempt): fixed the description but left Georgia's uneven figures everywhere else and required redundant font-family/font-size/line-height overrides.
- Sanitize HTML on save / strip tags on display: complex or formatting-destroying.
- Fix Quill config: snow theme doesn't expose font-size controls easily.

**Rationale:** The Georgia rule is a theme quirk, not an intentional design choice — one line in the vendored stylesheet fixes the root cause site-wide, and `font-variant-numeric` handles digit uniformity. The stylesheet is a static vendored copy (no libman.json/package.json pipeline exists), so the edit is durable, though it would be lost if the vendor file is ever re-downloaded.

### 4. Password toggle: vanilla JS with inline SVG icons

**Choice:** Small JS function that finds all `.password-toggle` containers, injects a button with eye/eye-slash SVG, and toggles `type` on click. Uses `aria-label` and `aria-pressed` for accessibility. Injected via a shared partial or inline script.

**Alternatives considered:**
- Bootstrap Icons font: already loaded, but SVG inline avoids FOUC.
- jQuery: project uses jQuery already, but vanilla JS is simpler for this.
- A NuGet package: overkill for ~30 lines of JS.

**Rationale:** Vanilla JS + inline SVG is self-contained, works in both themes, no extra requests.

### 5. Toggle placement: inside `form-floating` wrapper

**Choice:** Position the toggle button absolutely inside the `form-floating` div, right-aligned with padding. CSS handles both light/dark mode via `[data-bs-theme=dark]` selector.

**Alternatives considered:**
- Appended after the input: breaks `form-floating` label positioning.
- Separate row below input: adds visual noise.

**Rationale:** Inside `form-floating` keeps the toggle close to the input without breaking the existing layout.

## Risks / Trade-offs

- **[Risk] Vendored bootstrap.css edit lost on re-download** → The Georgia removal lives in a static vendored file with no libman.json/package.json pipeline. **Mitigation:** documented in this change; the same one-line fix would need reapplying if the vendor file is refreshed.
- **[Risk] Number input styling quirks across browsers** → Hiding native spinners requires both `appearance: textfield` (Firefox) and webkit pseudo-element overrides (Chrome/Edge). **Mitigation:** the Playwright harness runs against real `bootstrap.css`/`site.css` and asserts hidden spinners behavior; both browser families covered by webkit + standard prefixes.
- **[Risk] Password toggle breaks form-floating label** → Absolute positioning must not overlap the label. **Mitigation:** Test with all five pages; use `right` + `padding-right` to keep button clear of text.
- **[Risk] Toggle JS runs before DOM ready** → Script must execute after DOMContentLoaded or be placed at bottom of page. **Mitigation:** Use `DOMContentLoaded` listener or place script in `Scripts` section.
- **[Trade-off] No server-side min validation change** → Client-side `min` can be bypassed. **Mitigation:** Existing server-side validation already handles invalid quantities; this is UX improvement, not security.
