---
title: Controls
description: Learn the layout rules, interaction semantics, and usage of Flourish controls.
---

# Controls

Flourish controls are explicit custom controls for WPF applications. They use Flourish theme, typography, and interaction resources without installing implicit styles for WPF base types.

Every page should place its content in [Chunk](chunk.md) controls to establish predictable sections and spacing. Actions within those sections use [Button](button.md) and its specialized derivatives to communicate their intent. The [Card family](card.md) uses `ListCard` for compact configuration rows and reserves `Card` and `IconCard` for longer explanatory or display-oriented information. [OutputCard](output-card.md) provides a dedicated surface for operation messages.

A ListCard keeps its title and description to one line each, contains one immediately applied interactive control, and never adds an Apply action. `OutputCard` appends raw, ongoing, and completed operation messages to a read-only history whose internal viewport scrolls without allowing the history length to determine the control's desired height. [Overlay](overlay.md) gives floating content a shared temporary or strong dismissal contract.

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
- [Card](card.md): longer information surfaces, single-control `ListCard` configuration rows, and optional icon or image presenters.
- [OutputCard](output-card.md): compact append-only operation messages in a scrolling viewport.
- [Overlay](overlay.md): temporary hover details and strong floating surfaces with explicit dismissal semantics.
- [ScrollViewer](scroll-viewer.md): smooth page scrolling, virtualized logical scrolling, and slender scroll bar presentation.

The remaining controls will move into this section as their refactoring is completed. For now, the [control library overview](../articles/control-library.md) describes the migration status.
