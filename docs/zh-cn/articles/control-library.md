---
title: 控件库
description: 使用显式 Flourish 自定义控件、HoverReveal 与语义主题资源。
---

# 控件库

Flourish 不仅提供 Shell 组合 API，也包含一套控件库。应用页面、Shell 区域、对话框以及独立承载的 WPF 窗口可以共享紧凑尺寸、小圆角、排版、配色与交互状态。

视觉契约采用显式接入方式：需要 Flourish 外观时，应使用 `Flourish*` 自定义控件。加载主题不会为 WPF 基础类型安装隐式样式，因此普通的 WPF `<Button>`、`<TextBox>` 或 `<ListBox>` 会保留原生 WPF 外观；`<flourish:FlourishButton>`、`<flourish:FlourishTextBox>` 与 `<flourish:FlourishListBox>` 才会使用统一的 Flourish 模板。

## 控件库结构

项目按职责划分：

- `ArkheideSystem.Flourish.Abstract` 存放运行时与 Builder 契约。
- `ArkheideSystem.Flourish.Controls` 存放可复用视觉控件及其语义属性。
- `ArkheideSystem.Flourish.Themes` 存放规范资源入口 `FlourishThemeResources` 与主题资源图。
- `Assets` 存放内置资源；`Internal/Composition`、`Internal/Configuration` 与 `Internal/Imaging` 存放实现细节，`Services` 存放内部运行时服务。
- `Views/Windows` 与 `Views/Page` 存放库自身的展示界面，不再形成额外的公共控件层。

每个公共视觉控件都会让实现与资源字典保持相邻，例如 `Button.xaml` 与 `Button.xaml.cs` 共同承载 `FlourishButton`，其他公共控件也遵循相同配对。这些类型是可模板化的 WPF 自定义控件，而不是 `UserControl` 组合，因此使用方可以替换或扩展模板，而不必继承固定的视觉树。

原有的物理 `Styles` 层已移除。`ArkheideSystem.Flourish.Styles.FlourishStyles` 与 `ArkheideSystem.Flourish.Controls.FlourishControlResources` 只作为带 `[Obsolete]` 标记的源码兼容入口保留，二者都不再是规范资源入口。

## 主题资源图

`Themes/Generic.xaml` 是唯一组合根。`FlourishThemeResources` 会加载该文件，应用不应单独合并其子字典：

- `Themes/Colors/Colors.xaml` 是配色入口；`Colors.Light.xaml` 与 `Colors.Dark.xaml` 分别存放亮色、暗色语义画刷。
- `Themes/Controls.xaml` 直接收录全部公共控件字典。它是内部聚合细节，不是应用级入口。
- `Themes/Typography.xaml` 定义共享字体族、字号与排版 token。
- `Themes/Layout.xaml` 定义展示界面共用的 Shell 与内容布局度量。

这条单根资源链避免同一控件字典先经过样式层、再经过主题包装层被重复收录，也让运行时主题切换只有一个权威配色位置。

## 加载控件资源

由 `FlourishBuilder` 创建的运行时会在显示 Shell 前，把完整 Flourish 资源图加载到 `Application.Resources`。如果控件只会在 `IFlourish.Show(Application)` 或 `Run(Application)` 之后创建，就不需要再次声明资源。

以下情况应显式合并 `FlourishThemeResources`：WPF 设计器需要控件资源、Shell 启动前就要创建控件，或者应用希望脱离 Flourish Shell 独立使用控件库。

```xml
<Application
  x:Class="MyApp.App"
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

`http://schemas.arkheide.system/flourish` 是公共控件与主题资源的稳定 XAML 命名空间。相较于绑定当前程序集结构的 `clr-namespace`，应优先使用这个 URI。只需在 Application 级别加入一次 `FlourishThemeResources`，不要再通过 URI 合并 `Themes/Generic.xaml` 或任一子字典。

## 公共控件覆盖范围

公共控件集合覆盖 Shell 与 Gallery 当前实际使用的控件：

