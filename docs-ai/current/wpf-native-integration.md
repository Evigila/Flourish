# Native WPF integration

## 2026-10-09 Essential 1.4.0 bridge update

The user authorized the optional bridge dependency to advance to Essential.Culture.Wpf 1.4.0. UseEssentialCulture accepts a loaded LocalizationCatalog, including Essential.FromFiles modules and CatalogLoadOptions, and creates isolated context state. EssentialTextProvider exposes policy-aware AvailableCultures and SetCulture; validation/events and disposal retain their existing ownership. Static-facade consumers configure their complete catalog before constructing any provider/view; the original optional format override remains distinct from independent context formatting. Native framework captions now use Essential's loaded catalog and parent fallback rather than a separate JSON dictionary. Module fixtures exercise real generated keys and build/publish paths. This supersedes the reconstruction's dated 1.3.0 dependency below and does not publish or version-bump Flourish packages.

## Scope and authority

The 2026-10-07 reconstruction replaces the former WPF implementation, Gallery, tests and Culture hosting adapter. Current Flourish.Blazor components, public contracts, CSS and executable Gallery are the sole UI/UX reference. The old WPF service hierarchy, hosted shell, platform materials, branded control family and compatibility aliases are removed. This is a native WPF port: Windows controls, dependency-property binding, native templates, DrawingContext, Popup and modal Window replace browser rendering. The native target remains net10.0-windows. WinUI and Core are outside this task.

The Blazor Gallery was launched on loopback and observed in the Codex browser for reference, including real buttons, inputs and Dialog focus restoration. That task-scoped authorization does not change the repository prohibition on using Computer Use for WPF testing. Native validation uses code inspection, builds, STA/dispatcher regressions and RenderTargetBitmap exports; visible desktop acceptance remains manual.

## Package and dependency boundaries

| Project / NuGet ID | Responsibility | Direct native dependency |
|---|---|---|
| Flourish.WPF.Abstract / Arkheide.Flourish.WPF.Abstract | Public native control, table, editing, shell and text-provider contracts | None |
| Flourish.WPF.Framework / Arkheide.Flourish.WPF.Framework | Native geometry, templates, interaction and explicit usage inventory | Abstract |
| Flourish.WPF.Design / Arkheide.Flourish.WPF.Design | Optional current Blazor role palette, native theme resources and appearance lifetime | Abstract, Framework |
| Flourish.WPF / Arkheide.Flourish.WPF | Dependency-only convenience package; no packaged aggregate DLL | Abstract, Framework, Design |

Namespaces begin with ArkheideSystem.Flourish.WPF. The optional Flourish.Extensions.Culture.WPF bridge retains its prepared Arkheide.Flourish.Extensions.Culture.WPF package identity and existing Essential.Culture.Wpf 1.3.0 dependency. No external dependency version, font or icon library was added. Material Symbols is the same licensed artwork already owned by Blazor; scripts/Convert-MaterialIcons.ps1 converts its existing WOFF2 through Windows DirectWrite to the bundled native font and filled vector snapshots. Framework embeds the current Blazor three-language Culture.json directly rather than maintaining a second library caption catalog.

Design contains no competing templates. Framework alone uses native system paint; installing the aggregate supplies Design without activating it. Consumers enable a scoped DesignResources.Apply session and dispose it with their window. Detached native menus, tooltips, dropdowns and dialogs share the owning resource dictionaries so runtime changes reach the same instances. ThemeSession follows the Windows light/dark setting and high contrast, and detaches system event handlers when unloaded/disposed.

The existing public Core/Blazor six-package 1.1.2 manifest remains unchanged. Native local candidates inherit the root version for verification; they have not been published and add no Windows target to the published Blazor packages.

## Starting and consuming

```powershell
.\start.bat -Project wpf
dotnet build src/Flourish.WPF/Flourish.WPF.slnx -c Release -m:1 -nr:false
dotnet test tests/Tests.Flourish.WPF/Tests.Flourish.WPF.csproj -c Release --no-build
dotnet test tests/Tests.Flourish.Extensions.Culture.WPF/Tests.Flourish.Extensions.Culture.WPF.csproj -c Release --no-build
foreach ($project in 'Flourish.WPF.Abstract', 'Flourish.WPF.Framework', 'Flourish.WPF.Design', 'Flourish.WPF') {
    dotnet pack "src/Flourish.WPF/$project/$project.csproj" -c Release --no-build --no-restore -m:1 -o artifacts/wpf-packages
}
pwsh -File build/Test-WpfPackageConsumers.ps1 -PackageDirectory artifacts/wpf-packages
```

The module solution groups all four native projects, Gallery, optional bridge and both test projects. Gallery uses the aggregate and bridge, demonstrates every explicit control entry and shows its native API. FrameworkBuilder configures project identity, top-bar content/actions and unique route factories. ApplicationShell owns native navigation and calls the consumer's asynchronous NavigationGuard. Consumers retain their domain model, permissions, submission and persistence.

For native resource setup, call FrameworkResources.Apply(window) and retain DesignResources.Apply(window, ThemeMode.Light) if Design is desired. Applications may instead merge `/Flourish.WPF.Framework;component/Themes/Generic.xaml` in application resources. A caller-owned EssentialTextProvider is installed with FrameworkBuilder.UseEssentialCulture(provider), observing the desktop culture context without changing the consumer's business data.

