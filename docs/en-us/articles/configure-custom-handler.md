---
title: Custom shell content
description: Insert application-provided WPF elements and commands into predefined shell regions.
---

# Custom shell content

Flourish exposes extension regions in the title bar, the logo information surface, navigation panel, dynamic toolbar, content frame, and status bar. Use `ConfigureCustomHandler` to place application-provided WPF elements or commands in these regions.

## Add custom content and commands

```csharp
builder
    .ConfigureShell(shell => shell.UseTitleBar().UseStatusBar())
    .ConfigureTitleBar(titleBar => titleBar.SetProfile())
    .ConfigureCustomHandler(custom =>
    {
        custom
            .SetProfileContent(_ => new Button { Content = "Foo Bar" })
            .AddTitlebarAction("Sync", "\uE895", "sync.run")
            .AddFooterCommand(
                FlourishRegion.FooterEnd,
                "Help",
                "\uE946",
                "help.open");
    });
```

## Surface prerequisites

Custom content does not enable its owning surface. Enable title bar regions with `UseTitleBar()`, navigation regions with `UseNavigation()`, toolbar regions with `UseDynamicToolbar()`, and footer regions with `UseStatusBar()` in [Shell configuration](shell-configuration.md). `SetProfileContent` also requires `SetProfile()` in [Title bar](configure-title-bar.md).

The `FlourishRegion.TitlebarApplicationInfo` region is rendered as the Body of the logo information surface. Configure a logo with `SetLogo()` before adding this content. The Body is application-defined and can present dynamic details without making Flourish responsible for the underlying project or document lifecycle.

## Element factories

Element factories receive `IServiceProvider`, so they can resolve application services when needed. Use the same factory form even when an element has no dependencies. Element factories must return elements without an existing WPF parent.

```csharp
builder.ConfigureCustomHandler(custom =>
{
    custom
        .Add(
            FlourishRegion.TitlebarEnd,
            services => new SyncStatusView(
                services.GetRequiredService<SyncService>()))
        .Add(
            FlourishRegion.TitlebarApplicationInfo,
            services => new ApplicationDetailsView(
                services.GetRequiredService<ApplicationDetailsService>()));
});
```

The application defines the content, bindings, and accessibility semantics of custom elements.

## Commands and callbacks

Footer command helpers require an explicit `FlourishRegion.FooterStart` or `FlourishRegion.FooterEnd`. Command helpers dispatch stable command keys through `ICommandDispatcher`; callback helpers execute the supplied local behavior directly. Display text can be localized while command keys remain unchanged.

## Related features

- [Title bar](configure-title-bar.md) controls built-in title bar content.
- [Dynamic toolbar](dynamic-toolbar.md) provides page-specific commands.
- [Status bar](status-bar.md) describes background-task indicators, custom items, and system status.
- [Command dispatch](commands.md) handles command keys.
- [Dependency injection](configure-services.md) provides application services to factories and command handlers.
