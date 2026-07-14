---
title: 控件库
description: 加载 Flourish 主题资源，并使用显式自定义控件、语义外观与悬停行为。
---

# 控件库

Flourish 为应用页面、Shell 扩展区域、对话框和独立承载的窗口提供带主题的 WPF 自定义控件。需要使用 Flourish 主题与交互状态时，应选择 Flourish XAML 命名空间中的对应控件。

加载 Flourish 资源不会为 WPF 基础类型安装隐式样式。WPF `<Button>`、`<TextBox>` 或 `<ListBox>` 会保留原生外观；`<flourish:Button>`、`<flourish:FlourishTextBox>` 与 `<flourish:FlourishListBox>` 会显式使用 Flourish 模板。

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

`http://schemas.arkheide.system/flourish` 是 Flourish 控件与主题资源的公共 XAML 命名空间。在 Application 级别加入一个 `FlourishThemeResources` 实例；Flourish 不支持通过 URI 合并主题字典。

## 可用控件

公共控件库包含 20 个控件。除下表明确列出其他基类外，每个控件都继承对应的 WPF 控件，并保留该基类的属性、事件、命令、数据绑定、验证和自动化行为。

| Flourish 控件 | 基类 | Flourish 特有契约 |
| --- | --- | --- |
| `Button` | WPF `Button` | `Appearance` 选择四种语义强调级别之一。 |
| `IconButton` | `Button` | 增加 `Icon`，用于仅图标或图标加文字的操作。 |
| `WindowCaptionButton` | `IconButton` | 提供专用窗体标题栏几何和关闭按钮反馈。 |
| `CardButton` | `Button` | 增加 `Icon`、`IconPosition` 与 `Title`；继承的 `Content` 是描述。 |
| `FlourishCard` | `ContentControl` | `Appearance` 选择表面效果；控件承载一棵内容树。 |
| `FlourishTextBlock` | `TextBlock` | `Role` 选择语义排版。 |
| `FlourishLabel` | `Label` | 无新增属性；保留 `Target` 和原生访问键支持。 |
| `FlourishTextBox` | `TextBox` | 无新增属性；提供 Flourish 输入模板。 |
| `FlourishPasswordBox` | `Control` | 提供密码访问、长度与掩码设置、变更事件及编辑方法。 |
| `FlourishSearchBox` | `FlourishTextBox` | 增加 `Placeholder` 和搜索图标。 |
| `FlourishCheckBox` | `CheckBox` | 无新增属性；支持 WPF 两态和三态选择。 |
| `FlourishRadioButton` | `RadioButton` | 无新增属性；使用 WPF `GroupName` 分组。 |
| `FlourishComboBox` | `ComboBox` | 为数据项自动生成 `FlourishComboBoxItem` 容器。 |
| `FlourishComboBoxItem` | `ComboBoxItem` | 组合框的主题化项目容器。 |
| `FlourishListBox` | `ListBox` | `Appearance` 选择标准列表或导航列表；`IsCompact` 折叠导航几何。 |
| `FlourishListBoxItem` | `ListBoxItem` | 增加导航可见性、分组标题和命令项状态。 |
| `FlourishScrollViewer` | `ScrollViewer` | `IsCompact` 选择紧凑滚动条。 |
| `FlourishScrollBar` | `ScrollBar` | 主题化的垂直或水平滚动条。 |
| `FlourishToolTip` | `ToolTip` | 默认样式启用可感知 Shell 区域的定位。 |
| `FlourishGridSplitter` | `GridSplitter` | `Variant` 选择标准或导航面板调整控件。 |

`Content`、`Text`、`ItemsSource`、`SelectedItem`、`Command`、`IsEnabled`、`Margin` 和 `ToolTip` 等普通 WPF 属性按相应基类的方式使用。下文列出 Flourish 增加的属性及其默认值；完整的继承成员与签名参见 [Controls API](xref:ArkheideSystem.Flourish.Controls)。

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
        <flourish:Button
          Appearance="Subtle"
          Content="取消" />
        <flourish:Button
          Appearance="Primary"
          Content="保存" />
      </StackPanel>
    </StackPanel>
  </flourish:FlourishCard>
