# Blazor project boundaries and solution naming

This document records the current dependency and API placement audit after the 2026-10-04 solution naming task. It supplements the directory map; it does not redefine its explanatory tree.

## Implemented naming and solution view

Three executable Galleries use Gallery.Flourish.Blazor, Gallery.Flourish.WPF and Gallery.Flourish.WINUI3. Four test projects use Tests.Flourish.Core, Tests.Flourish.WPF, Tests.Flourish.Blazor and Tests.Flourish.Blazor.Native. Physical project directories/files, assembly names, root/source namespaces, XAML classes/resource identities, linked sources and friend declarations are updated together. Library names/package IDs are unchanged. WPF retains its existing UserSecretsId Gallery.WPF so the project rename does not replace that user-settings identity.

The root Flourish.slnx contains virtual folders Flourish.Blazor, Flourish.WPF and Flourish.WINUI3 with their libraries/Galleries; Flourish.Core is a root project; Tests contains all four test projects; Solutions contains the three platform solution files. The root has 14 projects. Platform mappings are preserved. Each platform solution points to the renamed Gallery/tests.

## Native is a verification host

Tests.Flourish.Blazor.Native is a Web executable under tests, not a fifth library layer. Its project references Framework only; Program registers AddFlourishFramework; App loads framework.css without Design. Its sample exercises a form, menus, dialog/sheet, mask, notices, progress and table. The existing launch address is localhost:5189. It verifies that optional Design can be omitted without losing behavior. Gallery.Flourish.Blazor, on localhost:5188, verifies Framework plus Design. Retain both because they validate different supported consumers.

## Current dependency graph

- Shared references Core.
- Abstract references Shared and the ASP.NET Core framework.
- Framework references Abstract and Shared.
- Design references Framework, Abstract and Shared.
- Gallery directly references Framework and Design; Abstract/Shared are transitive.
- Native directly references Framework; it does not reference Design.

Shared is substantive: navigation/appearance records, component enums, typed selection/table/grid metadata, internal TableData and reusable DataFilter/DataSorter/DataPagination/InputMaskFormatter logic. It currently mixes public contracts and implementation helpers.

## Namespace and assembly are independent

Gallery Program imports ArkheideSystem.Flourish.Blazor because both registration extensions declare that namespace. There is no current monolithic Flourish.Blazor.csproj at that location. AddFlourishFramework is implemented by Framework/ServiceCollectionExtensions.cs; AddFlourishDesign by Design/DesignServiceCollectionExtensions.cs.

Builder interfaces such as IFrameworkBuilder, ITopBarBuilder and ISubNavigationBuilder are in the Abstract assembly and its .Abstract namespace. NavigationItem/AppearanceState have that namespace but are physically compiled in Shared. ButtonVariant, SelectOption and TableColumn are public contracts in Shared with the .Components namespace. Razor component types are public Framework types. Thus the user's expectation of concentrated contracts is not fully met by current physical boundaries, despite the existing Abstract project.

WPF presently has one Flourish.WPF.csproj; Abstract is a directory/namespace within it, not a separate project. Hosting/ApplicationBuilder.cs and Hosting/ServiceCollectionExtensions.cs declare ArkheideSystem.Flourish.Abstract. WPF controls also have public APIs under .Controls; not every public type is in Abstract.

## Recommended next boundary change, not implemented here

Retain the requested four library projects. Move public DTOs/enums/interfaces from Shared to Abstract; keep reusable implementation logic in Shared and reduce public exposure where consumer requirements permit. Then Abstract can depend on Core, Shared can depend on Abstract when its algorithms need contracts, Framework can depend on both, and optional Design can depend on the framework/contracts it uses. This avoids Abstract depending on an implementation assembly.

Directly merging current Shared into Framework while preserving Abstract's dependency on its types would create a cycle. Merging it wholly into Abstract would carry algorithms into the contract package. Either merge would require a deliberate type/API migration, not merely project-file edits.

Registration methods must compose implementation classes and can remain in Framework/Design. If the desired Program experience is the WPF-style Abstract import, those extensions may declare the .Blazor.Abstract namespace while remaining in their implementing assemblies. Moving their implementation into the Abstract assembly would require backward references to Framework/Design, create cycles and compromise optional Design. That namespace change has not been made by this naming task; existing calls remain compatible.

## Verification and manual acceptance

The root's 14 projects and the Blazor platform solution build with zero warnings/errors. Core has 367 passing tests, WPF 901 and Blazor 102, plus 9 palette checks. All 77 Gallery guide routes retain source/default sections and both CSS bundles; the renamed Native host renders controls and loads Framework without a Design asset.

Initial Core/WPF runs from external temporary outputs failed repository discovery because their existing tests walk upward from AppContext.BaseDirectory. Rerunning from ignored repository artifacts fixed discovery without changing those helpers. A separate old WPF assertion listed only Core/WPF packages; it now exactly lists the existing six Core/WPF/Blazor library packages. No new package is added.

Reload Flourish.slnx in Visual Studio, confirm the requested root groups and no unavailable projects, select the renamed startup project, and run its existing profile. Check Gallery's sample source/copy pages, Native's unthemed controls and WPF localization/resources. Test Explorer should display Tests.Flourish.Core and Tests.Flourish.WPF. WINUI3 compiles but its initial window remains a manual smoke check. Human-maintained docs/roadmap.md still names historical Gallery paths; ownership rules leave it unchanged.

