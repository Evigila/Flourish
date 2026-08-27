---
title: Custom shell content
description: Insert application-provided WPF elements and commands into predefined shell regions.
---

# Custom shell content

Use `ConfigureContent` to add WPF elements or commands to predefined Shell regions.

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
                ShellRegion.FooterEnd,
                "Help",
                "\uE946",
                "cmd_help_open");
    });
```

## Surface prerequisites

Custom content does not enable its surface. Call the corresponding `SetEnabled()` method; `SetProfileContent` also requires `SetProfile()` in [Title bar](configure-title-bar.md). See [Shell configuration](shell-configuration.md).

`ShellRegion.TitleBarApplicationInfo` is the Body of the logo information surface and requires `SetLogo()`. This application-defined Body does not participate in project lifecycle operations, which use `IProjectBehavior`; see [Projects](projects.md).

## Element factories

Element factories receive `IServiceProvider` and must return elements without an existing WPF parent.

```csharp
builder.ConfigureContent(custom =>
{
    custom
        .AddRegionContent(
            ShellRegion.TitleBarEnd,
            services => new SyncStatusView(
                services.GetRequiredService<SyncService>()))
        .AddRegionContent(
            ShellRegion.TitleBarApplicationInfo,
            services => new ApplicationDetailsView(
                services.GetRequiredService<ApplicationDetailsService>()));
});
```

The application defines the content, bindings, and accessibility semantics of custom elements.

## Commands and callbacks

Footer command helpers require an explicit `ShellRegion.FooterStart` or `ShellRegion.FooterEnd`. Command helpers dispatch stable command keys through `ICommandDispatcher`; callback helpers execute the supplied local behavior directly. Display text can be localized while command keys remain unchanged.

## Related features

- [Title bar](configure-title-bar.md)
- [Projects](projects.md)
- [Dynamic toolbar](dynamic-toolbar.md)
- [Status bar](status-bar.md)
- [Command dispatch](commands.md)
- [Dependency injection](configure-services.md)
