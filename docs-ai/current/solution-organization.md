# Solution organization

## Repository ownership

Flourish owns UI contracts, rendering/behavior, optional design layers, Gallery applications and optional Culture bridges. Essential is an independent external utility product consumed through NuGet; no maintained Flourish project references a sibling Essential source project.

Blazor and native WPF use Abstract contracts, Framework rendering/interaction, optional Design styling, and a dependency-only convenience package. Core contains the older platform-neutral desktop/application services; the current native WPF reconstruction is a distinct graph. WinUI3 remains an empty library and placeholder Gallery, not an implemented UI framework.

Both optional Culture bridges depend on their platform's Framework and Essential package. Framework itself has no Essential dependency. Gallery owns demonstration/business data, application translation keys and navigation/commands. Flourish owns control presentation and lifecycle.

## Solution entries and names

The root `Flourish.slnx` and four focused solutions under `src/Flourish.Blazor`, `src/Flourish.WPF`, `src/Flourish.WinUI3`, and `src/Flourish.Extensions` define development/build groups. They are not separate services. The complete 22-project graph and all maintained files are listed in [1_architecture.md](1_architecture.md).

Project and solution names follow [codedesign.md](../common/codedesign.md#solution-and-project-names); namespaces keep their existing organization-qualified identities and package IDs remain distinct. The physical WinUI Gallery directory uses `Gallery.Flourish.WINUI3`, while project references use `Gallery.Flourish.WinUI3`; Windows resolves this casing, but a case-sensitive checkout is a portability limitation.

## Dependency and release ownership

Root Directory.Build.props currently pins the user-authorized VersionPrefix 1.4.0 and EssentialCultureVersion 1.4.0. Extensions consume Essential packages, and generator-enabled applications/tests use Essential.Culture.Generator. All project/package/SDK dependencies and actual resolved versions belong to [1_dependency.md](1_dependency.md), which separates the earlier 1.1.3 audit snapshots from the new release restore evidence.

The release manifest packages the same six Core/Blazor entries in their dependency order. All six 1.4.0 packages are published from v1.4.0, publicly indexed and verified by fresh public-only consumers. Native WPF verification uses local candidates outside that public release set, and its Culture bridge remains excluded. Follow [NuGet release and integration](nuget-release-integration.md) and [release verification](release-verification.md) for publication boundaries and dated evidence. Local candidate packing alone does not establish publication.

Human DocFX configs still name the deleted single-project WPF csproj/favicon paths. They are reported as stale integration inputs and remain unchanged under the documentation ownership rule.

## History

The [previous solution migration guide](1_archived/2026-10-10_previous-solution-organization.md) retains the two-repository migration and earlier APIs/versions as evidence. New change/bug records use `1_changelogs/` and `1_bugreports/`; existing older histories remain immutable at their original paths. Current architecture and dependency files supersede historical tree/version statements.
