# Rebuild native WPF from current Blazor

Local timestamp: 2026-10-07 11:27:26, America/Sao_Paulo.

## Accepted scope

The user authorized complete deletion of the former WPF solution contents and required current Flourish.Blazor source as the sole UI/UX authority. The former WPF controls, host/runtime services, materials, Gallery and corresponding tests are replaced rather than adapted through compatibility APIs. Native WPF remains the rendering platform and targets net10.0-windows. Core, WinUI and the existing public six-package Blazor release are outside this reconstruction.

The Blazor Gallery was launched on loopback and inspected in the Codex browser under the user's task-scoped authorization, including actual Button/input paint and Dialog opening/closing with focus restoration. WPF tests use native code and offscreen rendering, with no Computer Use or displayed test windows.

## Architecture and packages

Flourish.WPF.Abstract owns native public contracts; Framework owns one native rendering/behavior core and depends on Abstract; optional Design depends on Abstract/Framework and supplies the current source palette without alternate control templates. The aggregate installs all three and contains no packaged DLL. Project names start with Flourish and owned namespaces with ArkheideSystem. Framework has no Core, hosting or dependency-injection dependency. Both the module and root solution include the new projects.

The local candidates are Arkheide.Flourish.WPF.Abstract, Arkheide.Flourish.WPF.Framework, Arkheide.Flourish.WPF.Design and Arkheide.Flourish.WPF, all 1.1.2. The existing release manifest and published Blazor package targets are unchanged. The optional Essential.Culture.WPF bridge is rewritten around the native ITextProvider and retains its existing Essential.Culture.Wpf 1.3.0 dependency.

There are 75 explicit General, Scenario or BuildingBlock native entries, with executable Gallery factories and native API metadata. Blazor's four Standalone native-POST input splits and browser ApplicationLayout are represented by native input/binding or shell capabilities. Internal template helpers are excluded from the public inventory; no compatibility family or retired scenario wrapper is retained.

## Native implementation

Native dependency properties, templates, DrawingContext, Popup and modal Window replace browser rendering. Buttons retain the six source variants, structured text and busy/disabled behavior. Shared input/form controls provide numeric/date binding, reference creation, mask normalization, asynchronous autocomplete, one complete-key multi-selector and one binding/external ValidationMessages renderer. Shared action menus support pointer/keyboard lifetimes; Dialog owns awaited results, keyed views, bottom sheets, closing guards, cancellation and resource/focus lifetimes.

ApplicationShell implements current responsive navigation, selected filled artwork, top-bar composition, async route guards and provider cleanup. Shared presentation controls include cards, access Brand/Primary surfaces, asymmetric Hero, retained offers, responsive grids and scrolling headings. Sticky heading collapse retains a layout placeholder to avoid native scroll-extent feedback. Long Hero text can wrap differently because native font shaping does not reproduce CSS negative letter spacing; full copy/actions remain scrollable and require manual typography acceptance.

DataTable is the interactive record core, with sorting owned by headers, one DataSearch renderer, local/remote boundaries, native paging, display snapshots, resizing, retained custom cells and typed bulk proposals. ListView shares data/columns; EditingGrid remains a distinct editing scenario with atomic paste/proposals and undo/redo. LineChart uses native finite geometry and the same display selector. Consumers continue to own domain acceptance, data creation, persistence and navigation/link callbacks.

Design ports the current light/dark role palette, approved color seeds, native high contrast and a disposable ThemeSession. Detached surfaces share live dictionaries. Existing Blazor Material Symbols artwork is converted through Windows DirectWrite into native outline/font and compressed filled geometry resources; all 4,299 official names are checked in both forms. Its existing Apache-2.0 license and source identity are packaged. No external package version, SDK, font or icon library is added. Framework embeds the current Blazor three-language library catalog directly, avoiding a second caption catalog.

## Corrections found during verification

- Native input padding was applied twice and selection initially displayed record ToString text. Template padding and selected-item template propagation now follow the actual native input/selection contract.
- Nested WPF ToggleButtons supplied theme foreground that masked inherited dark text. Foreground now flows explicitly through both selection triggers and their ExpansionIndicator.
- Replacing a cell template after table creation did not refresh the renderer. Templates are dependency properties; ordinary Refresh also updates mutable non-INPC/delegate cell values while retaining the consumer's existing subtree and unsaved editor draft.
- WPF bindings may omit NotifyOnValidationError. Field observes the actual Validation.Errors property/collection as well as notified events and consumer errors, and cleans old subscriptions on content replacement/unload.
- Removing a storyboard did not immediately clear the progress opacity clock. A template-owned native pulse now clears it directly, and dead storyboard actions are removed. The Framework-only fixed false resource was deleted so native reduced-motion metadata/inheritance is preserved; Design supplies its live system preference.
- Card/Hero layout panels are internal template helpers, eliminating accidental public Gallery/catalog entries. High-contrast chrome paint uses matching native Highlight/HighlightText roles for every state.

## Verification

The final complete WPF Release solution build passed with zero warnings and zero errors. The final canonical native xUnit run passed 139/139, with zero failures and zero skips; artifacts/wpf-test/native-wpf-release-final.trx records the result. The optional bridge's canonical Release run passed 6/6, covering formatting/fallback, consumer catalog isolation, all three shared library languages and disposal.

Native tests use an offscreen PresentationSource to run actual Loaded/Unloaded and dispatcher lifetimes without showing a Window. Exactly 79 PNGs under artifacts/wpf-render comprise all 75 loaded Gallery examples, light/dark contact sheets, the negative/single-point chart and a complete 1440x1200 Hero. The contact sheets and representative loaded Shell/Hero exports were inspected. These results do not certify visible-window focus, OS dialogs, assistive technology or display scaling.

All four candidate packages were generated locally and their dependency graph inspected. Abstract has no package dependency; Framework depends only on Abstract; Design depends on Abstract/Framework; aggregate depends on all three and contains no DLL. Framework includes the existing artwork license/source. Package authoring retains the existing missing-README advisory; no README is introduced.

build/Test-WpfPackageConsumers.ps1 passed four fresh NuGet-only consumers using one local feed and an isolated package cache, without ProjectReferences or external packages. Framework-only actual outline/filled bitmap rendering passed without loading Design, Design supplied the source palette and aggregate supplied all three layers. Evidence: artifacts/wpf-package-consumers/4d7dda533487468ca1f79099c5934a71. Framework DLL SHA256 agrees between the final Release output and the isolated restored package: CA4AEE1F021DC00A01896B7CEDAFDCD2B7A97E51134D565E303D4A1591AEAA7A.

## Documentation and manual acceptance

[Native WPF integration](../wpf-native-integration.md) supplies startup, resource/provider registration, native platform differences, local packaging commands and the complete manual checklist. The architecture directory map, index, startup and affected runtime/release guides explicitly supersede the former WPF framework. History remains append-only. Existing unrelated dirty Blazor source/tests and AI records are preserved.

Human DocFX configurations still reference the deleted single-project WPF path and former Assets directory; the roadmap still describes the retired Core-dependent host. docs/ was not changed. These references need a separately authorized migration to the three current API assemblies. The workflow's restore path already points to the nested aggregate; documentation publishing is not validated by this task.

Manual acceptance covers window widths/navigation/scrolling, light/dark/system and three languages, 100–200% DPI/high contrast/reduced motion, keyboard/pointer menus and selection, IME/paste/masks/validation, native date/file selection, modal focus/closing/reopening, record search/sort/remote paging/retained cells/bulk editing, spreadsheet acceptance/clipboard/history, and presentation/offer/chart geometry. No public package upload, deployment, Git commit or push occurred.
