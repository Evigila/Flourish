# Retire parallel UI documentation authorities

Date: 2026-10-05_141953, America/Sao_Paulo.

The user explicitly requested removal of UI-related standards across Colligere, Flourish and Essential. Flourish repository source, controls and style implementations are now the single UI authority; a second Markdown token/skin specification is not retained. Root rules require exclusively Flourish controls/styles, prohibit host skins/clones, and require missing controls to be skipped, recorded and reported before custom work. Consumers retain business/backend work and approved brand/color-role configuration. Native HTML/POST remains only a host protocol boundary, never a replacement control policy.

## Scope and deleted files

- `docs-ai/common/uiuxdesign.md` — deletion-time SHA256 `3244A39AADE526184C280032B799B74E11D4DA68EFFE5DC7A0002DD477FF1DC7`.
- `docs-ai/current/blazor-color-roles.md` — deletion-time SHA256 `8B1BE65F953A96E48DA64FA88CC7B521E287BDA530CA2EC6CE00EEBBBA2F790A`.
- `docs-ai/current/blazor-extraction.md` — deletion-time SHA256 `E57D993340FF12F5F0B2807F49E1AF2C96DFA4CEEEB9271A0A3265A34687EE77`.
- `docs-ai/current/blazor-manual-tests.md` — deletion-time SHA256 `01F84A150E786A643FC6683DF433F2AC3A22FB37BF3E0FF1540AD4AA55C0DF88`.

Eight verified active UI documents were removed in total: four from Colligere and four from Flourish. Essential already had no common UI standard and retains its technical Blazor adaptation/localization guides. Source/CSS/Razor, human docs/, existing append-only records and bug reports were not deleted. No archive was recreated. Previously deleted README files remain deleted.

Colligere's mixed design document was replaced only for its nonvisual material by business-contracts.md: stable route/PageKey references, identities/member authority, native GET/POST/antiforgery/session boundaries, dirty-form completion, query/pool/worklist scope, transactions and domain facts remain documented without pixel/style prescriptions. Active reading maps and obsolete UI links were updated. Technical Culture/package/runtime guides remain active; visual authority statements inside retained adoption notes now defer to code.

## Recovery and verification

Repository HEAD before retirement: ec31d2684cc147a91f575139495ee621e4f6640d. Committed original documents remain recoverable with git show ec31d2684cc147a91f575139495ee621e4f6640d:<path>. The fenced diff below records every deletion target's uncommitted content difference from that HEAD before deletion: recover the committed file and apply this diff to reconstruct the complete former working version. It is historical evidence only, not an active UI contract. SHA256 values above identify the exact pre-deletion bytes; Git-normalized line endings may differ on reconstruction.

All eight exact targets were resolved inside their respective repository and verified tracked before deletion. The post-change scan must find no active AGENTS mandatory path/count/reading-order requirement for a removed standard, and no active guide link to a removed UI file. Historical references remain untouched. No build, browser/Computer Use, commit, package release, deployment or business request was performed for this documentation-only change. Essential's missing ensure/common bootstrap is a separate pending confirmation; this task does not claim a completed monthly audit or create those files.

## Historical uncommitted differences