</StackPanel>
```

### 按钮与内容表面

`Button.Appearance` 描述操作语义，默认值为 `Standard`：

- `Standard` 是默认操作。
- `Primary` 表示一组操作中的主要操作。
- `Subtle` 降低视觉强调。
- `Danger` 表示破坏性操作，并使用警示反馈。

所有普通 `Button` 使用相同的模板和几何。只有承载位置确实需要不同尺寸时，才设置继承的 `Width`、`Height`、`MinWidth` 或 `Padding` 等尺寸属性。`Appearance` 只改变语义强调，不选择结构布局。

只要操作带有图标（包括工具栏和状态区域操作），就应使用 `IconButton`。`Icon` 的类型为 `object`，可接受字形字符串或可视元素；继承的 `Content` 可选，设置后作为文字标签。仅图标的 `IconButton` 默认使用无内边距的紧凑 `30 × 30` 几何；图标加文字的按钮保持普通 `Button` 几何。标题栏和状态栏等具有自身紧凑度量的宿主，应显式设置本地尺寸属性。

```xml
<flourish:IconButton
  Appearance="Subtle"
  Command="{Binding RefreshCommand}"
  Icon="&#xE72C;"
  ToolTip="刷新" />

<flourish:IconButton
  Appearance="Primary"
  Command="{Binding AddCommand}"
  Content="添加项目"
  Icon="&#xE710;" />
```

`WindowCaptionButton` 仅用于窗体标题栏中的最小化、最大化、还原和关闭命令。它使用专门的标题栏尺寸，而不使用普通按钮几何。关闭命令应设置 `Appearance="Danger"`，其余标题栏命令使用 `Subtle`。

`CardButton` 表示可交互卡片，而不是普通按钮的一种外观：

| 属性 | 类型与默认值 | 用途 |
| --- | --- | --- |
| `Icon` | `object`，`null` | 显示在文字旁的字形字符串或可视元素。 |
| `IconPosition` | `Dock`，`Top` | 将图标放在 `Left`、`Top`、`Right` 或 `Bottom`。 |
| `Title` | `string`，空 | 卡片标题。 |
| `Content` | 继承的 `object`，`null` | 辅助描述或更丰富的描述内容。 |

```xml
<flourish:CardButton
  Command="{Binding OpenReportsCommand}"
  Content="查看已生成的报告和最近导出。"
  Icon="&#xE8A5;"
  IconPosition="Left"
  Title="报告" />
```

外部 `Margin` 等位置属性由布局容器控制。鼠标与键盘焦点使用不同状态，键盘焦点保持可见。

`FlourishCard.Appearance` 默认值为 `Standard`，其他取值为 `Subtle`、`Accent`、`Elevated` 和 `Hero`。`Elevated` 提供层次效果，`Hero` 使用渐变背景和层次效果显示引导内容。`FlourishCard` 是不可交互的内容表面；需要让整个表面触发操作时，请使用 `CardButton`。

### 文字与输入

`FlourishTextBlock.Role` 默认值为 `Body`。可用角色包括 `Body`、`Paragraph`、`Caption`、`Muted`、`FieldLabel`、`Subtitle`、`Description`、`CardTitle`、`SectionTitle`、`PageTitle`、`Status` 和 `Icon`。

`Paragraph` 提供换行正文和更大的行距，`Description` 表示标题下方的辅助文字，`CardTitle` 表示卡片或紧凑内容区域中的标题。正文、辅助、标签、状态和图标角色使用 `Regular`，`CardTitle`、`SectionTitle` 和 `PageTitle` 使用 `Bold`。Role 使用当前字体和主题资源；显式设置的文字属性具有更高优先级。

标题需要访问键或需要通过 `Target` 将焦点移到其他控件时，请使用 `FlourishLabel`：

```xml
<flourish:FlourishLabel
  Content="_用户名"
  Target="{Binding ElementName=UserNameBox}" />
