---
title: Feature configuration
description: Map each startup builder to its runtime service.
---

# Feature configuration

Each feature has a focused startup builder and, when mutable, a matching runtime service.

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
        font.SetFont("Segoe UI", 11, 13, 14, 14, 18, 25))
    .ConfigureLayout(layout =>
        layout.SetCenterContent(contentWidth: 1200).SetSmoothScrollingEnabled())
    .ConfigureToolTips(toolTips =>
        toolTips.SetEnabled().SetSettings(200, 5));
```

Builders record startup defaults. Runtime services expose immutable `Current` snapshots and `Changed` events.