## Control and platform contracts

There are 74 reviewed native entries after the user-authorized 2026-10-09 IdentityCard consolidation into Card. General, Scenario and BuildingBlock remain the only classifications. Blazor's four Standalone native-POST input splits and browser-only ApplicationLayout are represented by the corresponding native binding/input or shell capability; no duplicate native renderer is kept to mirror an HTTP protocol.

DataTable is the interactive record entry. TableColumn uses native BindingPath, and cell templates are DataTemplate instances receiving TableCellContext. ListView shares TableData and columns as a read-only display scenario; EditingGrid remains a distinct spreadsheet scenario with typed GridCellChange proposals, validation and consumer acceptance. Sorting belongs to headers. Table/chart display uses the same MultiSelectBox complete stable-key snapshots, mandatory/fixed choices and reordering. Remote queries remain consumer events and are not filtered again locally. Width/selection/display snapshots can be persisted by consumers; the current table-preference boundary stores sorting rather than introducing a new persistence service.

Native multi-selection supports search and asynchronous creation without owning business creation. SearchAutocomplete supports native text changes, local suggestions or consumer asynchronous search, bounded results and cancellation of stale/unloaded requests. MaskedInput directly shares the current Blazor formatter semantics: `0` accepts ASCII digits, `A` accepts ASCII letters/digits and uppercases letters; Value is normalized and Text is formatted, including paste and programmatic updates. Native Field combines binding errors and consumer Errors through one ValidationMessages renderer.

Dialog uses ShowAsync(owner), CloseAsync(result), Busy, Dismissible, CanClose, Presentation and keyed Views/View. Cancellation/disposal completes with null. A native owner Window supplies modality; the consumer supplies business/protocol state and ordinary Buttons. BrowserControlled, form names, SSR/progressive browser rendering, popover APIs and GET/POST fields have no separate native equivalent. FilePicker and DateBox retain native Windows selection interfaces. Navigation targets and EditingGrid link actions are consumer callbacks rather than browser URLs executed by the library.

PageHeading keeps its native scroll extent stable while collapsing the visible heading, using the current source's compact thresholds and a retained layout placeholder. Only top-level owned scrolling headings become sticky; nested Section/DisplayBoard headings remain ordinary content. Native font shaping may wrap long Hero copy differently from CSS letter spacing, and the Gallery scrolls naturally to retain the complete copy. Both geometries require narrow-window and display-scaling acceptance.

## Verification and acceptance boundaries

The [reconstruction record](currentproject-changelogs/2026-10-07_112726_rebuild-native-wpf-from-blazor.md) records a clean full Release build, 139/139 native tests, 6/6 Culture bridge tests and four isolated NuGet consumers. Exactly 79 offscreen native exports under artifacts/wpf-render include all 75 loaded Gallery samples, two contact sheets, a chart and a complete Hero; they use the actual production templates and drawing code. Font glyph outlines and filled artwork, role palettes, binding/events, native table headers/editing and asynchronous lifetimes are executable checks. These checks do not establish visible-window keyboard, OS dialog or assistive-technology acceptance.

Manual checks:

1. Start the WPF Gallery. Navigate every category, follow API links and resize across the 760px shell, 860px split presentation and 560px narrow geometry. Check primary/secondary selection, scrolling headings and full 232px desktop secondary region.
2. Switch light/dark/system, then language and approved primary/accent colors. Keep a dropdown open during theme changes; verify readable native popup paint and retention of edited values. Repeat at 100%, 150% and 200% display scaling and Windows high contrast/reduced motion.
3. Use only the keyboard for Button, menus, SelectBox, MultiSelectBox, SearchAutocomplete and ReferenceDropdown. Check disabled/busy prevention, Escape/outside dismissal, typed search/paste, fixed members, bounds, order and asynchronous creation without duplicates.
4. Check Field labels, numeric/date input, multiline text, clipboard mask normalization and binding/cross-field validation. Open native date/file dialogs and cancel; confirm domain values remain consumer controlled.
5. Open centered and bottom-sheet Dialog. Check focus entry/trap/return, Escape, Alt+F4, Dismissible=false, Busy, denied asynchronous closing, awaited result, cancellation and repeated reopening.
6. Exercise DataTable header sorting/search, remote search, paging, cards, retained custom cells, column resizing/visibility/order and bulk edits across pages. In EditingGrid, check F2/type/Tab/Escape, typed errors, atomic paste/delete, save/undo/redo events and consumer acceptance. Check chart negative/zero values, display order and independent scales.
7. Check prominent/stacked cards, split Hero, access Brand/Primary surface, OfferStage pointer/keyboard activation, UniformGrid outer corners, content widths and reduced-motion behavior. Inspect screen-reader names, labels, status and native ranges.

Human DocFX configurations still refer to `src/Flourish.WPF/Flourish.WPF.csproj` and its former assets path. The current aggregate is nested at `src/Flourish.WPF/Flourish.WPF/Flourish.WPF.csproj`; API documentation must eventually target the actual three assemblies. docs/ remains human maintained, so this task reports that migration instead of silently changing those files.
