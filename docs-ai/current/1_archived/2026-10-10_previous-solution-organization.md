# Superseded project guidance

**Status:** Historical; does not govern new work. Archived on 2026-10-10 (America/Sao_Paulo, UTC-03:00).

**Original scope:** `docs-ai/current/solution-organization.md`.

**Reason:** Migration evidence was mixed with obsolete package versions and former bridge contracts.

**Replacement:** [Current guidance](../solution-organization.md).

**Provenance:** SHA-256 of original bytes `aadfd8817c40aa1043c4cf261fcc8566a688f4ca16c1cc0b86ef5a418e939d46`. The original prose below is retained; relative Markdown links are rebased to preserve their destinations.

---

# Solution organization

## Native WPF supersession on 2026-10-07

The former WPF single-project host/service implementation and its Culture hosting adapter have been deleted under the user's explicit reconstruction request. Current native projects are Flourish.WPF.Abstract, Framework, Design and the dependency-only Flourish.WPF aggregate, targeting net10.0-windows. The rebuilt optional EssentialTextProvider is owned/disposed by its native consumer and configured on FrameworkBuilder; Gallery consumes this bridge. Earlier WPF descriptions below are dated migration evidence, not current APIs. See [native WPF integration](../wpf-native-integration.md).

The existing public six-package Core/Blazor 1.1.2 release and scripts/ReleaseSettings.psd1 remain unchanged. Native WPF build/pack verification creates local candidates only; this task does not publish, version-bump or add Windows targets to the already released Blazor packages.

The 2026-10-05 user-authorized migration leaves two active repository roots: Flourish and Essential. Extension is no longer an independent source root or solution. The previous local Culture integration records use the old paths; this document supersedes those paths without editing history.

## Ownership

- Flourish owns the UI frameworks, optional skins, Gallery hosts and UI integrations.
- Flourish.Extensions.Culture.WPF adapts the desktop shell to Essential.Culture.Wpf.
- Flourish.Extensions.Culture.Blazor adapts the neutral ITextProvider contract to Essential.Culture.Blazor.
- Essential owns independent utility modules. Culture is the first module, containing Core, Generator, Wpf, Avalonia, WinUI and Blazor projects.
- Essential libraries have no Flourish reference. Core, Abstract, Framework, Design and the dependency-only Blazor convenience package have no Essential reference. Shared is retired; only applications and the optional extension choose the translation implementation.

## Solution entries

Flourish.slnx includes the original framework/Gallery projects, both extension libraries and both extension test projects. Its new Flourish.Extensions folder holds the libraries; the existing Tests folder holds their tests. Solutions includes src/Flourish.Extensions/Flourish.Extensions.slnx for focused extension development.

Essential.slnx is the root product/test solution. Its Essential.Culture folder contains six libraries, Tests contains the four test projects, and Solutions links the Culture module and demo solutions. src/Essential.Culture/Essential.Culture.slnx opens the module and its tests. demo/Essential.Culture.Demo.slnx retains the five runnable examples.

Source roots:

```text
repos/
  Flourish/
    Flourish.slnx
    src/Flourish.Extensions/
      Directory.Build.props
      Flourish.Extensions.slnx
      Flourish.Extensions.Culture.WPF/
      Flourish.Extensions.Culture.Blazor/
    tests/
      Tests.Flourish.Extensions.Culture.WPF/
      Tests.Flourish.Extensions.Culture.Blazor/
  Essential/
    Essential.slnx
    Directory.Build.props
    src/Essential.Culture/
      Directory.Build.props
      Essential.Culture.slnx
      Essential.Culture/
      Essential.Culture.Generator/
      Essential.Culture.Wpf/
      Essential.Culture.Avalonia/
      Essential.Culture.WinUI/
      Essential.Culture.Blazor/
    tests/
    demo/
```

The complete explanatory file inventories are in each repository's docs-ai/current/currentproject-architecture.md.

The 2026-10-05 naming audit covers all 18 maintained projects and five solutions, not only the six release packages. Existing Core, Blazor, WPF, bridge and Tests/Gallery project names already follow the shared product/role conventions. The WinUI3 Gallery directory/csproj and platform solution now align their casing with Flourish.WinUI3: Gallery.Flourish.WinUI3 and Flourish.WinUI3.slnx. Explicit AssemblyName=Gallery.Flourish.WINUI3 and its existing ArkheideSystem.Gallery.Flourish.WINUI3 namespaces remain unchanged. Prepared package identities and version 1.1.0 are not renamed.

