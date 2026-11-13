# PageBody centered containers

## Symptoms and cause

The user expected PageBody's compact/default demonstration to change horizontal centered-container gutters. Its CompactSpacing parameter instead reduced vertical page-heading, section and form spacing. The existing horizontal scaling belonged to NavigationSurface.CenteredContentGutterScale, which applied to every descendant centered page and was absent from PageBody's public API.

## Correction

PageBody now owns CenteredContainer, with Standard and Expanded values and Standard as the default. Expanded halves each standard side gutter, including the minimum gutter, using the existing configured reference width and library geometry. It applies only when effective Fluid is false and FullWidth is false. FullWidth precedence, inherited Fluid and FillHeight remain intact. Every PageBody resets the internal scale so nested and sibling pages choose independently. Neither mode changes vertical spacing.

Removed the Blazor CompactSpacing API, its CSS rules, sample checkbox/state, caller usage and API/localization descriptions. Removed NavigationSurface.CenteredContentGutterScale and its sample control without aliases; that surface continues forwarding the configured content width. PageBody's Gallery preview demonstrates standard, expanded, fluid and full-width content. Its actual selector provides the same four modes. DisplayBoard gives direct PageBody previews its available width so centered gutters are visible rather than shrink-wrapping to the label.

Updated Framework usage guidance, Gallery API/catalog descriptions and the 1.1.4-preview ChangeLog in English, Chinese and Portuguese. Active technical guides explain the new ownership; historical release descriptions remain evidence of previously published APIs. Native WPF's independent PageBody still lacks responsive centered geometry and retains its older vertical API; native parity is outside this Blazor/Gallery correction.

## Verification and limits

- Blazor and Gallery Release builds: zero warnings and zero errors.
- Blazor executable suite: 433/433, including defaults, expanded rendering, effective width precedence, invalid enum values, nested reset and horizontal-only CSS behavior.
- Gallery executable suite: 8,178 checks, including four actual control previews and real SelectBox/Button events across language refreshes. Includes 453 post-event checks and 219 ChangeLog checks.
- Page-body action Node suite: 5/5; Culture catalogs: 22,060 checks; ChangeLog validation: 85 checks.
- Generated Framework and Design CSS contain the centered geometry and no retired compact-spacing rules. Diff whitespace checks passed.

No Computer Use or browser visual test was performed. Pixel placement and responsive appearance require the manual checks below. No dependency, package version, tag or publication changed.

## Manual regression checklist

1. Open the PageBody guide. Confirm Control example shows Standard and Expanded centered containers separately, alongside fluid/full-width previews, and that the compact form-spacing control is gone.
2. On a wide viewport, switch Scenario example between Standard and Expanded. Expanded should reduce each side's empty space by half, align heading and content consistently, and leave vertical section spacing unchanged.
3. Narrow the viewport. Expanded should also halve minimum side gutters; content and right-aligned actions must remain bounded without horizontal overflow.
4. Switch to fluid and full width, then back to a centered mode. The independent full-height EditingGrid should retain its stage size. Increment the counter, switch modes/languages and verify the count and selected mode persist; Reset should reset only the counter.
5. In a consumer, place a Standard PageBody inside an Expanded PageBody and verify the child uses its own Standard gutters. Check light/dark themes and English/Chinese/Portuguese labels.
