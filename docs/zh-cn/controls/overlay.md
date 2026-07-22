---
title: Overlay
description: 使用统一主题表面、Vertical ActionCard 和明确的关闭语义构建浮层视图。
---

# Overlay

`Overlay` 是承载锚定浮动内容的主题化容器。它负责表面样式与关闭请求约定；Popup、Canvas 或 Shell 宿主负责定位、打开状态和最终关闭操作。

Overlay 通常与 `ActionCard Variant="Vertical"` 组合，使图标、标题、文案和局部操作从上到下靠左排列。内容需要不同结构时也可以直接提供自定义布局；Profile 浮层就是适合自定义排版的典型场景。

## 变体

| 变体 | 行为 | 典型用途 |
| --- | --- | --- |
| `Temporary` | 设置 `PlacementTarget` 后，指针同时离开触发器与 Overlay 时，会在短暂宽限后引发 `DismissRequested`。 | Tooltip、Logo 详情和设备信息。 |
| `Strong` | 指针移动不会请求关闭；宿主保持打开，直到用户执行明确关闭操作。 | Profile 与可交互任务视图。 |

宽限时间允许指针跨过锚点与 Overlay 之间的间隙。指针返回任一元素都会取消待处理的关闭请求。

```xml
<flourish:Overlay
  x:Name="DetailsOverlay"
  PlacementTarget="{Binding ElementName=DetailsButton}"
  Variant="Temporary"
  DismissRequested="DetailsOverlay_DismissRequested">
  <flourish:ActionCard
    Variant="Vertical"
    Icon="&#xE946;"
    Title="工作区详情"
    Content="查看当前工作区的同步状态。">
    <flourish:Button
      Command="{Binding OpenWorkspaceCommand}"
      Content="打开工作区" />
  </flourish:ActionCard>
</flourish:Overlay>
```

在 `DismissRequested` 中更新外围 Popup 或 Shell 宿主拥有的打开状态。Strong Overlay 不会因指针移动而引发此事件。

## 内容布局

Vertical ActionCard 是具有单个局部操作的标准浮窗视图：

- Icon、Title、Content 和 Body 依次向下排列并统一靠左；
- Body 中只放一个按钮、选择器、输入框或同类局部控件；
- 任一可选区域为空时，其空间和相关间距完全折叠。

需要多个区域、资料头像、状态摘要或其他专用结构时，可以让 Overlay 直接承载自定义布局：

```xml
<flourish:Overlay Variant="Strong">
  <Grid>
    <Grid.ColumnDefinitions>
      <ColumnDefinition Width="Auto" />
      <ColumnDefinition Width="*" />
    </Grid.ColumnDefinitions>
    <Image
      Width="48"
      Height="48"
      Source="{Binding Avatar}" />
    <StackPanel Grid.Column="1" Margin="12,0,0,0">
      <TextBlock Text="{Binding DisplayName}" />
      <flourish:Button
        Command="{Binding SignOutCommand}"
        Content="退出登录"
        Variant="Text" />
    </StackPanel>
  </Grid>
</flourish:Overlay>
```

自定义布局不会改变 Overlay 的关闭语义；宿主仍需管理打开状态与键盘、外部点击等关闭方式。

## 在应用界面中托管 Overlay

应用页面可以将 Overlay 放入 WPF `Popup`。将 Popup 与 Overlay 的定位目标都设为可交互触发器，由 Popup 管理 `IsOpen`，并在 Overlay 请求关闭时更新该状态：

```xml
<flourish:Button
  x:Name="DetailsButton"
  Content="显示详情"
  Click="DetailsButton_Click" />
<Popup
  x:Name="DetailsPopup"
  AllowsTransparency="True"
  Placement="Bottom"
  StaysOpen="True">
  <flourish:Overlay
    x:Name="DetailsOverlay"
    Variant="Temporary"
    DismissRequested="DetailsOverlay_DismissRequested">
    <flourish:ActionCard
      Variant="Vertical"
      Title="工作区详情"
      Content="当前工作区已同步。" />
  </flourish:Overlay>
</Popup>
```

```csharp
DetailsPopup.PlacementTarget = DetailsButton;
DetailsOverlay.PlacementTarget = DetailsButton;

private void DetailsButton_Click(object sender, RoutedEventArgs e) =>
    DetailsPopup.IsOpen = true;

private void DetailsOverlay_DismissRequested(object sender, RoutedEventArgs e) =>
    DetailsPopup.IsOpen = false;
```

Strong Overlay 的宿主还必须提供明确关闭方式，例如操作按钮、<kbd>Esc</kbd> 和外部点击。Popup 可以通过 `StaysOpen="False"` 提供外部点击关闭行为。

## Shell 集成

Flourish Shell 功能将 Overlay 托管在窗口范围内的浮层中，而不是页面 Popup 中。Shell 负责计算锚定位置、更新宿主可见性，并处理 `DismissRequested`、外部点击和 <kbd>Esc</kbd>。添加 Shell 功能时，应调用对应的 Shell 集成点，而不是要求 Overlay 自行打开。

使用 [Button](button.md) 或 `CardButton` 作为触发器。这些控件提供点击或命令激活、键盘焦点和自动化语义。普通 `Card` 只呈现信息；`ActionCard` 的交互属于其 Body，而不是完整卡片表面。

## Tooltip 集成

启用 `UseTips` 后，Flourish 控件使用包含单个 Temporary Overlay 的 `FlourishToolTip` 模板呈现自有提示。打开、延迟、Popup 定位与关闭仍由 WPF `ToolTipService` 负责，因此嵌套 Overlay 不设置 `PlacementTarget`。

省略 `UseTips` 或在运行时禁用 ToolTips 功能时，Flourish 控件会使用原生 WPF Tooltip 外观和默认行为呈现同一份提示内容。附加到原生 WPF 与第三方控件的 Tooltip 保留各自模板和行为。

## 相关控件

- [ActionCard](card.md#actioncard)提供常见的 Vertical 浮窗内容结构。
- [Button](button.md)可作为 Overlay 触发器或内部操作。
- [ScrollViewer](scroll-viewer.md)可承载超出 Overlay 可用高度的内容。
- [Overlay API](xref:ArkheideSystem.Flourish.Controls.Overlay) 与 [OverlayVariant API](xref:ArkheideSystem.Flourish.Controls.OverlayVariant) 列出完整成员。
