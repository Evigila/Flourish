# Localize collapsible Gallery page headings

The user clarified that the untranslated large title was PageHeading above the right-hand content. Its scroll-collapse behavior was working; affected callers still supplied literal Chinese labels or Chinese topic/category dictionaries even though navigation labels already used generated Culture keys.

Updated primary page headings and browser titles across Framework, Controls, Foundations and scenario pages. Topic/category maps now contain keys resolved during render; affected pages inherit LocalizedComponentBase. Existing Appearance/Form/Reconnect lifecycle logic also releases localization subscriptions. API names, brands and actual record names retain their identity. Added nine complete three-language labels, bringing Gallery to 165 keys; Framework remains at 121.

Added a focused Gallery regression project to both root and Blazor solutions using existing dependencies, and expanded HTTP checks to inspect actual PageHeading H1 values on 31 routes in all three languages. Updated the active Culture guides and architecture inventory. See [the bug report](../bugfix-reports/2026-10-06_gallery-page-heading-localization.md) for cause, evidence and scope.

Verification: Release Gallery and full Blazor solution builds passed with zero warnings/errors; 42 actual scoped language-change/disposal checks, 433 real HTTP checks, 3,146 catalog-integrity checks and 12 existing bridge checks passed. No Computer Use testing, new external dependency, Colligere change, commit or publication occurred. The temporary verification host was stopped; the user's active instance was preserved.

Manual acceptance: restart Gallery; switch en-US/zh-CN/pt-BR on Framework, Controls, Foundations and Examples; scroll to exercise collapse/expand and long titles; check that component API names and record names remain unchanged. Remaining explanatory content and language-preference persistence are outside this heading repair.
