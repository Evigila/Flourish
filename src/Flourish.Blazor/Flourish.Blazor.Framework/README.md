# Native Blazor framework

Package: Arkheide.Flourish.Blazor.Framework. Target: .NET 10 Razor Class Library.

Framework owns the application shell, navigation behavior, native InputBase controls, forms, local data tables, menus/dialogs and their functional CSS/JS. It references Abstract and Shared. It has no dependency on Design, no color palette and no required visual-theme service.

## Configure the shell in Program

~~~csharp
using ArkheideSystem.Flourish.Blazor;

builder.Services.AddFlourishFramework(framework => framework
    .ConfigureTopBar(top => top
        .SetAppName("Application")
        .SetIcon("app.svg", "")
        .AddMenu("Actions", menu => menu
            .AddMenuItem("Refresh", "application.refresh")))
    .ConfigureNavigation(navigation => navigation
        .AddNav("Home", "home", "/", exact: true)
        .AddNav("Records", "list", "/records", secondary => secondary
            .AddSubNav("Create", "edit", "/records/create"))
        .AddNavButton("Refresh", "refresh", "application.refresh")
        .AddFixedNav("Settings", "settings", "/settings")
        .AddFixedNavButton("Sign out", "close", "session.sign-out"))
    .SetCommandParser<ApplicationCommandParser>());
~~~

`SetCommandParser<TParser>()` accepts a Core `ICommandParser`. The parser is created in the current Blazor scope, so handlers may depend on scoped services without crossing circuits:

~~~csharp
public sealed class ApplicationCommandParser(NavigationManager navigation) : ICommandParser
{
    public void RegisterCommands(ICommandRegistrar commands)
    {
        commands.Register("application.refresh", Refresh);
        commands.Register("session.sign-out", () => navigation.NavigateTo("/logout"));
    }

    private static void Refresh() { }
}
~~~

Use `ApplicationLayout` as the router's default layout. It creates `ApplicationShell` and adds the Framework stylesheet through `HeadContent`. When Design is registered, it also adds the Design stylesheet and applies its scoped theme automatically. The host retains its standard `App.razor`, Router, render mode, middleware, authentication and authorization configuration. Pages only provide route content.

~~~razor
<RouteView RouteData="routeData" DefaultLayout="typeof(ApplicationLayout)" />
~~~

`InjectToLeft<TComponent>()`, `InjectToCenter<TComponent>()` and `InjectToRight<TComponent>()` register component types. `DynamicComponent` creates them in the active scope; configuration never stores a component instance or captured scoped service.

`ApplicationShell` remains available as a low-level component for hosts that need runtime-filtered navigation or tenant titles. Its runtime parameters do not grant route access; the host still owns authorization.

Functional CSS owns visibility, popup positioning, focus-related hiding, scroll containment, responsive navigation, column layout and resize hit areas. Framework alone keeps native browser colors, typography, borders and focus. JS modules load on demand from this package.

The SDK-only CSS build expands modular sources into one public `framework.css` asset. Internal source CSS is not a runtime or package asset. The package contains no consumer-specific page, route, DTO, service or compatibility stylesheet.
