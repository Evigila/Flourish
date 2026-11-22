# Plain button text can appear off-center after the body size reduction

Recorded 2026-11-21 in America/Sao_Paulo (UTC-03:00).

## Investigation

The preview.9 Button used two layout paths for the same action. The Text parameter wrapped copy in a span trimmed between the Latin cap and alphabetic metrics. ChildContent used its normal line box. Separately, a native button centered wrapped lines through the browser default and the anchor used the inherited start alignment.

A local Edge fixture used vendored Noto Sans and Noto Sans SC, including Latin, Portuguese accents, Chinese and mixed-script labels at 14px and 17px. The 85px-wide 14px Export XML action had wrapped anchor line centers 5.82px and 13.49px toward the start. The native button line centers deviated by 0.01px. The Text-backed wrapped button height was 48.80px and ChildContent was 55.59px because only the Text path trimmed its lines. At 17px the corresponding anchor offsets were 0.96px and 10.28px, with heights of 54.53px and 62.78px.

Single-line wrapper centers were already within 0.01px in most measured cases. At 14px the sampled capital, title-case, Chinese and mixed-script ink centers were within 0.5px. The descender-heavy gyp label still painted about 3px below the geometric center on either plain Button path. Comparable offsets in native inputs, selects and table text show the difference between centered line boxes and glyph-specific ink. Table action and trigger boxes were centered within their cell in the fixture. A uniform upward shift improved the descender-heavy sample while worsening stable Chinese and uppercase samples. [CSS Inline Layout font metrics](https://drafts.csswg.org/css-inline-3/#text-box-trim)

## Resolution

Plain Text uses a natural line box and wrapped Button links share the native Button center default. After the change, the 14px Export XML paths reserve the same 55.59px height and both line centers are within 0.01px. At 17px they share 62.78px height. The default is low-specificity, so intentional alignments win. Menu items and sortable headings remain start-aligned. Header branding remains start-aligned as it truncates. Structured labels retain their established typography. See the [change record](../1_changelogs/2026-11-21_105001_center-plain-button-labels.md).
