# Audit remaining Gallery H2 and body translation coverage

The user reported internal H2 headings and body prose remaining untranslated. Source inspection confirmed direct literal Section titles/paragraphs in Framework, Examples and Appearance, plus Chinese descriptions stored in ComponentCatalog, ParameterMeaning, ComponentUsageCatalog and sample metadata. ComponentGuide also supplies explicit Chinese API captions/TableText.

Section outputs caller-supplied strings/RenderFragments. LocalizedComponentBase only rerenders; neither it nor the optional Culture bridge translates arbitrary literals. Overview and Localization already demonstrate correct key-based heading/body rendering. Three-language completeness for existing keys does not establish complete adoption by callers.

Recorded [the open coverage audit](../bugfix-reports/2026-10-06_gallery-body-localization-coverage.md), including metadata lifetime, catalog ownership, API/data identity boundaries and subsequent verification needs. This was analysis and documentation only: no product code, style, font, build configuration, generated state or dependency changes; no build, Computer Use, commit or publication.
