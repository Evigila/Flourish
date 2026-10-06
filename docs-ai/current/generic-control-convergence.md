# Generic control convergence

## Decision and scope

On 2026-10-06 the user requested removing ConfirmationHost in favor of Dialog and ordinary Button, reviewing ReconnectDialog and other overlapping specialized controls, and shortening descriptions inside Gallery boards. This explicitly supersedes earlier recommendations to add a dedicated confirmation host/service and reconnect renderer. Pre-production breaking refactors are authorized; deleted entries have no aliases, forwarding components or alternate renderer.

The review covered the 89 exported Blazor components in ComponentUsageCatalog at the start of this follow-up. Nine redundant wrappers, specialized entries or unused construction helpers were removed. The current maintained catalogue covers 80 exports, 683 parameter rows and 307 defaults. Desktop controls are outside this review. Existing General/Scenario classifications describe usage responsibilities; a Scenario label alone is not evidence of an application-specific contract.

Flourish source and component implementations remain the UI authority. This document records contract ownership and review findings, not a second visual specification. The [API guide](component-api-organization.md) describes the supported contracts.

## Removed entries

| Retired entry | Current production composition and reason |
| --- | --- |
| Components.ConfirmationHost | Direct Dialog and ordinary Button. A confirmation is caller content and a result, not an independent global renderer. ConfirmationService and shell/DI mounting are also removed. |
| Components.ReconnectDialog | Direct browser-controlled Dialog with DialogView, Notice, ProgressBar and Button. Connection protocol belongs to the host; keyed visibility, native modality and focus are reusable Dialog capabilities. |
| Components.StaticDialog | Dialog BrowserControlled supports pre-rendered native content through the same modal core, without a second component. |
| Components.BottomSheet | Dialog Presentation.BottomSheet selects the existing alternate presentation. |
| Components.StandaloneNumberBox | StandaloneTextBox Type=number retains native numeric attributes and unbound transport. |
| Patterns.RecordListPage | PageBody, PageHeading and Section directly compose the same generic page structure. |
| Primitives.AccessSurface | ContentSurface DocumentFlow and PresentationBand FullHeight directly compose full-height documents. Existing Tone, Dotted and single-main semantics are preserved. |
| Primitives.SelectionDropdownSurface | ReferenceDropdown owns its small private structural wrapper. No complete standalone interaction justified a public component. |
| Primitives.ToggleIndicator | No real production caller used this exported helper. Existing ToggleSwitch and ToggleSection continue to own their actual rendering. |

Associated Gallery guides/samples, catalogue records, retired component tests, styles and unused imports were removed or migrated. AccessBrand paint was named AccessBrand.css; ReferenceDropdown owns its former structural-wrapper rules. There is no reconnect-specific stylesheet. Nine shared Reconnect_* localized resource keys remain: removing a renderer does not establish that every external protocol consumer stopped using its text keys. This does not retain a retired component or service.

## Dialog ownership

[Dialog](../../src/Flourish.Blazor/Flourish.Blazor.Framework/Components/Dialog.razor) supports controlled IsOpen/IsOpenChanged and independently awaited ShowAsync(CancellationToken) on an existing component reference. CloseAsync(result) follows Busy/CanClose. Only explicit true is accepted by current confirmation callers; dismissal, cancellation and disposal yield null. Concurrent ownership is refused; callers preserve pending business text instead of overwriting it. NavigationGuard owns one actual Dialog, and business pages directly render their own Dialog and Button actions.

BrowserControlled pre-renders a closed native dialog and does not depend on a working circuit. It cannot switch lifecycle owner or combine circuit opening/closing callbacks. Dismissible=false disables incidental close actions for connection interruption. DialogView accepts keyed RenderFragment content; the generic controls.js setDialogView helper changes owned view/action visibility and repairs focus, while synchronizeDialog manages modality. Empty associated action footers collapse; nested dialog actions only affect their nearest dialog.

