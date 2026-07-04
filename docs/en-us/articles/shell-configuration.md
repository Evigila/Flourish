---
title: Shell configuration
description: Configure the Flourish application shell.
---

# Shell configuration

Shell configuration is performed through `IFlourishShellBuilder`. It groups the high-level window and shell concerns in one place:

- title bar behavior
- navigation panel behavior
- window size and placement
- global font configuration
- material effect selection
- dynamic toolbar visibility
- motion configuration

```csharp
builder.ConfigureShell((_, shell) =>
{
    shell
        .UseTitlebar((_, titlebar) =>
        {
            titlebar
                .ShowLogo()
                .ShowTitle()
                .ShowSubTitle()
                .ShowSearch()
                .ShowBreadcrumb()
                .ShowNavToggle()
                .SetTitle("Gallery")
                .SetSubtitle("Flourish sample");
        })
        .UseNavigationPanel((_, nav) =>
        {
            nav.SetInitiallyOpen().SetTitle("Navigation");
        })
        .SetWindowProperty((_, window) =>
        {
            window.SetWindowSize().SetWindowMinSize().SetWindowPosition();
        })
        .UseMaterialEffect()
        .UseMotion()
        .UseDynamicToolbar();
});
```

Prefer configuring shell behavior through the builder interfaces instead of constructing shell services directly. The builder path keeps the public contract stable while implementation details can evolve.
