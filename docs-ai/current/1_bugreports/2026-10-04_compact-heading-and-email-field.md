# Compact title and Field validation presentation

## Symptoms

Page titles stayed at 50px after the heading area collapsed. Split/navigation/menu triangles used different shapes and state presentations. CheckBox squares were shorter than nearby input fields. Field's scenario always displayed an error, even before interaction.

## Causes

Compact heading selectors explicitly reused the expanded page-title size, including responsive and surface-specific overrides. Triangles were independently drawn with icon fonts, CSS borders and local clip paths. CheckBox and list/form variants retained separate 24px/28px squares. The Field scenario supplied a fixed Error string rather than deriving it from an EditContext.

## Mitigation

Reserve 38px only for actual compact heading selectors and retain expanded 50px. Reuse the native disclosure-open glyph through ExpansionIndicator and native SelectBox picker styling, with matching rotation and host-owned semantics. Size visible checkboxes from the standard 48px input-height token, without enlarging invisible accessibility proxies or multiline fields. Replace the guide's fixed error with Required/EmailAddress model validation, clear stale success on edits and preserve the consumer Error API.

SelectBox's popup now keeps its physical right edge aligned in customizable-select engines; a vertical-only fallback can move it above the trigger without swapping horizontal alignment.

## Evidence and regression checks

Gallery/test builds passed without warnings; all 98 .NET checks and focused Node audits passed. Isolated headless Edge verified title 50/38/50 transitions at desktop and narrow widths, wider-picker right alignment and typed value updates, common triangle rotation and close states, 48px visible checkbox squares, reduced motion and the complete email error/success/edit sequence. All 78 guide HTTP routes retained their five standard sections and default column.

## Limits

Optional Design owns the visual size and animation; Framework-only consumers retain native typography and presentation. Browser engines without appearance:base-select retain their native select popup positioning. Cross-browser and 200% zoom checks remain in the manual checklist. This report supersedes earlier fixed-50px compact-heading and small-checkbox choices for Flourish only; existing history and common standards remain intact.