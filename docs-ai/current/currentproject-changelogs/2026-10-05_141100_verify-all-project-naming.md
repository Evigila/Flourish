# Verify all maintained project names

## Scope and mapping

User-authorized solution/project naming migration, preserving existing public namespaces, explicit RootNamespace/AssemblyName, all prepared NuGet package identities and runtime/UI behavior.

All 18 maintained projects were inspected. Ordinary Core, Blazor, WPF and WinUI3 libraries, both Product.Extensions.Culture bridges, six Tests projects and three Galleries already use product-first or approved role-prefix naming. Only one Gallery project's casing needed alignment with its actual Flourish.WinUI3 target:

- src/Gallery.Flourish.WINUI3/Gallery.Flourish.WINUI3.csproj becomes src/Gallery.Flourish.WinUI3/Gallery.Flourish.WinUI3.csproj.
- src/Flourish.WinUI3/Flourish.WINUI3.slnx becomes src/Flourish.WinUI3/Flourish.WinUI3.slnx.
- Root solution folder casing becomes /Flourish.WinUI3/; root/platform solution references and current directory guide were updated.

The Gallery still emits Gallery.Flourish.WINUI3.dll and retains ArkheideSystem.Gallery.Flourish.WINUI3 namespaces/XAML class identities. No library, bridge, WPF project, source class or UI component was renamed or changed.

## Verification and limits

- Static validation: 18 maintained csproj files, 22 ProjectReference paths and five solutions resolve.
- Root restore/Release build with warnings as errors: exit 0, zero warnings and errors; includes WPF, WinUI3 and all Galleries.
- Core 367 and WPF Culture bridge five automated tests pass. WPF: 900 pass, one existing directory-shape failure because BackgroundTasks, Localization and Shell/StatusBar contain no maintained files and are absent on this checkout. The required-directory assertion and WPF sources remain unchanged.
- Blazor 135 checks and Culture bridge 12 checks pass.
- Existing six 1.1.0 prepared packages pass inspection without repack. Framework SHA256 remains 77C780A168A8EA21DA3C4D306408285127324CEFFA26A70DE557AC5BEDB82DBA.
- Test evidence: artifacts/test-results/project-naming. Dirty changes were preserved; moves used verified exact native PowerShell paths within the repository, with unoccupied temporary destinations.

No dependencies, API identities, translation behavior or library UI behavior changed. No overwrite, broad deletion, commit, tag, push or publication occurred. Open the root and four focused solutions, verify Gallery startup names, and launch the existing desktop Galleries manually if desktop UI acceptance is required; no Computer Use testing was performed.
