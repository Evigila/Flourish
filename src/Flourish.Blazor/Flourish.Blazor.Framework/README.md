# Native Blazor components and behavior

Package: Arkheide.Flourish.Blazor.Framework. Target: .NET 10 Razor Class Library.

Framework contains the application shell, native InputBase controls, forms, local data table, menus/dialogs and their functional CSS/JS. It references Abstract and Shared. It has no dependency on Design, no theme service requirement, no color palette, and no added external PackageReference.

The application owns the ASP.NET host, Router, render mode, authentication, authorization, models, remote queries and persistence.

## Use without a skin

Register the native runtime:

~~~csharp
using ArkheideSystem.Flourish.Blazor;
builder.Services.AddFlourish(app => app
    .UseTitleBar(bar => bar.SetApplicationTitle("Application"))
    .UseNavigation(nav => nav.AddGroup("main", "Main", "home", group => group.AddItem("Home", "/", exact: true))));
~~~

Import ArkheideSystem.Flourish.Blazor.Components and ArkheideSystem.Flourish.Blazor.Abstract. Load the required functional stylesheet:

~~~html
<link rel="stylesheet" href="_content/Arkheide.Flourish.Blazor.Framework/framework.css" />
~~~

Use ApplicationLayout or ApplicationShell. Use body class f-document only when the full-height shell owns document scrolling. Keep shell and interactive pages inside the same render boundary.

Functional CSS owns visibility, popup positioning, focus-related hiding, scroll containment, responsive navigation, column layout and resize hit areas. Native controls retain browser typography, colors, borders and focus. JS modules import themselves from this package. Framework alone does not resolve IAppearanceService.

## Opt into visual design

Reference Arkheide.Flourish.Blazor.Design, register AddFlourishDesign, load its design.css after framework.css, and wrap ApplicationShell or your interactive subtree in ThemeScope. Design is an independent explicit dependency; Framework never automatically loads it.

Existing component and contract namespaces remain stable. Package/assembly paths changed during the four-project split. ConfigureAppearance on the old application builder is replaced by the Design registration. Update references and static asset paths together.

Local DataTable owns column-based search, stable typed/null-last sorting, paging, visibility, list/card modes and column resizing. The complete supplied local Items collection is its input; remote pagination, virtualization and business actions remain host concerns. Per-user runtime table preferences use scoped ITablePreferences, which hosts can replace with their own scoped implementation.

No browser storage or business persistence is implied. Interactive Server with prerendering is the current verified hosting mode; other hosts need their own acceptance checks.

## Generic assets and build output

Primitives.DataTable, DataColumn, DataFilter, DataSorter and DataPagination expose neutral data contracts. CardField uses Title, Identifier and Metric. FormSurface accepts host content/actions/contents and a CssClass hook; business-specific compositions and selectors stay in the consuming application.

The SDK-only CSS build expands modular sources into the single public framework.css asset. Internal source CSS is not a runtime, package or publish asset. There is no consumer-specific compatibility entry. Closed primitive sheets initialize their content and browser bridge on first opening and retain used content thereafter. Automatic table measurement is invalidated by actual DOM/layout changes and releases observers on disposal.