```diff
diff --git a/docs-ai/current/blazor-extraction.md b/docs-ai/current/blazor-extraction.md
index 3ff616d..a3cd6f5 100644
--- a/docs-ai/current/blazor-extraction.md
+++ b/docs-ai/current/blazor-extraction.md
@@ -1,33 +1,37 @@
 # Blazor implementation and consumer boundaries
 
-This is the current source guide for the four Blazor libraries, their Gallery and Framework-only verification host. It consolidates the former extraction, rendering-review and project-boundary documents. Historical implementation and run evidence remains in append-only [change records](currentproject-changelogs/) and [bug reports](bugfix-reports/). Use the [manual checklist](blazor-manual-tests.md) for acceptance, the [color contract](blazor-color-roles.md) for paint values and the [directory map](currentproject-architecture.md) for file locations.
+## 2026-10-05 consumer compatibility follow-up
+
+Colligere's NuGet-only integration exposed presentation differences that package graph/SSR smoke tests alone could not find. Primitives.DataTable now offers optional ShowSortControls (default false), including hidden sortable columns in either List or Grade, plus CardValueLines (default one, validated one through eight). Colligere opts into its previously approved two-line/48px cards and independent sort selector. Both Framework-only and Design consumers honor the card-height variables. PrimaryNavigationItem and Icon expose an optional Filled flag; legacy products/customers/orders/invoices/inventory/company names map to existing bundled Material Symbols rather than a missing-icon glyph. The default outlined mode remains unchanged. FormActionBar uses the matching primitive filled UniformGrid, preserving native-button geometry and actual configured column counts. Native filled actions consume primary-ink for configured light seeds. No additional font/package is introduced. The complete six-package preparation and four clean NuGet consumers were rerun after these fixes; see the new change record. Host runtime regressions exercise the options, sorting safeguards and original callback boundaries.
+
+This is the current source guide for the three Blazor libraries, the dependency-only convenience package, their Gallery and Framework-only verification host. It supersedes the former Shared project boundary under the user-authorized 2026-10-05 package consolidation. Historical implementation and run evidence remains in append-only [change records](currentproject-changelogs/) and [bug reports](bugfix-reports/). Use the [manual checklist](blazor-manual-tests.md), [color contract](blazor-color-roles.md) and [directory map](currentproject-architecture.md).
 
 ## Projects, packages and solutions
 
-All four Blazor libraries live under src/Flourish.Blazor/, target .NET 10 and are separately packable at version prefix 1.1.0. Configuration does not establish public-feed publication.
+Abstract, Framework and Design live under src/Flourish.Blazor/, target .NET 10 and are separately packable at version prefix 1.1.0. Flourish.Blazor is a dependency-only convenience package. Shared is retired before the first public release. Configuration does not establish public-feed publication.
 
 | Project / package | Current responsibility | Direct project references |
 |---|---|---|
-| Flourish.Blazor.Shared / Arkheide.Flourish.Blazor.Shared | Navigation/appearance records, control/table/selection/grid metadata, local filter/sort/pagination and mask helpers | Flourish.Core |
-| Flourish.Blazor.Abstract / Arkheide.Flourish.Blazor.Abstract | Shell/navigation/layout/appearance builders and service interfaces, ITablePreferences and TableSortPreference | Shared |
-| Flourish.Blazor.Framework / Arkheide.Flourish.Blazor.Framework | Razor Components, Primitives and Patterns; registration; functional CSS and browser interactions | Abstract, Shared |
-| Flourish.Blazor.Design / Arkheide.Flourish.Blazor.Design | Optional visual foundation/skins, appearance palettes and scoped theme runtime | Framework, Abstract, Shared |
+| Flourish.Blazor / Arkheide.Flourish.Blazor | Dependency-only convenience package; installs contracts, framework and Design without registering services or enabling a theme | Abstract, Framework, Design |
+| Flourish.Blazor.Abstract / Arkheide.Flourish.Blazor.Abstract | Public builders, services, navigation/appearance records and control/table/selection/grid contracts | Flourish.Core |
+| Flourish.Blazor.Framework / Arkheide.Flourish.Blazor.Framework | Razor Components, Primitives and Patterns; registration; functional CSS/browser interactions; local filter/sort/pagination and mask helpers | Abstract |
+| Flourish.Blazor.Design / Arkheide.Flourish.Blazor.Design | Optional visual foundation/skins, appearance palettes and scoped theme runtime | Framework, Abstract |
 
-Framework does not reference Design. The Blazor projects have no WPF, WinUI or Colligere project reference and add no direct external PackageReference. Shared retains Core ApplicationTheme and NotificationSeverity identities without registering desktop hosting. Core's existing configuration, hosting-abstraction and logging-abstraction packages remain transitive dependencies.
+Framework does not reference Design. The three libraries have no WPF, WinUI or Colligere reference and add no direct external PackageReference. Abstract directly references Core for ApplicationTheme and command contracts. Core's existing configuration, hosting-abstraction and logging-abstraction packages remain transitive; hosts do not install Core separately.
 
-The executable projects are Gallery.Flourish.Blazor, Gallery.Flourish.WPF and Gallery.Flourish.WINUI3. Verification projects are Tests.Flourish.Core, Tests.Flourish.WPF, Tests.Flourish.Blazor and Tests.Flourish.Blazor.Native. The root Flourish.slnx contains 14 projects: Core at root, platform groups for libraries/Galleries, Tests for verification and Solutions for the three platform solution files. Each platform solution uses the renamed paths. WPF retains UserSecretsId Gallery.WPF to preserve its existing settings identity.
+The executable projects include the three Gallery hosts. The root Flourish.slnx retains the desktop projects and tests for future work. The Blazor solution now includes Core, the three libraries, the convenience package, the Blazor Culture bridge, Gallery and four verification projects. This is the release solution for the six Core/Blazor packages; WPF and its bridge remain maintained source projects outside this release.
 
 Gallery.Flourish.Blazor is the Framework plus Design host on localhost:5188. Tests.Flourish.Blazor.Native is a Web verification executable under tests on localhost:5189, references Framework alone and loads framework.css without Design. Retain both: they exercise different supported consumers. Native is not a fifth library layer.
 
 ## Public API placement and future boundary work
 
-Namespaces and assemblies are separate concerns. Registration extensions AddFlourishFramework and AddFlourishDesign declare ArkheideSystem.Flourish.Blazor while residing in Framework and Design respectively. AddFlourish remains Framework's obsolete compatibility entry point; new hosts use AddFlourishFramework. There is no current monolithic Flourish.Blazor.csproj.
+Namespaces and assemblies are separate concerns. Registration extensions AddFlourishFramework and AddFlourishDesign retain ArkheideSystem.Flourish.Blazor while residing in Framework and Design. AddFlourish remains Framework's obsolete compatibility entry point. The new Flourish.Blazor.csproj is a package of dependencies only, not a monolithic component assembly.
 
-Builder interfaces live in Abstract and its .Abstract namespace. NavigationItem and AppearanceState have that namespace but compile in Shared; ButtonVariant, SelectOption and TableColumn are Shared public contracts in .Components. Razor components are public Framework types. Shared therefore mixes contracts with implementation helpers; the presence of Abstract does not mean all public contracts already reside there.
+Builder interfaces, NavigationItem, AppearanceState, ButtonVariant, SelectOption, TableColumn and grid/selection contracts now compile in Abstract with their established namespaces and public signatures. Rendering and public local-data/mask helpers compile in Framework. Shared's internal TableData implementation moves into Framework, retaining test visibility.
 
 WPF presently has one library project: Abstract is a directory/namespace within it. Its registration/builders can use the .Abstract namespace while their implementation remains in that assembly; controls expose their own public APIs.
 
-A future boundary change may move public DTOs/enums/interfaces from Shared to Abstract, keep reusable algorithms in Shared and review exposure against consumer requirements. The resulting direction can be Abstract → Core, Shared → Abstract, Framework → both and optional Design → the framework/contracts it needs. This is a recommendation, not an implemented dependency graph. Merging Shared directly into Framework while Abstract still depends on its types would create a cycle; merging it wholly into Abstract would carry implementation algorithms into the contract package. Registration implementation belongs with the classes it composes. A desired registration namespace can change independently, but that migration has not been implemented.
+The implemented direction is Abstract → Core, Framework → Abstract and Design → Framework/Abstract. The convenience package depends on all three Blazor libraries. Culture integration remains a separate optional package depending only on Abstract and Essential.Culture.Blazor. Moving the former Shared types changes assembly identity, so existing binaries and assembly-qualified reflection consumers must rebuild. No Shared compatibility assembly is shipped for the first public release. Framework/Design static asset base paths are unchanged.
 
 ## Framework consumption and optional Design
 
@@ -35,9 +39,9 @@ A native host references Framework and calls AddFlourishFramework. Registration
 
 ApplicationLayout emits the required Framework stylesheet through HeadContent. A custom layout rendering ApplicationShell directly must load _content/Arkheide.Flourish.Blazor.Framework/framework.css. Removing both library stylesheets is not a supported native mode: functional CSS owns accessibility/hidden content, scroll ownership, grids, popup placement, resize targets and responsive navigation.
 
-To opt into Design, reference Design, call AddFlourishDesign separately and load _content/Arkheide.Flourish.Blazor.Design/design.css after Framework. IAppearanceBuilder.SetColors, SetTheme and SetFont configure startup appearance; the former IApplicationBuilder.ConfigureAppearance API is removed. ApplicationLayout consumes the optional scoped IThemeProvider without Framework depending on Design, applies the application Light/Dark/System classes and subscribes to theme changes. Hosts can replace preference storage and supply CSS role hooks.
+The convenience package installs Design along with Framework and Abstract, but never activates it. To opt into Design, call AddFlourishDesign separately and load _content/Arkheide.Flourish.Blazor.Design/design.css after Framework (ApplicationLayout supplies the link when IThemeProvider is registered). A host requiring no Design package can reference Framework alone. IAppearanceBuilder.SetColors, SetTheme and SetFont configure startup appearance; ApplicationLayout applies Light/Dark/System classes and subscribes to scoped changes. Functional CSS remains required.
 
-Native CSS keeps real masked-input text visible, system-colored popup surfaces, relative-size icons, disclosure/switch state and named layout containers. Design owns decorative masks, fixed reading sizes, palettes and skin. There is one application theme; ThemeScope and its Gallery guide were removed. Host inline role variables retain CSS precedence.
+Native CSS keeps real masked-input text visible, system-colored popup surfaces, relative-size icons, disclosure/switch state and named layout containers. Design owns decorative masks, fixed reading sizes, palettes and skin. Its generic legacy aliases map page-gutter to the public gutter token (24px, 20px narrow), field-label-gap to the preserved 7px gap and checkbox-size to the approved 48px control-height token. There is one application theme; host inline variables retain CSS precedence.
 
 ## Program configuration, navigation and scroll ownership
 
@@ -158,7 +162,7 @@ ProgressBar retains numeric/unknown/Stopped semantics, solid Primary fill and th
 
 ## EditingGrid and external consumers
 
-Shared GridContracts defines columns, rows, cells, changes, options, text and editor kinds. Framework owns EditingGrid/GridInteractions and its browser bridge. Stable row keys, unique column keys and exactly one cell per column are required; malformed dimensions are rejected. Editors include Text, Decimal, Date, Multiline, Select, Masked and Link. Raw Value and formatted Display are host inputs.
+Abstract GridContracts defines columns, rows, cells, changes, options, text and editor kinds. Framework owns EditingGrid/GridInteractions and its browser bridge. Stable row keys, unique column keys and exactly one cell per column are required; malformed dimensions are rejected. Editors include Text, Decimal, Date, Multiline, Select, Masked and Link. Raw Value and formatted Display are host inputs.
 
 CellChanged supplies a proposed edit. EditDisabled and per-cell Disabled/ReadOnly/Hidden guard UI edits; link cells retain intended navigation. Primary/secondary error descriptions are encoded and associated with cells/editors. GridInteractions provides edit lifecycle, undo/redo/save, load-more, streamed paste/apply and error callbacks. Host parsing, authorization, drafts, transactions, persistence and cursor semantics stay outside the renderer. MeasurementRevision/resize/disconnect are presentation lifecycle, not business saves.
 
diff --git a/docs-ai/current/blazor-manual-tests.md b/docs-ai/current/blazor-manual-tests.md
index 0860d74..ab61e82 100644
--- a/docs-ai/current/blazor-manual-tests.md
+++ b/docs-ai/current/blazor-manual-tests.md
@@ -11,11 +11,11 @@ This is the current checklist for Gallery, Framework-only consumption and approv
 
 ## Solutions, public boundaries and packages
 
-- Open Flourish.slnx in Visual Studio: Core is at root; platform groups contain libraries/Galleries; Tests has four projects; Solutions has three platform solutions. All 14 projects load without unavailable paths.
+- Open Flourish.slnx and the Blazor solution in Visual Studio. Confirm the retired Shared project is absent, the new dependency-only Flourish.Blazor project loads, and the Blazor solution includes Core, Culture bridge, Gallery and its verification projects. Desktop source projects remain available through the root/platform solutions.
 - Select each intended Gallery startup profile. Confirm Gallery.Flourish.WPF resources/localization and existing user settings, Gallery.Flourish.WINUI3's initial window and Tests.Flourish.Core/WPF discovery. Platform solutions use the renamed project paths.
 - Native retains working forms, masks, menus, dialog/sheet, notices, progress and table with no Design asset. Add Design explicitly in a separate consumer and confirm behavior survives its skin.
 - Pack/consume Framework alone from a local feed in a separate .NET 10 host, then add Design. Verify static assets and dependencies without sibling source paths, copied Gallery CSS or internal APIs. Framework has no Design dependency; the icon font includes license/provenance. Package configuration does not prove public publication.
-- Qualify duplicate Components/Primitives names and check each actual API. Shared still contains both metadata and algorithms; the proposed contract migration is not already implemented.
+- Qualify duplicate Components/Primitives names and check each actual API. Public contracts now come from Abstract, processing helpers from Framework, with established namespaces. Rebuild old consumers rather than retaining Shared.dll. Install the convenience package without AddFlourishDesign and confirm native mode; call AddFlourishDesign separately and confirm its theme activates.
 
 ## Project branding and browser icons
 
```
