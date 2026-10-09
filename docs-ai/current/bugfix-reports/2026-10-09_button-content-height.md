# Button content height and vertical padding

## Symptom and cause

The Portuguese Conta de exemplo Button in the Gallery control preview contains a primary name and an email description, but its vertical spacing was visibly compressed. The same structure appears in the scenario preview. The user clarified that standard single-line height must not sacrifice multiline spacing.

Default Design .f-button already had min-height:48px without a fixed height. Its padding:0 13px was the cause: stacked label line boxes plus the 4px label gap nearly occupied the minimum height, leaving little free vertical space. Wider content and natural wrapping could grow the button, but still had no reserved block padding. The sample already bounds width at 24rem with max-width:100%, so changing its translation or width would not correct this shared defect.

## Correction and scope

Shared Design .f-button now uses padding:10px 13px and retains min-height:48px without height/max-height restrictions. The content sets the natural height, and 10px padding remains above and below it. Single-line text at 17px/1.2 fits the minimum. The default 26px inline icon plus 20px padding and 2px borders totals 48px, preserving the existing short icon/text size.

This uses the current Button contract and renderer; no multiline variant, sizing parameter, JavaScript measurement or Gallery skin is introduced. Structured label hierarchy, wrapping, bounded width, logical alignment and account/draft actions remain unchanged. Later icon-only, menu-item, UGB/FormActions, compact navigation/back-to-top and table-header rules keep their specialized geometry. SplitButton primary/secondary controls have separate classes and are not changed by this ordinary Button rule. Native WPF is outside this Blazor Gallery correction.

Framework usage metadata and Gallery/API guidance describe minimum rather than uniform height in all three languages. The 1.1.4-preview ChangeLog records content-driven button height.

## Verification and limits

- Framework and Gallery Release builds: zero warnings and zero errors.
- Framework: 436/436 executable checks.
- Gallery: 8,283 checks, including 474 post-event and 231 ChangeLog checks.
- Production stylesheet cascade suite: 16/16. The added regression covers all six ordinary variants, native buttons and links, structured/simple labels and available/unavailable/busy attributes. It checks positive vertical padding, natural height, standard short-label/icon fit and icon/menu spacing boundaries.
- Culture catalogs: 22,331 checks; Gallery 1,553 complete keys, Framework 286, Culture extension 6.
- ChangeLog: 94 checks across four versions. Generated Release Design CSS contains the corrected padding. git diff --check passes.

The cascade fixture does not perform browser layout. No Computer Use testing was performed. Final glyph metrics, text-box-trim support, responsive wrapping and pixel placement require the manual checks below. No dependency/package/version/tag/publication or Git commit changed.

## Manual checks

1. In Portuguese, inspect Conta de exemplo in both Button previews. Name and email should retain visible space above/below the content, and the button may be taller than neighboring single-line examples.
2. Narrow the page and switch between Portuguese, English and Chinese. Wrapped content should increase height without clipping or losing padding; the 24rem/max-width bounds should still prevent overflow.
3. Compare single-line text, icon/text, busy and icon-only buttons. Short ordinary examples should retain the standard minimum size, while icon-only actions remain square. Check focus rings and account selection/draft saving still behave correctly.
