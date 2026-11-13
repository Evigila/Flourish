# CSS package fixture fails on an absent default NuGet cache

## Symptoms

Flourish v1.1.0 release runs 37539504631 and 37539506569 failed at Test-CssAssets.ps1's package-build after main builds/tests and 21 CSS bundle checks succeeded. Publishing was skipped before OIDC login or any package push.

## Cause and evidence

The standalone fixture configured the user's default .nuget/packages directory as a second local feed. The fixture needs only its own generated Test.Generic.Assets package; a local feed path that does not exist triggers NuGet NU1301. The new release validation restores repository dependencies into a deliberately isolated cache, so a clean runner need not have the default global directory.

The remote job retained the inner SDK log only in an unuploaded temporary directory, so its exact NuGet diagnostic was not visible. A controlled local reproduction with an absent cache directory failed at the identical fixture and produced NU1301. Evidence: artifacts/css-assets-786fc32fa3ad471dbc851e043a8050e3/package-build.log.

## Correction and limitations

Remove the global-cache package source and restore only from the generated fixture feed. Print captured SDK output before throwing, while retaining per-step fixture logs. This changes test isolation and diagnostics, not product CSS, package assets or assertion coverage.

All 194 corrected integration checks passed in artifacts/css-assets-a5d5845f3a434c979c37fa2a1bb2c0ae. The first remote failure is explained by the controlled reproduction; the corrected release must also pass on the clean GitHub runner before its remote result is claimed. Existing v1.1.0 tags remain unchanged, and synchronized Flourish package version 1.1.1 is used for recovery.

## Regression checks

- Run build/Test-CssAssets.ps1 with a fixture-only source and isolated package cache; all 194 development, package, incremental and publish compression checks must pass.
- Complete scripts/Test-Release.ps1 on a clean runner without relying on the default user cache.
- Verify exactly six Core/Blazor packages and public package consumption after publication.
- Manual product checks remain the existing original-launcher Gallery styles/icons and three-language interaction checklist; this script-only correction introduces no additional UI behavior.
