# SplitButton primary availability and wizard headings

## Cause and solution

Registration must block native submission before an invitation token is valid while retaining retry/login navigation. SplitButton previously exposed only Disabled/Busy, which lock both parts. A consumer replacement primary renderer would violate the single-control boundary.

PrimaryDisabled (default false) now independently blocks primary native submit, link navigation/focus and callback execution. Secondary availability remains governed by Disabled || Busy. Global locks retain their behavior. Independent native-button locking uses disabled rather than leaving stale aria-disabled when the host token controller unlocks submit. FullWidth and the existing square secondary trigger remain unchanged. No specialized registration renderer, retired alias or compatibility API is added.

Gallery's SplitButton sample, catalog and parameter meanings explain the capability. Native IDs, token fragments and server validation remain consumer protocol responsibilities.

The earlier "only choices" instruction also incorrectly removed page titles. Current source/API guidance and Gallery WizardExample retain PageHeading on every step, outside the pure UGB choice area. Extra step explanations/dropdown substitutes are omitted; real forms, statuses and errors remain. The page-body-actions regression reflects the corrected composition.

## Verification and limits

Three new genuine rendering/callback checks cover independent native disabling/unlocking, link/callback safety with usable secondary actions, and global lock precedence. All 359 console and 25 selected grid/palette/split-menu checks pass. Catalog coverage is 80 components, 684 rows and 308 defaults. An earlier console build emitted an existing PresentationChecks.cs nullable-dictionary CS8620 warning; it was not altered.

Gallery builds with zero warnings/errors using OutputPath=bin/AccessWizardVerification/net10.0/. The ordinary-output retry failed because a running Gallery locked its DLL; that instance was retained. Framework 1.1.0 was packed into the existing local feed and hash-verified by Colligere in cache 36db9738964a7e4c6345632e2e6d5aa1. Pack retains its existing missing-readme advisory. No dependency/version change, remote publication, restart, Computer Use or commit occurred.

Consumer Release compilation and 179 final focused cases pass. Its full suite retains the preceding three failures and 85 database-dependent skips, documented in Colligere's registration/wizard report; a fully green baseline is not claimed.

## Manual acceptance

1. In Gallery compare PrimaryDisabled with Disabled/Busy: only independent primary locking leaves the menu usable. Check pointer/keyboard/link behavior and that disabled primary callbacks do not run.
2. Check full-width split geometry and uniform menu hover widths in both themes/narrow layouts.
3. Check /examples/display/wizard at every step: PageHeading persists while choices remain actual grid buttons.
4. After the user's normal restart, compare consumer registration with Login and verify invitation retry/submit without logging token/password values.
