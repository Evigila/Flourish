# Primary-preview text truncation

Date: 2026-10-29, America/Sao_Paulo (UTC-03:00). Status: source and automated verification complete; visual acceptance remains manual.

## Cause and correction

The expanded preview already had `white-space:nowrap`, `overflow:hidden` and `text-overflow:ellipsis` on labels/reasons. Its grid row used `place-items:center start`, so the text container kept start alignment and an intrinsic width. Long localized captions could therefore overflow the bounded rail and be clipped without reaching their own ellipsis boundary.

The expanded `.f-primary-preview-copy` now uses `justify-self:stretch`, filling its existing `minmax(0,1fr)` text track. This activates the existing truncation rules without changing rail width, icons, DOM, accessible labels, weak hover behavior, animation, RTL or tooltips. Ordinary icon tooltips remain suppressed while expanded; full ARIA names and disabled reasons remain available. No host workaround or new public API is needed.

## Verification and delivery

- Twelve-project Release build: zero warnings/errors.
- Framework: 487/487; Gallery: 8653/8653; CSS bundler: 21/21.
- Focused preview Node regression: 42/42, including both shell hosts, long Portuguese captions, full accessible names, RTL, existing width/animation/reduced-motion and cleanup checks.
- Six candidate .14 archives and thirteen delivered assets match their source/Release bytes. The preceding .13 archives remain unchanged. See [dependency hashes](../1_dependency.md).

The preview is `1.4.4-preview.shell.20261027.14` in the independent `artifacts/admin-account-and-navigation-20261029/nuget` feed. No external dependency, maintained file, JavaScript or font changed. Existing .13 working-tree changes are retained. No Computer Use, browser measurement, public publication, commit or push was performed.

## Manual acceptance

Choose long Portuguese primary-navigation labels in NavigationSurface and ApplicationShell. Expand with the pointer or keyboard and verify a single-line ellipsis, stable icons, full accessible names and unchanged animation/secondary visibility. Repeat at narrower widths and in RTL; confirm disabled explanations and Escape still work.
