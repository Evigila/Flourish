# Header alignment and presentation spacing

Colligere reported low header actions and overlapping Hero copy. Both originated in Flourish CSS composition rather than an unstandardized host button. The correction stays in the library and preserves the existing controls.

## Causes and evidence

Design's general f-inline-actions added margin-top:16px to a centered header group. Centered parent flex alignment did not remove that margin, so the actual button centers shifted downward. Framework now resets margin only on the direct header action group; ordinary page action groups remain unchanged.

Foundation's f-root p margin-block-start:0 had greater specificity than the old Hero subtitle margin. The intended separation disappeared. Hero now uses a grid gap of 40px with zero paragraph margins and safe artistic line height, followed by additional action spacing. It delegates height/background/container composition to PresentationBand.

The initial dot implementation derived an alpha color from primary-ink; existing palette tests rejected it. Primary now uses the existing primary-preview role directly. Review found that later Tone background shorthands would also erase the previously imported dotted background image on Canvas/Surface. All three tones now set background-color. Wide Hero descendant selectors could alter nested content containers and headings; direct-child selectors remove that leakage.

## Regression and limitations

PresentationChecks covers parameter defaults and invalid values, single Band/container composition, native attributes and ARIA, encoded long copy, natural height without clipping, gap declarations, direct-child selectors and background-image preservation. TextChecks covers configured fallback, literal and translated names, per-scope refresh, explicit compatibility overrides and unsubscription. Palette checks still prohibit private color derivation.

The final complete Release preparation passes Blazor 234, bridge 12, Core 367, all ten JavaScript files, CSS 21/194, six packages and 95 clean-consumer checks. Seven Gallery routes plus dots/identity contracts pass HTTP. These establish source, rendering and package behavior, not measured browser geometry; manual theme/viewport/zoom acceptance remains required.

No host skin, business rule, authentication protocol or dependency change was introduced. Default 450px is a minimum, not a fixed clipping box. Dots remain disabled by default on production banners and enabled by default on the independent preview DisplayBoard.
