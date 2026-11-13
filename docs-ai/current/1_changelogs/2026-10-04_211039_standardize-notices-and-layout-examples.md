# Standardize notices and layout examples

Recorded using project local time: America/Sao_Paulo.

## Changes

- Unify Notice and Primitives.StatusNotice severity colors in Design. Information uses Surface / Info text; Warning uses Warning background / Warning ink; Success, Error and Subtle use the existing Notice mappings. Remove conflicting legacy assignments and retain announcement semantics.
- Standardize four shell/layout examples using Field, TextBox, Button and PrimaryNavigationItem. Preserve native form validation, immediate draft dirty state, interaction locking and local navigation. Retain actual semantic contents-link markup.
- Replace Foundations' theme controls with three rectangular UniformGridButton cells in one row. Palette apply/reset use ordinary Filled/Outlined Button, preserving the appearance service callbacks and validation.
- Add the first Icons section, “使用Google Material Design Symbols”, with standard Underline links to the official repository, Apache 2.0 license, icon browser and usage documentation. Preserve local font loading and catalog behavior.
- Update current architecture descriptions, extraction notes, color mappings and manual acceptance. Add a semantic CSS regression and a bug report. No new dependency or public API.

## Verification

Eight-project Blazor solution builds with zero warnings/errors. Blazor: 105/105; palette: 10/10; controls DOM: 38/38. Catalog retains 77 guides, 546 rows and 253 defaults. All guide routes and Framework-only HTTP assets pass. Headless installed Edge verifies both-theme notice color parity and ARIA roles; theme and palette callbacks; 320px containment; Icons links/search; form required/email validation; boundary lock/unlock/save; local navigation; and draft protection cancellation/saved navigation. No browser exceptions. Git whitespace check passes.

## Scope

Changes are limited to Flourish. Preserve preceding uncommitted work and unrelated user deletions. No Computer Use, external dependency, VS server restart or Git commit. Manual checks for other browser engines, screen readers, zoom and native refresh remain in blazor-manual-tests.md.
