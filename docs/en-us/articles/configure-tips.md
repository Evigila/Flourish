---
title: ConfigureTips
description: Configure Flourish tooltip timing and placement constraints.
---

# ConfigureTips

`ConfigureTips` configures tooltip behavior. Tooltips are active only when [`ConfigureShell`](configure-shell.md) enables `UseTips()`.

```csharp
builder
    .ConfigureShell(shell => shell.UseTips())
    .ConfigureTips(tips =>
    {
        tips.SetDelay(200).SetSpawnableMargin(5);
    });
```

## Details

`SetDelay` controls the initial hover delay in milliseconds. It should be long enough to avoid visual noise in dense WPF tools and short enough to help discover icon-only controls.

`SetSpawnableMargin` keeps tooltips away from shell edges. This is useful when navigation is collapsed, footer commands are near the bottom edge, or toolbar commands sit close to the window border.

Disabling tips through `UseTips(false)` overrides these detailed settings.

## Related APIs

- [`ConfigureShell`](configure-shell.md) owns the `UseTips` switch.
- [`ConfigureTitleBar`](configure-title-bar.md), [`ConfigureNavigation`](configure-navigation.md), and [`ConfigureFooter`](configure-footer.md) contain built-in controls that can show tips.
