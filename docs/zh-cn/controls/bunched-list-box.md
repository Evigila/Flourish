---
title: BunchedListBox
description: 使用一个由父控件管理、可在项目之间连续移动的交互显示层呈现可选集合。
---

# BunchedListBox

当相邻项目需要表现为一个协调的集合时，推荐使用 `BunchedListBox`。它保留原生 WPF `ListBox` 的选择、键盘、自动化、数据绑定、滚动和虚拟化行为，同时把悬停、按下与选中背景从每个项目容器移到父控件统一管理的显示层。

每个项目确实需要独立交互表面时使用 `ListBox`。需要让指针反馈或单选指示器在项目之间连续移动时使用 `BunchedListBox`。

## 绑定集合

通过标准 `ListBox` 成员绑定项目和选择。数据项目会自动包装为 `BunchedListBoxItem`。

```xml
<flourish:BunchedListBox
  ItemsSource="{Binding Projects}"
  SelectedItem="{Binding SelectedProject, Mode=TwoWay}" />
```

控件支持数据模板、项目容器样式、`SelectedValuePath` 及其他继承的集合成员，并继承 `ListBox` 的 `Appearance` 和 `IsCompact` 外观选项。

## 统一交互层

默认模板包含一个不参与命中测试的显示层，负责悬停、按下和选中反馈。在项目之间移动时，控件会重定向已有指示器，而不是先结束一个项目的动画再启动下一个项目的动画。显示层根据已实现容器的实际几何信息定位，因此支持不等高项目、边距、横向面板、从右向左布局和滚动。

显示层会裁剪到实际滚动视口，并位于项目内容下方。如果自定义 `BunchedListBoxItem` 样式需要显示统一反馈，请保持项目背景透明。

`HoverReveal.IsEnabled`、`HoverReveal.IsMotionEnabled` 和 `HoverReveal.AnimationDuration` 控制显示层是否以动画方式移动。关闭动效后仍会保留即时静态反馈。可以使用 `HoverReveal.OverrideColor` 局部替换悬停笔刷。

## 选择

`SelectionMode="Single"` 使用一个移动到已实现选中容器的选择指示器。`Multiple` 和 `Extended` 为每个已实现的选中容器在统一显示层中显示一个指示器。离屏选中项不会被强制加入可视树，因此不会破坏 UI 虚拟化。

键盘选择、Ctrl/Shift 选择、`ScrollIntoView` 和选择事件仍使用原生 `ListBox` 行为。

```xml
<flourish:BunchedListBox
  ItemsSource="{Binding Tasks}"
  SelectionMode="Extended" />
```

## 使用显式容器

通常应让父控件生成 `BunchedListBoxItem`。只有导航项需要 `IsGroupHeader` 或 `IsCommandItem` 等继承的项目级状态时，才直接声明容器。

```xml
<flourish:BunchedListBox Appearance="Borderless">
  <flourish:BunchedListBoxItem
    Content="Workspace"
    IsGroupHeader="True" />
  <flourish:BunchedListBoxItem Content="Overview" />
</flourish:BunchedListBox>
```

显式提供的 `FlourishListBoxItem` 会被视为数据，并由 `BunchedListBoxItem` 包装；直接提供容器时请使用 Bunched 容器类型。

## 自定义模板

默认控件模板提供 `PART_InteractionViewport`、`PART_IndicatorLayer`、`PART_SelectionChrome`、`PART_HoverChrome`、`PART_PressedChrome` 和 `PART_ScrollViewer`。自定义模板可以省略这些部件并保留普通 `ListBox` 选择行为，但统一交互反馈需要相应部件。显示层必须保持不参与命中测试，并且不能包装项目呈现器，以免中断命中测试、逻辑滚动或虚拟化。

## 相关内容

- [BunchedListBox API](xref:ArkheideSystem.Flourish.Controls.BunchedListBox) 列出继承成员与声明成员。
- [BunchedListBoxItem API](xref:ArkheideSystem.Flourish.Controls.BunchedListBoxItem) 说明生成的容器。
- [动效](../articles/configure-motion.md)配置悬停揭示与减少动态效果行为。
- [WPF ListBox 文档](https://learn.microsoft.com/dotnet/desktop/wpf/controls/listbox)介绍原生选择模型。
