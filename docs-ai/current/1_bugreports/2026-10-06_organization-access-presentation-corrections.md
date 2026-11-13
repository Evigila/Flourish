# Organization access presentation corrections

## Scope and historical evidence

The user authorized Colligere to adopt the current access-method example and reverse-correct its reproduction of the former tenant portal. The reference is Colligere commit 5394774, Product.razor and PublicLayout.razor.css, before d4c5d9a. Existing PresentationHero.SideContent/StackTitleWords, AccessFormSurface.Tone and NavigationChoices.Compact already express the layout; no new portal-specific renderer or compatibility entry is added.

The intended scene has a flexible left artistic name, a right 360–470px operation column, one Primary board and compact ordinary method links. At 860px it stacks; mobile retains a bounded, wrapping title. The current Gallery example and Colligere use the same production entries. Consumers retain their own protocols, real business labels and authorization.

## Confirmed defects and correction ownership

1. AccessFormSurface's direct Section had margin cleared but retained Foundation's 44–84px top padding. The library now clears margin and top padding for that contained Section; ordinary document sections are unaffected.
2. Primary Field labels inherited the Field's muted foreground, not the surface's paired ink. Standard controls.css now gives both Field and label the Primary ink role. Inputs keep their own normal/disabled skins.
3. presentation.css contained per-scene Button paint and width, failing the strict no-control-reskin guard. Available Quiet's contextual role mapping now lives with the shared Button in controls.css, with separate hover and press and explicit native/ARIA disabled exclusions. Secondary, Danger and Elevated are not overridden. Full-width operation actions belong to Framework presentation layout, for direct method actions, native-form FormLayout submits and direct Section continuation actions.
4. Split typography's stronger selector overrode generic narrow-screen rules. Explicit split breakpoints now restore 58/12vw/88 at 860px and 52/16vw/72 at 560px. The bounded max-width and overflow-wrap protections remain; historical max-content/no-wrap overflow is not restored.
5. Primary hero copy paint previously matched only the unsplit structure. A separate direct-child path covers the actual split layout without leaking into nested presentation content.

These are shared library rules, not Colligere-specific classes, palette literals, aliases or a second access API. The former native-GET navigation disguised as ARIA tabs is not restored.

## Regression evidence

`organization-access-presentation.test.mjs` follows Framework/framework.css and Design/design.css recursively in actual import order. Its bounded DOM/CSS evaluation includes descendant/direct-child matching, :not/:is/:where, attributes, source order, specificity, important declarations and width breakpoints. Seven tests exercise available/disabled Quiet states, explicit-variant/input parity, both Field foreground layers, the strict scene boundary, panel spacing/action widths, responsive geometry/long names and actual Primary split copy.

Together with palette-checks.mjs, 28/28 Node tests pass. The unchanged strict presentation guard now passes. The generic variant selector check separately identifies general variants; the new cascade matrix verifies the legitimate Primary context rather than ignoring it.

Framework and Design Release compilation/packaging succeed locally, with no public version change or publication. Colligere consumes the actual 1.1.0 candidates via a SHA256-verified cache, not sibling ProjectReferences, and passes 94 focused tests including 29 real portal SSR cases and native sign-out/re-entry. Its full Web run retains three prior unrelated failures: 1846 passes, three failures, 85 skips, total 1934.

The complete Flourish console entry and isolated Gallery build were attempted but cannot be reported passing: current catalog parameter-documentation changes leave CatalogChecks using removed ComponentParameter.Description, and Gallery currently fails on generated localization Key members such as Examples_SearchForm/Appearance_Primary. Those files and dependencies were not changed here. No unrelated API/localization rollback or fake fallback is introduced. The earlier successful 373-contract run predates these current errors and is not substituted for this verification.

## Limits and manual checks

No running Aspire/Gallery instance was stopped, no Computer Use was performed, and no package/dependency version, publication, staging or commit was introduced. Rebuilt local candidates do not hot-replace an existing host process. Physical-browser acceptance remains manual.

- After a normal restart, compare the real Gallery access-method example and Colligere's organization portal at desktop, 860px, 560px and narrow phone widths.
- Check long names remain bounded/wrapping, art stays separate from the operation area and only one Primary board is shown without extra section top space.
- In both themes, check paired field/link foregrounds, selected Secondary links, separate Quiet hover/press, keyboard focus and disabled Quiet/explicit action identity.
- Confirm native form and method activation remain consumer-owned; the fictitious Gallery does not acquire authentication.

This report supersedes only the access-style limitation in the earlier secondary-label/chart-heading report. Previous reports and append-only history remain intact.