<flourish:FlourishTextBox
  x:Name="UserNameBox"
  Text="{Binding UserName, UpdateSourceTrigger=PropertyChanged}" />
```

`FlourishSearchBox` 提供搜索图标和 `Placeholder`，同时保留文字绑定、命令、选择和 `TextChanged`。

`FlourishPasswordBox` 有意遵循 WPF 密码输入语义：

| 成员 | 类型与默认值 | 用途 |
| --- | --- | --- |
| `Password` | `string`，空；不是依赖属性 | 获取或设置当前密码。由于它不是依赖属性，不要将其用作绑定目标。 |
| `SecurePassword` | 只读 `SecureString` | 从控件读取密码，而不要求控件返回托管字符串。 |
| `PasswordChar` | `char`，继承自 WPF | 修改掩码字符。 |
| `MaxLength` | `int`，`0` | 限制输入长度；`0` 表示控件不施加限制。 |
| `PasswordChanged` | 冒泡路由事件 | 在用户或代码修改密码时响应。 |
| `Clear()`、`SelectAll()` | 方法 | 清空或选择全部密码。 |
| `FocusEditor()` | 返回 `bool` 的方法 | 应用模板并将键盘焦点移到内部编辑器。 |

```xml
<flourish:FlourishPasswordBox
  MaxLength="128"
  PasswordChanged="PasswordBox_PasswordChanged" />
<flourish:FlourishSearchBox
  Placeholder="搜索报表"
  Text="{Binding Query, UpdateSourceTrigger=PropertyChanged}" />
```

### 选择控件

`FlourishCheckBox` 使用标准 `IsChecked` 和 `IsThreeState` 属性。`FlourishRadioButton` 使用 `IsChecked` 和 `GroupName`。`FlourishComboBox` 保留选择、可编辑文字、项目模板和 `ItemsSource` 行为，并自动使用 `FlourishComboBoxItem` 包装数据项。

```xml
<StackPanel>
  <flourish:FlourishCheckBox
    Content="包含已归档报表"
    IsChecked="{Binding IncludeArchived}" />
  <flourish:FlourishRadioButton
    Content="摘要"
    GroupName="ReportMode"
    IsChecked="True" />
  <flourish:FlourishRadioButton
    Content="详细"
    GroupName="ReportMode" />
  <flourish:FlourishComboBox
    ItemsSource="{Binding Formats}"
    SelectedItem="{Binding SelectedFormat}" />
</StackPanel>
```

`FlourishListBox.Appearance` 默认值为 `Standard`；`Navigation` 会移除通用列表边框并启用导航呈现。`IsCompact` 默认值为 `false`，用于折叠导航几何。在导航列表中，数据项应公开 `IsVisible`、`IsGroupHeader`、`IsCommandItem`、`IsEnabled` 和 `Label`；Flourish 会将这些值绑定到自动生成的容器，并将 `Label` 用作工具提示。直接提供的 `FlourishListBoxItem` 实例会保留自身的本地值和绑定。

`FlourishListBoxItem` 增加以下属性：

| 属性 | 默认值 | 在导航列表中的效果 |
| --- | --- | --- |
| `IsItemVisible` | `true` | 显示或隐藏项目。 |
| `IsGroupHeader` | `false` | 将项目显示为分组标题。 |
| `IsCommandItem` | `false` | 将项目显示为命令而不是页面目标；它不会新增 `ICommand`。 |

普通选择列表只需要 `ItemsSource`、`SelectedItem` 和可选的 `ItemTemplate`。

### 滚动、工具提示与布局调整

`FlourishScrollViewer` 保留所有 WPF 滚动属性和命令。设置 `IsCompact="True"` 可使用紧凑滚动条，默认值为 `false`。`FlourishScrollBar` 通常由滚动查看器模板创建，也可以直接配合标准 `Orientation`、`Minimum`、`Maximum`、`Value` 和 `ViewportSize` 属性使用。

```xml
<flourish:FlourishScrollViewer
  HorizontalScrollBarVisibility="Disabled"
  IsCompact="True"
  VerticalScrollBarVisibility="Auto">
  <StackPanel />
