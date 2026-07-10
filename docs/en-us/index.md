---
title: Flourish
description: Documentation for the Flourish WPF shell composition library.
---

# Flourish

Flourish is an open-source desktop application composition library for WPF. It provides a shell layer with host-based startup, configurable window chrome, navigation, dynamic toolbar commands, footer status integration, page caching, material effects, and motion options.

Application setup can stay in `Program` or `App.xaml.cs`, visual resources can be added from `App.xaml`, and shell behavior is configured with fluent builders.

> [!NOTE]
> Flourish targets WPF and therefore supports Windows desktop applications only. Projects should use a Windows target framework such as `net10.0-windows` and enable WPF.

## What Flourish provides

- Host-based startup with `FlourishBuilder` and `IFlourish`
- Shell window configuration for title bar, navigation panel, material effect, font, and window sizing
- Page registration and navigation through dependency injection
- Contextual dynamic toolbar items connected to command parsing
- Status bar text, custom status items, and built-in LAN/power items
- Motion settings for page transitions, navigation panel animation, and hover reveal
- Theme resources that can be merged from `App.xaml`

## Start here

- [Getting started](articles/getting-started.md)
- [Shell configuration](articles/shell-configuration.md)
- [Navigation](articles/navigation.md)
- [Dynamic toolbar](articles/dynamic-toolbar.md)
- [API reference](xref:ArkheideSystem.Flourish.Abstract)

## Project links

- [GitHub repository](https://github.com/typoo910/Flourish)
- [Issues](https://github.com/typoo910/Flourish/issues)
- [Pull requests](https://github.com/typoo910/Flourish/pulls)

Issues and pull requests are welcome for bug reports, documentation fixes, API feedback, and examples.