| Flourish 控件 | 用途 |
| --- | --- |
| `FlourishButton`、`FlourishCard` | 操作，以及分组或可交互表面，支持语义外观。 |
| `FlourishTextBlock`、`FlourishLabel` | 语义文字角色，以及支持访问键的表单标签。 |
| `FlourishTextBox`、`FlourishPasswordBox`、`FlourishSearchBox` | 具有统一字段状态的文字、密码与搜索输入。 |
| `FlourishCheckBox`、`FlourishRadioButton` | 独立选择与互斥选择。 |
| `FlourishComboBox`、`FlourishComboBoxItem` | 下拉选择；自动生成的选项容器同样是 Flourish 控件。 |
| `FlourishListBox`、`FlourishListBoxItem` | 列表选择及其自动生成的项目容器，也作为导航基础。 |
| `FlourishScrollViewer`、`FlourishScrollBar` | 统一滚动表面与滚动条外观。 |
| `FlourishToolTip`、`FlourishGridSplitter` | 主题化提示与紧凑布局调整区域。 |

控件模板可以把 WPF 的 `ToggleButton`、`RepeatButton` 和 `Thumb` 作为私有模板部件，并使用局部命名资源。Flourish 不会为这些基础类型或其他原生 WPF 控件注册宽泛的隐式样式，因此加载主题不会顺带改变无关控件。

## 使用控件

在页面 XAML 中显式引用 Flourish 控件：

```xml
<StackPanel
  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  xmlns:flourish="http://schemas.arkheide.system/flourish"
  Margin="24">
  <flourish:FlourishTextBlock
    Role="PageTitle"
    Text="账户" />

  <flourish:FlourishTextBlock
    Role="Subtitle"
    Text="管理当前用户资料与登录状态。" />

  <flourish:FlourishTextBlock
    Margin="0,24,0,0"
    Role="SectionTitle"
    Text="账户信息" />
  <flourish:FlourishTextBlock
    Margin="0,6,0,0"
    Role="Description"
    Text="编辑当前用户可见的资料。" />

  <flourish:FlourishCard Margin="0,12,0,0">
    <StackPanel>
      <flourish:FlourishTextBlock
        Role="FieldLabel"
        Text="显示名称" />
      <flourish:FlourishTextBox Text="Ada Lovelace" />
      <flourish:FlourishSearchBox
        Margin="0,12,0,0"
        Placeholder="搜索账户" />
      <StackPanel
        Margin="0,16,0,0"
        HorizontalAlignment="Right"
        Orientation="Horizontal">
        <flourish:FlourishButton
          Appearance="Subtle"
          Content="取消" />
        <flourish:FlourishButton
          Margin="8,0,0,0"
          Appearance="Primary"
          Content="保存" />
      </StackPanel>
    </StackPanel>
  </flourish:FlourishCard>
</StackPanel>
```

### FlourishButton

`FlourishButton.Appearance` 用语义表达按钮意图，无需暴露模板资源键：

- `Standard` 是默认的中性操作。
- `Primary` 是一组操作中的主要操作。
- `Subtle` 用于安静表面上的低强调操作。
- `Card` 让整个按钮成为可交互卡片。
- `Danger` 表示破坏性操作，并使用红色交互反馈。

一个局部决策区域通常只应有一个 Primary 操作。外部 Margin 仍由布局容器负责；Appearance 只控制按钮自身，不负责其摆放。

鼠标点击不会在按钮上残留焦点边框。通过键盘导航获得焦点时，按钮仍会显示随主题变化的焦点指示，使焦点状态不会与悬停或按下反馈混淆。

### FlourishTextBlock

`FlourishTextBlock.Role` 选择语义排版。可用角色包括 `Body`、`Paragraph`、`Caption`、`Muted`、`FieldLabel`、`Subtitle`、`Description`、`CardTitle`、`SectionTitle`、`PageTitle`、`Status` 和 `Icon`。`Description` 用于紧凑的辅助说明，`Paragraph` 用于带舒适行距的正文段落；标题角色会逐级增大字号和字重。

Role 会使用当前字体和主题资源，因此应优先用它替代页面中反复出现的 `FontSize`、`FontWeight` 与 `Foreground`。内容确实存在一次性需求时，显式属性仍然拥有更高优先级。

### FlourishCard 与 FlourishSearchBox

`FlourishCard` 在主题表面上组合一个内容树，外观包括无描边的 `Standard`、`Subtle`、`Accent`，以及带阴影的展示型 `Elevated` 和 `Hero`。`Hero` 适合页面顶部的少量引导内容，`Elevated` 适合需要纵深的展示内容；大量重复内容应优先使用无阴影表面。可点击内容使用 `FlourishButton Appearance="Card"`，不要用点击事件伪装普通展示卡。

遵循页面布局层级时，区域的 `SectionTitle` 与 `Description` 应放在卡片之外，卡片内部只承载操作、字段和具体细节。正文段落之间使用较大的 Section 间距，同一组字段或按钮则使用较小的 peer 间距。

