---
title: Controls
description: Learn the layout rules, interaction semantics, and usage of Flourish controls.
---

# Controls

Flourish controls are explicit custom controls for WPF applications. They use Flourish theme, typography, and interaction resources without installing implicit styles for WPF base types.

Every page should place its content in [Chunk](chunk.md) controls to establish predictable sections and spacing. Actions within those sections use [Button](button.md) and its specialized derivatives to communicate their intent, while [Card](card.md) and `IconCard` group non-interactive information on themed surfaces.

## Get started

When an application starts its Shell through `FlourishBuilder`, Flourish adds the control and theme resources to `Application.Resources` before showing the Shell. Load the theme resources explicitly at application scope when controls must render in the WPF designer, be created before Shell startup, or work without the Flourish Shell:

```xml
<Application
  x:Class="Foobar.App"
  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  xmlns:flourish="http://schemas.arkheide.system/flourish"
  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <flourish:FlourishThemeResources />
      </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
  </Application.Resources>
</Application>
```

Reference controls in XAML through the `http://schemas.arkheide.system/flourish` namespace. See the [Controls API](xref:ArkheideSystem.Flourish.Controls) for complete type and member signatures.

## Dedicated documentation

- [Chunk](chunk.md): page sections, page-leading hero regions, and content spacing.
- [Button](button.md): ordinary, icon, card, and window-caption buttons.
- [Card](card.md): non-interactive information surfaces and optional icon or image presenters.

The remaining controls will move into this section as their refactoring is completed. For now, the [control library overview](../articles/control-library.md) describes the migration status.
