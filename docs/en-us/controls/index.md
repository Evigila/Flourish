---
title: Controls
description: Learn the layout, content, interaction, spacing, and typography contracts of Flourish controls.
---

# Controls

Flourish provides explicit WPF custom controls that share one theme, typography system, and interaction language. They do not install implicit styles for WPF base types.

Use [PageBody](page-body.md) as the root of every content page. Its direct children are one leading [HeaderChunk](chunk.md#headerchunk) and the full-width [Chunk](chunk.md) sections that follow. Put all other content inside those sections. When no control or text role selects another font tier, Flourish uses the Standard size.

## Choose a control

| Documentation | Use it for |
| --- | --- |
| [PageBody](page-body.md) | The scrolling root and validated vertical section stack of a navigated page. |
| [Chunk](chunk.md) | The leading HeaderChunk, ordinary full-width sections, and standard section spacing. |
| [Document](document.md) | Several indented Paragraph elements as the only body of a chunk. |
| [CodeSpace](code-space.md) | Exact, copyable code text in a fixed monospaced presentation. |
| [Presenter](presenter.md) | Full-width Split, TopDown, or Overlay layouts with images, icon groups, copy, and supporting controls. |
| [Card](card.md) | Concise Card information and ActionCard layouts with one local interactive control. |
| [OutputCard](output-card.md) | Small-text output, logs, progress, results, and failures in a scrolling viewport. |
| [Button](button.md) | Text, icon, whole-card, and window-caption actions. |
| [DataGrid](data-grid.md) | Tabular data whose boundary scrolling continues through the page. |
| [Overlay](overlay.md) | Temporary or strong floating surfaces, commonly composed with a vertical ActionCard. |
| [ScrollViewer](scroll-viewer.md) | Smooth page scrolling, logical scrolling, and Flourish scroll bars. |

Use `Card` for one paragraph and optionally one icon, `Document` for several prose paragraphs, and `CodeSpace` for exact copyable code. Use `Presenter` for an image, several icons, or another composed visual. `Card` has no arbitrary body; `ActionCard.Body` provides exactly one local action region. Use a member of the Button family whenever the complete visual surface is interactive.

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

Reference controls in XAML through the `http://schemas.arkheide.system/flourish` namespace. See the [Controls API](xref:ArkheideSystem.Flourish.Controls) for complete type and member signatures, and the [design principles](../conception/index.md) for application-wide composition rules.
