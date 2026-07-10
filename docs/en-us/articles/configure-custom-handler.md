---
title: Custom shell content
description: Insert application-provided WPF elements and commands into predefined shell regions.
---

# Custom shell content

Flourish exposes extension regions in the title bar, navigation panel, dynamic toolbar, content frame, and status bar. Use `ConfigureCustomHandler` when an application needs content that the built-in feature builders do not provide.

## Add custom content and commands

```csharp
builder
    .ConfigureShell(shell => shell.UseTitleBar().UseStatusBar())
    .ConfigureTitleBar(titleBar => titleBar.SetProfile())
    .ConfigureCustomHandler(custom =>
    {
        custom
            .SetProfileContent(() => new Button { Content = "User" })
            .AddTitlebarAction("Sync", "\uE895", "sync.run")
            .AddFooterCommand("About", "\uE946", "app.about");
    });
```

## Surface prerequisites

Custom content does not enable its owning surface. [Shell configuration](shell-configuration.md) must enable the corresponding title bar, navigation, toolbar, or status bar feature. `SetProfileContent` requires `UseTitleBar()` and [Title bar](configure-title-bar.md) must display the profile trigger with `SetProfile()`.

## Element factories

Factory overloads receive `IServiceProvider` when custom elements need application services. Element factories must return elements without an existing WPF parent.

```csharp
builder.ConfigureCustomHandler(custom =>
{
    custom.Add(
        FlourishRegion.TitlebarEnd,
        services => new SyncStatusView(
            services.GetRequiredService<SyncService>()));
});
```

## Commands and callbacks

Command helpers route stable command keys through `ICommandParser`. Callback helpers execute the supplied local behavior directly. Display text can be localized while command keys remain unchanged.

## Related features

- [Title bar](configure-title-bar.md) controls built-in title bar content.
- [Dynamic toolbar](dynamic-toolbar.md) provides page-specific commands.
- [Status bar](status-bar.md) configures built-in status items.
- [Command parser](command-parser.md) handles command keys.