A host reconnect composite supplies the existing fixed Blazor IDs, scoped vocabulary and protocol transitions. It retains reconnect/resume/reload and document-visibility behavior. A class-only observer maps native circuit states to generic views; it neither renders controls nor duplicates modality. Gallery's /examples/overlays/reconnect uses fictional states and the same library helpers, without calling a server.

## Remaining convergence candidates

These are implementation findings, not permission to drop existing behavior. A useful merge first supplies the complete generic capability, migrates consumers/Gallery/tests, then removes the duplicate.

| Priority | Candidate | Required generic capability before removing the current entry |
| --- | --- | --- |
| High | AccessBrand and LogoDisplayer | Reconcile alignment, configured identity/logo, artistic presentation, alternative text and heading semantics in one identity display entry. |
| High | AccessActions, InlineActions and FormActions | A single action layout needs semantic nav/div ownership, accessible labels, alignment/margins and equal-column form actions. Similar children do not make these current layout contracts interchangeable. |
| High | AccessPanel and AccessFormSurface | General surface/layout variants need their existing width/alignment and form spacing; avoid introducing nested decorative surfaces. |
| Medium | ServiceMenu and ActionMenu | ActionMenu needs the service entry's hover, links and SSR/native ChildContent behavior. Its current hover and native-content combination is intentionally constrained. |
| Medium | DataPager and DataTable's internal pager | One paging renderer must preserve progressive SSR, page-size choices, synchronized upper/lower regions and loaded-result semantics, with one event contract. |
| Medium | ProgressBar and ProgressRing | A general shape variant must preserve native progress/text behavior and the ring's stopped state, not merely replace its paint. |
| Lower | DropdownSurface and ExpansionIndicator | Review actual callers and Gallery obligations before internalizing remaining construction-only helpers. |

The remaining access-named entries are presentation/layout concepts, not authentication services. Their naming makes them good convergence candidates, but direct replacement with an incomplete generic surface would silently change geometry or semantics.

## Distinct responsibilities reviewed

| Group | Reason to retain distinct contracts |
| --- | --- |
| Bound inputs and Standalone inputs, including masks | EditContext/value parsing/validation and native unbound GET/POST transport differ. Rounded geometry is shared; binding behavior is not interchangeable. |
| SelectBox, ReferenceDropdown, SearchAutocomplete and MultiSelectBox | Scalar values, nullable references, query/candidate selection and complete multiple-selection snapshots differ. The earlier DisplayOptions/MultiSelectDropdown duplicate is already consolidated into MultiSelectBox. |
| DataTable, ListView and EditingGrid | Interactive records, static comparisons and spreadsheet cell editing have different lifecycle/data contracts. |
| Disclosure and ToggleSection | Native disclosure opening and controlled business-boolean visibility/retained child state differ. |
| Card, IdentityCard and OfferCard | Ordinary content, identity slots and selectable staged offers have different semantics, registration and focus/selection obligations. |
| CodeBlock, CopyText and DisplayBoard | Formatted code, inline copyable text and executable preview/source presentation serve separate responsibilities. |
| UniformGridButton and Button | A tiled finite choice/action layout is different from an ordinary action. Confirmation actions use Button directly. |

Do not manufacture convergence by changing a catalogue enum, keeping an adapter or moving generic rendering into a consumer. Business/security/session/data contracts remain unchanged, and no dependency was added.

## Gallery description placement

Thirty-four sample components move technical explanations into SampleFor.Description. ComponentGuide renders them before DisplayBoard and preserves paragraph breaks. Boards retain actual controls, fictional business content, necessary labels, short status/hints and required validation. Shared code captions and usage notes stay outside their backgrounds. The independent reconnect example demonstrates composition rather than restoring a specialized production entry.

## Verification and manual acceptance

The [convergence change record](currentproject-changelogs/2026-10-06_141857_converge-dialog-and-simplify-gallery.md) records final library, package and consumer gates. No Computer Use or physical browser measurement was performed. After restarting current candidates, manually review centered/bottom dialogs, acceptance/cancellation/close veto, focus restoration, reconnect states, narrow layouts and the reduced board copy.
