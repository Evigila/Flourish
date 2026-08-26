---
title: Feature configuration
description: Map each startup builder to its runtime service.
---

# Feature configuration

Flourish no longer has a catch-all shell builder. Each feature owns a focused startup builder and, when it is mutable after startup, a matching runtime service.

| Startup entry | Typical startup calls | Runtime service |
| --- | --- | --- |
| `ConfigureAppearance` | `SetEffect`, `SetThemeColors`, `SetCornerRadius` | `IMaterialEffectService`, `IAppearanceService` |
| `ConfigureFont` | `SetFont`, `SetOverrideFont<TPage>` | `IFontService` |
| `ConfigureLayout` | `SetCenterContent`, `SetSmoothScrollingEnabled` | `IContentLayoutService`, `IScrollService` |
| `ConfigureToolTips` | `SetEnabled`, `SetSettings` | `IToolTipService` |
| `ConfigureProjects` | `SetMultiProjectEnabled` | `IProjectService` |
| `ConfigureTitleBar` | `SetEnabled`, `SetApplicationTitle`, `SetSearch`, `SetProfile` | `ITitleBarService`, `IProfileFlyoutService` |
| `ConfigureNavigation` | `SetEnabled`, `AddNavigable`, `SetPanelWidth` | `INavigationService` |
| `ConfigureContent` | `AddRegionContent`, `SetProfileContent` | `IShellRegionService` |
| `ConfigureToolbar` | `SetEnabled`, `Set<TPage>` | `IToolbarService` |
| `ConfigureMotion` | `SetEnabled`, transition methods | `IMotionService` |
| `ConfigureWindow` | `SetSize`, `SetStartupLocation`, `SetTrayExit` | `IWindowService`, `IWindowCloseService`, `ITrayService` |
| `ConfigureStatusBar` | `SetEnabled`, status items and indicators | `IStatusBarService` |

```csharp
builder
    .ConfigureAppearance(appearance =>
        appearance.SetEffect(effect: MaterialEffect.Auto))
    .ConfigureFont(font =>
        font.SetFont("Segoe UI", 12, 14, 22, 16, 24, 32))
    .ConfigureLayout(layout =>
        layout.SetCenterContent(contentWidth: 1200).SetSmoothScrollingEnabled())
    .ConfigureToolTips(toolTips =>
        toolTips.SetEnabled().SetSettings(200, 5));
```

The builder records startup defaults only. Runtime services operate on the built application and expose immutable `Current` snapshots plus `Changed` events where state is observable.
