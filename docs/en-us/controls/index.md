---
title: Controls
description: Learn the layout, content, interaction, spacing, and typography contracts of Flourish controls.
---

# Controls

Flourish provides explicit WPF custom controls that share one theme, typography system, and interaction language. They do not install implicit styles for WPF base types.

Every content page starts with exactly one [ChunkHero](chunk.md#chunkhero) and continues with one or more full-width [Chunk](chunk.md) sections. Put all content inside those sections. When no control or text role explicitly selects another font tier, Flourish uses the Standard size.

## Choose a control

| Documentation | Use it for |
| --- | --- |
| [Chunk](chunk.md) | Page hierarchy, the single leading hero, full-width sections, and standard section spacing. |
| [Paragraph](paragraph.md) | Several indented text paragraphs as the only body of a chunk. |
| [Presenter](presenter.md) | Full-width Split or Overlay layouts with images, icon groups, composed visuals, copy, and supporting controls. |
| [Card](card.md) | Concise Card and IconCard information, plus compact ListCard setting rows. |
| [OutputCard](output-card.md) | Small-text output, logs, progress, results, and failures in a scrolling viewport. |
| [Button](button.md) | Text, icon, whole-card, and window-caption actions. |
| [DataGrid](data-grid.md) | Tabular data presentation and interaction. |
| [Overlay](overlay.md) | Temporary hover details and strongly dismissible floating surfaces. |
| [ScrollViewer](scroll-viewer.md) | Smooth page scrolling, logical scrolling, and Flourish scroll bars. |

Use `Card` for one paragraph and `Paragraph` for several. Use `IconCard` for one icon, and `Presenter` for an image, several icons, or another composed visual. Cards do not have arbitrary bodies; only `ListCard.ActionBody` provides one local action region. Use a member of the Button family whenever the complete visual surface is interactive.

## Get started

When an application starts its Shell through `FlourishBuilder`, Flourish adds the control and theme resources to `Application.Resources` before showing the Shell. Load the resources explicitly at application scope when controls must render in the WPF designer, be created before Shell startup, or work without the Flourish Shell:

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

Reference controls in XAML through the `http://schemas.arkheide.system/flourish` namespace. See the [Controls API](xref:ArkheideSystem.Flourish.Controls) for complete type and member signatures, and the [design principles](../conception/index.md) for the application-wide composition rules.
