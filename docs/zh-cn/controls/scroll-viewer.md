---
title: ScrollViewer 与 ScrollBar
description: 使用平滑像素滚动与标准 Flourish 滚动条承载超出视口的内容。
---

# ScrollViewer 与 ScrollBar

`ScrollViewer` 用于承载可能超出可用视口的内容。其水平与垂直 `ScrollBar` 部件统一使用标准 Flourish 外观：可见滑块较窄，并保留更大的透明交互区域。鼠标滚轮输入可以平滑呈现，同时不必在每个动画帧触发布局计算。

通过 Flourish XML 命名空间区分该控件与同名的 WPF 类型：

```xml
<flourish:ScrollViewer
  HorizontalScrollBarVisibility="Disabled"
  VerticalScrollBarVisibility="Auto">
  <Grid>
    <!-- 页面内容 -->
  </Grid>
</flourish:ScrollViewer>
```

## 平滑滚动

`IsSmoothScrollingEnabled` 默认为 `true`。鼠标滚轮滚动期间，控件使用渲染变换推进可见内容，并以较低频率同步逻辑偏移。滚动条、键盘导航、滑块拖动与程序化滚动仍以逻辑偏移为准。

需要立即执行 WPF 原生像素滚动时，设置 `IsSmoothScrollingEnabled="False"`。

应用还可以在组合阶段为 Flourish 自有的 Shell、导航与页面滚动区域应用同一策略：

```csharp
builder.ConfigureShell(shell =>
    shell.UseSmoothScroll(enabled: true));
```

`UseSmoothScroll` 为内置模板创建、因而无法从应用 XAML 直接访问的 Flourish 滚动区域提供默认值。完整 Shell 需要立即使用原生滚动时，将其设为 `false`。如果只有某一个由应用持有的 `ScrollViewer` 需要不同的行为，仍应在该控件上显式设置 `IsSmoothScrollingEnabled`。

## 嵌套视口

使用默认物理滚动模式时，鼠标滚轮输入从最深层的 Flourish `ScrollViewer` 开始处理。只要内部视口在当前方向上仍可移动，它就会消费滚轮；到达顶部或底部边界后，继续向外滚动的输入会保留给父级视口，因此紧凑的内部历史不会阻断页面滚动。

## 自定义模板

平滑像素滚动要求 `PART_ScrollContentPresenter` 保持静止，并在其内部放置名为 `PART_SmoothScrollContentHost` 的 `ContentPresenter`。控件只对这个专用宿主应用逐帧变换，使视口裁剪区域保持固定。模板缺少该宿主时，鼠标滚轮输入会安全回退到原生滚动。

用于 `CanContentScroll="True"` 的模板应让虚拟化 `IScrollInfo` 或 `ItemsPresenter` 与 `PART_ScrollContentPresenter` 保持直接连接，并省略平滑滚动宿主，从而保留 WPF 逻辑滚动与容器回收。

## 虚拟化项目控件

当 `CanContentScroll` 为 `true` 时，`ScrollViewer` 保留 WPF 逻辑滚动，不会将项目偏移误作像素偏移，因此项目虚拟化面板可以保持正确行为。包含大量项目的控件还应在所属控件上启用容器回收：

```xml
<ListBox
  ScrollViewer.CanContentScroll="True"
  VirtualizingPanel.IsVirtualizing="True"
  VirtualizingPanel.VirtualizationMode="Recycling" />
```

不要在虚拟化项目控件外再嵌套一个 `ScrollViewer`；应由项目控件拥有滚动视口。

## 滚动条外观

Flourish 为页面、Shell、导航和控件内部视口统一使用一种标准 `ScrollBar` 外观。可见滑块比透明交互区域更窄，因此滚动条能保持轻盈外观，同时不必要求过于精确的指针拖动。较小的圆角可以柔化两端，同时避免较短的横轴轮廓显得尖锐或呈胶囊状。滚动条不再提供需要选择的紧凑外观变体。

## 相关功能

- [控件](index.md)
- [Chunk](chunk.md)
- [DataGrid](data-grid.md) 在内部滚动边界使用相同的滚轮交接规则。
