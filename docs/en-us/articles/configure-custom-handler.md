---
title: Custom shell content
description: Insert application-provided WPF elements and commands into predefined shell regions.
---

# Custom shell content

Flourish exposes extension regions in the title bar, the logo information surface, navigation panel, dynamic toolbar, content frame, and status bar. Use `ConfigureContent` to place application-provided WPF elements or commands in these regions.

## Add custom content and commands

```csharp
builder
    .ConfigureTitleBar(titleBar => titleBar.SetEnabled().SetProfile())
    .ConfigureStatusBar(statusBar => statusBar.SetEnabled())
    .ConfigureContent(custom =>
    {
        custom
            .SetProfileContent(_ => new Button { Content = "Foo Bar" })
            .AddTitleBarAction("Sync", "\uE895", "cmd_sync_run")
            .AddFooterCommand(
                FlourishRegion.FooterEnd,
                "Help",
                "\uE946",
                "cmd_help_open");
    });
```

## Surface prerequisites

Custom content does not enable its owning surface. Call `SetEnabled()` through `ConfigureTitleBar`, `ConfigureNavigation`, `ConfigureToolbar`, or `ConfigureStatusBar` for the surface that owns the region. `SetProfileContent` also requires `SetProfile()` in [Title bar](configure-title-bar.md). [Shell configuration](shell-configuration.md) explains the feature-specific Builder and Service model.

The `FlourishRegion.TitleBarApplicationInfo` region is rendered as the Body of the logo information surface. Configure a logo with `SetLogo()` before adding this content. The Body is application-defined and can present dynamic details, but it does not participate in project creation, saving, activation, deletion, or close handling. Those Shell entry points are coordinated by `IProjectBehavior`; see [Projects](projects.md).

## Element factories

Element factories receive `IServiceProvider`, so they can resolve application services when needed. Use the same factory form even when an element has no dependencies. Element factories must return elements without an existing WPF parent.

```csharp
builder.ConfigureContent(custom =>
{
    custom
        .AddRegionContent(
            FlourishRegion.TitleBarEnd,
            services => new SyncStatusView(
                services.GetRequiredService<SyncService>()))
        .AddRegionContent(
            FlourishRegion.TitleBarApplicationInfo,
            services => new ApplicationDetailsView(
                services.GetRequiredService<ApplicationDetailsService>()));
});
```

The application defines the content, bindings, and accessibility semantics of custom elements.

## Commands and callbacks

Footer command helpers require an explicit `FlourishRegion.FooterStart` or `FlourishRegion.FooterEnd`. Command helpers dispatch stable command keys through `ICommandDispatcher`; callback helpers execute the supplied local behavior directly. Display text can be localized while command keys remain unchanged.

## Related features

- [Title bar](configure-title-bar.md) controls built-in title bar content.
- [Projects](projects.md) explains the persistent catalog and replaceable lifecycle behavior.
- [Dynamic toolbar](dynamic-toolbar.md) provides page-specific commands.
- [Status bar](status-bar.md) describes background-task indicators, custom items, and system status.
- [Command dispatch](commands.md) handles command keys.
- [Dependency injection](configure-services.md) provides application services to factories and command handlers.
