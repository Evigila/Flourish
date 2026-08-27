---
title: Flourish
description: Documentation for the Flourish WPF shell composition library.
---

# Flourish

Flourish is an open-source WPF Shell and control library with Host-based startup, navigation, commands, status, themes, motion, and public controls. Configure it from the application entry point with fluent builders.

> [!NOTE]
> Flourish targets WPF and therefore supports Windows desktop applications only. Projects should use a Windows target framework such as `net10.0-windows` and enable WPF.

## What Flourish provides

- Host-based startup with `ApplicationBuilder` and `IApplicationRuntime`
- Explicit `Flourish*` controls that leave native and third-party controls unchanged
- Shell window configuration for title bar, navigation panel, material effect, font, and window sizing
- Page registration and navigation through dependency injection
- Page-specific dynamic toolbar items connected to command dispatch
- Background tasks, status indicators, custom status, and LAN/power details
- Motion settings for page transitions, navigation panel animation, and hover reveal
- Theme resources that can be merged from `App.xaml`

## Start here

- [Getting started](articles/getting-started.md)
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
