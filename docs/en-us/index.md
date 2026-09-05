---
title: Flourish
description: Documentation for Flourish Core and WPF, with a roadmap for the future WinUI 3 implementation.
---

# Flourish

Flourish is a Windows application shell and control library. Platform-neutral contracts and services live in Core, and the complete UI implementation targets WPF. The WinUI 3 projects are currently standard blank Windows App SDK baselines whose future work is tracked in the roadmap.

> [!NOTE]
> WPF is the current UI implementation. WinUI 3 does not yet expose a Flourish Shell, control library, or supported package.

## What Flourish provides

- Platform-neutral commands, background tasks, localization, settings, projects, notifications, navigation menu state, and Shell state
- WPF Host-based startup with `ApplicationBuilder` and `IApplicationRuntime`
- Explicit `Flourish*` controls that leave native and third-party controls unchanged
- Shell window configuration for title bar, navigation panel, material effect, font, and window sizing
- Page registration and navigation through dependency injection
- Page-specific dynamic toolbar items connected to command dispatch
- Background tasks, status indicators, custom status, and LAN/power details
- Motion settings for page transitions, navigation panel animation, and hover reveal
- Theme resources that can be merged from `App.xaml`

## Start here

- [WPF getting started](articles/getting-started.md)
- [WinUI 3 implementation roadmap](https://github.com/Evigila/Flourish/blob/master/docs/roadmap.md)
- [Control library](articles/control-library.md)
- [Shell configuration](articles/shell-configuration.md)
- [Navigation](articles/navigation.md)
- [Dynamic toolbar](articles/dynamic-toolbar.md)
- [Background tasks](articles/background-tasks.md)
- [API reference](xref:ArkheideSystem.Flourish.Abstract)

## Project links

- [GitHub repository](https://github.com/Evigila/Flourish)
- [Issues](https://github.com/Evigila/Flourish/issues)
- [Pull requests](https://github.com/Evigila/Flourish/pulls)

Issues and pull requests are welcome.
