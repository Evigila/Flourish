# Compact action alignment and Appearance integration link

## Symptoms and cause

The user observed centered Apply colors and Restore default actions on the Gallery Colors and themes page. The page used a raw `f-inline-actions` container, whose Framework CSS default was `justify-content:center`. The production InlineActions component independently defaulted its Alignment parameter to Center. DisplayBoard also forced raw action rows back to center, so a page-only alignment change would leave conflicting defaults in executable examples.

Configure Design used a plain anchor for See framework integration rather than the existing Button link contract.

## Correction

The user defined the preferred behavior: content that leaves free row space should align to the right; a single full-row element or a group filling the row should retain its full width. InlineActions now defaults to HorizontalAlignment.End, and raw action rows use the same Framework default. Explicit Start and Center remain supported. The flex wrapping, growing input rule and full-width FormActions/rectangle grid tracks remain intact; justification uses only remaining space.

DisplayBoard provides its direct action rows with the available width and no longer overrides their justification. Its centered preview behavior remains available for other content. The Appearance form now composes the production InlineActions with its existing Primary submit Button and Secondary reset Button. Their submit type, handlers and palette behavior are unchanged. Configure Design composes InlineActions with a Secondary Button using `Href="/framework"`, retaining native navigation semantics and the shared control appearance.

Framework usage metadata, Gallery catalog/API descriptions and their three-language text now identify End as the default and explain full-row sizing. The active API guide records the default change; historical Center-default records remain untouched. The 1.1.4-preview ChangeLog includes the correction.

## Verification and limits

- Blazor and Gallery Release builds: zero warnings and zero errors.
- Blazor executable checks: 429/429, including omitted and explicit alignments, native Dialog action forms and catalog default values.
- Gallery executable checks: 7,808, including actual Appearance rows, translated action labels, submit/reset button types, the standard framework link and the three-choice theme group. Includes 171 post-event checks and 203 ChangeLog checks.
- Focused page-body action Node suite: 5/5. Preserves page gutters, growing inputs, full-row action sizing and existing grid sizing contracts.
- Culture catalogs: 22,049 checks; Gallery has 1,545 complete keys and Framework has 286.
- ChangeLog validation: 73 checks. CSS bundle validation: 21 checks. `git diff --check` passed.

No Computer Use or browser visual test was performed. These checks establish composition, contracts and CSS rules; actual pixel placement, focus and responsive appearance remain manual acceptance items. No dependency, package version, release tag or publication changed.

## Manual regression checklist

1. Open Colors and themes on a wide viewport. Apply colors and Restore default should sit together at the content row's right edge.
2. Enter valid palette values and apply them. Then restore defaults. Enter invalid values and confirm the existing validation still prevents submission.
3. In Configure Design, check See framework integration has the standard Outlined Button appearance, aligns right, exposes keyboard focus and navigates to the existing framework page.
4. Narrow the viewport and use English, Chinese and Portuguese. Wrapped action lines should stay right-aligned without horizontal overflow.
5. Confirm the three theme choices still fill their row. In the FormActions and UniformGrid examples, confirm full-row groups retain their width; the InlineActions examples should retain explicit Start and Center while their omitted default uses End.
