---
title: 控件
description: 了解 Flourish 控件的布局规则、交互语义与使用方式。
---

# 控件

Flourish 控件是一组面向 WPF 应用的显式自定义控件。它们使用 Flourish 的主题、排版与交互资源，不会为 WPF 基础类型安装隐式样式。

所有页面内容都应由 [Chunk](chunk.md) 建立可预期的章节与间距。章节内的操作使用 [Button](button.md) 及其专用衍生控件表达语义。[Card 卡片家族](card.md)使用 `ListCard` 承载紧凑配置行，并将 `Card` 与 `IconCard` 保留给较长说明或展示型信息。

ListCard 的标题与说明各自只有一行，Body 中只有一个立即应用的交互控件，并且绝不增加 Apply 操作。Output 与 Result Card 通常只保留标题，说明与输出均放入 `Body`。

## 开始使用

应用通过 `FlourishBuilder` 启动 Shell 时，Flourish 会在显示 Shell 前将控件和主题资源加入 `Application.Resources`。需要让 WPF 设计器显示控件、在 Shell 启动前创建控件，或者不使用 Flourish Shell 时，请在应用级别显式加载主题资源：

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

在 XAML 中使用 `http://schemas.arkheide.system/flourish` 命名空间引用控件。完整的类型与成员签名参见 [Controls API](xref:ArkheideSystem.Flourish.Controls)。

## 已有专项文档

- [Chunk](chunk.md)：页面章节、页首焦点区与内容间距。
- [Button](button.md)：普通按钮、图标按钮、卡片按钮与标题栏按钮。
- [Card](card.md)：较长信息表面、单控件 `ListCard` 配置行、仅保留标题的 Output 与 Result 模式，以及可选的图标或图片展示内容。
- [ScrollViewer](scroll-viewer.md)：平滑页面滚动、虚拟化逻辑滚动与细长滚动条外观。

其余控件将随重构逐步迁移到本栏目。当前可从[控件库概览](../articles/control-library.md)了解迁移状态。
