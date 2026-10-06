# Top bar injection alignment and offer label semantics

ShellHeader and ApplicationShell now share one library-owned alignment boundary for injected controls. OfferCard identifies plans without adding headings to a consumer page. These changes address Colligere's authenticated sign-out alignment and single-title Pricing composition without consumer CSS or compatibility APIs.

## Cause

The earlier rule reset only shell-header-actions > InlineActions. Direct Button, Services, LeadingActions and ApplicationShell tracks did not share that contract. Button's align-self:start overrides a parent align-items:center, and InlineActions contributes a content-flow margin-top:16px. A new unscoped slot > * rule would still tie these Design rules at one-class specificity and lose when Design loads later. Unequal-height buttons inside an otherwise centered group also need their own alignment reset.

## Correction

Wrap ShellHeader's three optional fragments in f-topbar-slot only when present and annotate ApplicationShell's three configured tracks with that same class. Scope functional CSS to :is(.shell-header,.f-titlebar) .f-topbar-slot. Direct roots, native form children and immediate InlineActions members center with zero block margins. Two or more class specificity overrides the existing one-class document defaults without important declarations or theme paint. This does not recursively restyle menu items, replace popup positioning, reveal hidden fields, fix child heights or alter native transport. Brand and logo remain in separate stretched tracks; the identity keeps its existing renderer.

Gallery's ShellHeader sample switches between anonymous Elevated action groups and a direct Quiet sign-out action beside the identity. All six Button variants, optional fragments, grouped actions, native POST content and actual ApplicationShell injections have executable checks. CSS checks cover both document defaults and the stronger structural selectors.

OfferCard now renders its Title as p.f-offer-title, not h3. Preserve encoded offer identity, the aria-hidden compact duplicate and the existing OfferStage list/target/rotation behavior. Both Framework and Design active-state and transition selectors target this label. No old h3 selector or heading-mode compatibility switch remains. Current API guidance describes both contracts.

## Verification

The complete scripts/Test-Release.ps1 gate passes. It verifies zero-warning/error Release builds, 333/333 Blazor checks, 87 registered components, existing Core/Culture regressions, eleven configured JavaScript files, CSS bundle/SDK gates, six local packages and four isolated package-consumer scenarios with 95 checks. Evidence: artifacts/topbar-pricing-release-final-2026-10-06.log; package consumers: artifacts/package-consumers/4774ad5820924fbc8e9fba141e3e7f0b/.

Colligere restored those candidates into d99eb5a8729bf5e88354e1753e9787cb and passed Release compilation and all UI regressions. Framework DLL SHA256 E09283D5631099A2E0A04637DD298918A60FB7808B8BDC8E73036DF7CE81293D matches the library build, restored package and consumer output. Its complete suite still has the two previously documented nonvisual order-contract failures, not new UI failures.

## Manual acceptance and limits

Check both Gallery identity states, direct actions of each variant, grouped actions with wrapped unequal-height labels, native form submission, Services popup focus/hover, long identity names, narrow layouts and light/dark themes. Check OfferStage labels and target selection with normal and reduced motion. Components retain the current top-bar sizing contract; this change does not promise that arbitrarily oversized injected content fits a fixed-height header.

Automated checks establish source, rendered structure, event/transport and package contracts, not physical browser geometry. No Computer Use, dependency changes, runtime process termination, Git commit/push or public publishing occurred. Earlier uncommitted changes and historical reports remain unchanged.
