---
title: 控件库
description: 加载 Flourish 主题资源，并使用显式自定义控件、语义变体与悬停行为。
---

# 控件库

Flourish 为应用页面、Shell 扩展区域、对话框和独立承载的窗口提供带主题的 WPF 自定义控件。需要使用 Flourish 主题与交互状态时，应选择对应的 `Flourish*` 控件。

加载 Flourish 资源不会为 WPF 基础类型安装隐式样式。WPF `<Button>`、`<TextBox>` 或 `<ListBox>` 会保留原生外观；`<flourish:FlourishButton>`、`<flourish:FlourishTextBox>` 与 `<flourish:FlourishListBox>` 会显式使用 Flourish 模板。

## 加载控件资源

`FlourishBuilder` 创建的运行时会在显示 Shell 前，把 Flourish 控件与主题资源加入 `Application.Resources`。如果控件只会在 `IFlourish.Show(Application)` 或 `Run(Application)` 之后创建，则无需再次声明资源。

以下情况应显式加入 `FlourishThemeResources`：WPF 设计器需要控件资源、Shell 启动前就要创建控件，或者应用不使用 Flourish Shell。

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

`http://schemas.arkheide.system/flourish` 是 Flourish 控件与主题资源的公共 XAML 命名空间。在 Application 级别加入一个 `FlourishThemeResources` 实例即可；不要再通过 URI 合并 Flourish 主题字典。

> [!WARNING]
> `FlourishStyles` 与 `FlourishControlResources` 已过时。请使用 `FlourishThemeResources`。

## 可用控件

公共控件库提供以下控件族：

| Flourish 控件 | 用途 |
| --- | --- |
| `FlourishButton` | 操作按钮，支持主要、低强调、卡片和破坏性外观。 |
| `FlourishCard` | 在主题表面上组合相关内容。 |
| `FlourishTextBlock`、`FlourishLabel` | 语义文字角色与支持访问键的表单标签。 |
| `FlourishTextBox`、`FlourishPasswordBox`、`FlourishSearchBox` | 文字、密码与搜索输入。 |
| `FlourishCheckBox`、`FlourishRadioButton` | 独立选择与互斥选择。 |
| `FlourishComboBox`、`FlourishComboBoxItem` | 下拉选择及自动生成的项目容器。 |
| `FlourishListBox`、`FlourishListBoxItem` | 列表选择及自动生成的项目容器。 |
| `FlourishScrollViewer`、`FlourishScrollBar` | 滚动区域与滚动条。 |
| `FlourishToolTip`、`FlourishGridSplitter` | 主题化提示与布局调整。 |

属性、变体和附加行为参见 [Controls API](xref:ArkheideSystem.Flourish.Controls)。

## 使用控件

在页面 XAML 中显式引用 Flourish 控件：

```xml
<StackPanel
  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  xmlns:flourish="http://schemas.arkheide.system/flourish">
  <flourish:FlourishTextBlock
    Role="PageTitle"
    Text="账户" />

  <flourish:FlourishTextBlock
    Role="Subtitle"
    Text="管理当前用户资料与登录状态。" />

  <flourish:FlourishTextBlock
    Role="SectionTitle"
    Text="账户信息" />
  <flourish:FlourishTextBlock
    Role="Description"
    Text="编辑当前用户可见的资料。" />

  <flourish:FlourishCard>
    <StackPanel>
      <flourish:FlourishTextBlock
        Role="FieldLabel"
        Text="显示名称" />
      <flourish:FlourishTextBox Text="Foo Bar" />
      <flourish:FlourishSearchBox Placeholder="搜索账户" />
      <StackPanel>
        <flourish:FlourishButton
          Appearance="Subtle"
          Content="取消" />
        <flourish:FlourishButton
          Appearance="Primary"
          Content="保存" />
      </StackPanel>
    </StackPanel>
  </flourish:FlourishCard>
</StackPanel>
```

### FlourishButton

`FlourishButton.Appearance` 描述操作语义：

- `Standard` 是默认操作。
- `Primary` 表示一组操作中的主要操作。
- `Subtle` 降低视觉强调。
- `Card` 将整个按钮显示为可交互卡片。
- `Danger` 表示破坏性操作，并使用警示反馈。

外部 `Margin` 等位置属性由布局容器控制。鼠标与键盘焦点使用不同状态，键盘焦点保持可见。

### FlourishTextBlock

`FlourishTextBlock.Role` 选择语义排版。可用角色包括 `Body`、`Paragraph`、`Caption`、`Muted`、`FieldLabel`、`Subtitle`、`Description`、`CardTitle`、`SectionTitle`、`PageTitle`、`Status` 和 `Icon`。

`Paragraph` 提供换行正文和更大的行距，`Description` 表示标题下方的辅助文字，`CardTitle` 表示卡片或紧凑内容区域中的标题。Role 使用当前字体和主题资源；显式设置的文字属性具有更高优先级。

### FlourishCard 与 FlourishSearchBox

`FlourishCard` 组合一个内容树，外观包括 `Standard`、`Subtle`、`Accent`、`Elevated` 和 `Hero`。`Elevated` 提供层次效果，`Hero` 使用渐变背景和层次效果显示引导内容。需要让整个表面可交互时，使用 `FlourishButton Appearance="Card"`。

`FlourishSearchBox` 提供搜索图标和 `Placeholder`，同时保留文字绑定、命令、选择和 `TextChanged`。

## HoverReveal 与减少动态效果

参与悬停反馈的 Flourish 控件使用公共 `HoverReveal` 附加行为。通过[动效](configure-motion.md)进行应用级配置，并遵循操作系统的减少动态效果偏好：

```csharp
builder
    .ConfigureShell(shell => shell.UseMotion())
    .ConfigureMotion(motion =>
        motion
            .EnableHoverRevealAnimation(TimeSpan.FromMilliseconds(140))
            .RespectSystemReducedMotion());
```

附加属性可以提供局部覆盖：

```xml
<flourish:FlourishButton
  flourish:HoverReveal.IsEnabled="True"
  flourish:HoverReveal.AnimationDuration="0:0:0.14"
  Content="预览" />
```

接入该行为的自定义模板应提供名为 `HoverChrome` 和 `HoverRevealScale` 的元素，并设置 `flourish:HoverReveal.IsParticipant="True"`。`IsEnabled` 和 `AnimationDuration` 会沿视觉树继承，`IsParticipant` 不继承。缺少任一命名元素时，该行为不生效。

替换模板自行定义静态悬停和按下状态时，设置 `flourish:HoverReveal.TemplateHandlesInteraction="True"`；否则由 HoverReveal 提供这些指针状态。

## 主题与语义资源

切换主题时，Flourish 控件、自动生成的项目容器、弹出层和滚动条无需重建页面即可更新。[主题](configure-themes.md)用于配置主题模式、品牌色和共用圆角，[排版](configure-font.md)用于配置全局和页面字体。

覆盖语义主题资源时，应在亮色和暗色主题下验证结果，并保持文字对比度。

## 相关功能

- [快速开始](getting-started.md)说明运行时启动和资源加载。
- [主题](configure-themes.md)配置主题模式、品牌色和圆角。
- [排版](configure-font.md)修改全局和页面字体。
- [动效](configure-motion.md)配置 HoverReveal 与减少动态效果行为。
- [Themes API](xref:ArkheideSystem.Flourish.Themes)说明 `FlourishThemeResources`。
