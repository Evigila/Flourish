# Solution organization

The 2026-10-05 user-authorized migration leaves two active repository roots: Flourish and Essential. Extension is no longer an independent source root or solution. The previous local Culture integration records use the old paths; this document supersedes those paths without editing history.

## Ownership

- Flourish owns the UI frameworks, optional skins, Gallery hosts and UI integrations.
- Flourish.Extensions.Culture.WPF adapts the desktop shell to Essential.Culture.Wpf.
- Flourish.Extensions.Culture.Blazor adapts the neutral ITextProvider contract to Essential.Culture.Blazor.
- Essential owns independent utility modules. Culture is the first module, containing Core, Generator, Wpf, Avalonia, WinUI and Blazor projects.
- Essential libraries have no Flourish reference. Flourish Framework, Design, Abstract and Shared have no Essential reference. Only applications and the optional extension choose the translation implementation.

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

## Source and package modes

The extension projects reference their same-repository Flourish projects and always consume Essential.Culture through PackageReference at 1.3.0. Gallery consumes the Culture generator as a package too. When sibling Essential/artifacts/packages exists, the root props add that folder as a local NuGet source. This is package verification rather than a cross-repository source reference. EssentialCultureRoot, UseLocalEssentialCulture, UseLocalFlourish and UseLocalCultureIntegration are retired. See [NuGet release and integration](nuget-release-integration.md).

All eight Flourish packages, including both extensions, inherit VersionPrefix=1.1.0 from the root props. EssentialCultureVersion=1.3.0 sets the external Culture dependency. Essential root props hold shared organization, license, repository and package-output metadata; Culture module props own VersionPrefix=1.3.0 and the existing package README. The current Culture tests and demos explicitly import those module settings. Future modules own their own version settings rather than inheriting Culture's version from the root.

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

Both root solutions and the extension module build in Release with warnings treated as errors. Culture's five demos and six packages build after migration. All 1,492 tests/checks pass: Flourish Core 367, WPF 901, Blazor 118, WPF extension 5, Blazor extension 12 and Essential 89. Package inspection verifies the original six Culture identities and the two new extension identities. Headless browser checks verify negotiated SSR, independent live cultures, formatting, localized defaults, preserved navigation state, command routing, reload behavior and Framework-only operation, with no page or asset errors. Computer Use was not used.

Manual checks:

1. Close old Essential/Extension solution tabs and open the new Flourish.slnx and Essential.slnx. Check their solution folders, focused module entries and test projects.
2. Start Gallery.Flourish.Blazor from the new Flourish solution. On localhost:5188, switch language and navigate; use two browser profiles to confirm separate live selections.
3. For a WPF extension consumer, update its using directive to ArkheideSystem.Flourish.Extensions.Culture.WPF and keep UseEssentialCulture(). Launch the real desktop window and verify runtime locale changes and generated XAML/C# keys.
4. Prepare Essential packages before Flourish, then test a clean NuGet consumer without source references. Follow [the release guide](nuget-release-integration.md) for local-feed validation and the separately authorized publishing step.