</flourish:FlourishScrollViewer>
```

`FlourishToolTip` 保留 WPF 工具提示的内容、计时和定位属性。其默认样式会启用 `FlourishToolTipPlacement.IsEnabled`，让提示保持在 Shell 内，并感知标题栏、工具栏、面包屑、导航、状态栏和内容区域。若要使用普通 WPF 定位，请在工具提示上将该附加属性设置为 `False`。

对 `IconButton` 和 `WindowCaptionButton`，字符串等简单的 `ToolTip` 内容会自动包装为 `FlourishToolTip`。因此仅图标控件可以沿用现有的 Flourish Tips 策略和感知 Shell 区域的定位，同时仍使用标准 WPF `ToolTip` 写法。显式提供的 `FlourishToolTip` 会保持不变。

```xml
<flourish:Button Content="刷新">
  <flourish:Button.ToolTip>
    <flourish:FlourishToolTip Content="重新加载当前报表" />
  </flourish:Button.ToolTip>
</flourish:Button>
```

`FlourishGridSplitter.Variant` 默认值为 `Standard`。仅在导航面板的调整边缘使用 `NavigationPane`。标准 `ResizeDirection`、`ResizeBehavior`、键盘移动和对齐规则仍然适用。

## HoverReveal 与减少动态效果

`Button`、它的衍生按钮控件、`FlourishComboBoxItem` 和 `FlourishListBoxItem` 使用公共 `HoverReveal` 附加行为。通过[动效](configure-motion.md)进行应用级配置，并遵循操作系统的减少动态效果偏好：

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
<flourish:Button
  flourish:HoverReveal.IsEnabled="True"
  flourish:HoverReveal.AnimationDuration="0:0:0.14"
  flourish:HoverReveal.OverrideColor="{DynamicResource FlourishPrimarySurfaceBrush}"
  Content="预览" />
```

| 附加属性 | 默认值 | 继承 | 用途 |
| --- | --- | --- | --- |
| `HoverReveal.IsEnabled` | `true` | 是 | 启用显示行为；`IsMotionEnabled` 为 `false` 时，有效值也会被禁用。 |
| `HoverReveal.AnimationDuration` | 140 毫秒 | 是 | 设置显示动画时长。 |
| `HoverReveal.OverrideColor` | `null` | 否 | 可选的 `Brush`；参与控件模板使用它替代默认的显示颜色。 |
| `HoverReveal.IsMotionEnabled` | `true` | 否 | 为单个参与控件提供运行时动效策略。 |
| `HoverReveal.IsParticipant` | `false` | 否 | 让自定义控件模板参与该行为。 |
| `HoverReveal.TemplateHandlesInteraction` | `false` | 否 | 声明自定义模板自行提供静态悬停和按下状态。 |

接入该行为的自定义模板应提供名为 `HoverChrome` 和 `HoverRevealScale` 的元素，将 `HoverChrome` 的背景绑定到 `flourish:HoverReveal.OverrideColor`，设置 `flourish:HoverReveal.IsParticipant="True"`，并将 `flourish:HoverReveal.IsMotionEnabled` 绑定到 `{DynamicResource FlourishHoverRevealEnabled}`。`IsEnabled` 和 `AnimationDuration` 会沿视觉树继承；`OverrideColor`、`IsMotionEnabled` 与 `IsParticipant` 不继承。缺少任一命名元素时，该行为不生效。

`Button.Appearance="Danger"` 默认提供语义化红色 `FlourishDangerHoverRevealBrush`，而不是标准的蓝色显示层。若某个破坏性操作需要不同的反馈颜色，本地设置的 `HoverReveal.OverrideColor` 优先级更高。

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
