# currentproject-architecture

## 2026-10-09 DisplayBoard preview alignment

Framework DisplayBoard owns centering for direct preview action rows when the enclosing board is centered and no InlineActions alignment was supplied. InlineActions marks explicit parameter presence internally so the shared rule respects explicit Start/Center/End while retaining ordinary page End defaults. The direct-child rule excludes nested pages, forms and dialogs and preserves full-row sizing. No Gallery-specific skin or second layout API is introduced. This supersedes only the earlier same-day decision to remove DisplayBoard action-row centering. See [the correction report](bugfix-reports/2026-10-09_display-board-action-alignment.md).

## 2026-10-09 Culture extension ownership and configuration separation

The user requested a later same-day redesign of the initial browser-persistence implementation. The old AddFlourishCulture, AddFlourishPreferences, Framework BrowserPreferences and Gallery LanguagePicker are removed without aliases; the dated persistence report remains evidence of that superseded design. ConfigureCulture now extends IFrameworkBuilder from the optional Flourish.Extensions.Culture.Blazor project. Framework supplies only the generic ConfigureServices(Action<IServiceCollection, IConfiguration>) registration hook and remains free of Essential/Culture dependencies.

The Culture extension owns catalog stream loading, the internal Essential session, scoped CultureSession, renderer subscriptions, standard request negotiation through IStartupFilter, browser culture persistence and LanguagePicker. Its Razor project depends on Framework and the existing Essential.Culture.Blazor package; Framework depends on Abstract, so this optional integration introduces no cycle. Essential remains the single lookup/formatting core. SelectAsync and SelectFormatAsync serialize the complete save-before-apply transaction, return false on browser/connection/cancellation failure and retain the old pair. Personal choices remain independent by browser/application base path; the standard cookie restores both identifiers before SSR. Core's desktop singleton JSON store is not reused for Web users.

Gallery service registration has three entries: AddFlourishFramework(builder.Configuration, ...ConfigureCulture...), AddFlourishDesign(builder.Configuration) and AddScoped<RecordStore>(). Gallery owns project/navigation/command configuration, application catalog keys, routes, examples and business data; library registrations own their technical setup. Framework automatically adds interactive Razor services in a Web host and loads optional appsettings.Flourish.json below all existing host/business/environment/command-line sources. Non-ConfigurationManager inputs receive file defaults overlaid by the host snapshot. Design reads its own appearance defaults. Gallery appsettings.json remains empty and available for business/host settings. Neither JSON file is rewritten by browser language choices. Theme/palette, keyed control presentation and optional navigation preferences remain follow-up candidates. See [Culture Web integration](culture-web-integration.md) for current APIs and the distinction from historical verification.

## 2026-10-09 PageBody centered-container ownership

Blazor PageBody now owns the Standard/Expanded centered-container choice through CenteredContainer. Expanded halves each horizontal gutter for its own page, including minimum gutters. Framework resets the scale on every PageBody and Design uses it for matching heading/content margins. NavigationSurface still forwards the configured reference width but its former CenteredContentGutterScale is removed. PageBody.CompactSpacing and its vertical CSS/checkbox are removed, with ordinary vertical rhythm restored. Fluid, FullWidth and FillHeight retain their precedence and behavior. Historical release notes preserve the old APIs as release evidence, not current contracts. Native WPF has a separate older PageBody implementation without responsive centered geometry; that parity debt remains outside this Blazor/Gallery correction.

## 2026-10-09 single card summary entry

The user authorized removing IdentityCard because Card already covers its content, identity and record summary capabilities. Card is the maintained Blazor and native WPF entry; consumers compose semantic facts and ordinary actions through its existing content slots. The duplicate component, Gallery/navigation/API registration and identity-only styles are removed without aliases. The native control catalogue now contains 74 entries. Earlier 1.1.2 IdentityCard release evidence remains historical and no longer describes the current API. The 1.1.4-preview ChangeLog records the duplication reason.

## 2026-10-07 Native WPF reconstruction

The user authorized complete deletion of the former WPF solution implementation and requested current Flourish.Blazor source as the sole UI/UX authority. Four native projects follow Abstract → Framework → optional Design; a dependency-only aggregate installs all three. The native target remains net10.0-windows. The former WPF host/services and legacy branded controls are removed. Core, WinUI and the six-package public Blazor release remain outside this reconstruction.

There are 74 explicit native control entries and executable Gallery factories after the 2026-10-09 card consolidation. The four Standalone native-POST input splits and browser ApplicationLayout merge into native capabilities without aliases. Consumers own business data, acceptance and persistence; Framework owns geometry, selection, keyboard, focus and popup lifetimes. The optional Culture adapter implements the new desktop ITextProvider and uses the current Blazor library catalog directly. See [native WPF integration](wpf-native-integration.md) for use, platform differences, verification and manual acceptance. This supersedes earlier descriptions of the old WPF framework in active guides and dated records.

Human DocFX configuration still names the deleted single-project WPF path. This discrepancy is reported rather than modifying docs/ without a task-scoped exception.

## 2026-10-06 Authorized identity access and full space editor release

The complete gate, Trusted Publishing and public indexing are now complete for all six 1.1.2 packages. Four fresh public-only consumers passed 129 checks. Source commit 108fbff8950925b909a70b0b9830226ae8226521 is tagged v1.1.2; see release-verification.md. The older dated source-only statements below are superseded for these released capabilities, not ongoing consumer blockers.

Blazor 1.1.2 collects the previously deferred source repairs under the user's explicit publication/upgrade authorization. Existing PageBody owns full-height editing and compact short-form spacing; UniformGrid optionally fixes Rectangle cell height; IdentityCard owns its semantic heading and available width; ordinary Button owns structured labels; PresentationFooter supports per-instance identity; NavigationChoices exposes its existing compact button variants. Native access spacing shares one controls.js lifetime through the already-used input-behaviors scanner. Contracts, Gallery, three-language descriptions and real regressions remain paired. See [the release repair report](bugfix-reports/2026-10-06_identity-access-and-editor-release.md). Earlier dated source-only decisions are superseded for these queued fixes, not erased.

## 2026-10-06 Surface headings and form widths

ApplicationShell, NavigationSurface and fixed-stage ContentSurface share all compact PageHeading structural rules. Direct Field-owned ReferenceDropdown fills its field, and start-aligned DisplayBoard fills standard form containers. Gallery and production-controller regressions exercise the same entries. The [report](bugfix-reports/2026-10-06_surface-heading-and-form-width.md) records successful source verification and the user's explicit publication deferral: Colligere remains on public 1.1.1 until a future release and upgrade.

## 2026-10-06 Organization access presentation correction

The existing split PresentationHero/Primary AccessFormSurface/Compact NavigationChoices composition is now adopted by Colligere's resolved tenant entry. Shared Section spacing, paired Field foreground, Quiet availability paint, operation-action widths and split mobile typography are corrected in the library. Standard control paint remains in controls.css and geometry in Framework; no host skin or new specialized API is required. The [report](bugfix-reports/2026-10-06_organization-access-presentation-corrections.md) supersedes the preceding access-style-gate limitation, records 28 passing Node checks and distinguishes successful production package compilation from the current catalog/localization full-verification blockers.

## 2026-10-06 Secondary label parity and chart heading

Direct SecondaryNavigationItem now uses the configured shell's shrinking f-navigation-label and complete title. LineChart Heading/Id supplies a standard Section H2/anchor with ControlsContent and the owned icon-only display selector in one right-aligned header row. These are library rendering capabilities, not host skin/state adapters. The [follow-up report](bugfix-reports/2026-10-06_secondary-label-parity-and-chart-heading.md) corrects the preceding audit's incomplete configured/direct entry comparison and records the remaining full-style-gate limitation.

## 2026-10-06 Workspace navigation and inline controls

SecondaryNavigationItem uses canonical is-selected and Foundation paint; NavigationSurface no longer adds competing secondary-link paint. SectionNavigator/BackToTop share the DocumentKey lifetime of the replaced main region. LineChart.ControlsContent composes business filters beside its owned display selector; InlineActions supports input plus naturally sized Button inside Field. Gallery and real rendering/lifecycle/CSS regressions exercise these capabilities. See [the report](bugfix-reports/2026-10-06_navigation-selection-and-content-lifetime.md).

## 2026-10-06 account-entry and wizard composition correction

Generic SplitButton.PrimaryDisabled independently locks the primary action while retaining its secondary menu. Disabled/Busy still lock both parts; native form/token attributes remain protocol responsibilities. Gallery documents and demonstrates the capability. Its finite-choice WizardExample retains PageHeading on every step: only the choice area omits extra explanatory titles/selectors. See [the report](bugfix-reports/2026-10-06_split-button-primary-availability.md).

This document explains the maintained directory and file tree from the repository root. Every listed node has a short responsibility description. Flourish owns framework libraries, optional Design libraries and Culture bridges. External Culture dependencies are NuGet packages; application pages and business data remain in their consuming hosts.

## Current Blazor boundary after the 2026-10-06 breaking refactor

Framework has one interactive record entry: Components.DataTable with Abstract TableColumn/TableSearchRequest, shared TableData and Components.TablePreferences. Primitives.DataTable/DataSearch and their independent contracts, processing helpers and old styles are deleted. ListView is the static comparison scenario; EditingGrid remains a distinct spreadsheet with editing-grid-columns.js, not a second record-list contract. Display ordering acts on whole items; sorting is exposed only through headers, not a toolbar.

The active API is AddFlourishFramework and its project/top-bar/navigation/layout builders, plus the generic extension service hook. Optional ConfigureCulture belongs to the Culture extension; AddFlourishDesign independently activates Design. Retired application/title/group builders, NavigationGroups shell adapters, independent Culture/preference registrations, icon/button/grid aliases, ThemePalette and CSS theme aliases are removed. The current component inventory has only General, Scenario and BuildingBlock classifications. See [the current API guide](component-api-organization.md) for supported entry points and ownership.

Current Framework composition entries include FormGroup.razor, InlineActions.razor, ImagePreview.razor, AttributionFooter.razor and CopyText.razor. The later user-authorized generic-control convergence removes ConfirmationHost/ConfirmationService and ReconnectDialog instead of retaining a renamed host or adapter. Dialog now owns awaiting results, cancellation and browser-controlled keyed views; business and protocol composites instantiate it directly with ordinary Button. Components.Dialog owns lazy/retained content and the one native top-layer lifecycle, including asynchronous row-action focus restoration through primitives/interaction-origin.js. The retired bottom-sheet.js controller is deleted. A native reconnect protocol composite uses Dialog BrowserControlled, DialogView, Notice, ProgressBar and Button. The host owns protocol identifiers and state transitions; controls.js owns view visibility, modality and focus. The reconnect-specific stylesheet is deleted; DialogResultChecks/DialogViewChecks cover the shared lifecycles.

Gallery adds Components/Pages/ShellExample.razor at /examples/shell. The configured application shell is a standalone document, not an embedded copy inside another shell. Login, account-selection and product/Pricing examples use current production entries and fictitious business data. NoCompatibilityApiChecks.cs rejects retired public contracts; DialogResultChecks.cs and DialogViewChecks.cs verify the real shared Dialog and Button behavior. Existing API, interop, dropdown and lifecycle checks use only current contracts.

Browser baseline checks require current native Dialog/Popover, ResizeObserver and the respective observer/media APIs. No old browser portal, modal-backdrop or clipboard execCommand renderer is selected. Current responsive/reduced-motion behavior and readable SSR remain supported. ReferenceDropdown uses Value/ValueChanged; ShellHeader and both layout Patterns use Class. Desktop projects and their platform-specific behavior are outside this Blazor refactor and are not included in the 1.1.0 package set.

