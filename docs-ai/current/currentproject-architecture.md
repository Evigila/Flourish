# currentproject-architecture

## 2026-10-06 Authorized identity access and full space editor release

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

The active API is AddFlourishFramework and its project/top-bar/navigation builders. Retired application/title/group builders, NavigationGroups shell adapters, icon/button/grid aliases, ThemePalette and CSS theme aliases are removed. The current component inventory has only General, Scenario and BuildingBlock classifications. See [the current API guide](component-api-organization.md) for supported entry points and ownership.

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
│   ├── Test-GalleryCulture.ps1 — Checks loopback SSR language negotiation, application/control text and formatting in all three languages.
│   ├── Test-GalleryNavigation.ps1 — Checks loopback sample navigation, retained panels, repeated requests and query fallbacks.
│   └── Test-CultureIntegration.cjs — Checks live Gallery language changes, independent browser sessions, formatted values and the Framework-only host with a headless browser.
├── script/ — Contains local documentation preview commands.
│   ├── preview-docs-en-us.ps1 — Builds and serves the existing English DocFX site locally.
│   └── preview-docs-zh-cn.ps1 — Builds and serves the existing Chinese DocFX site locally.
├── scripts/ — Contains allowlisted local application startup, release preparation, package inspection and user-confirmed tag publishing commands.
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
│   │   │   │   ├── ControlContracts.cs — Defines button/dialog variants, menu actions and typed select options.
│   │   │   │   ├── PresentationTone.cs — Defines Canvas, Surface and Primary color-role choices for presentation bands.
│   │   │   │   ├── TableContracts.cs — Defines columns, row actions, sorting/view choices and localizable captions.
│   │   │   │   ├── UniformGridShape.cs — Defines rectangular and square cell shapes.
│   │   │   │   └── UniformGridVariant.cs — Defines the unique Elevated, Filled, Outlined and Danger cell paint variants.
│   │   │   ├── Primitives/ — Defines specialized selection and editing-grid models; interactive record contracts live in Components.
│   │   │   │   ├── GridContracts.cs — Describes editing-grid columns, rows, cells, proposed edits and captions.
│   │   │   │   └── SelectionContracts.cs — Defines service links and typed reference/selection models.
│   │   │   ├── ApplicationData.cs — Defines navigation, top-bar, placement and appearance records in their established namespaces.
│   │   │   ├── ApplicationContracts.cs — Declares project branding, top-bar display, navigation, layout and appearance contracts with optional text references.
│   │   │   ├── Flourish.Blazor.Abstract.csproj — Defines the public Blazor contracts package with a direct Core dependency.
│   │   │   ├── ITablePreferences.cs — Declares per-user column widths, visibility and ordering preferences.
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
│   │   │   ├── DesignServiceCollectionExtensions.cs — Registers optional theme configuration and per-scope appearance services.
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
│   │   │   │   ├── Card.razor — Groups related content in a surface with selectable visual variants.
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
│   │   │   │   ├── IdentityCard.razor — Displays a person's name, initials and supporting facts.
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
│   │   │   │   ├── ApplicationOptions.cs — Collects immutable project identity and shell options, validating paths and favicon fallback.
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
│   │   │   └── ServiceCollectionExtensions.cs — Registers framework behavior, commands, navigation and the default literal text provider.
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
│   │   ├── Flourish.Extensions.Culture.Blazor/ — Connects the scoped Blazor text provider to Essential.Culture.Blazor.
│   │   │   ├── CultureServiceCollectionExtensions.cs — Registers the scoped text-provider adapter through AddFlourishCulture.
│   │   │   ├── CultureTextProvider.cs — Resolves catalog-qualified text, formatting and change events from the current localization session.
│   │   │   └── Flourish.Extensions.Culture.Blazor.csproj — Defines the optional Blazor Culture extension package with public framework contracts and localization dependencies.
│   │   ├── Flourish.Extensions.Culture.WPF/ — Connects the WPF shell culture service to Essential.Culture.
│   │   │   ├── AssemblyInfo.cs — Allows the WPF extension tests to inspect internal integration classes.
│   │   │   ├── EssentialCultureBuilderExtensions.cs — Registers the WPF culture connection through UseEssentialCulture.
│   │   │   ├── EssentialCultureHostedService.cs — Starts and stops culture synchronization and shell text updates with the application host.
│   │   │   ├── Flourish.Extensions.Culture.WPF.csproj — Defines the optional WPF Culture extension package and its framework and localization dependencies.
│   │   │   └── ShellCultureApplicator.cs — Refreshes shell labels from stable localization tokens on the WPF dispatcher.
│   │   ├── Directory.Build.props — Sets extension package metadata, versions and the optional local Essential.Culture module path.
│   │   └── Flourish.Extensions.slnx — Opens the WPF and Blazor Culture extensions with their tests.
│   ├── Flourish.WinUI3/ — Contains the WinUI3 library project placeholder.
│   │   ├── Flourish.WinUI3.csproj — Defines the WinUI3 library placeholder and Windows App SDK dependency.
│   │   └── Flourish.WinUI3.slnx — Groups the WinUI3 placeholder library and its Gallery host.
│   ├── Flourish.WPF/ — Contains the existing WPF framework, themes and shell implementation.
│   │   ├── Abstract/ — Defines public interfaces, options and immutable state contracts.
│   │   │   ├── FontChangedEventArgs.cs — Carries the changed font scope and values to subscribers.
│   │   │   ├── FontChangeKind.cs — Identifies whether a font change affects global text, icons or one page.
│   │   │   ├── IAppearanceBuilder.cs — Declares startup palette, radius and material configuration.
│   │   │   ├── IAppearanceService.cs — Declares runtime appearance changes.
│   │   │   ├── IApplicationBuilder.cs — Declares the startup configuration and application build surface.
│   │   │   ├── IApplicationRuntime.cs — Declares access to the built host and application lifecycle.
│   │   │   ├── ICustomContentBuilder.cs — Declares injection of application content into shell regions.
│   │   │   ├── IFontBuilder.cs — Declares global and page-specific font configuration.
│   │   │   ├── IFontService.cs — Declares runtime text and icon font updates.
│   │   │   ├── IMaterialEffectService.cs — Declares system backdrop and immersive dark-mode changes.
│   │   │   ├── IMessageService.cs — Declares modal messages and custom response choices.
│   │   │   ├── INavigationBuilder.cs — Declares navigation panel and navigation tree configuration.
│   │   │   ├── INavigationGroupBuilder.cs — Declares the items inside a configured navigation group.
│   │   │   ├── INavigationService.cs — Declares runtime routes, history, menu and navigation panel operations.
│   │   │   ├── IProfileFlyoutService.cs — Declares profile flyout presentation separately from authentication.
│   │   │   ├── IScrollService.cs — Declares runtime smooth-scrolling policy changes.
│   │   │   ├── IShellRegionService.cs — Declares runtime registration of content in shell regions.
│   │   │   ├── IShortcutService.cs — Declares shortcut registration, resolution and command dispatch.
│   │   │   ├── ITitleBarBuilder.cs — Declares title bar branding, search, breadcrumbs and action configuration.
│   │   │   ├── IToolbarBuilder.cs — Declares default and page-specific toolbar items.
│   │   │   ├── IToolbarService.cs — Declares runtime changes to the active toolbar.
│   │   │   ├── ITrayService.cs — Declares notification-area icon, tooltip and menu changes.
│   │   │   ├── IWindowBuilder.cs — Declares shell sizing, frame and close behavior at startup.
│   │   │   ├── IWindowService.cs — Declares runtime window size, state and presentation operations.
│   │   │   ├── MaterialEffect.cs — Lists the supported native window backdrop effects.
│   │   │   ├── MessageDialogOption.cs — Describes a custom response choice in a message dialog.
│   │   │   ├── NavigatedEventArgs.cs — Carries the destination and parameter after navigation.
│   │   │   ├── NavigationRoute.cs — Describes a destination that creates a WPF page.
│   │   │   ├── NavigationState.cs — Stores the current destination and available history operations.
│   │   │   ├── PageCacheSnapshot.cs — Stores page cache policy and the currently retained page types.
│   │   │   ├── PageFontOverride.cs — Stores a font override for one WPF page type.
│   │   │   ├── ProfileFlyoutState.cs — Stores the current profile flyout state.
│   │   │   ├── ShortcutRegistrationInfo.cs — Stores an immutable snapshot of a registered shortcut.
│   │   │   ├── ShortcutRegistrationOptions.cs — Configures shortcut scope, priority and duplicate handling.
│   │   │   ├── ShortcutRegistryChangedEventArgs.cs — Carries a new shortcut registration snapshot.
│   │   │   ├── ShortcutResolutionContext.cs — Identifies the active window and page used to resolve a shortcut.
│   │   │   ├── ThemeColors.cs — Stores brand colors used to derive WPF resources.
│   │   │   ├── TrayState.cs — Stores an immutable notification-area icon snapshot.
│   │   │   └── WindowStateSnapshot.cs — Stores an immutable shell window state.
│   │   ├── Appearance/ — Implements theme, font and native material services.
│   │   │   ├── AppearanceBuilder.cs — Collects startup colors, corner radii and material settings.
│   │   │   ├── AppearanceOptions.cs — Stores startup appearance and font settings.
│   │   │   ├── AppearanceService.cs — Publishes runtime appearance changes and derived theme resources.
│   │   │   ├── FontBuilder.cs — Configures text and icon fonts and page-specific text scales.
│   │   │   ├── FontService.cs — Updates global font resources and page-specific overrides at runtime.
│   │   │   ├── MaterialEffectPlatform.cs — Resolves native backdrop capabilities for the current Windows version.
│   │   │   ├── MaterialEffectService.cs — Applies the selected Windows material and immersive dark mode.
│   │   │   └── ThemeService.cs — Applies WPF theme resources to attached application and window scopes.
│   │   ├── Assets/ — Stores packaged framework artwork or built-in localization resources.
│   │   │   └── favicon.ico — Provides the packaged Windows application icon.
│   │   ├── Commands/ — Registers action keys and dispatches their handlers.
│   │   │   └── ShortcutService.cs — Registers scoped shortcuts and dispatches their commands.
│   │   ├── Configuration/ — Validates startup options and manages owned persistent settings.
│   │   │   └── PageTypeValidation.cs — Rejects invalid WPF page types during registration.
│   │   ├── Controls/ — Contains public WPF control templates and their implementation.
│   │   │   ├── ActionCard.xaml — Defines the template for action card content and a fixed action region.
│   │   │   ├── ActionCard.xaml.cs — Presents a title and copy beside a fixed action area.
│   │   │   ├── BunchedIndicatorAnimator.cs — Moves shared list selection and pointer indicators between item bounds.
│   │   │   ├── BunchedListBox.xaml — Defines the template for the list's shared hover, press and selection indicator layers.
│   │   │   ├── BunchedListBox.xaml.cs — Coordinates selection, hover and pressed backgrounds for grouped list items.
│   │   │   ├── BunchedListBoxInteractionController.cs — Finds item bounds and redirects the parent-owned interaction surface.
│   │   │   ├── BunchedListBoxItem.xaml — Defines the template for the grouped list item's content and inherited states.
│   │   │   ├── BunchedListBoxItem.xaml.cs — Provides item content and state for the parent's shared indicators.
│   │   │   ├── Button.xaml — Defines the template for button variants, content alignment and interaction states.
│   │   │   ├── Button.xaml.cs — Exposes action content, visual variants and click behavior.
│   │   │   ├── Card.xaml — Defines the template for card surface variants and content alignment.
│   │   │   ├── Card.xaml.cs — Groups related content in a surface with selectable visual variants.
│   │   │   ├── CardButton.xaml — Defines the template for the clickable card's icon, title and descriptive content.
│   │   │   ├── CardButton.xaml.cs — Combines an icon, title and description in a clickable card.
│   │   │   ├── CheckBox.xaml — Defines the template for checkbox indicator, label and selection states.
│   │   │   ├── CheckBox.xaml.cs — Exposes a binary selection value and optional label.
│   │   │   ├── Chunk.xaml — Defines the template for a full-width section heading, body and supporting content.
│   │   │   ├── Chunk.xaml.cs — Groups a page section with a title, body and supporting content.
│   │   │   ├── CodeSpace.xaml — Defines the template for monospaced code and the copy action.
│   │   │   ├── CodeSpace.xaml.cs — Displays monospaced code and provides a built-in copy action.
│   │   │   ├── ComboBox.xaml — Defines the template for the selected value, dropdown panel and option states.
│   │   │   ├── ComboBox.xaml.cs — Selects one value and creates styled item containers.
│   │   │   ├── ComboBoxItem.xaml — Defines the template for dropdown option content and interaction states.
│   │   │   ├── ComboBoxItem.xaml.cs — Provides the content container for a dropdown option.
│   │   │   ├── DataGrid.xaml — Defines the template for table header, row, cell and selection presentation.
│   │   │   ├── DataGrid.xaml.cs — Wraps the native WPF grid while preserving selection and editing behavior.
│   │   │   ├── Document.xaml — Defines the template for the rounded reading surface and its paragraphs.
│   │   │   ├── Document.xaml.cs — Groups readable paragraphs on one document surface.
│   │   │   ├── GridSplitter.xaml — Defines the template for the draggable resize separator.
│   │   │   ├── GridSplitter.xaml.cs — Resizes adjacent layout regions using a draggable separator.
│   │   │   ├── HeaderChunk.xaml — Defines the template for the leading title and emphasized presentation surface.
│   │   │   ├── HeaderChunk.xaml.cs — Presents a full-width leading title and presentation area.
│   │   │   ├── HoverReveal.cs — Provides attached settings for pointer reveal behavior.
│   │   │   ├── HoverRevealAnimator.cs — Applies reveal visuals to template parts.
│   │   │   ├── HoverRevealInteraction.cs — Tracks pointer entry, press and capture changes for reveal behavior.
│   │   │   ├── Label.xaml — Defines the template for label content and native access-key display.
│   │   │   ├── Label.xaml.cs — Displays content labels with native access-key support.
│   │   │   ├── ListBox.xaml — Defines the template for list surfaces and selection presentation.
│   │   │   ├── ListBox.xaml.cs — Displays selectable items with semantic presentation modes.
│   │   │   ├── ListBoxItem.xaml — Defines the template for list item content and interaction states.
│   │   │   ├── ListBoxItem.xaml.cs — Provides a selectable item container and its interaction states.
│   │   │   ├── OutputCard.xaml — Defines the template for the scrollable output history surface.
│   │   │   ├── OutputCard.xaml.cs — Displays append-only output messages in a scrollable card.
│   │   │   ├── Overlay.xaml — Defines the template for the floating surface and its content host.
│   │   │   ├── Overlay.xaml.cs — Hosts floating content with configurable placement and open lifetime.
│   │   │   ├── PageBody.xaml — Defines the template for the scrolling viewport and stacked content host.
│   │   │   ├── PageBody.xaml.cs — Hosts vertically stacked page content in its scrolling viewport.
│   │   │   ├── Paragraph.xaml — Defines the template for reading-size text and paragraph spacing.
│   │   │   ├── Paragraph.xaml.cs — Displays a reading-size paragraph inside a document.
│   │   │   ├── PasswordBox.xaml — Defines the template for concealed input and focus states.
│   │   │   ├── PasswordBox.xaml.cs — Edits a concealed password value.
│   │   │   ├── Presenter.xaml — Defines the template for the selected text and visual content composition.
│   │   │   ├── Presenter.xaml.cs — Arranges title, copy, body and presentation content in the selected composition.
│   │   │   ├── PresenterMode.cs — Defines how presentation content shares space with copy and body.
│   │   │   ├── PresenterPosition.cs — Defines the presentation area's position relative to text content.
│   │   │   ├── RadioButton.xaml — Defines the template for the option indicator, label and selection states.
│   │   │   ├── RadioButton.xaml.cs — Selects one option within a mutually exclusive group.
│   │   │   ├── RoundedClipCoordinator.cs — Keeps template clipping aligned with rounded bounds.
│   │   │   ├── ScrollBar.xaml — Defines the template for the track, thumb and orientation.
│   │   │   ├── ScrollBar.xaml.cs — Displays and manipulates a scroll position.
│   │   │   ├── ScrollViewer.xaml — Defines the template for the scrolling viewport and scrollbars.
│   │   │   ├── ScrollViewer.xaml.cs — Hosts scrollable content and supports render-based smooth scrolling.
│   │   │   ├── SearchBox.xaml — Defines the template for the search glyph, input and placeholder.
│   │   │   ├── SearchBox.xaml.cs — Edits a search query and exposes search and clear actions.
│   │   │   ├── TextBlock.xaml — Defines the template for semantic text roles and text layout.
│   │   │   ├── TextBlock.xaml.cs — Displays text using the selected semantic typography role.
│   │   │   ├── TextBox.xaml — Defines the template for editable text, placeholder and validation presentation.
│   │   │   ├── TextBox.xaml.cs — Edits text with label, placeholder and validation semantics.
│   │   │   ├── ToolTip.xaml — Defines the template for the tooltip surface and contextual copy.
│   │   │   ├── ToolTip.xaml.cs — Displays contextual help near its owner.
│   │   │   ├── ToolTipPlacement.cs — Chooses tooltip placement from its shell region.
│   │   │   ├── ToolTipPolicy.cs — Connects participating controls with the configured tooltip policy.
│   │   │   ├── WindowCaptionButton.xaml — Defines the template for native-sized caption actions and pointer states.
│   │   │   └── WindowCaptionButton.xaml.cs — Invokes native-sized minimize, maximize or close commands.
│   │   ├── Hosting/ — Composes startup options and services with the application host.
│   │   │   ├── ApplicationBuilder.cs — Creates and configures an application before its runtime is built.
│   │   │   ├── ApplicationCompositionRoot.cs — Registers configured WPF and shared services in the host.
│   │   │   ├── ApplicationOptions.cs — Collects and validates the shell configuration before registration.
│   │   │   ├── DefaultApplicationBuilder.cs — Collects WPF shell options and builds the host once.
│   │   │   ├── HostedApplicationRuntime.cs — Starts the configured host, exposes its services and shows the shell.
│   │   │   ├── PreferenceLoader.cs — Applies saved theme, font and shell preferences during startup.
│   │   │   ├── PreferencePersistenceService.cs — Saves selected runtime preference changes to the owned settings store.
│   │   │   └── ServiceCollectionExtensions.cs — Registers the framework, commands and navigation services.
│   │   ├── Layout/ — Implements content sizing, alignment and scrolling policy.
│   │   │   ├── CenteredPageContentLayout.cs — Constrains and centers page content without restricting its scrolling viewport.
│   │   │   └── ScrollService.cs — Changes WPF smooth-scrolling policy at runtime.
│   │   ├── Messaging/ — Provides notifications and modal message behavior.
│   │   │   └── MessageDialogOptionValidator.cs — Checks that message choices have usable labels and unique identifiers.
│   │   ├── Motion/ — Provides startup and runtime animation policy.
│   │   │   └── MotionService.cs — Publishes WPF animation resources from runtime motion settings.
│   │   ├── Navigation/ — Manages destinations, menu state, panel geometry and navigation history.
│   │   │   ├── FrameNavigationContentHost.cs — Adapts page navigation to a WPF Frame without a separate history policy.
│   │   │   ├── INavigationContentHost.cs — Defines how a navigation service presents a WPF page.
│   │   │   ├── INavigationPageProvider.cs — Defines cached page lookup for a registered route.
│   │   │   ├── IPageFactory.cs — Defines creation of a navigable WPF page.
│   │   │   ├── NavigablePageRegistration.cs — Stores the page type and cache policy registered for a navigation key.
│   │   │   ├── NavigablePageRegistrationState.cs — Owns the collection of registered page metadata.
│   │   │   ├── NavigationBuilder.cs — Collects the navigation panel, groups and items selected at startup.
│   │   │   ├── NavigationGroupDefinition.cs — Stores a configured group and its child navigation items.
│   │   │   ├── NavigationItemDefinition.cs — Stores and validates a route or command item in the WPF navigation tree.
│   │   │   ├── NavigationMenuService.cs — Adapts WPF navigation definitions to the shared menu state.
│   │   │   ├── NavigationOptions.cs — Stores navigation panel settings and the configured menu tree.
│   │   │   ├── NavigationPaneColumnLayout.cs — Applies left or right navigation column widths without leaving stale constraints.
│   │   │   ├── NavigationPanelChangedEventArgs.cs — Carries an updated navigation panel snapshot.
│   │   │   ├── NavigationPanelDimensions.cs — Defines valid visible and collapsed navigation widths.
│   │   │   ├── NavigationPanelService.cs — Updates navigation visibility, side, width and open state.
│   │   │   ├── NavigationPaneTransitionController.cs — Animates navigation opening and closing through render transforms.
│   │   │   ├── NavigationRouteRegistry.cs — Owns runtime route registrations and page-type metadata.
│   │   │   ├── NavigationRoutesChangedEventArgs.cs — Carries the current route collection after registration changes.
│   │   │   ├── NavigationRuntimeFacade.cs — Combines routing, history, caching, menu and panel services behind the public API.
│   │   │   ├── NavigationService.cs — Creates and presents registered pages and maintains navigation history.
│   │   │   ├── PageCacheChangedEventArgs.cs — Carries page cache configuration and cached types after a change.
│   │   │   ├── PageCacheService.cs — Creates, reuses and evicts registered page instances.
│   │   │   ├── PageTransitionController.cs — Animates a cached page presentation without repeatedly laying out its content.
│   │   │   └── ServiceProviderPageFactory.cs — Creates pages from dependency injection or an activator fallback.
│   │   ├── Profile/ — Manages profile identity, remembered credentials and presentation state.
│   │   │   ├── ProfileFlyoutService.cs — Controls profile flyout visibility and presentation state.
│   │   │   ├── ProfileImageBrushCache.cs — Reuses image brushes for profile artwork.
│   │   │   ├── ProfileImageLoader.cs — Loads and resizes profile images while preserving transparency.
│   │   │   ├── ProfileSecretStore.cs — Stores remembered profile credentials using the configured secrets provider.
│   │   │   └── ProfileViewOptions.cs — Stores WPF-specific profile page and flyout settings.
│   │   ├── Projects/ — Manages project metadata, persistent catalogs and active selection.
│   │   │   ├── DefaultProjectBehavior.cs — Handles the shell's default create, save and activate project commands.
│   │   │   └── ProjectSaveFileDialog.cs — Abstracts the Windows file dialog used when a project needs a save path.
│   │   ├── Shell/ — Groups services and views for application chrome.
│   │   │   ├── Regions/ — Owns application content injected into named shell positions.
│   │   │   │   ├── ShellRegionOptions.cs — Stores configured custom content by shell region.
│   │   │   │   ├── ShellRegionRegistration.cs — Describes one application-owned element placed in a shell region.
│   │   │   │   └── ShellRegionService.cs — Publishes runtime additions and changes to shell region content.
│   │   │   ├── TitleBar/ — Owns branding, search, breadcrumbs and title bar state.
│   │   │   │   ├── TitleBarBuilder.cs — Collects startup title bar branding and optional features.
│   │   │   │   ├── TitleBarLogoLoadCoordinator.cs — Cancels stale logo loads and reuses completed path results.
│   │   │   │   └── TitleBarVisualAssets.cs — Resolves and loads artwork for title bar branding.
│   │   │   └── Toolbar/ — Owns default and per-view action groups and toolbar state.
│   │   │       ├── ToolbarBuilder.cs — Configures default and page-specific toolbar buttons.
│   │   │       ├── ToolbarOptions.cs — Stores WPF toolbar definitions by page type.
│   │   │       └── ToolbarService.cs — Adapts WPF page types to shared toolbar state.
│   │   ├── Themes/ — Contains WPF visual resources and the theme loader.
│   │   │   ├── Colors/ — Contains light, dark and shared palette resources.
│   │   │   │   ├── Colors.Dark.xaml — Defines the dark palette's color and brush resources.
│   │   │   │   ├── Colors.Light.xaml — Defines the light palette's color and brush resources.
│   │   │   │   └── Colors.xaml — Collects theme color dictionaries and shared brushes.
│   │   │   ├── Controls.xaml — Collects templates and styles for the WPF control library.
│   │   │   ├── Generic.xaml — Loads the canonical control and theme resource dictionaries.
│   │   │   ├── Layout.xaml — Defines shared margins, sizing and layout resources.
│   │   │   ├── ThemeResources.cs — Loads the WPF control and theme resource dictionaries.
│   │   │   └── Typography.xaml — Defines text families, font sizes and semantic text resources.
│   │   ├── ToolTips/ — Owns contextual help timing and placement policy.
│   │   │   ├── ToolTipPlacementCalculator.cs — Calculates placement from geometry without inspecting the visual tree.
│   │   │   └── ToolTipService.cs — Publishes tooltip policy resources to WPF scopes.
│   │   ├── Views/ — Contains internal shell views or Gallery demonstration pages.
│   │   │   ├── Page/ — Contains built-in WPF pages used by shell features.
│   │   │   │   ├── ProfilePage.xaml — Defines the built-in profile page's identity fields and image actions.
│   │   │   │   └── ProfilePage.xaml.cs — Presents the built-in profile form and image selection.
│   │   │   └── Windows/ — Contains shell windows, floating views and view controllers.
│   │   │       ├── ApplicationInfoOverlay.xaml — Defines the view layout for application identity and version information.
│   │   │       ├── ApplicationInfoOverlay.xaml.cs — Displays the application's title, version and supporting information.
│   │   │       ├── CustomContentBuilder.cs — Registers application-provided content in predefined shell regions.
│   │   │       ├── MessageBoxWindow.xaml — Defines the view layout for modal message content and response buttons.
│   │   │       ├── MessageBoxWindow.xaml.cs — Presents a modal message and its response buttons.
│   │   │       ├── MessageService.cs — Creates message windows and returns the selected response.
│   │   │       ├── NavigationPaneView.xaml — Defines the view layout for grouped and fixed navigation lists.
│   │   │       ├── NavigationPaneView.xaml.cs — Displays grouped and fixed navigation items and raises selection requests.
│   │   │       ├── NotificationHost.xaml — Defines the view layout for active notification cards and their actions.
│   │   │       ├── NotificationHost.xaml.cs — Displays the shell's active notifications and action buttons.
│   │   │       ├── ProfileOverlay.xaml — Defines the view layout for profile flyout content.
│   │   │       ├── ProfileOverlay.xaml.cs — Presents the profile flyout and its configured page content.
│   │   │       ├── ProjectSelectorController.cs — Connects the shell selector with active project state.
│   │   │       ├── ShellContentHost.xaml — Defines the view layout for the navigated page and centered content area.
│   │   │       ├── ShellContentHost.xaml.cs — Hosts the navigated page and shared content layout.
│   │   │       ├── ShellNavigationController.cs — Connects navigation runtime state with the shell view.
│   │   │       ├── ShellNotificationController.cs — Reconciles active notifications with displayed shell cards.
│   │   │       ├── ShellProfileController.cs — Connects profile state and flyout content with shell presentation.
│   │   │       ├── ShellRegionElementFactory.cs — Creates framework buttons for registered title bar and footer content.
│   │   │       ├── ShellStatusSurfaceController.cs — Coordinates status flyouts and their selected anchors.
│   │   │       ├── ShellTitleBarController.cs — Connects branding, search and breadcrumb state with the title bar.
│   │   │       ├── ShellToolbarController.cs — Selects and reuses toolbar buttons for the active page.
│   │   │       ├── ShellWindow.xaml — Defines the view layout for the frame, title bar, navigation, toolbar and floating layers.
│   │   │       ├── ShellWindow.xaml.cs — Hosts the WPF frame, title bar, navigation, toolbar and floating surfaces.
│   │   │       ├── StatusBarView.xaml — Defines the view layout for status items and flyout anchors.
│   │   │       ├── StatusBarView.xaml.cs — Displays shell status items and their flyout anchors.
│   │   │       ├── StatusItemViewCache.cs — Reuses stable status item views while reconciling runtime snapshots.
│   │   │       ├── StatusOverlay.xaml — Defines the view layout for expanded status information.
│   │   │       ├── StatusOverlay.xaml.cs — Hosts the expanded information for a selected status item.
│   │   │       ├── TitleBar.xaml — Defines the view layout for branding, search, breadcrumbs and caption actions.
│   │   │       ├── TitleBar.xaml.cs — Displays shell identity, search, breadcrumbs, actions and caption buttons.
│   │   │       ├── ToolbarCommandButtonIndex.cs — Updates only toolbar buttons affected by a command availability change.
│   │   │       ├── ToolbarView.xaml — Defines the view layout for active page toolbar buttons.
│   │   │       └── ToolbarView.xaml.cs — Displays and reuses buttons for the current toolbar.
│   │   ├── Windowing/ — Owns platform window state, close behavior and tray integration.
│   │   │   ├── ShellFrameController.cs — Switches between native and custom window frames.
│   │   │   ├── TrayIconService.cs — Creates the Windows notification-area icon and handles its menu commands.
│   │   │   ├── WindowBuilder.cs — Collects startup window sizes, frame and close options.
│   │   │   ├── WindowCloseOptionSynchronizer.cs — Keeps shared close policy aligned with WPF tray settings.
│   │   │   ├── WindowFrameFixService.cs — Attaches native frame fixes to the WPF window.
│   │   │   ├── WindowOptions.cs — Stores initial window sizing, frame and tray settings.
│   │   │   └── WindowService.cs — Updates WPF window state and publishes immutable snapshots.
│   │   ├── AssemblyInfo.cs — Maps public XAML namespaces and grants tests access to internal types.
│   │   ├── Flourish.WPF.csproj — Defines the existing Windows WPF library, shared Core reference and package assets.
│   │   └── Flourish.WPF.slnx — Groups the existing WPF library, shared Core, Gallery and tests.
│   ├── Gallery.Flourish.Blazor/ — Hosts the Blazor Gallery and its demonstration pages.
│   │   ├── Commands/ — Contains Gallery-only command key mappings.
│   │   │   └── GalleryCommandParser.cs — Maps Gallery menu and theme command keys to scoped handlers.
│   │   ├── Components/ — Contains Razor components composed by this project.
│   │   │   ├── Catalog/ — Contains the shared component guide and its scoped presentation styles.
│   │   │   │   ├── ComponentGuide.razor — Composes source-owned usage guidance, preferred production links, API tables and compiled examples.
│   │   │   │   └── ComponentGuide.razor.css — Constrains guide width and spaces the API parameter area.
│   │   │   ├── Pages/ — Contains routed Gallery pages or verification pages.
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
│   │   │   │   │   ├── CardSample.razor — Demonstrates empty and titled cards alongside a card with local example content.
│   │   │   │   │   ├── CodeBlockSample.razor — Demonstrates escaped code text and selectable code language labels.
│   │   │   │   │   ├── DisclosureSample.razor — Demonstrates initial open states and expandable supporting content.
│   │   │   │   │   ├── DisplayBoardSample.razor — Demonstrates preview and code boards with variant selection and local action feedback.
│   │   │   │   │   ├── IconSample.razor — Demonstrates named Material Symbols and a local icon action.
│   │   │   │   │   ├── IdentityCardSample.razor — Demonstrates identity content with optional columns and side content.
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
│   │   │   ├── App.razor — Defines the HTML document and interactive Blazor root.
│   │   │   ├── LanguagePicker.razor — Uses the standard SelectBox to switch the current Blazor session between Chinese and English.
│   │   │   ├── AccessExampleLayout.razor — Loads public library resources and scoped theme state without adding a second main landmark around independent access pages.
│   │   │   └── Routes.razor — Routes application pages through the framework layout and handles missing destinations.
│   │   ├── Localization/ — Contains Gallery-owned translations supplied to the optional Culture extension.
│   │   │   └── Culture.json — Stores the Gallery text catalog used for generated keys and embedded application translations.
│   │   ├── Models/ — Contains Gallery-only demonstration and editor data.
│   │   │   ├── AccessExampleState.cs — Defines deterministic local access fixtures, validation, busy snapshots and account selection without authentication or credential persistence.
│   │   │   ├── BusinessRecord.cs — Defines the editable record used by Gallery examples.
│   │   │   ├── CatalogSections.cs — Maps components to eight categories and independent page routes for the directory.
│   │   │   ├── ComponentCatalog.cs — Lists all 97 public components with source-owned usage metadata, snippets, reflected parameters, container configuration and initialized defaults.
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
│   │   ├── appsettings.json — Sets host logging levels and allowed HTTP hosts.
│   │   ├── Gallery.Flourish.Blazor.csproj — Defines the Gallery host, embeds examples and translations, and references the optional Flourish Culture extension and Essential key generator.
│   │   └── Program.cs — Configures shell branding, navigation, commands, Design, request cultures and the optional Culture extension for the Gallery host.
│   ├── Gallery.Flourish.WinUI3/ — Hosts the initial WinUI3 application window; its original assembly and namespaces remain unchanged.
│   │   ├── App.xaml — Declares the initial WinUI3 application resources.
│   │   ├── App.xaml.cs — Creates and activates the initial WinUI3 window.
│   │   ├── Gallery.Flourish.WinUI3.csproj — Defines the initial WinUI3 Gallery executable with the preserved Gallery.Flourish.WINUI3 assembly identity.
│   │   ├── MainWindow.xaml — Defines the placeholder WinUI3 window content.
│   │   └── MainWindow.xaml.cs — Initializes the placeholder WinUI3 main window.
│   └── Gallery.Flourish.WPF/ — Hosts the WPF Gallery and its demonstration pages.
│       ├── Localization/ — Contains localized Gallery interface copy.
│       │   └── Culture.json — Contains Gallery-specific culture entries consumed by generated localization keys.
│       ├── Views/ — Contains the WPF Gallery's routed demonstration pages.
│       │   ├── AboutPage.xaml — Composes Gallery examples for application identity and package information.
│       │   ├── AboutPage.xaml.cs — Binds interactive sample state for application identity and package information.
│       │   ├── ActionCardPage.xaml — Composes Gallery examples for action cards with fixed copy and action regions.
│       │   ├── ActionCardPage.xaml.cs — Binds interactive sample state for action cards with fixed copy and action regions.
│       │   ├── AppearancePage.xaml — Composes Gallery examples for theme colors, radius and runtime appearance APIs.
│       │   ├── AppearancePage.xaml.cs — Binds interactive sample state for theme colors, radius and runtime appearance APIs.
│       │   ├── BackgroundTasksPage.xaml — Composes Gallery examples for background task registration, cancellation and progress.
│       │   ├── BackgroundTasksPage.xaml.cs — Binds interactive sample state for background task registration, cancellation and progress.
│       │   ├── BunchedListBoxPage.xaml — Composes Gallery examples for grouped list selection and shared interaction indicators.
│       │   ├── BunchedListBoxPage.xaml.cs — Binds interactive sample state for grouped list selection and shared interaction indicators.
│       │   ├── ButtonPage.xaml — Composes Gallery examples for button variants and command invocation.
│       │   ├── ButtonPage.xaml.cs — Binds interactive sample state for button variants and command invocation.
│       │   ├── CardButtonPage.xaml — Composes Gallery examples for clickable cards with icons and descriptions.
│       │   ├── CardButtonPage.xaml.cs — Binds interactive sample state for clickable cards with icons and descriptions.
│       │   ├── CardPage.xaml — Composes Gallery examples for card surface variants and content composition.
│       │   ├── CardPage.xaml.cs — Binds interactive sample state for card surface variants and content composition.
│       │   ├── CheckBoxPage.xaml — Composes Gallery examples for binary selection and fixed checkbox layouts.
│       │   ├── CheckBoxPage.xaml.cs — Binds interactive sample state for binary selection and fixed checkbox layouts.
│       │   ├── ChunkPage.xaml — Composes Gallery examples for titled full-width page sections.
│       │   ├── ChunkPage.xaml.cs — Binds interactive sample state for titled full-width page sections.
│       │   ├── CodeSpacePage.xaml — Composes Gallery examples for code presentation and copy actions.
│       │   ├── CodeSpacePage.xaml.cs — Binds interactive sample state for code presentation and copy actions.
│       │   ├── ComboBoxPage.xaml — Composes Gallery examples for single selection and option containers.
│       │   ├── ComboBoxPage.xaml.cs — Binds interactive sample state for single selection and option containers.
│       │   ├── CommandsPage.xaml — Composes Gallery examples for command keys, parser registration and runtime dispatch.
│       │   ├── CommandsPage.xaml.cs — Binds interactive sample state for command keys, parser registration and runtime dispatch.
│       │   ├── ConfigurationPage.xaml — Composes Gallery examples for startup configuration entry points.
│       │   ├── ConfigurationPage.xaml.cs — Binds interactive sample state for startup configuration entry points.
│       │   ├── ControlLibraryPage.xaml — Composes Gallery examples for the available public WPF controls.
│       │   ├── ControlLibraryPage.xaml.cs — Binds interactive sample state for the available public WPF controls.
│       │   ├── CustomHandlerConfigurationPage.xaml — Composes Gallery examples for application content injected into shell regions.
│       │   ├── CustomHandlerConfigurationPage.xaml.cs — Binds interactive sample state for application content injected into shell regions.
│       │   ├── DataGridPage.xaml — Composes Gallery examples for native table selection, columns and editing.
│       │   ├── DataGridPage.xaml.cs — Binds interactive sample state for native table selection, columns and editing.
│       │   ├── DocumentPage.xaml — Composes Gallery examples for readable paragraphs inside a document surface.
│       │   ├── DocumentPage.xaml.cs — Binds interactive sample state for readable paragraphs inside a document surface.
│       │   ├── DynamicToolbarConfigurationPage.xaml — Composes Gallery examples for default and page-specific toolbar configuration.
│       │   ├── DynamicToolbarConfigurationPage.xaml.cs — Binds interactive sample state for default and page-specific toolbar configuration.
│       │   ├── GridSplitterPage.xaml — Composes Gallery examples for resizable page layout regions.
│       │   ├── GridSplitterPage.xaml.cs — Binds interactive sample state for resizable page layout regions.
│       │   ├── HeaderChunkPage.xaml — Composes Gallery examples for leading titles and presentation layouts.
│       │   ├── HeaderChunkPage.xaml.cs — Binds interactive sample state for leading titles and presentation layouts.
│       │   ├── HomePage.xaml — Composes Gallery examples for the WPF Gallery categories and starting links.
│       │   ├── HomePage.xaml.cs — Binds interactive sample state for the WPF Gallery categories and starting links.
│       │   ├── LabelPage.xaml — Composes Gallery examples for content labels and access keys.
│       │   ├── LabelPage.xaml.cs — Binds interactive sample state for content labels and access keys.
│       │   ├── ListBoxPage.xaml — Composes Gallery examples for selectable list presentation modes.
│       │   ├── ListBoxPage.xaml.cs — Binds interactive sample state for selectable list presentation modes.
│       │   ├── MotionConfigurationPage.xaml — Composes Gallery examples for page and navigation animation configuration.
│       │   ├── MotionConfigurationPage.xaml.cs — Binds interactive sample state for page and navigation animation configuration.
│       │   ├── NavigationRuntimePage.xaml — Composes Gallery examples for runtime routes, cache, history and navigation panel APIs.
│       │   ├── NavigationRuntimePage.xaml.cs — Binds interactive sample state for runtime routes, cache, history and navigation panel APIs.
│       │   ├── OutputCardPage.xaml — Composes Gallery examples for appending messages to a scrollable output history.
│       │   ├── OutputCardPage.xaml.cs — Binds interactive sample state for appending messages to a scrollable output history.
│       │   ├── OverlayPage.xaml — Composes Gallery examples for floating content, placement and dismissal behavior.
│       │   ├── OverlayPage.xaml.cs — Binds interactive sample state for floating content, placement and dismissal behavior.
│       │   ├── PageBodyPage.xaml — Composes Gallery examples for stacked page content and scrolling layout.
│       │   ├── PageBodyPage.xaml.cs — Binds interactive sample state for stacked page content and scrolling layout.
│       │   ├── PasswordBoxPage.xaml — Composes Gallery examples for concealed password input.
│       │   ├── PasswordBoxPage.xaml.cs — Binds interactive sample state for concealed password input.
│       │   ├── PresenterPage.xaml — Composes Gallery examples for copy and visual content composition modes.
│       │   ├── PresenterPage.xaml.cs — Binds interactive sample state for copy and visual content composition modes.
│       │   ├── ProfileConfigurationPage.xaml — Composes Gallery examples for profile identity and startup profile settings.
│       │   ├── ProfileConfigurationPage.xaml.cs — Binds interactive sample state for profile identity and startup profile settings.
│       │   ├── ProjectRuntimePage.xaml — Composes Gallery examples for project catalog, activation and multi-project operations.
│       │   ├── ProjectRuntimePage.xaml.cs — Binds interactive sample state for project catalog, activation and multi-project operations.
│       │   ├── RadioButtonPage.xaml — Composes Gallery examples for mutually exclusive option groups.
│       │   ├── RadioButtonPage.xaml.cs — Binds interactive sample state for mutually exclusive option groups.
│       │   ├── RuntimeRoutePage.xaml — Composes Gallery examples for runtime registration of navigable pages.
│       │   ├── RuntimeRoutePage.xaml.cs — Binds interactive sample state for runtime registration of navigable pages.
│       │   ├── ScrollBarPage.xaml — Composes Gallery examples for scroll position and scrollbar appearance.
│       │   ├── ScrollBarPage.xaml.cs — Binds interactive sample state for scroll position and scrollbar appearance.
│       │   ├── ScrollViewerPage.xaml — Composes Gallery examples for scrolling content and smooth-scrolling policy.
│       │   ├── ScrollViewerPage.xaml.cs — Binds interactive sample state for scrolling content and smooth-scrolling policy.
│       │   ├── SearchBoxPage.xaml — Composes Gallery examples for search queries and placeholder behavior.
│       │   ├── SearchBoxPage.xaml.cs — Binds interactive sample state for search queries and placeholder behavior.
│       │   ├── StatusBarConfigurationPage.xaml — Composes Gallery examples for status items and optional system indicators.
│       │   ├── StatusBarConfigurationPage.xaml.cs — Binds interactive sample state for status items and optional system indicators.
│       │   ├── TextBlockPage.xaml — Composes Gallery examples for semantic typography roles.
│       │   ├── TextBlockPage.xaml.cs — Binds interactive sample state for semantic typography roles.
│       │   ├── TextBoxPage.xaml — Composes Gallery examples for editable text fields and validation presentation.
│       │   ├── TextBoxPage.xaml.cs — Binds interactive sample state for editable text fields and validation presentation.
│       │   ├── TitleBarRuntimePage.xaml — Composes Gallery examples for runtime branding, breadcrumbs, search and title bar APIs.
│       │   ├── TitleBarRuntimePage.xaml.cs — Binds interactive sample state for runtime branding, breadcrumbs, search and title bar APIs.
│       │   ├── ToolbarStatusPage.xaml — Composes Gallery examples for toolbar and status content changes at runtime.
│       │   ├── ToolbarStatusPage.xaml.cs — Binds interactive sample state for toolbar and status content changes at runtime.
│       │   ├── ToolTipPage.xaml — Composes Gallery examples for contextual help and placement.
│       │   ├── ToolTipPage.xaml.cs — Binds interactive sample state for contextual help and placement.
│       │   ├── ToolTipsConfigurationPage.xaml — Composes Gallery examples for tooltip delay and placement configuration.
│       │   ├── ToolTipsConfigurationPage.xaml.cs — Binds interactive sample state for tooltip delay and placement configuration.
│       │   ├── WindowCaptionButtonPage.xaml — Composes Gallery examples for minimize, maximize and close actions.
│       │   ├── WindowCaptionButtonPage.xaml.cs — Binds interactive sample state for minimize, maximize and close actions.
│       │   ├── WindowRuntimePage.xaml — Composes Gallery examples for window frame, size, close and tray APIs.
│       │   └── WindowRuntimePage.xaml.cs — Binds interactive sample state for window frame, size, close and tray APIs.
│       ├── App.xaml — Declares WPF Gallery application resources and theme dictionaries.
│       ├── App.xaml.cs — Connects the WPF application lifetime with the configured host.
│       ├── AssemblyInfo.cs — Declares theme lookup and grants the WPF tests access to Gallery internals.
│       ├── FlourishCulture.Json — Adds Spanish translations for built-in framework culture entries.
│       ├── Gallery.Flourish.WPF.csproj — Defines the WPF Gallery executable and library references.
│       ├── GalleryCommandParser.cs — Maps WPF Gallery action keys to runtime service calls.
│       ├── MemberRow.cs — Defines sample member data for WPF table demonstrations.
│       └── Program.cs — Configures WPF Gallery's host, shell, navigation and command parser.
├── tests/ — Contains library regression tests and the unthemed Blazor verification host.
│   ├── Tests.Gallery.Flourish.Blazor/ — Checks actual localized Gallery pages with the scoped Essential service.
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
│   ├── Tests.Flourish.Extensions.Culture.Blazor/ — Checks scoped translation behavior and the optional Blazor adapter dependency boundary.
│   │   ├── Program.cs — Runs the adapter registration, fallback, formatting and session isolation checks.
│   │   └── Tests.Flourish.Extensions.Culture.Blazor.csproj — Defines the Blazor Culture extension verification executable.
│   ├── Tests.Flourish.Extensions.Culture.WPF/ — Checks WPF culture registration, hosted lifetime and shell label updates.
│   │   ├── Culture.json — Contains the test translations used by the WPF extension checks.
│   │   ├── EssentialCultureBuilderExtensionsTests.cs — Checks service registration through the WPF culture builder extension.
│   │   ├── EssentialCultureHostedServiceTests.cs — Checks startup, culture synchronization and shutdown of the WPF connection.
│   │   ├── ShellCultureApplicatorTests.cs — Checks shell label translation and refresh behavior.
│   │   ├── TestAssembly.cs — Runs extension tests serially to protect shared desktop culture state.
│   │   └── Tests.Flourish.Extensions.Culture.WPF.csproj — Defines the Windows WPF Culture extension test project.
│   └── Tests.Flourish.WPF/ — Checks WPF configuration, resources, interaction and rendered structure.
│       ├── Abstract/ — Groups regression checks for abstract.
│       │   ├── CommandKeyApiContractTests.cs — Checks that public command APIs preserve icon and command-key parameter ordering.
│       │   └── FlourishServiceCollectionExtensionsTests.cs — Checks that parser and navigable page registrations follow the public API contract.
│       ├── Controls/ — Groups regression checks for controls.
│       │   ├── BunchedListBoxTests.cs — Checks that generated containers share parent-owned pointer and selection indicators.
│       │   ├── FlourishControlStylesTests.cs — Checks that canonical themes load and publish expected typography resources.
│       │   ├── FlourishControlTextPresenterTests.cs — Checks that control text line boxes and flow spacing remain separate.
│       │   ├── FlourishDataGridTests.cs — Checks that native grid selection, editing and item counts remain available.
│       │   ├── FlourishHoverRevealContractTests.cs — Checks that participating templates use one shared reveal layer.
│       │   ├── FlourishInputStylesTests.cs — Checks that input gutters, alignment and dropdown styles follow shared resources.
│       │   ├── FlourishPublicControlsTests.cs — Checks that controls, theme resources and XAML namespace mappings remain public.
│       │   ├── FlourishTitlebarTests.cs — Checks that breadcrumb visibility follows the current navigation state.
│       │   ├── FlourishXamlArchitectureTests.cs — Checks that folders and public namespaces preserve framework implementation boundaries.
│       │   ├── GalleryControlPageStructureTests.cs — Checks that control families have dedicated Gallery pages and routes.
│       │   ├── GalleryNavigationTreeTests.cs — Checks that Gallery configuration, shell and fixed routes use the intended navigation tree.
│       │   ├── HoverRevealVisualTests.cs — Checks that pointer press and capture loss clear stale reveal animations.
│       │   ├── PageBodyTests.cs — Checks that implicit page content is stacked and accepted by the body host.
│       │   ├── PresenterPresentationLayoutTests.cs — Checks that presentation modes retain their fill and alignment contracts.
│       │   ├── ProfileImageLoaderTests.cs — Checks that profile images are bounded, frozen and retain transparency.
│       │   └── ToolTipPlacementCalculatorTests.cs — Checks that tooltip geometry selects the intended side and viewport position.
│       ├── Gallery/ — Groups regression checks for gallery.
│       │   └── GalleryLocalizationTests.cs — Checks that generated localization keys match concise culture strings.
│       ├── Hosting/ — Groups regression checks for hosting.
│       │   └── PreferenceLoaderFontTests.cs — Checks that saved font settings restore only supported typography contracts.
│       ├── Infrastructure/ — Provides temporary files, test paths and UI-thread helpers.
│       │   ├── DispatcherTest.cs — Executes assertions on a WPF dispatcher thread.
│       │   ├── GalleryLocalizationTestResolver.cs — Resolves Gallery XAML localization to English for structure assertions.
│       │   ├── StaTest.cs — Executes assertions on a single-threaded apartment.
│       │   ├── TemporaryDirectory.cs — Creates isolated temporary test files and removes them after use.
│       │   └── TestPaths.cs — Locates source and output paths used by structure assertions.
│       ├── Internal/ — Groups regression checks for internal.
│       │   ├── Composition/ — Groups regression checks for composition.
│       │   │   ├── DefaultFlourishBuilderTests.cs — Checks that configuration callbacks validate arguments and building freezes the builder.
│       │   │   ├── FlourishCompositionContractTests.cs — Checks that unconfigured features retain defaults without enabling optional shell regions.
│       │   │   ├── FlourishCustomHandlerBuilderTests.cs — Checks that custom content maps to explicit shell regions through the public contract.
│       │   │   ├── FlourishDataBuilderTests.cs — Checks that locale and culture configuration preserve persistence policy.
│       │   │   ├── FlourishDynamicToolbarBuilderTests.cs — Checks that page-specific toolbar helpers preserve public defaults.
│       │   │   ├── FlourishNavigationBuilderTests.cs — Checks that navigation startup settings and persistence flags remain independent.
│       │   │   ├── FlourishNavigationEnumValidationTests.cs — Checks that undefined navigation enum values are rejected.
│       │   │   ├── FlourishRuntimeTests.cs — Checks that the built runtime exposes the host's service provider.
│       │   │   ├── FlourishTitlebarBuilderTests.cs — Checks that optional title bar pages and features configure the correct startup options.
│       │   │   ├── FlourishWindowPropertyBuilderTests.cs — Checks that window size, tray and close configuration apply correctly.
│       │   │   └── NavigationCompositionTests.cs — Checks that registered routes form one valid tree and duplicate keys are rejected.
│       │   ├── Configuration/ — Groups regression checks for configuration.
│       │   │   └── FlourishNavigationItemTests.cs — Checks that route and command definitions expose the correct derived state.
│       │   ├── Imaging/ — Groups regression checks for imaging.
│       │   │   └── TitleBarLogoLoadCoordinatorTests.cs — Checks that new logo paths cancel stale loads and failed paths are cached.
│       │   └── Interaction/ — Groups regression checks for interaction.
│       │       ├── BunchedIndicatorAnimatorTests.cs — Checks that one visible indicator retargets smoothly without restarting opacity.
│       │       ├── NavigationPaneColumnLayoutTests.cs — Checks that left and right layout changes clear stale column constraints.
│       │       ├── NavigationPaneTransitionControllerTests.cs — Checks that navigation animations use render geometry and obey width limits.
│       │       ├── PageTransitionControllerTests.cs — Checks that page transitions avoid relayout and cancellation restores presentation.
│       │       ├── RoundedClipCoordinatorTests.cs — Checks that rounded clipping preserves uniform and asymmetric corner geometry.
│       │       ├── ShellToolbarControllerTests.cs — Checks that active toolbar changes reuse cached buttons and ignore unrelated updates.
│       │       ├── StatusItemViewCacheTests.cs — Checks that status updates reuse views and remove obsolete entries.
│       │       └── ToolbarCommandButtonIndexTests.cs — Checks that availability changes refresh only matching command buttons.
│       ├── Services/ — Groups regression checks for services.
│       │   ├── DefaultProjectBehaviorTests.cs — Checks that default project commands create, save and activate the intended files.
│       │   ├── FlourishMessageOptionValidatorTests.cs — Checks that modal response choices reject empty or duplicate definitions.
│       │   ├── FlourishToolbarServiceTests.cs — Checks that page-specific items fall back to the static toolbar when needed.
│       │   ├── FontServicePageTests.cs — Checks that page fonts cross frame boundaries while preserving explicit local fonts.
│       │   ├── FontServicePropagationTests.cs — Checks that global font updates replace only affected resource keys.
│       │   ├── FrameNavigationContentHostTests.cs — Checks that WPF frames use the shared bounded navigation history.
│       │   ├── MaterialEffectPlatformTests.cs — Checks that backdrop enum values and Windows capability fallbacks remain stable.
│       │   ├── NavigationRouteAndCacheRuntimeTests.cs — Checks that runtime route ownership and cache policy updates stay synchronized.
│       │   ├── NavigationRuntimeSurfaceTests.cs — Checks that navigation panel snapshots validate collapsed and visible widths.
│       │   ├── NavigationServiceTests.cs — Checks that unknown destinations fail before creating a page.
│       │   ├── PageCacheServiceTests.cs — Checks that cache policy controls page reuse and eviction.
│       │   ├── ProfileSecretStoreTests.cs — Checks that remembered credentials use the configured secrets provider.
│       │   ├── RuntimeAppearanceServiceTests.cs — Checks that runtime fonts and appearance changes validate supported values.
│       │   ├── RuntimeLayoutAndAppearanceServiceTests.cs — Checks that layout changes validate widths and suppress unchanged snapshots.
│       │   ├── RuntimeMotionPolicyTests.cs — Checks that runtime animation settings update attached policy resources.
│       │   ├── RuntimeNotificationAndTrayServiceTests.cs — Checks that notification and tray mutations validate and publish runtime state.
│       │   ├── RuntimeScrollServiceTests.cs — Checks that smooth-scrolling changes publish only new values.
│       │   ├── RuntimeShellStateServiceTests.cs — Checks that title bar identity and feature changes publish only material updates.
│       │   ├── RuntimeToolTipPolicyTests.cs — Checks that attached windows follow shared tooltip delay resources.
│       │   ├── RuntimeWindowServiceTests.cs — Checks that window changes publish immutable state without duplicate events.
│       │   ├── ServiceProviderPageFactoryTests.cs — Checks that registered pages resolve through DI and missing pages use an activator.
│       │   ├── ShortcutServiceTests.cs — Checks that scoped shortcut registration, disposal and duplicate handling remain stable.
│       │   ├── ToolbarStatusRegionRuntimeTests.cs — Checks that runtime toolbar and status content use stable identifiers.
│       │   └── WindowCloseOptionSynchronizerTests.cs — Checks that WPF tray options synchronize the shared close policy throughout host lifetime.
│       ├── Windows/ — Groups regression checks for windows.
│       │   ├── CenteredPageContentLayoutTests.cs — Checks that centering limits content width without constraining its scrolling viewport.
│       │   ├── DynamicShellIconRoleTests.cs — Checks that dynamic shell glyphs inherit icon typography only where required.
│       │   ├── FlourishExtractedShellControlsTests.cs — Checks that the shell composes extracted hosts and delegates their events to controllers.
│       │   ├── FlourishMessageBoxRenderingTests.cs — Checks that modal windows use native frame shadows without reserving extra client space.
│       │   ├── FlourishNavigationPaneTests.cs — Checks that fixed and grouped selection stay exclusive and raise one user request.
│       │   ├── FlourishProfilePageRenderingTests.cs — Checks that profile image actions resolve shared buttons and icons correctly.
│       │   ├── FlourishShellNavigationLayoutTests.cs — Checks that collapsed navigation resets indentation and centers icons.
│       │   ├── FlourishShellProfileFlyoutTests.cs — Checks that profile content initializes lazily after successful navigation.
│       │   ├── FlourishShellRenderingContractTests.cs — Checks that floating surfaces and status lists use shared controls and virtualization.
│       │   ├── FlourishShellTitleBarFlyoutTests.cs — Checks that brand selectors and logos retain direct controls and transparent artwork.
│       │   ├── FlourishShellWindowFrameTests.cs — Checks that native and custom frames switch without recreating the window.
│       │   ├── FlourishShellWindowShortcutTests.cs — Checks that text composition and AltGraph input do not trigger shell shortcuts.
│       │   ├── GalleryOverlayPageTests.cs — Checks that Gallery overlays use real popup hosts and canonical page layout.
│       │   ├── GalleryProjectRuntimePageTests.cs — Checks that multi-project controls reflect runtime mode and activation state.
│       │   └── ShellNotificationControllerTests.cs — Checks that notifications reuse the newest views and actions delegate to runtime services.
│       ├── coverage.runsettings — Configures code coverage collection and its assembly filters.
│       ├── TestAssembly.cs — Runs WPF test collections serially to avoid process-wide UI state races.
│       └── Tests.Flourish.WPF.csproj — Defines the Windows WPF regression test project and Gallery references.
├── .gitattributes — Sets repository text normalization and file handling rules.
├── .gitignore — Excludes generated builds, local settings and caches from version control.
├── AGENTS.ensure.json — Records the most recent required-documentation audit and repair.
├── AGENTS.md — Defines repository ownership, documentation and collaboration rules.
├── Directory.Build.props — Defines Flourish package version 1.1.0, Culture dependency version 1.3.0, metadata and the optional sibling local package feed.
├── Directory.Build.targets — Rejects architecture-specific library packaging before NuGet generation.
├── Flourish.slnx — Groups framework libraries, Galleries and optional extensions, exposes Core at the root, and separates Tests and Solutions.
├── global.json — Selects the .NET SDK version used by the repository.
├── LICENSE.txt — Contains the repository's license terms.
└── publish-helper.bat — Forwards Prepare or Publish arguments to the PowerShell release helper and returns its exit code.
```

## Gallery localization caller adoption on 2026-10-06

Pages, all 80 samples and shared ComponentGuide resolve natural-language UI at render time. Gallery metadata stores PurposeKey/VariantsKey/DescriptionKey; Framework ComponentUsageInfo.Scenario/Guidance now use provider-neutral TextReference entries with English fallbacks. Field delegates optional MessageFormatter to the same ValidationMessages renderer, which observes text-provider changes as well as EditContext updates. Existing business/record values and validation rules remain unchanged. See [the implementation report](bugfix-reports/2026-10-06_gallery-prose-localization.md) and [Culture integration](culture-web-integration.md).
