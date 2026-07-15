---
title: 控件
description: 了解 Flourish 控件的布局规则、交互语义与使用方式。
---

# 控件

Flourish 控件是一组面向 WPF 应用的显式自定义控件。它们使用 Flourish 的主题、排版与交互资源，不会为 WPF 基础类型安装隐式样式。

所有页面内容都应由 [Chunk](chunk.md) 建立可预期的章节与间距。章节内的操作使用 [Button](button.md) 及其专用衍生控件表达语义，非交互式信息则可以通过 [Card](card.md) 和 `IconCard` 组织在适配主题的表面中。

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
- [Card](card.md)：非交互式信息表面，以及可选的图标或图片展示内容。

其余控件将随重构逐步迁移到本栏目。当前可从[控件库概览](../articles/control-library.md)了解迁移状态。
