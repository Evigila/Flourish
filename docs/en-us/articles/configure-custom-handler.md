---
title: ConfigureCustomHandler
description: Insert custom WPF elements into Flourish shell regions.
---

# ConfigureCustomHandler

`ConfigureCustomHandler` inserts application-provided WPF elements into predefined shell regions. It is the unified extension-slot API for title bar, navigation, toolbar, content, and footer regions.

```csharp
builder.ConfigureCustomHandler(custom =>
{
    custom
        .SetProfileContent(() => new Button { Content = "RC" })
        .AddTitlebarAction("Sync", "\uE895", "sync.run")
        .AddFooterCommand("About", "\uE946", "app.about");
});
```

## Details

Custom handler configuration does not enable shell features by itself. The owning surface must be enabled through [`ConfigureShell`](configure-shell.md). For example, footer content needs `UseFooter()`, toolbar content needs `UseDynamicToolbar()`, and title bar content needs `UseTitleBar()`.

Factory overloads receive `IServiceProvider` when custom elements need application services. Element factories must return elements without an existing WPF parent.

Command helpers use stable command keys and route through `ICommandParser`. Callback helpers are available for small local behavior, but command keys are easier to test and localize.

## Related APIs

- [`ConfigureTitleBar`](configure-title-bar.md) controls the built-in title bar features.
- [`ConfigureFooter`](configure-footer.md) configures built-in footer status text and items.
- [`Command parser`](command-parser.md) describes command-key handling.
