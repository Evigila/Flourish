---
title: Title bar
description: Configure the built-in title bar content, search, navigation, profile, and theme controls.
---

# Title bar

The Flourish title bar can display application identity, search, breadcrumb navigation, a navigation toggle, profile access, and theme controls. Enable the surface through [Shell configuration](shell-configuration.md), then use `ConfigureTitleBar` to choose its content.

## Configure the title bar

```csharp
builder
    .ConfigureData(data =>
        data.SetAppCompany("Example Company").SetAppName("Foobar"))
    .ConfigureShell(shell =>
        shell.UseTitleBar().UseNavigation().UseProfile().UseThemes())
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
            .SetTitle("Foobar")
            .SetSubtitle("Desktop workspace")
            .SetSearchPlaceholder("Search");
    });
```

`UseTitleBar()` is the surface prerequisite. `UseProfile()` and `UseThemes()` independently control whether the corresponding title bar controls can operate.

## Built-in content

`Show...` methods control built-in title bar regions. They do not create custom content; [Custom shell content](configure-custom-handler.md) inserts application-provided WPF elements.

`SetLogo` accepts a pack URI or an `ImageSource`. `SetLogoFallbackText` supplies text for the logo region when an image is not used.

`SetBreadcrumbBehavior` controls when breadcrumb navigation is visible. `Always` keeps it visible, `Auto` follows navigation state, and `Hidden` suppresses it.

## Search

`SetSearchHandler` receives search text changes. A handler can resolve services from `IServiceProvider`, which is useful for message services, view models, or application search coordinators.

```csharp
builder.ConfigureTitleBar(titleBar =>
{
    titleBar.SetSearchHandler((services, searchText) =>
    {
        services.GetRequiredService<SearchCoordinator>().Update(searchText);
    });
});
```

## Profile and theme controls

`ShowProfile` controls the trigger while [Profile](configure-profile.md) defines its content and login behavior. The trigger requires both `UseTitleBar()` and `UseProfile()`.

`ShowThemeToggle` requires themes to be enabled. [Themes](configure-themes.md) selects the default theme and explains preference behavior.

## Window close behavior

`SetTrayExit(true)` makes the title bar close command hide the window in the Windows notification area. The tray menu can restore the window or exit the application. With tray exit disabled, the close command closes the window normally.

## Related features

- [Custom shell content](configure-custom-handler.md) adds title bar actions and custom regions.
- [Profile](configure-profile.md) configures profile content, authentication, and persistence.
- [Navigation](navigation.md) provides the panel controlled by `ShowNavToggle`.
- [Themes](configure-themes.md) provides the theme controlled by `ShowThemeToggle`.
