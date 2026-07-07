---
title: ConfigureShell
description: Enable or disable high-level Flourish shell features.
---

# ConfigureShell

`ConfigureShell` controls the high-level shell feature switches. Each `Use...` method accepts only `enabled` and defaults to `true`.

```csharp
builder.ConfigureShell(shell =>
{
    shell
        .UseTitleBar()
        .UseNavigation()
        .UseDynamicToolbar()
        .UseTips()
        .UseMotion()
        .UseMaterialEffect()
        .UseThemes()
        .UseFooter();
});
```

## Priority

`ConfigureShell` has the highest priority. A detailed configuration callback can still record options, but the matching shell feature does not appear unless it is enabled here.

For example, [`ConfigureFooter`](configure-footer.md) can set status text and footer items, but the footer remains hidden unless `UseFooter()` is enabled. [`ConfigureNavigation`](configure-navigation.md) can define groups and widths, but the panel remains hidden unless `UseNavigation()` is enabled.

## Related APIs

- [`ConfigureTitleBar`](configure-title-bar.md), [`ConfigureNavigation`](configure-navigation.md), and [`ConfigureFooter`](configure-footer.md) configure surfaces enabled by shell switches.
- [`ConfigureMotion`](configure-motion.md), [`ConfigureTips`](configure-tips.md), [`ConfigureMaterialEffect`](configure-material-effect.md), and [`ConfigureThemes`](configure-themes.md) configure behaviors enabled by shell switches.
- [`Shell configuration`](shell-configuration.md) provides an end-to-end example.
