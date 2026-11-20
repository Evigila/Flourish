# Add notice action slots

Recorded 2026-11-16 in America/Sao_Paulo (UTC-03:00).

## Changes

- Notice and FloatingNotice expose optional Actions RenderFragments. Framework owns a flexible message column and separate action column on the same row; existing callers without Actions retain their stacked markup.
- FloatingNotice forwards ChildContent explicitly so a title-only caller does not acquire an empty body fragment. Actions retain standard controls, callbacks, links and live-region semantics.
- Existing floating-notice natural-height measurement handles the flex row and aligns it with SaveChangesBar on wide screens. Narrow screens stack the complete surfaces; action focus and hover retain existing timeout suspension. Navigation links remain subject to unsaved-change guards.
- Gallery demonstrates a warning with actions alongside a fictional dirty-draft bar, with three-language captions and reflected API metadata.

## Verification and delivery

- Blazor rendering/API/event checks: 542/542 passed.
- Floating-notice/dock and save-bar script checks: 46/46 passed, including action navigation protection.
- Framework/tests and Gallery builds passed with zero warnings/errors.
- No Computer Use or manual browser acceptance was performed.
- Work was prepared in a source copy because the original Flourish checkout was read-only. A checked source patch accompanies Colligere's local preview.15 candidate; no source version-prefix change, public package publication, tag, commit or deployment occurred.
