# Native WPF integration

## Scope and package ownership

This is a native WPF implementation, not a WebView host or a Core/Blazor assembly adapter. Current Blazor control source/contracts/Gallery are its product reference while rendering, dependency-property binding, templates, DrawingContext, Popup and owner Window are native. Targets are net10.0-windows. WinUI remains an independent placeholder and is outside this implementation's certification.

| Project / package family | Responsibility | Direct source dependency |
| --- | --- | --- |
| Flourish.WPF.Abstract | Native controls/data/shell/text contracts | None |
| Flourish.WPF.Framework | Geometry, production controls/templates, interaction and icons | Abstract |
| Flourish.WPF.Design | Optional semantic resources/theme/appearance lifetime | Abstract, Framework |
| Flourish.WPF | Dependency-only convenience package, no aggregate DLL | Abstract, Framework, Design |
| Extensions.Culture.WPF | Optional Essential.Culture.Wpf 1.4.0 bridge | Framework |

Native public namespaces remain ArkheideSystem.Flourish.WPF; existing Arkheide.* package IDs are separate identities. The public six-package Core/Blazor manifest excludes native WPF and WinUI; local native packing/tests do not publish them or add Windows targets to already published packages.

Framework embeds the maintained shared caption JSON and existing Material Symbols icon artwork as source resources, not a browser rendering dependency. Required font/vector/codepoint resources retain their licenses/provenance. Design owns paint rather than parallel control templates. Framework alone uses supported native system paint; merely installing the aggregate does not activate Design.

## Starting and composing

Use debug.bat with Project wpf, or its existing numbered menu, then [startup](local-startup.md) for SDK/configuration/exit boundaries. The focused WPF solution also groups Gallery, the optional bridge and its tests. Windows and declared native tooling are prerequisites; the launcher does not install them.

Apply FrameworkResources to the owner scope or merge its actual Generic.xaml resource entry. Optional DesignResources.Apply(scope, ThemeMode, primary, accent) returns a ThemeSession that the consumer retains/disposes with the window. Theme/system/high-contrast subscriptions and detached popup resources follow that lifetime. Detached menus, tooltips, dropdowns and dialogs share the owner dictionaries rather than stale copies.

FrameworkBuilder configures project identity, top-bar content/actions and unique route factories. ApplicationShell owns native navigation and consults the consumer's asynchronous NavigationGuard. Hosts retain domain models, permissions, submission, persistence and confirmation meaning.

Use the optional Culture bridge through FrameworkBuilder.UseEssentialCulture and retain/dispose the returned provider. Independent contexts/catalogs isolate UI/format selection; static-facade users intentionally share configured state. See [Culture integration](culture-web-integration.md).

## Control and platform contracts

Native inputs preserve dependency-property binding, conversion, validation and host value ownership. Field combines binding errors and supplied messages through its shared validation entry. Scalar/reference/query/multiple choices retain distinct contracts and complete selection/order snapshots. DataTable, ListView and EditingGrid are separate scenarios, preserving table state, command callbacks, keyboard editing and accepted snapshots.

Dialog's ShowAsync(owner), CloseAsync(result), Busy, Dismissible, CanClose and keyed Views keep owner modality, async cancellation/completion and focus. Cancellation/disposal completes with null. The consumer supplies ordinary Buttons and business state. BrowserControlled, native HTML forms, SSR/progressive browser lifecycle and popover protocols have no implied native equivalent. FilePicker/DateBox use Windows selection interfaces; navigation/link actions are consumer callbacks.

Native heading/scroll/template behavior follows its actual implementation and retains layout extent; it should not be inferred from CSS thresholds. Long text, native shaping and display scaling need their own acceptance. Generic popup closing/availability, chart metrics and list selection remain library contracts rather than host visual substitutes.

## Verification and manual acceptance

Existing maintained STA/dispatcher/binding/template, offscreen rendering, bridge and isolated-consumer tests remain. Historical reconstruction reports retain original counts and exported snapshots; removed render artifacts are not current proof. Source inspection or offscreen exports do not establish visible-window keyboard, OS dialogs or assistive-technology acceptance.

Manually check:
1. Every cataloged Gallery category/control, navigation, resize, scroll regions and long content.
2. Light/dark/system, supported primary/accent roles, open-popup theme changes, high contrast/reduced motion and 100/150/200 percent display scaling.
3. Keyboard selection/search/create/multiselect, fixed members, availability/busy, repeated activation and stale asynchronous results.
4. Input binding/parsing/masks, cross-field errors and cancelled Windows file/date dialogs without changing host business values.
5. Centered/bottom dialogs, owner sizing/backdrop, focus entry/trap/return, close veto, Escape/Alt+F4 and disposal/reopening.
6. Table search/paging/sort/retained cells, grid clipboard/undo/redo and explicit host acceptance; charts with zero/negative values and independent scales.
7. Lifecycle cleanup after a window closes and separation between independent Culture contexts.

DocFX and its obsolete API project/favicon paths are removed. No replacement documentation framework, new dependency, public native release or Computer Use acceptance is introduced.
