---
title: Flourish
description: Documentation for the Flourish WPF shell composition library.
---

# Flourish

Flourish is an open-source desktop application composition and control library for WPF. It provides a shell layer with host-based startup, configurable window chrome, navigation, dynamic toolbar commands, status bar integration, page caching, material effects, motion options, and public `Flourish*` WPF controls.

Application setup normally lives in `App.xaml.cs` or another entry point configured by the application. Visual resources can be added from `App.xaml`, and shell behavior is configured with fluent builders.

> [!NOTE]
> Flourish targets WPF and therefore supports Windows desktop applications only. Projects should use a Windows target framework such as `net10.0-windows` and enable WPF.

## What Flourish provides

- Host-based startup with `FlourishBuilder` and `IFlourish`
- A reusable control library of explicit `Flourish*` custom controls that leaves native WPF and third-party controls, including their tooltip templates, unchanged
- Shell window configuration for title bar, navigation panel, material effect, font, and window sizing
- Page registration and navigation through dependency injection
- Page-specific dynamic toolbar items connected to command dispatch
- Host-managed background tasks, status indicators, custom status items, and consolidated LAN/power details
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

Issues and pull requests are welcome for bug reports, documentation fixes, API feedback, and examples.