## Source and package modes

The extension projects reference their same-repository Flourish projects and always consume Essential.Culture through PackageReference at 1.3.0. Gallery consumes the Culture generator as a package too. When sibling Essential/artifacts/packages exists, the root props add that folder as a local NuGet source. This is package verification rather than a cross-repository source reference. EssentialCultureRoot, UseLocalEssentialCulture, UseLocalFlourish and UseLocalCultureIntegration are retired. See [NuGet release and integration](../nuget-release-integration.md).

Maintained Flourish projects inherit VersionPrefix=1.1.0, but the first release manifest now contains exactly six Core/Blazor packages. Shared is removed and the Blazor convenience package is added; WPF and its bridge remain in source for future releases. EssentialCultureVersion=1.3.0 sets external Culture dependencies. Essential module props own Culture's version; future modules retain independent version ownership. See the release guide for the authoritative current package set.

New package identities:

| Project | Package | Public integration entry |
| --- | --- | --- |
| Flourish.Extensions.Culture.WPF | Arkheide.Flourish.Extensions.Culture.WPF | UseEssentialCulture() in ArkheideSystem.Flourish.Extensions.Culture.WPF |
| Flourish.Extensions.Culture.Blazor | Arkheide.Flourish.Extensions.Culture.Blazor | AddFlourishCulture() in Microsoft.Extensions.DependencyInjection |

WPF callers must update the old extension namespace/package. Blazor service registration stays the same. The desktop singleton/Dispatcher behavior and scoped Web behavior remain distinct. All six Essential package IDs, assemblies, translation namespaces, XAML helpers and Localizer APIs remain unchanged. Both integrations use packaged Culture 1.3.0, including WPF.

## Preservation and automation

Essential was renamed on disk with its .git directory intact; its original HEAD and remote are unchanged. The GitHub repository still has the historical Arkheide.Essential.Culture name. Renaming the remote repository is not part of this migration. Flourish's existing uncommitted branding and localization work was preserved.

All 23 maintained Extension resources have migration targets. Original Extension files and caches are preserved under Flourish/artifacts/solution-migration/Extension, excluded from Git, with extension-resource-manifest.json recording original/new paths and hashes. This is rollback material, not a third active project root. No recursive deletion was needed.

The two existing Extension README guides were preserved as human documentation under docs/extensions/overview.md and culture-wpf.md, with identity/path corrections. Only the corresponding repository-ownership statements in the existing bilingual DocFX articles were adjusted. No human prose was moved into docs-ai. The bridge's AI guide was migrated to culture-extension-bridge.md. Flourish has no maintained README; Essential retains its existing module/demo README files used by NuGet. No new README was authored.

Both repositories now use .github/workflows/build.yml to build, test, pack and verify their full release sets. Tags use vX.Y.Z and must match their version props. Flourish CI consumes Essential packages; publish all six Culture 1.3.0 packages before the Flourish 1.1.0 release. The user configures each repository's nuget environment, NUGET_USER secret and Trusted Publishing policy. Local preparation does not establish public package availability or confirm that external configuration. scripts/Publish-Helper.ps1 requires a clean master equal to origin/master and explicit tag confirmation before pushing. The former culture-vX.Y.Z extension-only convention is superseded.

## Verification and manual acceptance

Earlier migration verification built both root solutions and the extension module with warnings as errors, passed 1,492 tests/checks and covered the desktop bridges. Those run totals are historical. Current first-release acceptance uses the Blazor-focused solution: Core 367, Blazor 134 and bridge 12 checks, existing JS/CSS checks, six-package validation and 67 isolated package-consumer checks. Desktop source is preserved but not revalidated by this release scope. See release-verification.md for current evidence; no Computer Use is used.

Manual checks:

1. Close old Essential/Extension solution tabs and open the new Flourish.slnx and Essential.slnx. Check their solution folders, focused module entries and test projects.
2. Start Gallery.Flourish.Blazor from the new Flourish solution. On localhost:5188, switch language and navigate; use two browser profiles to confirm separate live selections.
3. For a WPF extension consumer, update its using directive to ArkheideSystem.Flourish.Extensions.Culture.WPF and keep UseEssentialCulture(). Launch the real desktop window and verify runtime locale changes and generated XAML/C# keys.
4. Prepare Essential packages before Flourish, then test a clean NuGet consumer without source references. Follow [the release guide](../nuget-release-integration.md) for local-feed validation and the separately authorized publishing step.