## General control convergence on 2026-10-06

The [generic-control audit](generic-control-convergence.md) identifies nine removed wrappers/scenarios and remaining groups needing genuine generic capabilities before consolidation. Public component coverage is now 80, with 695 parameter rows and 311 defaults. Confirmation and circuit interruption use the actual general Dialog; BottomSheet is only its Presentation value. StandaloneTextBox owns numeric native text, ContentSurface/PresentationBand compose full-height documents, and PageBody/PageHeading/Section compose lists.

Gallery adds Components/Pages/ReconnectExample.razor at /examples/overlays/reconnect and wwwroot/reconnect-example.js. The example only maps fictional states to the library's controls.js helpers; it does not clone modality or call a server. SampleFor.DescriptionKey identifies localized technical notes and moves technical notes outside the shared DisplayBoard.

The subsequent Gallery string-binding repair corrects NavigationChoicesSample and copied API examples without changing the production control contract. build/Test-GalleryNavigation.ps1 supplies a loopback-only real-endpoint regression for current links, retained panels, query fallbacks, repeated requests and related sample values. See [the diagnosis](bugfix-reports/2026-10-06_144432_gallery-navigation-choice-literal-key.md).

## Three-language Blazor catalog ownership on 2026-10-06

Gallery and Framework each maintain one Localization/Culture.json source with en-US, zh-CN and pt-BR under every key. Gallery has 1,483 application/documentation keys and generates Texts.Key accessors; Framework has 281 provider-neutral caption/usage keys and retains the embedded logical name Flourish.Blazor.Texts.json. The optional bridge resolves both registered catalogs through the request/circuit-scoped Essential adapter. Gallery's language picker and request UI negotiation support all three languages. See [the integration guide](culture-web-integration.md) for generator, desktop lifetime and preference boundaries.

## Restored split access composition on 2026-10-06

PresentationHero.SideContent and StackTitleWords provide the library-owned asymmetric organization/operation layout and accessible stacked title; AccessFormSurface.Tone chooses one operation surface, and NavigationChoices.Compact uses ordinary equal-width Button links. Gallery's /examples/display/access-methods directly composes these existing entries without host CSS, preserving the organization/account/pass query keys. The 860px/560px responsive geometry comes from the verified original Colligere layout at 5394774; that consumer was read only. The current Gallery catalog contains 1,483 complete three-language keys. See [the active API guide](component-api-organization.md) for the public contracts.

## Maintained tree

The 2026-10-06 banner/wizard/grid follow-up changes the shared PresentationBand minimum to 800px and the existing UniformGrid CSS to full-container rectangular tracks while retaining bounded squares. Gallery adds Components/Pages/WizardExample.razor at /examples/display/wizard, with localized finite choices using actual UniformGridButton and no new control/API family. See [the accepted change record](currentproject-changelogs/2026-10-06_100324_refine-banner-default-and-grid-choice-scenes.md). The API guide and source catalogs document current shape/scene behavior.

The subsequent [semantic-scene audit](bugfix-reports/2026-10-06_shared-display-and-production-scenes.md) introduced shared display selection, NavigationChoices and ValidationMessages. The user-authorized 2026-10-06 follow-up explicitly supersedes that audit's DisplayOptions/MultiSelectDropdown separation: Framework Components/MultiSelectBox.razor and MultiSelectOption.cs now provide the one general production selector and MultiSelectChange snapshot. DataTable and LineChart map display members to that entry with an explicit localized Display label and enabled ordering. Framework owns wwwroot/multi-select-box.js/css, Design owns optional wwwroot/multi-select-box.css paint, and the retired display-options assets and MultiSelectDropdown renderer/styles are deleted. Field still delegates error rendering to ValidationMessages. Gallery has one Inputs/MultiSelectBoxSample, adjacent to SelectBox, and its EditingGrid composite cell reuses the same general selector. The API guide explains current contracts; earlier audit history is retained with this explicit supersession.

The tree omits all docs/ and docs-ai/ directories, version-control internals, IDE state, generated builds and caches (bin/, obj/, artifacts/, TestResults/, .packages/, packages/, node_modules/, x64/, x86/ and ARM64/), local agent state and archived directories. Empty directories without maintained files are omitted.

The maintained tree below has been pruned against current paths after the 2026-10-06 breaking Blazor refactor. New convergence entries are described in the current boundary section below; generated package/cache evidence remains outside this maintained tree.

