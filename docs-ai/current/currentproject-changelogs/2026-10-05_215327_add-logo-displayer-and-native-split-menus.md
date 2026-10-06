# Login identity and native split menus

Flourish supplies the full-page login presentation through existing banner/access primitives, a new vertical LogoDisplayer scenario and an additive SplitButton native-menu mode. Framework retains behavior/layout ownership, optional Design owns paint, and Gallery demonstrates the production contracts. Authentication and form transport remain consumer-owned.

## Changes

PresentationBand and its Hero facade accept FullHeight=false by default. Opting in establishes at least 100dvh while retaining MinHeight and natural growth. AccessSurface.Banner owns one main and delegates its child presentation to that band; Dotted stays independently off by default. The library resets the browser body margin only when an independent access document is present. No ordinary business or marketing layout is silently converted.

LogoDisplayer renders the configured logo above the configured project name, including scoped TextReference refresh and explicit overrides. It accepts semantic heading levels 1–6, a title ID, optional context/description and the established root Class/AdditionalAttributes pattern. Framework uses a 96px contain image; Design gives the name responsive 56–80px artistic text, 24px logo/name gap and 40px space before following content. It is a supported access/display scenario, not an ordinary business-title variant. The catalog now contains 98 controls, 733 parameter rows and 324 documented defaults.

AccessFormSurface.Actions provides a named action slot without introducing a form. Its 16px grid gap plus 16px action offset give 32px effective separation when actions follow fields. Consumers retain native form or EditForm ownership and antiforgery placement.

SplitButton adds FullWidth, positive pixel Width, Busy/BusyLabel and MenuContent. The native menu reuses DropdownSurface with additive TriggerClass/TriggerAttributes. FullWidth takes precedence over Width; non-navigation secondary controls stay a fixed 48px square. Busy/Disabled block both callbacks and omit native menu links. SecondaryControls identifies the actual content; controlled Expanded/OnSecondaryClick and static aria-expanded/onclick conflicts are rejected rather than combining competing controllers. Legacy controlled callbacks and navigation-row geometry remain supported.

Native details supports static SSR without JavaScript and uses ordinary disclosure semantics. It intentionally does not claim application-menu Arrow navigation, automatic Escape/outside-click closing or viewport flipping. Existing interactive menus retain their own documented purpose. A CSS cascade defect found during implementation would have hidden the native panel even when details was open; the specific split-menu open rule corrects that without enabling old closed popovers.

The login Gallery now uses a full-height dotted band, vertical identity and full-width SplitButton with recovery/registration demonstrations and Back in its disclosure. Existing validation, outcome, busy/cancel and fictitious-account state remain intact. SSR submit stays disabled until interactive connection; no authentication transport or credential persistence was added. Dedicated LogoDisplayer and updated SplitButton samples document the new parameters.

## Verification

- Complete scripts/Test-Release.ps1: exit 0; Release compilation zero warnings/errors; Core 367, Blazor 250, bridge 12, all ten JavaScript files, CSS bundle 21 and SDK integration 194 passed.
- Six 1.1.0 candidates passed package identity/dependency/static-asset validation. Four fresh isolated NuGet-only consumers passed 95 checks. Evidence: artifacts/package-consumers/9639717e6d2249bd9837a9c95c1cf2c4. Packing retains expected README advisories.
- Sixteen new Blazor checks cover split native/controlled behavior, busy/disabled, width isolation, actual control IDs, conflict validation, full Framework/Design menu cascade, logo identity/culture/encoding, full-height default isolation, single-main composition and same-form Actions.
- Catalog: 98 components, 733 rows and 324 defaults. The final Gallery-only Back cleanup rebuilt without warnings/errors. Five Gallery routes and both actual CSS bundles passed strict HTTP checks; the owned process was stopped.
- Colligere full Release solution build zero warnings/errors and 248 focused Web checks passed, including 21 new login cases, with matching local candidate archives. User's running instance was not stopped or restarted.
- Essential and desktop sources, external dependencies and package versions are unchanged. No Computer Use, new commit, push, public NuGet upload, deployment or database reset occurred. Broader API convergence is not claimed completed.

## Manual acceptance

Inspect the login and accounts Gallery pages under both themes at wide/320px widths, short viewport heights and 200 percent zoom. Full-height content must grow and remain reachable. Verify vertical logo/name order, configured identity, increased submit separation, full input-width split geometry and fixed square triangle.

Use Tab and Enter/Space for the native disclosure; opening it must not submit the form. Check Busy/Disabled state removes usable secondary links, and compare the controlled SplitButton sample to ensure its independent expansion still works. Gallery recovery/registration are demonstration links only. Actual Colligere native POST and gated links can also be checked with JavaScript disabled after restarting its old instance through start.bat.
