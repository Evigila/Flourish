---
title: 控件
description: 了解 Flourish 控件的布局、内容、交互、间距和字体约定。
---

# 控件

Flourish 提供一组显式 WPF 自定义控件，共享统一的主题、字体系统和交互语言。它们不会为 WPF 基础类型安装隐式样式。

每个内容页面都以且仅以一个 [ChunkHero](chunk.md#chunkhero) 开头，随后放置一个或多个全宽 [Chunk](chunk.md)。所有内容都应位于这些区块中。当控件或文本角色没有显式选择其他字号层级时，Flourish 使用 Standard 字号。

## 选择控件

| 文档 | 用途 |
| --- | --- |
| [Chunk](chunk.md) | 页面层级、唯一头部区块、全宽区块和标准区块间距。 |
| [Paragraph](paragraph.md) | 将多段缩进文本作为 Chunk 的唯一主体。 |
| [Presenter](presenter.md) | 组合图片、图标组、组合视觉内容、文案和辅助控件的全宽 Split 或 Overlay 布局。 |
| [Card](card.md) | 简洁的 Card 和 IconCard 信息，以及紧凑的 ListCard 设置行。 |
| [OutputCard](output-card.md) | 在滚动视口中显示使用 Small 字号的输出、日志、进度、结果和错误。 |
| [Button](button.md) | 文本、图标、整卡和窗口标题栏操作。 |
| [DataGrid](data-grid.md) | 表格数据呈现与交互。 |
| [Overlay](overlay.md) | 临时悬浮详情和具有强关闭语义的浮动表面。 |
| [ScrollViewer](scroll-viewer.md) | 平滑页面滚动、逻辑滚动和 Flourish 滚动条。 |

一段正文使用 `Card`，多段正文使用 `Paragraph`。一个图标使用 `IconCard`，图片、多个图标或其他组合视觉内容使用 `Presenter`。卡片没有任意主体；只有 `ListCard.ActionBody` 提供一个局部操作区。完整视觉表面可交互时，使用按钮家族中的成员。

## 开始使用

应用程序通过 `FlourishBuilder` 启动 Shell 时，Flourish 会在显示 Shell 前将控件和主题资源加入 `Application.Resources`。需要让控件在 WPF 设计器中呈现、在 Shell 启动前创建，或不使用 Flourish Shell 时，请在应用级显式加载资源：

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

在 XAML 中通过 `http://schemas.arkheide.system/flourish` 命名空间引用控件。完整类型和成员签名参见 [Controls API](xref:ArkheideSystem.Flourish.Controls)，应用级组合规则参见[设计理念](../conception/index.md)。