`FlourishSearchBox` 是带搜索外观和 `Placeholder` 的 TextBox。它保留普通 TextBox 的 `Text`、绑定、命令、选择和 `TextChanged` 等能力，同时避免每个页面重复拼接搜索图标、边框和占位文本层。

## HoverReveal 与减少动态效果

支持悬停揭示的 Flourish 模板会使用公共 `HoverReveal` 附加行为。建议通过[动效](configure-motion.md)进行应用级配置，使 `RespectSystemReducedMotion()` 能在 Windows 请求减少动态效果时抑制动画：

```csharp
builder.ConfigureMotion(motion =>
    motion
        .EnableHoverRevealAnimation(TimeSpan.FromMilliseconds(140))
        .RespectSystemReducedMotion());
```

附加属性也可以提供局部覆盖，或供自定义模板接入：

```xml
<flourish:FlourishButton
  flourish:HoverReveal.IsEnabled="True"
  flourish:HoverReveal.AnimationDuration="0:0:0.14"
  Content="预览" />
```

参与该行为的自定义模板应同时提供名为 `HoverChrome` 和 `HoverRevealScale` 的元素，并在控件上设置 `flourish:HoverReveal.IsParticipant="True"`。`IsParticipant` 不继承，而 `IsEnabled` 和 `AnimationDuration` 是可继承的策略值，因此容器可以统一禁用或调整动效时长，而不会给每个视觉后代都挂载行为。缺少任一模板部件时，HoverReveal 会安全地不执行任何操作。

HoverReveal 可以自行管理指针交互，也允许参与该行为的模板提供自己的静态悬停和按下状态。只有模板为禁用揭示动效的情况提供无动画悬停 fallback，并自行处理按下状态，才应设置 `flourish:HoverReveal.TemplateHandlesInteraction="True"`。为控件局部指定 `Template` 时，除非同时在本地设置 `TemplateHandlesInteraction`，否则使用行为管理的交互。通过自定义 `Style` 替换模板时，应根据替换模板的能力显式设置该属性。文字输入控件使用悬停和焦点描边，不使用 HoverReveal。

普通按钮、交互式卡片按钮、列表项与导航项在亮色和暗色主题下使用同一套鲜明、随主题变化的蓝色揭示反馈。选中状态会在该反馈下保持可见，按下反馈则使用独立状态。`Danger` 是语义例外：破坏性按钮使用红色悬停和按下反馈，而不是蓝色揭示。

## 主题与语义 token

控件模板通过 `DynamicResource` 使用主题和排版资源。因此切换主题时，控件、自动生成的项目容器、ComboBox 弹出层和滚动条都能更新，无需重新创建页面。主要语义分组包括：

- 主要文字和弱化文字；
- 强调色和强调色上的内容；
- 输入控件边界、低对比分隔线与无描边表面；
- 悬停、按下、选中、焦点和禁用状态；
- 标准、强调、Hero、Elevated 与交互式卡片表面；
- 全局正文字体、图标字体与排版字号。

应用级修改应使用[主题](configure-themes.md)和[排版](configure-font.md)。如果需要覆盖单个画刷或排版资源，应覆盖语义资源，而不是复制 ControlTemplate；这样所有控件与状态仍能保持协调。应用品牌色后应同时检查亮色、暗色主题和文字对比度。

## 为什么暂不覆盖其他控件族

当前 Shell 与 Gallery 没有使用 `TreeView`、`DataGrid`、`ListView` 或 `GridView`，因此 Flourish 暂不承诺它们具备统一模板。这些控件族还需要为层级、虚拟化、编辑、排序、表头、列、选择和键盘导航定义额外契约。在没有真实产品页面验证完整状态模型时实现，会形成未经测试的视觉承诺。

这些标准 WPF 控件仍然可以正常使用，但暂不属于 Flourish 当前的视觉契约。后续应用或 Gallery 页面真正覆盖其完整状态模型时，可以再将其加入控件库。

## 相关功能

- [快速开始](getting-started.md)说明运行时启动和资源加载。
- [主题](configure-themes.md)选择当前亮色、暗色或跟随系统的配色。
- [排版](configure-font.md)修改全局字体和页面字体。
- [动效](configure-motion.md)配置 HoverReveal 与减少动态效果行为。
- [Controls API](xref:ArkheideSystem.Flourish.Controls)列出公共控件、枚举与附加属性。
- [Themes API](xref:ArkheideSystem.Flourish.Themes)列出规范资源入口。
