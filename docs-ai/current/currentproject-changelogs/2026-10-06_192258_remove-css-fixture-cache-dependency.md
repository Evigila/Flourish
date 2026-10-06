# Remove the CSS verification fixture's global cache dependency

## Scope and cause

Flourish v1.1.0 CI runs 37539504631 and 37539506569 failed before publishing, in build/Test-CssAssets.ps1's package-build fixture. Production builds and the preceding component, localization and JavaScript checks passed. The script added the user's default global NuGet cache as a local feed even though the fixture has no external dependencies. A missing directory makes NuGet restoration fail.

## Evidence and correction

A controlled absent-cache reproduction failed with NU1301 and retained its SDK log under artifacts/css-assets-786fc32fa3ad471dbc851e043a8050e3/package-build.log. The original remote fixture did not print its captured SDK log; the diagnosis matches the failed stage and has been reproduced locally, rather than claiming the remote log contained NU1301.

The fixture now restores exclusively from its own generated package feed. SDK failures print their captured diagnostics and retain the fixture log. All 194 existing CSS SDK integration checks pass with the correction; evidence is artifacts/css-assets-a5d5845f3a434c979c37fa2a1bb2c0ae. No product CSS, component behavior or assertion was weakened.

## Release boundary

Essential 1.3.0 has been published successfully. Flourish's old tag remains immutable and both old publish jobs were skipped. The synchronized Flourish package version moves to 1.1.1 for recovery, retaining the exact six Core/Blazor package scope and Essential 1.3.0 dependency. Full preparation and publication are pending in this record; later records must report their actual results.

The user has authorized the two repository commits and agent-executed publishing. The existing nuget environment and NUGET_USER variable supply the Trusted Publishing workflow identity; no local API key is needed.
