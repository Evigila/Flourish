---
title: ConfigureTitleBar
description: Configure the Flourish title bar after enabling it through ConfigureShell.
---

# ConfigureTitleBar

`ConfigureTitleBar` configures the built-in title bar content and behavior. The title bar is displayed only when [`ConfigureShell`](configure-shell.md) enables `UseTitleBar()`.

```csharp
builder
    .ConfigureShell(shell => shell.UseTitleBar())
    .ConfigureTitleBar(titleBar =>
    {
        titleBar
            .ShowLogo()
            .ShowTitle()
            .ShowSubTitle()
            .ShowSearch()
            .ShowBreadcrumb()
            .ShowNavToggle()
            .ShowProfile()
            .ShowThemeToggle()
            .SetTitle("Gallery")
            .SetSubtitle("Flourish sample")
            .SetSearchPlaceholder("Search images");
    });
```

## Details

`Show...` methods control built-in title bar regions. They do not create custom content; custom WPF elements are inserted with [`ConfigureCustomHandler`](configure-custom-handler.md).

`SetSearchHandler` receives search text changes. A handler can resolve services from `IServiceProvider`, which is useful for message services, view models, or application search coordinators.

`ShowThemeToggle` depends on [`ConfigureShell`](configure-shell.md) enabling themes with `UseThemes()`. The default theme is chosen through [`ConfigureThemes`](configure-themes.md).

## Related APIs

- [`ConfigureCustomHandler`](configure-custom-handler.md) replaces profile content or adds title bar actions.
- [`ConfigureNavigation`](configure-navigation.md) works with `ShowNavToggle`.
- [`ConfigureThemes`](configure-themes.md) works with `ShowThemeToggle`.
