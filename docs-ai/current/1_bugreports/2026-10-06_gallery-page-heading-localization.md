# Collapsible Gallery page headings remained untranslated

## Symptoms and scope

The user reported that the large title above the right-hand content, which collapses/expands with scroll, did not follow the selected language. The user explicitly identified PageHeading rather than the artistic organization heading in the access-method scene.

## Cause and evidence

The Gallery startup navigation used generated translation keys, but Framework, Examples and Appearance maintained separate literal Chinese topic dictionaries. Controls read another Chinese dictionary in CatalogSections. Several scenario pages supplied literal PageHeading.Title values and inherited ComponentBase rather than LocalizedComponentBase.

The existing bridge correctly localized Framework-owned captions. PageHeading.Title is caller-owned display text, so the bridge neither translates arbitrary strings nor connects a page heading to its navigation entry. Having three translations under every existing Culture.json key did not establish that every page used those keys. Earlier HTTP checks accepted translated navigation/captions and therefore missed literal H1 values.

## Mitigation

- Store generated keys in topic/category maps and parse them during rendering.
- Subscribe affected pages to the scoped language through LocalizedComponentBase.
- Preserve existing appearance/form/reconnection cleanup and invoke the localization base where lifecycle methods are overridden.
- Reuse existing navigation keys; add nine complete three-language keys only for missing page titles and the category back label.
- Preserve API names, brands and actual record names rather than translating identities/data.
- Add Tests.Gallery.Flourish.Blazor to both the root and Blazor solutions. It uses existing Gallery dependencies and the real Essential service.
- Expand HTTP assertions to inspect PageHeading's semantic H1, not merely any translated text in the page response.

## Verification

- Gallery and complete Blazor solution Release builds with TreatWarningsAsErrors: zero warnings/errors.
- Actual page language-change and disposal regression: 42 checks passed.
- Actual Gallery HTTP checks: 433 passed, including 31 primary-heading routes in en-US, zh-CN and pt-BR.
- Catalog integrity: 3,146 checks passed for 165 Gallery and 121 Framework keys.
- Existing Culture bridge regression: 12 passed.
- No Computer Use testing, dependency addition, business-rule change, Colligere write or Git commit.

Evidence: artifacts/gallery-heading-build.log, gallery-heading-solution.log, gallery-heading-refresh.log, gallery-heading-http.log and gallery-heading-bridge.log.

## Limitations and manual regression

This repair covers primary page headings and their page titles, not every remaining literal paragraph, section heading or example field caption. Existing language-picker preference persistence and full native navigation behavior are outside this repair. An already-running Gallery must load the new build before manual acceptance.

Restart Gallery, switch each of the three languages on Framework, Controls, Foundations and Examples, and verify the title changes without leaving the page. Scroll down and back up to confirm collapse/expand behavior and long Portuguese titles remain readable. Open a component detail and a real record detail to confirm API/data names remain unchanged; navigate repeatedly between pages and switch languages again.