```text
Flourish/ — Repository root for framework libraries, optional extensions, Gallery hosts and verification.
├── .config/ — Pins repository-local development tools.
│   └── dotnet-tools.json — Pins DocFX for documentation and CSharpier for code formatting.
├── .github/ — Contains repository automation configuration.
│   └── workflows/ — Defines GitHub Actions workflows.
│       ├── build.yml — Builds, tests and verifies the six Core/Blazor release packages; matching version tags publish through the nuget environment.
│       └── docs.yml — Builds and publishes the existing DocFX documentation site.
├── build/ — Contains maintained CSS build tasks and integration verification scripts.
│   ├── BundleCss.cs — Expands local CSS imports, detects invalid paths and creates deterministic bundles.
│   ├── CssBundle.targets — Runs the CSS bundler during project builds and static asset generation.
│   ├── Test-BlazorPackageConsumers.ps1 — Restores isolated NuGet-only consumers and verifies registration, SSR and locally published static assets.
│   ├── Test-CssAssets.ps1 — Checks framework and design asset paths and optional theme boundaries.
│   ├── Test-CssBundle.ps1 — Checks deterministic bundling, import resolution and packaging outputs.
│   ├── Test-CultureCatalogs.ps1 — Validates both Blazor source catalogs for complete three-language keys, placeholders and line breaks.
│   ├── Test-CulturePreferences.ps1 — Checks saved UI/format pairs, defaults, request isolation and browser module delivery on a loopback Gallery host.
│   ├── Test-ChangeLog.ps1 — Requires localized release notes for stable tags and one next-patch preview before publication.
│   ├── Test-GalleryCulture.ps1 — Checks loopback SSR language negotiation, application/control text and formatting in all three languages.
│   ├── Test-GalleryNavigation.ps1 — Checks loopback sample navigation, retained panels, repeated requests and query fallbacks.
│   ├── Test-WpfPackageConsumers.ps1 — Restores four isolated native NuGet-only consumers from the local candidate feed.
│   └── Test-CultureIntegration.cjs — Checks live Gallery language changes, independent browser sessions, formatted values and the Framework-only host with a headless browser.
├── script/ — Contains local documentation preview commands.
│   ├── preview-docs-en-us.ps1 — Builds and serves the existing English DocFX site locally.
│   └── preview-docs-zh-cn.ps1 — Builds and serves the existing Chinese DocFX site locally.
├── scripts/ — Contains allowlisted local application startup, release preparation, package inspection and user-confirmed tag publishing commands.
│   ├── Convert-MaterialIcons.ps1 — Converts existing Blazor WOFF2 artwork through Windows DirectWrite into native outline/filled resources.
│   ├── Publish-Helper.ps1 — Prepares packages and optionally validates clean master, confirms and pushes the matching release tag.
│   ├── Release-Common.ps1 — Loads release settings, reads versions, runs checked commands and validates release tag ancestry.
│   ├── ReleaseSettings.psd1 — Lists the release solution, version source, ordered package IDs, dependencies, required assets and checks.
│   ├── Start-Common.ps1 — Validates runnable-project selections and constructs checked restore/build/run arguments without shell-generated commands.
│   ├── Start-Project.ps1 — Selects a local Gallery or diagnostic host, verifies the existing SDK and launches Release by default.
│   ├── StartSettings.json — Declares the four runnable project identities, paths, platform kinds and existing HTTP launch profiles.
│   ├── Test-Release.ps1 — Builds and tests Release, packs every configured library and verifies the resulting package set.
│   ├── Test-StartProject.ps1 — Checks launcher plans, failure exit codes, allowlists, SDK-free discovery and batch behavior without starting applications.
│   └── Verify-PackageSet.ps1 — Checks exact package identities, versions, dependencies and required packaged assemblies and assets.
├── start.bat — Calls the repository startup script through Windows PowerShell without a Visual Studio launcher.
├── src/ — Contains framework libraries, optional extensions and runnable Gallery applications.
│   ├── Flourish.Blazor/ — Groups three Blazor libraries, the dependency-only convenience package and the release solution.
│   │   ├── Flourish.Blazor/ — Defines the dependency-only convenience package with opt-in service activation.
│   │   │   └── Flourish.Blazor.csproj — Installs Abstract, Framework and Design and restores Core transitively, with no packaged assembly.
│   │   ├── Flourish.Blazor.Abstract/ — Exposes public shell, text-provider, appearance, control and grid contracts.
│   │   │   ├── Components/ — Defines public control and table metadata independent of rendering implementations.
│   │   │   │   ├── CenteredContainer.cs — Defines Standard and Expanded centered PageBody widths.
│   │   │   │   ├── ControlContracts.cs — Defines button/dialog variants, menu actions and typed select options.
│   │   │   │   ├── PresentationTone.cs — Defines Canvas, Surface and Primary color-role choices for presentation bands.
│   │   │   │   ├── TableContracts.cs — Defines columns, row actions, sorting/view choices and localizable captions.
│   │   │   │   ├── UniformGridShape.cs — Defines rectangular and square cell shapes.
│   │   │   │   └── UniformGridVariant.cs — Defines the unique Elevated, FilledElevated and Danger cell paint variants.
│   │   │   ├── Primitives/ — Defines specialized selection and editing-grid models; interactive record contracts live in Components.
│   │   │   │   ├── GridContracts.cs — Describes editing-grid columns, rows, cells, proposed edits and captions.
│   │   │   │   └── SelectionContracts.cs — Defines service links and typed reference/selection models.
│   │   │   ├── ApplicationData.cs — Defines navigation, top-bar, placement and appearance records in their established namespaces.
│   │   │   ├── ApplicationContracts.cs — Declares shell/appearance contracts, optional text references and a provider-neutral extension registration callback.
│   │   │   ├── Flourish.Blazor.Abstract.csproj — Defines the public Blazor contracts package with a direct Core dependency.
│   │   │   ├── ITablePreferences.cs — Declares keyed sort preferences scoped to one user/circuit.
│   │   │   └── TextContracts.cs — Declares catalog-qualified text references and the scoped text provider contract without selecting a translation library.
│   │   ├── Flourish.Blazor.Design/ — Provides optional theme services and visual styles.
│   │   │   ├── Hosting/ — Composes startup options and services with the application host.
│   │   │   │   ├── AppearanceService.cs — Publishes runtime theme and palette changes as scoped appearance state and CSS variables.
│   │   │   │   └── DesignOptions.cs — Stores and validates the optional theme's startup colors, font and theme selection.
│   │   │   ├── wwwroot/ — Contains optional palette, typography and control skin assets.
│   │   │   │   ├── patterns/ — Contains optional skin styles for composed page surfaces.
│   │   │   │   │   ├── content-surface.css — Styles content surfaces, page headings and section spacing.
│   │   │   │   │   └── navigation-surface.css — Styles navigation surfaces, active items and top bar appearance.
│   │   │   │   ├── primitives/ — Contains optional skin styles for primitive components.
│   │   │   │   │   ├── DataPager.css — Styles pagination buttons and the page summary.
│   │   │   │   │   ├── EditingGrid.css — Styles editable cells and keeps popup colors independent of cell selection.
│   │   │   │   │   ├── FieldControl.css — Styles shared input fields, validation messages and focus states.
│   │   │   │   │   ├── InteractionBoundary.css — Styles keyboard and pointer focus origin states.
│   │   │   │   │   ├── MaskedInput.css — Styles masked inputs and their validation state.
│   │   │   │   │   ├── NoticeTrigger.css — Colors circular status indicators and their attached explanations.
│   │   │   │   │   ├── PrimaryNavigationItem.css — Styles primary item hover, pressed, active and disabled states.
│   │   │   │   │   ├── primitives.css — Imports the optional theme styles for primitive components.
│   │   │   │   │   ├── ReferenceDropdown.css — Styles reference selection, search and option states.
│   │   │   │   │   ├── SearchAutocomplete.css — Styles search suggestions, active options and query input states.
│   │   │   │   │   ├── ServiceMenu.css — Styles the rounded header menu trigger and its options.
│   │   │   │   │   ├── ShellHeader.css — Styles branding, identity and menu zones in the top bar.
│   │   │   │   │   ├── StandaloneMaskedInput.css — Styles mask-aware inputs outside a form field wrapper.
│   │   │   │   │   ├── SystemNavigationMenu.css — Styles system navigation menu items and states.
│   │   │   │   ├── multi-select-box.css — Paints the general multi-selection control through shared palette roles.
│   │   │   │   ├── controls.css — Defines button, input, card, floating-control and production Notice appearance.
│   │   │   │   ├── data.css — Defines themed table, sorting, selection and row-action appearance.
│   │   │   │   ├── data-search.css — Normalizes the canonical search Field font weight within the library.
│   │   │   │   ├── design.css — Imports the optional theme's foundation, control, table and composition styles.
│   │   │   │   ├── display-board.css — Styles preview board dots, code backgrounds, copy controls and board spacing.
│   │   │   │   ├── dropdown.css — Styles shared triggers, native pickers, menu options and unavailable actions.
│   │   │   │   ├── expansion-indicator.css — Styles shared triangle geometry, rotation timing and reduced-motion behavior.
│   │   │   │   ├── form-inputs.css — Styles standard date and file inputs and read-only input presentation.
│   │   │   │   ├── foundation.css — Defines fifteen color roles, text sizes, spacing, radius and shadows.
│   │   │   │   ├── layout.css — Defines page, form, section and responsive composition styles.
│   │   │   │   ├── line-chart.css — Styles chart series and axes with the existing semantic theme roles.
│   │   │   │   ├── native-overrides.css — Applies the optional theme to native form and focus presentation.
│   │   │   │   ├── presentation.css — Styles full-width bands, artistic heroes, wordmark footers, offer cards and access panels with shared Design color roles.
│   │   │   │   ├── scrollbars.css — Provides thin transparent scrollbar tracks, narrow thumbs and hidden arrow buttons.
│   │   │   │   ├── section-navigator.css — Styles section dots, active links and section-label tooltips.
│   │   │   │   ├── split-button.css — Styles both split-button actions and their shared selection and focus states.
│   │   │   │   ├── static-surfaces.css — Styles static server-rendered surfaces and shared form boundaries with Design tokens.
│   │   │   │   └── uniform-grid.css — Styles shared grid cells, title and body roles, interaction states and filled surfaces.
│   │   │   ├── _Imports.razor — Imports component namespaces and Razor directives for descendant files.
│   │   │   ├── AppearancePalette.cs — Validates configurable palette seeds and selects fixed dark defaults without deriving colors.
│   │   │   ├── AssemblyInfo.cs — Allows the Blazor test project to inspect theme internals.
│   │   │   ├── DesignServiceCollectionExtensions.cs — Registers optional per-scope appearance and reads its startup defaults from supplied configuration.
│   │   │   ├── Flourish.Blazor.Design.csproj — Defines the optional Razor theme package and bundled visual assets.
│   │   ├── Flourish.Blazor.Framework/ — Implements controls, shell behavior and browser interop.
│   │   │   ├── Components/ — Contains public shell, page, input, action and data components.
│   │   │   │   ├── Internal/ — Contains internal rendering helpers shared by public controls.
│   │   │   │   │   ├── TableData.cs — Filters, sorts, formats and pages loaded component table data.
│   │   │   │   │   └── UniformGridContent.cs — Builds the shared title, icon, text and content structure for grid cells.
│   │   │   │   ├── Patterns/ — Contains composed navigation, content and record-list page surfaces.
│   │   │   │   │   ├── ContentSurface.razor — Composes a title bar and content in an explicit business-scroll or document-flow mode, with an optional footer slot.
│   │   │   │   │   ├── NavigationSurface.razor — Composes a top bar, two navigation levels and page content.
│   │   │   │   ├── Primitives/ — Contains detailed dropdown, grid, input and page composition components.
│   │   │   │   │   ├── _Imports.razor — Imports component namespaces and Razor directives for descendant files.
│   │   │   │   │   ├── AccessBrand.razor — Renders a logo and application identity for an entry page.
│   │   │   │   │   ├── DataPager.razor — Renders controls for selecting a loaded result page.
│   │   │   │   │   ├── EditingGrid.razor — Renders editable cells and bridges keyboard, selection and clipboard operations.
│   │   │   │   │   ├── GridInteractions.cs — Bridges grid keyboard and clipboard requests to host callbacks.
│   │   │   │   │   ├── InputBehaviorBinding.cs — Loads shared browser input behavior through the JavaScript module cache.
│   │   │   │   │   ├── InteractionBoundary.razor — Tracks interaction origin and groups related focus behavior.
│   │   │   │   │   ├── MaskedInput.razor — Provides mask-aware input editing and validation.
│   │   │   │   │   ├── NavigationGuard.razor — Intercepts navigation and asks the host whether leaving is allowed.
│   │   │   │   │   ├── NoticeTrigger.razor — Associates a named status glyph with its hover and focus explanation.
│   │   │   │   │   ├── PrimaryNavigationItem.razor — Renders a primary destination with active, hover and disabled states.
│   │   │   │   │   ├── ReferenceDropdown.razor — Selects one reference with search, keyboard navigation and independent popup width.
│   │   │   │   │   ├── SearchAutocomplete.razor — Displays suggestions while a user enters a search query.
│   │   │   │   │   ├── SecondaryNavigationItem.razor — Renders a secondary route link with prefix matching and explicit current-page semantics.
│   │   │   │   │   ├── ServiceMenu.razor — Displays a dropdown of application destinations.
│   │   │   │   │   ├── ShellHeader.razor — Displays logo, application name, menus and identity actions.
│   │   │   │   │   ├── StandaloneMaskedInput.razor — Provides a mask-aware input outside a form's field bindings.
│   │   │   │   ├── AccessActions.razor — Arranges standard buttons for access-entry and small presentation action groups.
│   │   │   │   ├── AccessFormSurface.razor — Organizes access-page fields and submit controls without creating a form or authentication behavior.
│   │   │   │   ├── AccessPanel.razor — Provides a centered access panel with brand, content and action slots, without creating another main element.
│   │   │   │   ├── ActionMenu.razor — Shows click or hover action menus with dismissal and keyboard focus handling.
│   │   │   │   ├── ApplicationLayout.razor — Connects routed content to the shell and outputs stylesheet links and the configured tab icon.
│   │   │   │   ├── ApplicationShell.razor — Renders top bar content, route links and separate branch disclosure buttons.
│   │   │   │   ├── BackToTop.razor — Composes a standard icon button with native fragment fallback and shared region-scrolling lifecycle.
│   │   │   │   ├── Button.razor — Exposes ordinary action buttons with visual variants, busy states and click behavior.
│   │   │   │   ├── Card.razor — Groups ordinary content, identity and record summaries with semantic body content and actions.
│   │   │   │   ├── CheckBox.razor — Renders a bound checkbox with optional label wrapping and a preserved boolean submission value.
│   │   │   │   ├── CodeBlock.razor — Displays escaped code text in a formatted block with a language label.
│   │   │   │   ├── ContentContainer.razor — Centers and constrains content independently from an outer full-width background.
│   │   │   │   ├── DataSearch.razor — Composes canonical columns and search requests with standard fields and a same-row filter slot.
│   │   │   │   ├── DataTable.razor — Owns canonical browsing, retained editing purposes, native transports and bounded progressive SSR enhancement.
│   │   │   │   ├── DateBox.razor — Wraps typed native date input with binding, disabled state and standard field semantics.
│   │   │   │   ├── DialogView.cs — Defines a keyed content view shared by controlled and browser-owned dialogs.
│   │   │   │   ├── Dialog.razor — Displays modal content with focus handling and dismissal behavior.
│   │   │   │   ├── Disclosure.razor — Expands and collapses content below a labelled trigger.
│   │   │   │   ├── DisplayBoard.razor — Displays preview content or formatted code with an explicit copy action.
│   │   │   │   ├── DropdownSurface.razor — Renders a native details-based menu with a supplied trigger and child actions.
│   │   │   │   ├── EmptyState.razor — Displays a title, explanation and optional action when no data is available.
│   │   │   │   ├── ExpansionIndicator.razor — Displays the decorative expansion triangle from a host-controlled state.
│   │   │   │   ├── Field.razor — Associates a field label, required state and validation errors with its nested input.
│   │   │   │   ├── FilePicker.razor — Wraps native file selection with a visible optional label, accepted types and upload change notification.
│   │   │   │   ├── FormActions.razor — Groups the actions at the end of a form.
│   │   │   │   ├── FormLayout.razor — Composes form content, sections and its action area.
│   │   │   │   ├── Icon.razor — Renders the official Material Symbols Outlined name through one icon catalog.
│   │   │   │   ├── InputSemantics.cs — Combines field identity, required state, invalid state and error descriptions with explicit native attributes.
│   │   │   │   ├── LineChart.razor — Renders stable decimal series with theme roles, explicit scales and accessible exact values.
│   │   │   │   ├── ListView.razor — Renders static read-only tabular lists with shared columns, formatting, cell templates and accessible row headers, without JavaScript or table operations.
│   │   │   │   ├── LoadingState.razor — Displays a loading indicator and supporting message.
│   │   │   │   ├── MultiSelectBox.razor — Owns the general multi-selection menu, complete validated snapshots, search/creation and optional ordering.
│   │   │   │   ├── MultiSelectOption.cs — Defines stable option keys, selection constraints and complete MultiSelectChange snapshots.
│   │   │   │   ├── Notice.razor — Displays a semantic information, success, warning or error message.
│   │   │   │   ├── NumberBox.razor — Renders typed bound numeric input with field validation and input/change event selection.
│   │   │   │   ├── OfferCard.razor — Renders an identified offer title, description and standard actions, registering with its containing offer stage.
│   │   │   │   ├── OfferStage.razor — Groups responsive offer cards, localized pause/resume actions and scoped progressive carousel registration/disposal.
│   │   │   │   ├── PageBody.razor — Hosts vertically stacked page content in its scrolling viewport.
│   │   │   │   ├── PageHeading.razor — Displays a page title beneath its optional parent link and aligned actions.
│   │   │   │   ├── PresentationBand.razor — Composes a full-width color-role background with centered headings, actions and content.
│   │   │   │   ├── PresentationFooter.razor — Renders a presentation footer with brand, copyright, decorative watermark and standard links.
│   │   │   │   ├── PresentationHero.razor — Renders artistic product headings, subtitles, ordinary description text and standard actions within a centered track.
│   │   │   │   ├── ProgressBar.razor — Displays determinate or indeterminate progress along a bar.
│   │   │   │   ├── ProgressRing.razor — Displays circular determinate or indeterminate progress.
│   │   │   │   ├── SearchBox.razor — Edits a search query and exposes search and clear actions.
│   │   │   │   ├── Section.razor — Renders a titled content section with stable heading and anchor identities.
│   │   │   │   ├── SectionNavigator.razor — Shows supplied or discovered section links beside the host content.
│   │   │   │   ├── SelectBox.razor — Renders typed bound selections with native attributes and standard field semantics.
│   │   │   │   ├── SplitButton.razor — Combines independent primary and secondary actions with shared selection and disabled state.
│   │   │   │   ├── StandaloneCheckBox.razor — Renders a standalone checkbox with native POST value and explicit native outer-label composition.
│   │   │   │   ├── StandaloneSelectBox.razor — Renders typed native selections for standalone callbacks and static POST forms.
│   │   │   │   ├── StandaloneTextBox.razor — Renders native text input for static POST forms or callbacks without requiring a binding expression.
│   │   │   │   ├── TextBox.razor — Renders bound text or multiline input with native attributes, field semantics and input/change event selection.
│   │   │   │   ├── TextComponentBase.cs — Refreshes localized components and gives explicit text parameters precedence over library defaults.
│   │   │   │   ├── TextInputBase.cs — Provides scoped text lookup and localized parsing defaults for bound input components.
│   │   │   │   ├── ToggleSection.razor — Combines an enable switch with optional section content.
│   │   │   │   ├── ToggleSwitch.razor — Edits a binary setting through a switch control.
│   │   │   │   ├── UniformGrid.razor — Arranges cells with independent shape, appearance and automatic or explicit row and column layouts.
│   │   │   │   ├── UniformGridButton.razor — Exposes an action or link cell with appearance, title, icon, text, busy state and content slot.
│   │   │   │   └── UniformGridItem.razor — Displays a passive cell with appearance, title, icon, text and optional content.
│   │   │   ├── Hosting/ — Composes startup options and services with the application host.
│   │   │   │   ├── ApplicationOptions.cs — Collects immutable shell options, validates identity/assets and freezes extension registration callbacks.
│   │   │   │   ├── CommandRuntime.cs — Keeps command registrations and dispatch inside the current Blazor scope.
│   │   │   │   └── LiteralTextProvider.cs — Provides literal fallback text and current formatting when no optional Culture bridge is registered.
│   │   │   ├── Icons/ — Contains the bundled icon name catalog and its embedded font codepoint map.
│   │   │   │   ├── IconCatalog.cs — Lists official Material Symbols Outlined names and maps names to font codepoints.
│   │   │   │   └── MaterialSymbolsOutlined.codepoints — Maps official Material Symbols Outlined names to the codepoints embedded in the Framework assembly.
│   │   │   ├── Localization/ — Contains library-owned text catalogs independent of the selected translation provider.
│   │   │   │   └── Culture.json — Contains the framework's English, Chinese and Brazilian Portuguese control text.
│   │   │   ├── Primitives/ — Contains public local-data and mask processing implementations.
│   │   │   │   ├── DataPagination.cs — Tracks paging within an already loaded result set.
│   │   │   │   ├── InputMaskFormatter.cs — Formats and normalizes mask-aware input values.
│   │   │   │   └── NoticePresentation.cs — Maps semantic severity to CSS classes and accessible roles.
│   │   │   ├── wwwroot/ — Contains structural CSS, JavaScript behavior and the default project logo.
│   │   │   │   ├── icons/ — Contains locally served icon font assets.
│   │   │   │   │   └── material/ — Contains the pinned Google Material Symbols Outlined font, license and source metadata.
│   │   │   │   │       ├── LICENSE.txt — Preserves the Apache 2.0 license for the bundled Google icon assets.
│   │   │   │   │       ├── MaterialSymbolsOutlined.woff2 — Provides the self-hosted Material Symbols Outlined variable font used by the unique Icon renderer.
│   │   │   │   │       └── source.json — Records the upstream revision, license, file sizes and SHA-256 hashes for the bundled icon assets.
│   │   │   │   ├── multi-select-box.css — Defines multi-selection option, search and popup structure.
│   │   │   │   ├── multi-select-box.js — Owns selection snapshots and optional drag/keyboard order through the shared disclosure lifecycle.
│   │   │   │   ├── patterns/ — Contains structural styles and browser measurements for page surfaces.
│   │   │   │   │   ├── content-surface.css — Defines content stage, title and section layout behavior.
│   │   │   │   │   ├── navigation-surface.css — Defines navigation surface geometry, collapsed states and responsive behavior.
│   │   │   │   │   └── surfaces.js — Observes surface dimensions and reuses stable heading compaction and page-scroll reset behavior.
│   │   │   │   ├── presentation/ — Contains structural presentation layout and progressive offer behavior without Design paints.
│   │   │   │   │   ├── layout.css — Defines centered content tracks, full-width bands, responsive offers, access panels and action groups.
│   │   │   │   │   └── offers.js — Coordinates scoped offer anchors, rotation, focus, pointer and reduced-motion pauses, and listener cleanup.
│   │   │   │   ├── primitives/ — Contains primitive browser behavior and structural CSS.
│   │   │   │   │   ├── behavior.css — Defines primitive visibility, positioning and accessibility rules without a theme.
│   │   │   │   │   ├── editing-grid.js — Handles cell ranges, keyboard movement, editing and clipboard requests.
│   │   │   │   │   ├── EditingGrid.css — Defines editable grid cell sizing, selection geometry and functional layout.
│   │   │   │   │   ├── input-behaviors.js — Attaches native input selection and editing behavior at document level.
│   │   │   │   │   ├── interaction-origin.js — Tracks keyboard and pointer focus origin and remembers transient invokers.
│   │   │   │   │   ├── native.css — Keeps primitive form controls usable with browser-native appearance.
│   │   │   │   │   ├── navigation-guard.js — Warns before external navigation while unsaved changes exist.
│   │   │   │   │   ├── primary-navigation-item.js — Tracks primary navigation pointer and pressed states.
│   │   │   │   │   ├── reference-dropdown.js — Determines whether a dropdown still contains keyboard focus.
│   │   │   │   ├── browse.svg — Provides the default white Material Symbols browse logo and tab-icon fallback.
│   │   │   │   ├── clipboard.js — Copies text on request and restores focus and selection after a clipboard fallback.
│   │   │   │   ├── controls.js — Positions and dismisses action/display popups, handles menu keys, modal focus and input selection.
│   │   │   │   ├── back-to-top.css — Positions and centers the standard icon button without a second raw button skin.
│   │   │   │   ├── data-search.css — Owns canonical field layout and same-row filter spacing without broad descendant input/label overrides.
│   │   │   │   ├── data-table.css — Defines canonical retained-row, template, ordering and progressive view structures.
│   │   │   │   ├── data.js — Owns canonical sizing, display popup, unique native opening and visible-text progressive directory enhancement.
│   │   │   │   ├── display-board.css — Defines display board alignment, code overflow and copy-button placement without a visual theme.
│   │   │   │   ├── expansion-indicator.css — Defines native triangle content and expanded orientation without a visual theme.
│   │   │   │   ├── form-inputs.css — Styles standard date and file inputs and read-only input presentation.
│   │   │   │   ├── framework.css — Defines functional layouts, control states and the self-hosted icon font face without applying a visual theme.
│   │   │   │   ├── layout.css — Defines structural page and form layout without themed colors or typography.
│   │   │   │   ├── line-chart.css — Defines chart geometry and accessible exact-value table structure.
│   │   │   │   ├── section-navigator.css — Positions section links and provides native focus and tooltip behavior.
│   │   │   │   ├── section-navigator.js — Discovers headings, positions gutter links and synchronizes scrolling and cleanup.
│   │   │   │   ├── shell.js — Synchronizes navigation and page headings without repeating collapse when short documents clamp scrolling.
│   │   │   │   ├── split-button.css — Defines split-button alignment, independent action regions and label truncation.
│   │   │   │   └── uniform-grid.css — Defines responsive grid tracks, optional dimensions and child-cell shapes.
│   │   │   ├── _Imports.razor — Imports component namespaces and Razor directives for descendant files.
│   │   │   ├── AssemblyInfo.cs — Allows the Blazor test project to inspect framework internals.
│   │   │   ├── ComponentUsageCatalog.cs — Registers every exported component as a general, scenario or building-block entry with explicit production scope.
│   │   │   ├── Flourish.Blazor.Framework.csproj — Defines the Razor Framework package, embeds its text catalog and icon map, and includes licensed static font assets without a Culture dependency.
│   │   │   └── ServiceCollectionExtensions.cs — Loads low-priority framework defaults, registers behavior/commands/text and Web-host Razor services, then executes extension callbacks.
│   │   └── Flourish.Blazor.slnx — Defines the Core/Blazor release solution including the meta package, Culture bridge, Gallery and verification projects.
│   ├── Flourish.Core/ — Contains platform-independent configuration, commands and runtime state services.
│   │   ├── Abstract/ — Defines public interfaces, options and immutable state contracts.
│   │   │   ├── ApplicationTheme.cs — Defines the requested light, dark or system theme.
│   │   │   ├── BackgroundTaskContracts.cs — Declares background task definitions, state snapshots and runtime operations.
│   │   │   ├── CommandContracts.cs — Declares command keys, context, results, registration and dispatch contracts.
│   │   │   ├── ContentLayoutContracts.cs — Declares page width, alignment and scrolling configuration and runtime state.
│   │   │   ├── IDataBuilder.cs — Declares locale, culture-file and settings-path configuration.
│   │   │   ├── ISettingsEditor.cs — Declares edits inside one owned settings transaction.
│   │   │   ├── ISettingsStore.cs — Declares atomic transactions for the application's owned JSON settings.
│   │   │   ├── IThemeService.cs — Declares runtime selection of the application theme.
│   │   │   ├── LocalizationContracts.cs — Declares culture registration, translation lookup and runtime locale changes.
│   │   │   ├── MotionContracts.cs — Declares motion settings, transition types and runtime changes.
│   │   │   ├── NavigationMenuContracts.cs — Declares navigation items, groups and transactional menu changes.
│   │   │   ├── NavigationPanelDirection.cs — Defines which side of the shell hosts navigation.
│   │   │   ├── NavigationPanelState.cs — Stores an immutable snapshot of navigation panel settings.
│   │   │   ├── NotificationContracts.cs — Declares notification content, actions and immutable runtime state.
│   │   │   ├── PageCacheMode.cs — Defines whether navigation reuses page instances.
│   │   │   ├── ProfileContracts.cs — Declares profile identity, authentication requests and runtime state.
│   │   │   ├── ProjectContracts.cs — Declares project metadata, selection and runtime operations.
│   │   │   ├── SettingsUpdateResult.cs — Reports whether an owned settings transaction was saved.
│   │   │   ├── ShellRegion.cs — Names the shell positions that can host application content.
│   │   │   ├── ShortcutConflictPolicy.cs — Defines how duplicate shortcut gestures are resolved.
│   │   │   ├── ShortcutScope.cs — Defines the lifetime and resolution scope of a keyboard shortcut.
│   │   │   ├── StateContracts.cs — Defines runtime registration ownership and common immutable state contracts.
│   │   │   ├── StatusBarContracts.cs — Declares status item definitions, configuration and runtime operations.
│   │   │   ├── TitleBarContracts.cs — Declares title bar identity, search, breadcrumbs and runtime operations.
│   │   │   ├── ToolbarStateContracts.cs — Declares toolbar definitions, snapshots and transactional changes.
│   │   │   ├── ToolTipContracts.cs — Declares tooltip settings and runtime policy operations.
│   │   │   └── WindowCloseContracts.cs — Declares guarded shell close requests and close behavior.
│   │   ├── Assets/ — Stores packaged framework artwork or built-in localization resources.
│   │   │   └── FlourishCulture.Json — Contains built-in framework culture strings.
│   │   ├── BackgroundTasks/ — Manages background work, cancellation and progress state.
│   │   │   └── BackgroundTaskService.cs — Queues cancellable tasks and publishes their progress and lifecycle.
│   │   ├── Commands/ — Registers action keys and dispatches their handlers.
│   │   │   ├── CommandDispatcher.cs — Resolves registered commands, checks availability and invokes handlers.
│   │   │   └── CommandParserHostedService.cs — Registers command parser mappings when the host starts and disposes them on stop.
│   │   ├── Configuration/ — Validates startup options and manages owned persistent settings.
│   │   │   ├── ApplicationConfigurationPath.cs — Resolves and checks configuration paths owned by the application.
│   │   │   ├── ApplicationDataOptions.cs — Stores localization files and the settings file selected at startup.
│   │   │   ├── AppPreferenceService.cs — Reads and atomically saves the application's owned settings section.
│   │   │   ├── AppSettingsConfigurationSource.cs — Loads the configured JSON settings into host configuration.
│   │   │   ├── BuilderMutationGuard.cs — Rejects builder changes after startup configuration is frozen.
│   │   │   ├── DataBuilder.cs — Selects localization files, default locale and settings persistence.
│   │   │   ├── PreferenceConfigurationKeys.cs — Names the settings paths owned by runtime preferences.
│   │   │   └── ValueValidation.cs — Checks platform-neutral configuration values.
│   │   ├── Hosting/ — Composes startup options and services with the application host.
│   │   │   ├── CoreServiceCollectionExtensions.cs — Registers platform-independent services and their public interface aliases.
│   │   │   └── CoreServiceOptions.cs — Carries configured shared options into a platform's composition root.
│   │   ├── Layout/ — Implements content sizing, alignment and scrolling policy.
│   │   │   ├── ContentLayoutService.cs — Publishes page centering and maximum width changes.
│   │   │   ├── LayoutBuilder.cs — Configures content centering and smooth scrolling.
│   │   │   └── LayoutOptions.cs — Stores content width, alignment and scrolling defaults.
│   │   ├── Localization/ — Provides culture data, translation lookup and localization tests.
│   │   │   ├── LocaleKeys.cs — Names built-in localization entries.
│   │   │   └── LocalizationService.cs — Loads culture files and resolves localized strings and format arguments.
│   │   ├── Messaging/ — Provides notifications and modal message behavior.
│   │   │   └── NotificationService.cs — Shows, updates and dismisses versioned notification snapshots.
│   │   ├── Motion/ — Provides startup and runtime animation policy.
│   │   │   ├── MotionBuilder.cs — Configures whether motion runs and which page and navigation transitions apply.
│   │   │   └── MotionOptions.cs — Stores the shared motion defaults selected at startup.
│   │   ├── Navigation/ — Manages destinations, menu state, panel geometry and navigation history.
│   │   │   ├── NavigationMenuStateService.cs — Publishes immutable navigation menu snapshots after transactions.
│   │   │   ├── NavigationStackEntry.cs — Stores a navigation key and its opaque parameter in history.
│   │   │   └── PageHistoryService.cs — Owns bounded back and forward navigation stacks.
│   │   ├── Profile/ — Manages profile identity, remembered credentials and presentation state.
│   │   │   ├── IProfileCredentialStore.cs — Defines persistence and removal of protected profile credentials.
│   │   │   ├── ProfileOptions.cs — Stores profile naming and sign-in persistence preferences.
│   │   │   ├── ProfileService.cs — Coordinates sign-in, sign-out, remembered credentials and profile changes.
│   │   │   ├── SimpleProfileAuthService.cs — Provides the built-in simple profile sign-in implementation.
│   │   │   └── StoredProfileCredentials.cs — Stores and validates the serialized remembered identity.
│   │   ├── Projects/ — Manages project metadata, persistent catalogs and active selection.
│   │   │   ├── ProjectBuilder.cs — Configures optional multi-project shell behavior.
│   │   │   ├── ProjectCatalogStore.cs — Loads and saves project order, metadata and active selection.
│   │   │   ├── ProjectOptions.cs — Stores project catalog settings selected at startup.
│   │   │   └── ProjectService.cs — Manages project metadata and selection with persistent transactions.
│   │   ├── Shell/ — Groups services and views for application chrome.
│   │   │   ├── StatusBar/ — Owns status item configuration and runtime state.
│   │   │   │   ├── StatusBarBuilder.cs — Configures status items and optional network and power indicators.
│   │   │   │   ├── StatusBarOptions.cs — Stores the initial status bar items and feature switches.
│   │   │   │   └── StatusBarService.cs — Publishes status bar changes and updates optional system indicators.
│   │   │   ├── TitleBar/ — Owns branding, search, breadcrumbs and title bar state.
│   │   │   │   ├── TitleBarOptions.cs — Stores the configured title bar content and feature switches.
│   │   │   │   ├── TitleBarRuntimeFacade.cs — Combines title bar identity and search state behind the public API.
│   │   │   │   ├── TitleBarSearchService.cs — Publishes search text, placeholder and visibility changes.
│   │   │   │   ├── TitleBarSearchState.cs — Stores an immutable snapshot of the title bar search field.
│   │   │   │   └── TitleBarService.cs — Publishes application title, logo and title bar state changes.
│   │   │   └── Toolbar/ — Owns default and per-view action groups and toolbar state.
│   │   │       ├── ToolbarStateOptions.cs — Stores platform-neutral default and per-view toolbar items.
│   │   │       └── ToolbarStateService.cs — Publishes immutable toolbar state for the active view.
│   │   ├── ToolTips/ — Owns contextual help timing and placement policy.
│   │   │   ├── ToolTipBuilder.cs — Configures tooltip timing and placement policy.
│   │   │   └── ToolTipOptions.cs — Stores configured tooltip delays and presentation switches.
│   │   ├── Windowing/ — Owns platform window state, close behavior and tray integration.
│   │   │   └── WindowCloseService.cs — Runs ordered close guards before invoking the attached platform close action.
│   │   ├── AssemblyInfo.cs — Allows platform adapters and tests to access shared internal services.
│   │   └── Flourish.Core.csproj — Defines the platform-independent .NET library and its service dependencies.
│   ├── Flourish.Extensions/ — Groups optional integrations owned and packaged by Flourish.
│   │   ├── Flourish.Extensions.Culture.Blazor/ — Owns optional request/circuit localization and personal browser culture selection.
│   │   │   ├── Localization/ — Contains extension-owned selector, feedback and usage translations.
│   │   │   │   └── Culture.json — Embeds the English, Chinese and Brazilian Portuguese Culture catalog.
│   │   │   ├── wwwroot/ — Contains the extension's browser persistence asset.
│   │   │   │   └── browser-preferences.js — Writes/verifies the standard culture cookie with the application base path and retention.
│   │   │   ├── ComponentUsageCatalog.cs — Registers LanguagePicker as a reviewed Scenario production entry.
│   │   │   ├── CultureBuilder.cs — Validates/freezes application catalogs, default/supported cultures and browser retention.
│   │   │   ├── CultureFrameworkExtensions.cs — Implements ConfigureCulture, automatic library catalogs and internal request-localization startup.
│   │   │   ├── CultureSession.cs — Adapts Essential text and serializes persistent UI/format selections within one request/circuit scope.
│   │   │   ├── LanguagePicker.razor — Composes SelectBox, Dialog and Notice for either UI-language or formatting selection.
│   │   │   ├── LocalizedComponentBase.cs — Subscribes/disposes scoped culture changes through the component renderer.
│   │   │   ├── _Imports.razor — Imports existing Flourish contracts/controls for extension components.
│   │   │   └── Flourish.Extensions.Culture.Blazor.csproj — Defines the optional Razor extension with Framework and existing Essential.Culture.Blazor dependencies.
│   │   └── Flourish.Extensions.Culture.WPF/ — Optional desktop Essential adapter for the current native ITextProvider.
│   │       ├── EssentialCultureBuilderExtensions.cs — Disposable provider and FrameworkBuilder integration.
│   │       └── Flourish.Extensions.Culture.WPF.csproj — References WPF Abstract/Framework and existing Culture.Wpf 1.3.0.
│   ├── Flourish.WinUI3/ — Contains the WinUI3 library project placeholder.
│   │   ├── Flourish.WinUI3.csproj — Defines the WinUI3 library placeholder and Windows App SDK dependency.
│   │   └── Flourish.WinUI3.slnx — Groups the WinUI3 placeholder library and its Gallery host.
│   ├── Flourish.WPF/ — Native port of the current Blazor controls; the former WPF framework is retired.
│   │   ├── Flourish.WPF.Abstract/ — Native public contracts without rendering or Core service dependencies.
│   │   │   ├── ApplicationContracts.cs — Immutable shell configuration and navigation factories.
│   │   │   ├── ControlContracts.cs — Shared variants, actions and selection/dialog snapshots.
│   │   │   ├── TableContracts.cs — Record, editing-grid, search, bulk-edit and chart contracts.
│   │   │   ├── TextContracts.cs — Catalog-qualified text and desktop provider lifetime.
│   │   │   └── Flourish.WPF.Abstract.csproj — Packages the Windows-native contracts assembly.
│   │   ├── Flourish.WPF.Framework/ — Native controls, templates, geometry and interaction lifetimes.
│   │   │   ├── Controls/ — Implements the explicit native control inventory.
│   │   │   │   ├── ApplicationShell.cs — Responsive native navigation, async guards, text and focus lifetime.
│   │   │   │   ├── Button.cs — Shared ordinary/grid button contracts, structured text and busy state.
│   │   │   │   ├── Content.cs — Shared presentation, access, heading and internal layout/animation helpers.
│   │   │   │   ├── Data/ — One record core with distinct editing/chart scenarios.
│   │   │   │   │   ├── DataPager.cs — Native local/remote paging and bounded page input.
│   │   │   │   │   ├── DataPresentation.cs — Shared native record/card cells and retained custom templates.
│   │   │   │   │   ├── DataSearch.cs — One shared typed column-search renderer.
│   │   │   │   │   ├── DataTable.cs — Header sorting, search, display snapshots, selection and bulk proposals.
│   │   │   │   │   ├── DataText.cs — Optional provider captions and desktop subscription cleanup.
│   │   │   │   │   ├── EditingGrid.cs — Native spreadsheet editing, atomic proposals and undo/redo.
│   │   │   │   │   ├── LineChart.cs — Native finite chart geometry, series display and ranges.
│   │   │   │   │   ├── ListView.cs — Read-only display sharing the table data/column core.
│   │   │   │   │   └── TableData.cs — Culture-aware local search, stable sorting and numeric comparisons.
│   │   │   │   ├── Dialog.cs — Modal lifecycle, keyed views, dropdown and notice surfaces.
│   │   │   │   ├── Forms.cs — Shared binding/external validation and native form/action layouts.
│   │   │   │   ├── Icon.cs — Native existing artwork with inherited foreground and decorative automation.
│   │   │   │   ├── InputMaskFormatter.cs — Current Blazor mask normalization reused without alternate semantics.
│   │   │   │   ├── Inputs.cs — Native text/date/number/reference/search controls and async autocomplete.
│   │   │   │   ├── Menus.cs — Shared action/split menus and complete-key multi-selection snapshots.
│   │   │   │   ├── OfferStage.cs — Retained offers, responsive activation, timed rotation and keyboard/pointer state.
│   │   │   │   ├── Progress.cs — Native progress ranges/drawing and file selection boundary.
│   │   │   │   └── UniformGrid.cs — Responsive rectangle/square tracks and outer clipping.
│   │   │   ├── Themes/ — Native templates; Design contains no duplicate templates.
│   │   │   │   ├── Actions.xaml — Shared buttons, menus, tooltips and scroll templates.
│   │   │   │   ├── Content.xaml — Shared presentation, access, heading, offer and progress templates.
│   │   │   │   ├── Data.xaml — Shared table/search/pager/edit/chart templates.
│   │   │   │   ├── Generic.xaml — Source-assembly native default resource entry.
│   │   │   │   ├── Inputs.xaml — Native input/selection templates and detached popups.
│   │   │   │   ├── Resources.xaml — Framework-only system palette and layout constants.
│   │   │   │   └── Shell.xaml — Desktop/narrow navigation and overlay geometry.
│   │   │   ├── Icons/ — Existing licensed Material Symbols artwork converted to native resources.
│   │   │   │   ├── IconCatalog.cs — Cached native glyph outlines and frozen filled vectors.
│   │   │   │   ├── LICENSE.txt — Existing artwork's Apache-2.0 license, included in the package.
│   │   │   │   ├── MaterialSymbolsFilled.json.br — Compressed native filled geometry.
│   │   │   │   ├── MaterialSymbolsOutlined.codepoints — Existing official name-to-glyph mappings.
│   │   │   │   ├── MaterialSymbolsOutlined.ttf — Converted native variable font resource.
│   │   │   │   └── source.json — Original artwork identity and reproducible conversion evidence.
│   │   │   ├── AssemblyInfo.cs — Source-assembly WPF theme discovery.
│   │   │   ├── ComponentUsageCatalog.cs — Explicit General, Scenario and BuildingBlock classification.
│   │   │   ├── Converters.cs — Internal native template value converters.
│   │   │   ├── FrameworkBuilder.cs — Configures project, top bar and route factories.
│   │   │   ├── FrameworkResources.cs — Loads native templates and shares live resources with detached popups/windows.
│   │   │   ├── Motion.cs — Inherited reduced-motion policy.
│   │   │   └── Flourish.WPF.Framework.csproj — Depends only on Abstract and packages native resources.
│   │   ├── Flourish.WPF.Design/ — Optional current Blazor role palette expressed as native resources.
│   │   │   ├── AppearancePalette.cs — Computes approved primary/accent color roles.
│   │   │   ├── DesignResources.cs — Produces native Light, Dark, System and high-contrast resources.
│   │   │   ├── ThemeSession.cs — Owns runtime theme/color updates and system-event cleanup.
│   │   │   └── Flourish.WPF.Design.csproj — Depends on Abstract/Framework; introduces no independent control templates.
│   │   ├── Flourish.WPF/ — Dependency-only convenience package.
│   │   │   └── Flourish.WPF.csproj — Installs Abstract, Framework and Design without enabling Design automatically.
│   │   └── Flourish.WPF.slnx — Groups four WPF packages, Gallery, optional Culture bridge and native tests.
│   ├── Gallery.Flourish.Blazor/ — Hosts the Blazor Gallery and its demonstration pages.
│   │   ├── Commands/ — Contains Gallery-only command key mappings.
│   │   │   └── GalleryCommandParser.cs — Maps Gallery menu and theme command keys to scoped handlers.
│   │   ├── Components/ — Contains Razor components composed by this project.
│   │   │   ├── Catalog/ — Contains the shared component guide and its scoped presentation styles.
│   │   │   │   ├── ComponentGuide.razor — Composes source-owned usage guidance, preferred production links, API tables and compiled examples.
│   │   │   │   └── ComponentGuide.razor.css — Constrains guide width and spaces the API parameter area.
│   │   │   ├── Pages/ — Contains routed Gallery pages or verification pages.
│   │   │   │   ├── ChangeLog.razor — Selects localized release notes at /changelog with primary navigation only.
│   │   │   │   ├── AccessExamples.razor — Demonstrates independent login and saved-account pages with standard controls, local errors/busy/empty states and no authentication service.
│   │   │   │   ├── Appearance.razor — Shows Foundations topics, a three-cell rectangular theme chooser and standard palette action buttons.
│   │   │   │   ├── AppearanceRedirect.razor — Redirects the former appearance route to the Foundations landing page.
│   │   │   │   ├── Controls.razor — Separates production scenarios from construction helpers using current component contracts.
│   │   │   │   ├── DisplayExamples.razor — Demonstrates localized fictitious product, pricing and access pages using full-width presentation components and centered controls, without authentication or billing.
│   │   │   │   ├── Error.razor — Displays an error page and its diagnostic request identifier.
│   │   │   │   ├── Examples.razor — Separates example links, form creation, enum dropdowns and linked search into topic pages.
│   │   │   │   ├── Forms.razor — Demonstrates form composition, binding, validation and save feedback.
│   │   │   │   ├── Framework.razor — Separates setup, top bar, navigation, commands, layout and interactions into Framework topic pages.
│   │   │   │   ├── Icons.razor — Links official Material Symbols resources and shows the searchable catalog and icon usage.
│   │   │   │   ├── Localization.razor — Demonstrates live translation, separate formatting culture, framework default labels and registration code.
│   │   │   │   ├── NotFound.razor — Displays a missing-page message and a link back to Gallery.
│   │   │   │   ├── Overview.razor — Displays four starting-page links in an automatically wrapping rectangular button grid and the minimal setup code.
│   │   │   │   ├── RecordDetails.razor — Demonstrates a record detail page, editable fields and navigation actions.
│   │   │   │   ├── Records.razor — Demonstrates record search, filtering, paging and row actions.
│   │   │   │   └── SurfacePatterns.razor — Demonstrates the advanced Registry table scenario with standard fields, actions, notices and controlled bottom sheets.
│   │   │   ├── Samples/ — Contains 97 standalone Razor examples compiled and embedded in Gallery for live guides and source display.
│   │   │   │   ├── Content/ — Contains examples of cards, icons, facts, grids and expandable content.
│   │   │   │   │   ├── CardSample.razor — Demonstrates ordinary, prominent and identity summary cards through the shared entry.
│   │   │   │   │   ├── CodeBlockSample.razor — Demonstrates escaped code text and selectable code language labels.
│   │   │   │   │   ├── DisclosureSample.razor — Demonstrates initial open states and expandable supporting content.
│   │   │   │   │   ├── DisplayBoardSample.razor — Demonstrates preview and code boards with variant selection and local action feedback.
│   │   │   │   │   ├── IconSample.razor — Demonstrates named Material Symbols and a local icon action.
│   │   │   │   │   ├── SectionSample.razor — Demonstrates titled content sections and section-level actions.
│   │   │   │   │   ├── UniformGridItemSample.razor — Demonstrates passive information cells and local status changes.
│   │   │   │   │   └── UniformGridSample.razor — Demonstrates independent cell shapes and appearances with automatic and explicit grid layouts.
│   │   │   │   ├── Data/ — Contains examples of actions, data views, modal surfaces, feedback and progress.
│   │   │   │   │   ├── ActionMenuSample.razor — Demonstrates click or hover menus, disabled states and local action callbacks.
│   │   │   │   │   ├── ButtonSample.razor — Demonstrates button variants, disabled states, icons and asynchronous local actions.
│   │   │   │   │   ├── DataPagerSample.razor — Demonstrates primitive pagination states and paging through a local collection.
│   │   │   │   │   ├── DataTableSample.razor — Demonstrates empty states, list and card views, and local record actions.
│   │   │   │   │   ├── DialogSample.razor — Demonstrates modal opening, guarded closing and a local asynchronous action.
│   │   │   │   │   ├── DropdownSurfaceSample.razor — Demonstrates only the native details construction boundary and directs ordinary command menus to ActionMenu.
│   │   │   │   │   ├── EditingGridSample.razor — Demonstrates empty and populated editable grids with local edit callbacks.
│   │   │   │   │   ├── EmptyStateSample.razor — Demonstrates empty-result messages and adding local sample content.
│   │   │   │   │   ├── ExpansionIndicatorSample.razor — Demonstrates shared expansion markers and a working disclosure button.
│   │   │   │   │   ├── ListViewSample.razor — Demonstrates all 25 static rows, shared formatting, long and null values, accessible cell templates and an empty list.
│   │   │   │   │   ├── LoadingStateSample.razor — Demonstrates loading feedback during a simulated local request.
│   │   │   │   │   ├── NoticeSample.razor — Demonstrates notice severity variants and local draft feedback.
│   │   │   │   │   ├── NoticeTriggerSample.razor — Demonstrates compact notice triggers and changing their severity and message.
│   │   │   │   │   ├── ProgressBarSample.razor — Demonstrates determinate and unknown progress with local task controls.
│   │   │   │   │   ├── ProgressRingSample.razor — Demonstrates determinate, unknown and paused progress ring states.
│   │   │   │   │   ├── SplitButtonSample.razor — Demonstrates independent export actions, option disclosure and selected and disabled states.
│   │   │   │   │   └── UniformGridButtonSample.razor — Demonstrates cell appearance variants and independent action, link, disabled and busy behavior.
│   │   │   │   ├── Display/ — Contains presentation and access component examples using fictitious brands and local-only actions.
│   │   │   │   │   ├── AccessActionsSample.razor — Demonstrates standard auxiliary buttons for presentation and access navigation.
│   │   │   │   │   ├── AccessFormSurfaceSample.razor — Demonstrates standard email fields and locally prevented native form submission.
│   │   │   │   │   ├── AccessPanelSample.razor — Demonstrates access-panel width and emphasis variants with local-only entry feedback.
│   │   │   │   │   ├── ContentContainerSample.razor — Demonstrates a centered content track independent from its outer surface.
│   │   │   │   │   ├── OfferCardSample.razor — Demonstrates static fictitious cards inside their owning offer stage without purchase actions.
│   │   │   │   │   ├── OfferStageSample.razor — Demonstrates isolated offer anchors and progressively enhanced rotation with no license or price rules.
│   │   │   │   │   ├── PresentationBandSample.razor — Demonstrates all three full-width background roles with centered headings and standard actions.
│   │   │   │   │   ├── PresentationFooterSample.razor — Demonstrates fictitious branding, copyright, decorative wordmark and footer navigation.
│   │   │   │   │   └── PresentationHeroSample.razor — Demonstrates artistic product titles with ordinary description text and standard actions.
│   │   │   │   ├── Inputs/ — Contains examples of validated forms, value binding, selection and search inputs.
│   │   │   │   │   ├── CheckBoxSample.razor — Demonstrates unchecked, checked and disabled states with local value binding.
│   │   │   │   │   ├── DateBoxSample.razor — Demonstrates typed date binding and local feedback within an EditForm.
│   │   │   │   │   ├── FieldSample.razor — Demonstrates field layouts and email validation with required, invalid and valid states.
│   │   │   │   │   ├── FilePickerSample.razor — Displays selected file name and size metadata with a bounded count, without reading or uploading file contents.
│   │   │   │   │   ├── FormActionsSample.razor — Demonstrates form action columns and a simulated submitting state.
│   │   │   │   │   ├── FormLayoutSample.razor — Demonstrates single-column and multi-column forms with locally bound fields.
│   │   │   │   │   ├── MaskedInputSample.razor — Demonstrates mask-aware input binding inside a validated form.
│   │   │   │   │   ├── MultiSelectBoxSample.razor — Demonstrates general business multi-selection, search/creation, constraints and optional ordering.
│   │   │   │   │   ├── NumberBoxSample.razor — Demonstrates integer and decimal binding, numeric ranges and disabled input.
│   │   │   │   │   ├── ReferenceDropdownSample.razor — Demonstrates single-reference selection with short/long choices and host callbacks.
│   │   │   │   │   ├── LanguagePickerSample.razor — Demonstrates the extension-owned UI-language and formatting selectors with persistent personal choices.
│   │   │   │   │   ├── SearchAutocompleteSample.razor — Demonstrates initial matching candidates, filtering, selected IDs and empty states.
│   │   │   │   │   ├── SearchBoxSample.razor — Demonstrates delayed search changes and filtering a local list.
│   │   │   │   │   ├── SelectBoxSample.razor — Demonstrates empty and disabled selectors and binding an enum option.
│   │   │   │   │   ├── StandaloneCheckBoxSample.razor — Demonstrates a protocol-boundary boolean value and native submission value without saving a session.
│   │   │   │   │   ├── StandaloneMaskedInputSample.razor — Demonstrates mask-aware value binding without a form context.
│   │   │   │   │   ├── StandaloneSelectBoxSample.razor — Demonstrates protocol-boundary single selection using the shared option contract.
│   │   │   │   │   ├── StandaloneTextBoxSample.razor — Demonstrates local email text binding and protocol field attributes without a network submission.
│   │   │   │   │   ├── TextBoxSample.razor — Demonstrates single-line, multiline, password and disabled text input with local editing.
│   │   │   │   │   ├── ToggleSectionSample.razor — Demonstrates enabled, disabled and optional sections containing local input.
│   │   │   │   │   └── ToggleSwitchSample.razor — Demonstrates switch states and a locally bound reminder setting.
│   │   │   │   └── Layout/ — Contains examples of application shells, page surfaces, headings and navigation behavior.
│   │   │   │       ├── AccessBrandSample.razor — Demonstrates entry branding with optional context and descriptive content.
│   │   │   │       ├── ApplicationLayoutSample.razor — Demonstrates the configured application layout within a bounded local preview.
│   │   │   │       ├── ApplicationShellSample.razor — Demonstrates an application shell with local content and optional secondary navigation.
│   │   │   │       ├── ContentSurfaceSample.razor — Demonstrates a top bar and scrolling content surface with document switching.
│   │   │   │       ├── InteractionBoundarySample.razor — Demonstrates locking standard input and button controls inside a local editing region.
│   │   │   │       ├── NavigationGuardSample.razor — Demonstrates standard draft editing and opt-in protection through an Underline test link.
│   │   │   │       ├── NavigationSurfaceSample.razor — Demonstrates a navigation surface using standard primary items and secondary action buttons.
│   │   │   │       ├── PageBodySample.razor — Demonstrates page-body width modes around local example content.
│   │   │   │       ├── PageHeadingSample.razor — Demonstrates page heading states and locally handled title actions.
│   │   │   │       ├── PrimaryNavigationItemSample.razor — Demonstrates current, enabled and disabled primary navigation items.
│   │   │   │       ├── SecondaryNavigationItemSample.razor — Demonstrates secondary links for an explicit custom business-navigation host.
│   │   │   │       ├── SectionNavigatorSample.razor — Demonstrates section discovery and gutter navigation within scrollable content.
│   │   │   │       ├── ServiceMenuSample.razor — Demonstrates a header destination menu linked to local sample sections.
│   │   │   │       └── ShellHeaderSample.razor — Demonstrates header branding, identity and injected menu content.
│   │   │   ├── _Imports.razor — Imports component namespaces and Razor directives for descendant files.
│   │   │   ├── App.razor — Defines the HTML document/interactive root and reads document language from provider-neutral ITextProvider.
│   │   │   ├── AccessExampleLayout.razor — Loads public library resources and scoped theme state without adding a second main landmark around independent access pages.
│   │   │   └── Routes.razor — Routes application pages through the framework layout and handles missing destinations.
│   │   ├── Localization/ — Contains Gallery-owned translations supplied to the optional Culture extension.
│   │   │   └── Culture.json — Stores the Gallery text catalog used for generated keys and embedded application translations.
│   │   ├── Models/ — Contains Gallery-only demonstration and editor data.
│   │   │   ├── ChangeLog.json — Lists stable tag notes and the next preview in descending version order.
│   │   │   ├── ChangeLogCatalog.cs — Loads the embedded version catalog and its ReleaseNote data contract.
│   │   │   ├── AccessExampleState.cs — Defines deterministic local access fixtures, validation, busy snapshots and account selection without authentication or credential persistence.
│   │   │   ├── BusinessRecord.cs — Defines the editable record used by Gallery examples.
│   │   │   ├── CatalogSections.cs — Maps components to eight categories and independent page routes for the directory.
│   │   │   ├── ComponentCatalog.cs — Lists all current Framework and extension production controls with usage metadata, snippets, reflected parameters and initialized defaults.
│   │   │   ├── PaletteDraft.cs — Holds editable theme values and their validation for the Foundations page.
│   │   │   ├── ParameterMeaning.cs — Supplies plain parameter explanations from reflected component property names and types.
│   │   │   ├── SampleCatalog.cs — Associates component entries with compiled examples, route keys, preview parameters and embedded Razor source.
│   │   │   └── SampleFor.cs — Marks each compiled example with the component name it demonstrates.
│   │   ├── Properties/ — Contains local launch and development host settings.
│   │   │   └── launchSettings.json — Defines local launch profiles, URLs and development environment.
│   │   ├── Services/ — Contains circuit-local demonstration data.
│   │   │   └── RecordStore.cs — Keeps example records in one interactive server circuit's memory.
│   │   ├── wwwroot/ — Contains static assets served by the application or Razor class library.
│   │   │   ├── gallery-favicon.svg — Provides a Primary-colored browse icon specifically for browser tabs.
│   │   │   └── gallery.svg — Provides the white browse logo with visible dimensions reduced by twenty percent.
│   │   ├── appsettings.Flourish.json — Stores framework localization defaults, preference retention and startup appearance configuration.
│   │   ├── appsettings.json — Reserves an empty configuration object for consumer business/host settings.
│   │   ├── Gallery.Flourish.Blazor.csproj — Defines the Gallery host, embeds examples/translations and references same-repository Flourish projects plus the transitive Essential key generator.
│   │   └── Program.cs — Uses Framework/ConfigureCulture, optional Design and RecordStore service entries alongside Gallery navigation and ordinary host routing/endpoints.
│   ├── Gallery.Flourish.WinUI3/ — Hosts the initial WinUI3 application window; its original assembly and namespaces remain unchanged.
│   │   ├── App.xaml — Declares the initial WinUI3 application resources.
│   │   ├── App.xaml.cs — Creates and activates the initial WinUI3 window.
│   │   ├── Gallery.Flourish.WinUI3.csproj — Defines the initial WinUI3 Gallery executable with the preserved Gallery.Flourish.WINUI3 assembly identity.
│   │   ├── MainWindow.xaml — Defines the placeholder WinUI3 window content.
│   │   └── MainWindow.xaml.cs — Initializes the placeholder WinUI3 main window.
│   └── Gallery.Flourish.WPF/ — Native executable demonstrations and API inventory for every exported control.
│       ├── App.xaml / App.xaml.cs — Own the desktop application and visible error-reporting lifecycle.
│       ├── MainWindow.cs — Composes the native ApplicationShell, optional Design and Culture controls.
│       ├── GalleryCatalog.cs — Requires an executable factory and explicit usage entry for every native control.
│       ├── SimpleSamples.cs — Demonstrates direct controls and variants.
│       ├── Samples.cs / Samples.Scenarios.cs — Demonstrate real selection, records, editing, dialogs and compositions.
│       ├── Culture.json — Contains Gallery-owned runtime captions.
│       └── Gallery.Flourish.WPF.csproj — References the aggregate and optional Culture bridge.
├── tests/ — Contains library regression tests and the unthemed Blazor verification host.
│   ├── Tests.Gallery.Flourish.Blazor/ — Checks actual localized Gallery pages through the current scoped Culture extension.
│   │   ├── ChangeLogChecks.cs — Exercises version replacement and language changes without remounting the release page.
│   │   ├── Program.cs — Exercises language changes, rendered headings, preserved API/data identities and subscription disposal.
│   │   ├── DynamicSampleChecks.cs — Executes actual sample events and checks translated status with retained export/search/edit/multi-selection state.
│   │   ├── LocalizedValidationHarness.cs — Exercises real EditForm annotation messages through Field and ValidationMessages without changing stored keys.
│   │   └── Tests.Gallery.Flourish.Blazor.csproj — References the existing Gallery and ASP.NET framework without new external dependencies.
│   ├── Tests.Flourish.Blazor/ — Checks Blazor contracts, rendering and browser interop lifetimes.
│   │   ├── AccessExampleChecks.cs — Checks local login/account fixtures, validation, busy guards, saved-email snapshots, failure/empty states and cancellation.
│   │   ├── ApiFinalAuditChecks.cs — Checks scoped select formatting, explicit autocomplete ARIA, disabled/creating guards and native checkbox field names.
│   │   ├── BrandingChecks.cs — Checks brand defaults, display combinations, favicon priority, frozen builders and the public API migration.
│   │   ├── CatalogChecks.cs — Checks complete exported-component coverage, intended usage, preferred production entries, reflected parameters, defaults and grid container ownership.
│   │   ├── clipboard-dom.mjs — Checks exact clipboard text, fallback cleanup and focus and selection restoration.
│   │   ├── ComponentInventoryChecks.cs — Checks source-owned classification coverage, meaningful component scope, normalized generic lookup and production boundaries.
│   │   ├── controls-dom.mjs — Checks popup placement and lifecycles, modal focus and input behavior against a simulated DOM.
│   │   ├── ControlTextChecks.cs — Checks component default localization, explicit overrides and subscription disposal.
│   │   ├── DataTableChecks.cs — Checks dropdown styling hooks, localized counts, paging, visibility and column sizing.
│   │   ├── DataSearchChecks.cs — Checks unique canonical search state, input association, disabled callbacks and CSS ownership.
│   │   ├── DisplayBoardChecks.cs — Checks preview isolation, exact code encoding and the code board's copy control.
│   │   ├── DropdownChecks.cs — Checks dropdown keyboard selection, closing, validation and native input behavior.
│   │   ├── FrameworkConfigurationChecks.cs — Checks framework file defaults, host override priority, Design configuration and frozen integration registration.
│   │   ├── GridChecks.cs — Checks editable grid contracts, selection and host-owned operations.
│   │   ├── heading-dom.mjs — Checks heading stability when content shrinks, scroll positions clamp or page dimensions change.
│   │   ├── InputMigrationChecks.cs — Checks standalone POST attributes, typed input behavior and field identity and validation relationships.
│   │   ├── interaction-origin-dom.mjs — Checks pointer and keyboard focus origin changes against a simulated DOM.
│   │   ├── InteropFinalAuditChecks.cs — Checks reserved offer attributes, single-flight imports, late completion, idempotent/disconnected cleanup and document-flow mode races.
│   │   ├── LifecycleChecks.cs — Checks JavaScript registration, component disposal and circuit-local state.
│   │   ├── NavigationControlsChecks.cs — Checks shared directory ownership, BackToTop lifecycle/geometry and chart values/scale boundaries.
│   │   ├── ChartToolbarChecks.cs — Renders shared chart-filter/display controls and actual field/input/button rows.
│   │   ├── inline-toolbar-layout.test.mjs — Checks library-owned shared toolbar and flexible input action geometry.
│   │   ├── ListViewChecks.cs — Checks complete ordered SSR lists, shared formatting, accessibility, templates, validation, no-JavaScript operation and scoped localization disposal.
│   │   ├── measurement-lifecycle.test.mjs — Checks observer cleanup and measurements across browser component lifetimes.
│   │   ├── palette-checks.mjs — Audits the seventeen color roles, interaction states, shared notice semantics, aliases and approved paints.
│   │   ├── presentation-offers-dom.mjs — Checks scoped offer links, rotation pauses, card ordering, replaced anchors and disposal against a simulated DOM.
│   │   ├── PresentationChecks.cs — Checks presentation SSR structure, semantic headings, access-panel slots, validated modes, preserved native form transport and avoidance of nested main elements.
│   │   ├── ButtonUnavailableChecks.cs — Checks unavailable Elevated native submits/links, busy state, guarded callbacks and re-enabled actions.
│   │   ├── Program.cs — Runs platform-independent Blazor configuration, rendering and contract checks.
│   │   ├── action-menu-dom.mjs — Checks the unique production menu's disabled actions, focus, dispatch, native disclosure and disposal.
│   │   ├── section-navigator-dom.mjs — Checks section discovery, scrolling, tooltip dismissal and browser-listener cleanup.
│   │   ├── SectionNavigatorChecks.cs — Checks section identifiers, native fragment links, encoding and discovery lifecycle.
│   │   ├── SplitButtonChecks.cs — Checks independent actions, shared selection, disabled behavior and disclosure accessibility.
│   │   ├── Tests.Flourish.Blazor.csproj — Defines rendering and contract checks and links Gallery catalog models for API verification.
│   │   ├── tests.js — Checks browser measurement and data helpers in the JavaScript test runner.
│   │   ├── TextChecks.cs — Checks provider-free fallbacks, live scoped text changes, explicit overrides, stable navigation identities and subscription cleanup.
│   │   └── UniformGridChecks.cs — Checks independent grid shape, appearance and dimensions alongside passive and action cell behavior.
│   ├── Tests.Flourish.Blazor.Native/ — Hosts Framework without Design to verify native appearance and behavior.
│   │   ├── Components/ — Contains Razor components composed by this project.
│   │   │   ├── Pages/ — Contains routed Gallery pages or verification pages.
│   │   │   │   └── Controls.razor — Exercises the configured shell and controls without optional theme styling.
│   │   │   ├── _Imports.razor — Imports component namespaces and Razor directives for descendant files.
│   │   │   ├── App.razor — Defines the HTML document and interactive Blazor root.
│   │   │   └── Routes.razor — Routes application pages through the framework layout and handles missing destinations.
│   │   ├── Properties/ — Contains local launch and development host settings.
│   │   │   └── launchSettings.json — Defines local launch profiles, URLs and development environment.
│   │   ├── appsettings.json — Sets host logging levels and allowed HTTP hosts.
│   │   ├── Program.cs — Starts a host that registers Framework without the optional Design library.
│   │   └── Tests.Flourish.Blazor.Native.csproj — Defines the unthemed Blazor host with a Framework reference and no Design reference.
│   ├── Tests.Flourish.Core/ — Checks shared services independently of UI frameworks.
│   │   ├── Abstract/ — Groups regression checks for abstract.
│   │   │   └── PlatformNeutralContractTests.cs — Checks that theme and navigation state values remain platform-independent.
│   │   ├── Architecture/ — Groups regression checks for architecture.
│   │   │   └── CoreBoundaryTests.cs — Checks that the shared assembly has no UI framework or Windows target dependency.
│   │   ├── BackgroundTasks/ — Groups regression checks for background tasks.
│   │   │   └── BackgroundTaskServiceTests.cs — Checks that idle workers stop and restart when queued tasks arrive.
│   │   ├── Commands/ — Groups regression checks for commands.
│   │   │   ├── CommandDispatcherRegressionTests.cs — Checks that missing commands and invalid arguments have stable failure behavior.
│   │   │   ├── CommandDispatcherTests.cs — Checks that command priority, context and return values are preserved.
│   │   │   └── CommandParserHostedServiceTests.cs — Checks that parser mappings register once and are disposed with the host.
│   │   ├── Configuration/ — Groups regression checks for configuration.
│   │   │   ├── ApplicationDataOptionsTests.cs — Checks that default localization options contain built-in English and no custom files.
│   │   │   ├── AppPreferenceServiceTests.cs — Checks that theme settings read correctly and owned JSON updates remain atomic.
│   │   │   └── ValueValidationTests.cs — Checks that invalid configuration values are rejected without changing accepted values.
│   │   ├── Hosting/ — Groups regression checks for hosting.
│   │   │   └── CoreServiceCollectionExtensionsTests.cs — Checks that shared services register once through stable interface aliases.
│   │   ├── Infrastructure/ — Provides temporary files, test paths and UI-thread helpers.
│   │   │   └── TemporaryDirectory.cs — Creates isolated temporary test files and removes them after use.
│   │   ├── Layout/ — Groups regression checks for layout.
│   │   │   ├── ContentLayoutServiceTests.cs — Checks that runtime layout snapshots validate widths and suppress unchanged values.
│   │   │   └── LayoutBuilderTests.cs — Checks that content alignment options and fluent return values are preserved.
│   │   ├── Localization/ — Groups regression checks for localization.
│   │   │   ├── LocalizationServiceTests.cs — Checks that built-in cultures, locale normalization and custom files resolve correctly.
│   │   │   └── TemporaryDirectory.cs — Creates isolated temporary test files and removes them after use.
│   │   ├── Messaging/ — Groups regression checks for messaging.
│   │   │   └── NotificationServiceTests.cs — Checks that notification mutations publish valid immutable snapshots.
│   │   ├── Motion/ — Groups regression checks for motion.
│   │   │   └── MotionBuilderTests.cs — Checks that motion defaults and startup configuration remain stable.
│   │   ├── Navigation/ — Groups regression checks for navigation.
│   │   │   ├── NavigationMenuModelTests.cs — Checks that route and command item definitions retain presentation metadata.
│   │   │   ├── NavigationMenuStateServiceTests.cs — Checks that menu transactions publish one immutable snapshot per change.
│   │   │   ├── PageHistoryServiceBackStackTests.cs — Checks that clearing back history preserves the forward stack.
│   │   │   └── PageHistoryServiceTests.cs — Checks that back and forward stacks retain their ordering and bounds.
│   │   ├── Profile/ — Groups regression checks for profile.
│   │   │   ├── ProfileModelTests.cs — Checks that name order and initials follow the configured identity parts.
│   │   │   ├── ProfileServiceTests.cs — Checks that sign-in requests normalize identity and publish the correct state.
│   │   │   └── StoredProfileCredentialsTests.cs — Checks that stored identity schemas normalize and validate name parts.
│   │   ├── Projects/ — Groups regression checks for projects.
│   │   │   ├── ProjectCatalogPersistenceTests.cs — Checks that project catalogs round-trip order, metadata and active selection.
│   │   │   ├── ProjectServiceTests.cs — Checks that project mutations normalize metadata and update immutable state.
│   │   │   ├── ProjectTransactionTests.cs — Checks that failed persistence leaves project state and events unchanged.
│   │   │   └── TemporaryDirectory.cs — Creates isolated temporary test files and removes them after use.
│   │   ├── Shell/ — Groups regression checks for shell.
│   │   │   ├── StatusBar/ — Groups regression checks for status bar.
│   │   │   │   ├── StatusBarBuilderTests.cs — Checks that status configuration methods apply their defaults and return the builder.
│   │   │   │   └── StatusBarServiceTests.cs — Checks that status snapshots remain immutable and unchanged flags suppress events.
│   │   │   ├── TitleBar/ — Groups regression checks for title bar.
│   │   │   │   └── TitleBarStateServiceTests.cs — Checks that identity and search mutations publish only material changes.
│   │   │   └── Toolbar/ — Groups regression checks for toolbar.
│   │   │       ├── ToolbarItemTests.cs — Checks that toolbar item identifiers follow their command and display name.
│   │   │       ├── ToolbarStateServiceTests.cs — Checks that per-view toolbar definitions and fallback items remain immutable.
│   │   │       └── ToolbarStateTransactionTests.cs — Checks that failed toolbar transactions leave committed state unchanged.
│   │   ├── ToolTips/ — Groups regression checks for tool tips.
│   │   │   └── ToolTipBuilderTests.cs — Checks that tooltip startup defaults and fluent configuration remain stable.
│   │   ├── Windowing/ — Groups regression checks for windowing.
│   │   │   └── WindowCloseServiceTests.cs — Checks that ordered close guards run before platform close and respect cancellation.
│   │   └── Tests.Flourish.Core.csproj — Defines the shared service regression test project and test dependencies.
│   ├── Tests.Flourish.Extensions.Culture.Blazor/ — Checks extension configuration, translation, personal selection and the optional dependency boundary.
│   │   ├── BrowserPreferenceChecks.cs — Checks serial save/apply, supported choices, browser failures/retry, isolation, disposal and fresh request selection.
│   │   ├── browser-preferences.test.mjs — Checks the extension cookie writer, browser isolation and rejected storage against a simulated browser.
│   │   ├── Program.cs — Runs registration, lookup/fallback, format, session/persistence and lifecycle checks.
│   │   └── Tests.Flourish.Extensions.Culture.Blazor.csproj — Defines the Blazor Culture extension verification executable.
│   ├── Tests.Flourish.Extensions.Culture.WPF/ — Checks provider formatting, fallback, collision and disposal lifetimes.
│   │   ├── EssentialTextProviderTests.cs — Exercises the current native adapter contract.
│   │   ├── Culture.json / TestAssembly.cs — Isolate test translations and desktop-global test state.
│   │   └── Tests.Flourish.Extensions.Culture.WPF.csproj — Uses the existing xUnit/test SDK versions.
│   └── Tests.Flourish.WPF/ — Native STA/dispatcher assertions and offscreen WPF rendering; no Computer Use.
│       ├── AppearanceTests.cs — Verifies palette roles, high contrast, icon artwork and live theme resources.
│       ├── CatalogTests.cs — Verifies exact exported usage entries and every Gallery factory.
│       ├── ContentTests.cs — Exercises composite geometry, retained offer state, sticky headings and reduced motion.
│       ├── ControlTests.cs — Exercises native contracts, validation, masks, menus and async input state.
│       ├── DataTests.cs — Verifies culture-aware records, remote boundaries, retained cells, editing and chart geometry.
│       ├── NativeTest.cs — STA dispatcher and offscreen PresentationSource fixtures without visible windows.
│       ├── ShellTests.cs — Exercises real navigation buttons, guards and text-provider lifetimes.
│       ├── RenderTests.cs — Renders actual native controls at multiple widths/DPI and exports inspectable PNGs.
│       ├── Usings.cs — Test imports and disabled parallel execution for native dispatcher state.
│       └── Tests.Flourish.WPF.csproj — References the native layers and Gallery without adding packages.
├── .gitattributes — Sets repository text normalization and file handling rules.
├── .gitignore — Excludes generated builds, local settings and caches from version control.
├── AGENTS.ensure.json — Records the most recent required-documentation audit and repair.
├── AGENTS.md — Defines repository ownership, documentation and collaboration rules.
├── Directory.Build.props — Defines Flourish package version 1.1.3, Culture dependency version 1.3.0, metadata and the optional sibling local package feed.
├── Directory.Build.targets — Rejects architecture-specific library packaging before NuGet generation.
├── Flourish.slnx — Groups framework libraries, Galleries and optional extensions, exposes Core at the root, and separates Tests and Solutions.
├── global.json — Selects the .NET SDK version used by the repository.
├── LICENSE.txt — Contains the repository's license terms.
└── publish-helper.bat — Forwards Prepare or Publish arguments to the PowerShell release helper and returns its exit code.
```

## Gallery localization caller adoption on 2026-10-06

Pages, all 80 samples and shared ComponentGuide resolve natural-language UI at render time. Gallery metadata stores PurposeKey/VariantsKey/DescriptionKey; Framework ComponentUsageInfo.Scenario/Guidance now use provider-neutral TextReference entries with English fallbacks. Field delegates optional MessageFormatter to the same ValidationMessages renderer, which observes text-provider changes as well as EditContext updates. Existing business/record values and validation rules remain unchanged. See [the implementation report](bugfix-reports/2026-10-06_gallery-prose-localization.md) and [Culture integration](culture-web-integration.md).
