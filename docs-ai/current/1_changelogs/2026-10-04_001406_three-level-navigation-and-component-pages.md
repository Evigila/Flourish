# Three-level navigation and independent component pages

Recorded at 2026-10-04T03:14:06.3639852Z (project local time: 2026-10-04 00:14:06).

## Request and boundaries

The user requested another ISubNavigationBuilder callback on AddSubNav, tree rendering within the existing secondary rail, triangle expansion controls, one darker rounded background shared by third-level siblings, and one page per component while retaining the existing secondary categories. The work is limited to Flourish and Gallery. No Colligere code, external dependency, package publication or Git commit was changed.

## Implementation

- Abstract adds a callback overload while retaining the existing leaf overload and positional Boolean compatibility.
- Shared NavigationItem exposes optional Children and current ChildItems.
- Framework seals nested builders, normalizes application-local routes and rejects duplicate siblings or routes owned by different branches/modules. A destination may also be its own first child.
- ApplicationShell traverses enabled destinations for the most-specific match, including targets outside a parent's URL prefix. A shorter child prefix cannot displace a more-specific parent.
- Parent links navigate; adjacent native buttons toggle expansion. Nested lists remain in the same rail, with hidden panels and unique aria-controls relationships. Direct loads/navigation reveal the current ancestry; ordinary rerenders preserve a manual collapse. Expansion is per shell instance, and disabled parents disable descendant interaction and matching.
- Design provides --f-navigation-children-surface and a shared control-radius panel without extra indentation. Framework alone retains native links/buttons, hidden state and tree structure. Narrow windows keep a bounded vertical tree and reuse the existing expanded rail. Its focus loop includes disclosure buttons.
- Gallery keeps eight Controls categories and builds 73 third-level routes from the component catalog in Program. Category pages contain a searchable lightweight directory. Each component route renders one five-stage guide with its existing executable examples and complete source.
- Framework's navigation page includes a nested API snippet. Active documentation, the explanatory directory map and manual acceptance checklist were updated. The user's requested tree presentation is recorded as a Flourish extension to the common two-level guidance.

## Verification

- Final Gallery and console-fixture builds succeeded with zero warnings and errors, using separate temporary output directories to avoid the user's running Visual Studio output.
- 63/63 console checks passed. New coverage includes source-compatible leaf configuration, nested builder sealing, path/duplicate validation, optional metadata, most-specific matching, disclosure/panel semantics, disabled ancestors and real same-instance collapse/rerender/navigation/scope isolation.
- 21/21 CSS bundle checks passed.
- 855 local HTTP checks passed across eight category pages and all 73 independent component pages, plus Home, other modules and resource checks. Category pages contain no live guides; each detail contains one guide and all five stages, the selected leaf and an expanded owning category.
- Stable and fingerprint shell.js requests decoded correctly with the updated disclosure focus selector.
- The final prefix-specificity correction was covered by the console suite and final Gallery build; the HTTP route set uses the unchanged Gallery hierarchy.
- git diff --check passed. The temporary Gallery host was stopped; the user's Visual Studio process was not stopped.

## Manual acceptance and limits

Follow blazor-manual-tests.md, especially Third-level navigation acceptance: pointer and keyboard toggles, multiple expanded categories, long labels, back/forward/reload, theme backgrounds, Framework-only mode, 320px/zoom layouts, modal focus return and per-instance state. Computer Use was not used, as required by AGENTS.md. SSR and HTTP establish composition/routing/resource delivery, not browser pixel, focus or assistive-technology acceptance. The native mode deliberately obtains the darker rounded skin only when Design is loaded.
