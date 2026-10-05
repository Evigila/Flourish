# Shell header button variant correction

Colligere's public homepage restoration requested Elevated login/signup actions. Source review found that static-surfaces.css applied chrome text and interaction paint to every f-button inside ShellHeader, independently of the requested variant. A consumer's Elevated button could therefore inherit chrome ink instead of its surface/text contract. This is a source-confirmed cascade defect; no browser screenshot or Computer Use reproduction is claimed.

## Correction

Narrowed all three chrome override rules to available f-button-quiet actions. Explicit Elevated, Filled, Outlined and Danger variants retain controls.css paint; disabled/aria-disabled buttons also retain their unavailable-state contract. Added a palette-checks.mjs regression requiring the three overrides to target Quiet and exclude unavailable controls, with the ordinary Elevated paint declaration preserved. No public component API, package dependency/version, Framework renderer, desktop project or Culture behavior changed.

Only the local unpublished Arkheide.Flourish.Blazor.Design 1.1.0 candidate was repacked. SHA256 is D403A90E32BBCF0C509E240E3EFE9F29297A63F070E069431E005A03A62E6713. Colligere restored the same hash in a new isolated artifacts/package-cache-marketing rather than reusing the older same-version candidate.

## Verification

- Design pack succeeded; the existing no-README informational recommendation remains, consistent with the user prohibition on creating README files.
- Blazor console checks: 135/135 passed.
- All nine configured JavaScript files passed, including 11 palette checks; the Node runner reports 38 tests plus the existing mock-script checks.
- CSS bundle checks: 21 passed. Verify-PackageSet validated all six configured package identities, dependencies and required assets.
- Colligere Release solution build: zero warnings/errors. Its focused homepage/header/footer/theme/public-surface suite: 72 passed, zero failed/skipped.
- No full release preparation, fresh four-mode package-consumer suite or desktop tests were rerun for this CSS-only correction. Previous complete verification remains historical evidence, not a newly executed result.
- No Computer Use, Git commit/tag/push, public NuGet upload, deployment or data reset occurred.

Manual acceptance should place Quiet, Elevated and Filled actions in a header under Light/Dark/System modes, checking default, hover, press, focus and unavailable states. Controls/styles remain Flourish-owned; the consumer did not add a compensating button skin.
