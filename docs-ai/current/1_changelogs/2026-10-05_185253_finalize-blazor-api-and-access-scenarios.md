# Flourish Blazor final API and access scenario review

The user requested a final Flourish-only review of API consistency and complete page examples before an explicitly authorized local commit. Colligere pages are reserved for later individual optimization; neither Colligere nor Essential is changed in this follow-up.

## API conclusion

All 97 exported Framework components have explicit usage metadata, Gallery registration, reflected parameter/default documentation and executable examples. The catalog contains 707 parameter rows and 311 initialized defaults. General/scenario entry points, compatibility APIs and construction helpers remain separately identified.

This is not a fully converged API. Generic and advanced tables still have different column contracts, enums, state, templates, processing and browser controllers. Advanced sorting/searching still select pt-BR. The legacy sheet, toggles, disclosures, headings, forms, identity and notification renderers also require capability-preserving shared cores and compatibility adapters. Native protocol versus EditContext inputs, ListView versus EditingGrid, tile versus fluid layouts and display versus business titles are legitimate scenario distinctions, not duplicates to delete.

Gallery now states that ReferenceDropdown.SelectionChanged emits a nullable reference ID while MultiSelectDropdown.SelectionChanged requests a single-value toggle. Neither is presented as a Changed callback compatible with the corresponding @bind name. Controlled/default state semantics for DataTable.View/PageSize remain a separate migration decision. No public component API or callback payload was renamed.

## Confirmed defects and corrections

- SelectBox formatted its current typed value with thread culture while options/parsing used the scoped provider. SSR reproduced current value 1.5 against option 1,5 and no selection. All three paths now use the same provider, with pt-BR/en-US/de-DE refresh coverage.
- SearchAutocomplete emitted minimized/missing boolean ARIA and accepted delayed input/selection callbacks while Disabled. It now emits explicit true/false and refuses disabled operations.
- MultiSelectDropdown allowed existing selection changes during asynchronous creation. Trigger/choices now lock and callbacks refuse those changes, preserving the existing single-value toggle after completion.
- Bound CheckBox omitted its generated native POST name. NameAttributeValue is now rendered, with explicit host names retaining precedence.
- OfferCard/OfferStage allowed unmatched id/class/role attributes to replace identifiers and semantics already checked by their public contracts. Explicit reserved attributes now win while unrelated attributes remain available.
- OfferStage, RowActionMenu, ReferenceDropdown's focus query, ContentSurface, NavigationSurface and generic DataTable could lose ownership of late imports or continue behavior after disposal. Their module acquisition is single-flight; late imports are released, teardown is idempotent and proxy release still occurs after disconnected/canceled DOM cleanup. ContentSurface switching to DocumentFlow cannot install a late fixed-stage controller and releases an already-owned controller once.

The first 25 added regression cases produced 24 failures before correction. The later surface/table expansion added 24 cases and produced 21 failures before correction. The bound-checkbox name case and local fixture cases add coverage independently. Final new coverage totals five API/input cases, 49 interop/attribute cases and seven access-state cases, raising the Blazor suite from 166 to 227 checks.

## Executable access scenarios

Gallery adds /examples/display/login and /examples/display/accounts under Examples > Display pages; /examples/display/access becomes the overview. AccessExampleLayout only supplies public library resources and scoped theme state. AccessSurface owns the sole main; AccessPanel, AccessFormSurface, AccessActions and existing fields, buttons, cards, notices and layout controls provide the scene. No new public renderer, package or external dependency is added.

Local fixtures demonstrate required/malformed input, a visible 1200ms busy transition, selectable success/rejection/unavailability results, email prefill, remembering a fictitious email, available/disabled/stale account choices, removal and empty-state restoration. Passwords are cleared at completion/cancellation. The example performs no authentication, token handling, network call or browser persistence. Its static form stays disabled until interactive connection rather than silently submitting to a nonexistent authentication endpoint. Language and theme switching use the established scoped services.

The AccessSurface component sample previously nested main inside ApplicationShell.main and supplied raw panel/scoped host CSS. It now explains the independent-document requirement and links to the complete executable pages using standard Buttons. The maintained architecture map includes all six added files, covering 997 files and 143 directories.

## Verification

The complete scripts/Test-Release.ps1 preparation passed: Release compilation with warnings as errors reported zero warnings/errors; Core 367 tests, Blazor 227/227, Culture bridge 12, all ten configured JavaScript files with 53 Node-runner checks plus the existing mock suites, 21 CSS bundle checks and 194 CSS SDK integration checks passed. Six local 1.1.0 packages passed identities/dependencies/assets; four new isolated NuGet-only consumers passed 95 checks. Evidence is artifacts/package-consumers/efff6a1005f74ab6bda010c636e819ad. Package README advisories remain expected under the repository ownership rule, not suppressed by adding an unauthorized README.

Strict HTTP checks passed 119 Gallery routes: 16 directories, 97 component guides and six scenarios, plus four static resources. Every scenario has one main; login's static controls remain disabled until hydration. An initial Production-mode run of unpublished development output lacked static assets; restarting with the existing Development http profile and terminating HTTP errors passed the full check. Published NuGet-only consumers separately passed their asset verification. Owned local Gallery processes are stopped after verification. No Computer Use or browser-geometry assertion is included.

The earlier authorized Flourish package consolidation, naming, ListView, display extraction and documentation retirement remain part of the coordinated uncommitted source set. The user authorizes its local commit together with this review; there is no authorization to push, create a release tag, deploy or publish NuGet.

## Remaining convergence and manual acceptance

First define the table capability union and controlled/default-state contract, then implement one core and adapt old APIs without losing advanced editing/templates/transports/preferences. Converge low-risk aliases/layout/title/feedback next, followed by dialog/input lifecycle. Keep regression gates at capability and event/lifecycle level rather than merely counting entries.

SearchAutocomplete needs a browser-level Arrow/Enter default-action contract; its C# handler alone does not prevent implicit native form submission. Primitive NavigationGuard's delayed-import/disposal path and ReferenceDropdown's post-callback/FocusSearch disposal races remain dedicated follow-ups. These boundaries prevent interpreting the review or passing tests as a statement that every await branch is covered or that 1.1.0 already exposes a fully unified API.

Manual acceptance remains with the user: switch language and light/dark theme on both access pages; test required/invalid fields and all three results; confirm busy/disabled actions; select/prefill, reject unavailable or stale accounts, remove/clear/restore, remember an email and verify no password persistence; leave during a pending transition; rapidly switch routes and content layout modes; verify native checkbox POST names and culture-preserving numeric selection; verify autocomplete keyboard behavior within a real form. Registration/recovery and other consumer-specific pages remain separate future scenario requests.
