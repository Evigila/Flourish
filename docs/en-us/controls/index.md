---
title: Controls
description: Choose and load Flourish controls.
---

# Controls

Flourish controls share theming, typography, and interaction without styling WPF base types implicitly.

Use [PageBody](page-body.md) as the root, with one leading [HeaderChunk](chunk.md#headerchunk) and full-width [Chunk](chunk.md) sections. Put other content inside those sections; unspecified text uses `Standard`.

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
| [CheckBox](check-box.md) | Compact Boolean or three-state selections and icon-led selection cards. |
| [BunchedListBox](bunched-list-box.md) | Selectable collections whose shared hover, pressed, and selection layer moves continuously between items. |
| [DataGrid](data-grid.md) | Tabular data whose boundary scrolling continues through the page. |
| [GridSplitter](grid-splitter.md) | Live resizing between adjacent Grid rows or columns. |
| [Overlay](overlay.md) | Temporary or strong floating surfaces, commonly composed with a vertical ActionCard. |
| [ScrollViewer](scroll-viewer.md) | Smooth page scrolling, logical scrolling, and Flourish scroll bars. |

Use `Card` for one paragraph, `Document` for prose, `CodeSpace` for code, and `Presenter` for composed visuals. Use a Button-family control when the complete surface is interactive.

## Get started

When an application starts its Shell through `ApplicationBuilder`, Flourish adds the control and theme resources to `Application.Resources` before showing the Shell. Load the resources explicitly at application scope when controls must render in the WPF designer, be created before Shell startup, or work without the Flourish Shell:

```xml
<Application
  x:Class="Foobar.App"
  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  xmlns:flourish="http://schemas.arkheide.system/flourish"
  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <flourish:ThemeResources />
      </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
  </Application.Resources>
</Application>
```

Reference controls through `http://schemas.arkheide.system/flourish`. See the [Controls API](xref:ArkheideSystem.Flourish.Controls) and [design principles](../conception/index.md).
