# Public package consumer rejects the SDK asset dependency

## Symptoms and evidence

The first post-publication consumer validation failed restoration with NU1100 for Microsoft.AspNetCore.App.Internal.Assets 10.0.11. All six published Flourish 1.1.1 versions were already present in public indexes. The original failure remains in artifacts/package-consumers/d1afce7bb6784b64be6730553c6e64a3/FrameworkOnly/obj/project.assets.json; the fixture NuGet.Config preserves the duplicate source configuration.

## Cause and correction

Passing the public NuGet URI through PackageDirectory caused the fixture to register the same endpoint under both Flourish and nuget.org keys. NuGet deduplicated the endpoint, leaving the restrictive Flourish.* source mapping effective for the retained source; the SDK asset dependency could not resolve.

The consumer script now has an explicit PublicSource switch. It clears inherited feeds, adds NuGet.org exactly once and maps every dependency to that source. Mixing this mode with either local package directory is rejected before creating a fixture. The default candidate-feed and explicit Essential-directory behavior remains the existing path. This verification helper is not shipped in the NuGet packages, so the correction requires no package version replacement or new publication tag.

## Verification and limitations

The command build/Test-BlazorPackageConsumers.ps1 -PublicSource -Version 1.1.1 completed with exit code 0 and all 129 existing checks across FrameworkOnly, MetaNative, MetaDesign and MetaCulture. Evidence: artifacts/public-culture-release-1.1.1.log and artifacts/package-consumers/027abe99e60f46dbaa1d15a75b670ff6.

The fixture configuration has one public source and a wildcard mapping. All six Flourish 1.1.1 package metadata files and the transitive Essential Blazor/Core/Generator 1.3.0 metadata identify https://api.nuget.org/v3/index.json. Static CSS, JavaScript, icon font, rendering, service registration and three-language generated-key checks passed. These are automated local consumers of the public packages; physical browser acceptance remains user-operated.

## Regression checks

- After each authorized publication, verify every package index and run the explicit PublicSource consumer mode using its fresh fixture/cache.
- Retain the single public source and wildcard mapping; do not register the same endpoint under restricted local-feed and public-feed keys.
- Keep local preparation on its candidate-feed mode and verify exact package graphs/assets independently of source builds.
