---
title: 控件
description: 了解 Flourish 控件的页面层级、内容职责、交互、间距和字体约定。
---

# 控件

Flourish 提供一组显式 WPF 自定义控件，共享统一的主题、字体系统和交互语言。它们不会为 WPF 基础类型安装隐式样式。

导航内容页面使用 [PageBody](page-body.md) 作为根容器。其直接子项只能是一个位于最前方的 [HeaderChunk](chunk.md#headerchunk) 和后续全宽 [Chunk](chunk.md)。所有实际页面内容都应位于这些区块中。控件或文本角色没有显式选择其他字号层级时，使用 Standard 字号。

## 选择控件

| 文档 | 用途 |
| --- | --- |
| [PageBody](page-body.md) | 导航页面的严格滚动根容器与纵向区块堆栈。 |
| [Chunk](chunk.md) | HeaderChunk、Chunk、全宽页面层级和标准区块间距。 |
| [Document](document.md) | 使用多个 Paragraph 呈现 Large 字号的连续多段正文。 |
| [CodeSpace](code-space.md) | 以固定等宽样式呈现可精确复制的代码文本。 |
| [Presenter](presenter.md) | 以 Split、TopDown 或 Overlay 组合图片、图标组、文案和辅助控件。 |
| [Card](card.md) | 简洁 Card 信息和具有 Horizontal、Vertical 结构的 ActionCard。 |
| [OutputCard](output-card.md) | 在滚动视口中显示 Small 字号的输出、日志、进度、结果和错误。 |
| [Button](button.md) | 文字、图标、整卡和窗口标题栏操作。 |
| [DataGrid](data-grid.md) | 表格数据呈现、交互与页面滚轮协作。 |
| [Overlay](overlay.md) | Temporary、Strong 浮层以及 Vertical ActionCard 浮窗内容。 |
| [ScrollViewer](scroll-viewer.md) | 平滑页面滚动、逻辑滚动和 Flourish 滚动条。 |

一段正文使用 Card，多段正文使用 Document，需要精确复制的代码使用 CodeSpace。Card 可选择一个图标；图片、多个图标或其他组合视觉内容使用 Presenter。Card 没有任意 Body，ActionCard 只为一个局部交互控件提供 Body。完整视觉表面可交互时，使用 CardButton。

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
