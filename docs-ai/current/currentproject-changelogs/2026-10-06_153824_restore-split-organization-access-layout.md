# Restore the split organization access scene

## Request and original evidence

The user requested restoring the original Colligere access-method layout in the Flourish Gallery: organization name on the left, Workspace/Colligere/pass operations on the right, using standard Flourish controls.

The last original implementation is Colligere commit 5394774, parent of d4c5d9a, in src/Arkheide.Colligere.Web/Components/Pages/Product.razor and Components/Layout/PublicLayout.razor.css. The first migration removed the original geometry; a later migration introduced NavigationChoices. Restoring only the working navigation left out the organization heading and split composition.

Verified original geometry: a centered 1180px region; left flexible and right 360–470px columns; gap clamp(54px,9vw,120px); right-aligned whitespace-separated organization words in 64–112px typography; one Primary operation surface with a 28px radius. At 860px the columns stack; at 560px mode links stack and the operation surface uses 24px/18px padding with a 22px radius. The original Workspace mode had login-address/password, Colligere had a native login link, and pass had one secret/password field.

## Implementation and ownership

- Extended existing PresentationHero with SideContent and StackTitleWords. The library owns asymmetric columns, title typography, responsive boundaries and encoded full accessible title. Without a side slot the original hero composition remains unchanged.
- Added AccessFormSurface.Tone, default Surface, with Primary/Canvas role variants. All modes are inside one Primary surface around a standard Section heading, rather than separate/nested boards.
- Added NavigationChoices.Compact, default false. Compact uses standard Button links with selected Secondary and inactive Quiet appearance. Native same-site GET destinations, explicit ActiveKey validation, aria-current, disabled behavior and retained panels use the same contract.
- Gallery's NavigationChoicesSample now directly composes these production entries with Field, StandaloneTextBox, FormLayout and Button. It has no page-specific layout CSS. Workspace and pass actions remain disabled fictitious submissions; Colligere opens the existing Gallery login page through a real native link.
- Existing organization/account/pass query keys and the working ActiveKey expression are preserved. Mode labels are Workspace, Colligere and localized Pass. Added one three-language fictitious organization-name key; Gallery now has 156 keys and Framework remains at 121.
- Corrected input IDs so labels associate with their actual standard controls, updated public API/usage guidance, and expanded real HTTP layout regressions.

Colligere was read only as historical evidence. No authentication, authorization, antiforgery, session, native host endpoint or Colligere implementation was changed. No new specialized public component, alias, dependency or host renderer was added. Earlier Gallery navigation diagnostics remain historical evidence; this is a separate layout restoration.

## Verification

- Gallery Release build and complete Blazor solution Release build with TreatWarningsAsErrors: zero warnings/errors.
- Full component regression: 371/371 passed, including compact standard links, disabled choices, preserved panel instances on variant changes, single operation column, stacked accessible heading, default hero preservation and surface tone.
- Actual Gallery HTTP/navigation regression: 227 checks passed, including all three modes, invalid-query fallback, repeated visits, both guide instances, one shared operation surface and real input/label associations.
- Three-language actual HTTP regression: 43 checks passed. Catalog integrity: 3,047 checks passed for 156 Gallery and 121 Framework keys. Culture bridge: 12/12 passed.
- Git whitespace validation passed. No Computer Use test, public publication, commit, Git push or Colligere write occurred.

Evidence: artifacts/access-layout-build.log, access-layout-solution-build.log, access-layout-components.log, access-layout-navigation.log, access-layout-culture-http.log, access-layout-catalogs.log, access-layout-bridge.log and access-layout-diff-check.log. The isolated verification instance used 127.0.0.1:5270 and was stopped after verification; the user's active Gallery was preserved.

## Manual acceptance

1. Restart Gallery and visit /examples/display/access-methods. On a wide screen check the organization title at the left and the single operation surface at the right, with no extra heading/gray/nested boards.
2. Switch Workspace, Colligere and Pass repeatedly. Check current-link appearance, two Workspace fields, the Colligere login link and the single pass field. Confirm the disabled demo actions cannot submit.
3. Narrow the viewport through 860px and 560px. Check top/bottom composition, readable long organization words, stacked compact mode links and no horizontal overflow.
4. Switch en-US, zh-CN and pt-BR, then use Tab/Enter to navigate modes and the login link; confirm visible labels, accessible headings and focus order remain meaningful.

SSR, component and CSS-contract checks do not claim browser visual acceptance. Users retain the manual verification boundary.
