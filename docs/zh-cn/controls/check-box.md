---
title: CheckBox
description: 使用紧凑横向布局或图标卡片布局呈现布尔值及可选三态选择。
---

# CheckBox

`CheckBox` 表示一项独立选择。它沿用原生 WPF `CheckBox` 的状态与事件模型，并增加固定的 Horizontal 和 Vertical 布局。

## 选择布局

`Variant="Horizontal"` 是默认选项，适用于普通设置。它将圆形状态标识符放在内容前方。未选中时显示空心圆；选中后显示勾选标记，并同时高亮内容、标识符和控件边框。

```xml
<flourish:CheckBox
  Content="启用通知"
  IsChecked="{Binding NotificationsEnabled, Mode=TwoWay}" />
```

需要图标辅助表达的卡片式选择使用 `Variant="Vertical"`。图标位于左上方，内容位于图标下方，选中状态标识符位于右上方。未选中时不显示标识符；选中后，内容、图标前景、标识符和边框都使用主色高亮。

```xml
<flourish:CheckBox
  Content="云端工作区"
  Icon="&#xE753;"
  IsChecked="{Binding UsesCloudWorkspace, Mode=TwoWay}"
  Variant="Vertical" />
```

`Icon` 接受任意内容，但只由 Vertical 布局呈现。文字及其他遵循前景色的图标内容会继承选择高亮；显式指定自身颜色的图像或图形保留其原有颜色。

## 三态选择

只有当 `null` 明确表示继承、混合或未知值时才设置 `IsThreeState="True"`。第三状态会把勾选标记替换为一条横线，其余高亮行为与选中状态相同。

```xml
<flourish:CheckBox
  Content="使用继承的设置"
  IsChecked="{Binding InheritedSetting, Mode=TwoWay}"
  IsThreeState="True" />
```

| Variant | 未选中 | 选中 | 第三状态 |
| --- | --- | --- | --- |
| Horizontal | 空心圆标识符 | 勾选标识符，并高亮内容与边框 | 横线标识符，并高亮内容与边框 |
| Vertical | 隐藏标识符 | 右上角显示勾选标识符，并高亮图标、内容与边框 | 右上角显示横线标识符，并高亮图标、内容与边框 |

## 相关内容

- [CheckBox API](xref:ArkheideSystem.Flourish.Controls.CheckBox) 列出继承成员与声明成员。
- [CheckBoxVariant API](xref:ArkheideSystem.Flourish.Controls.CheckBoxVariant) 列出固定布局。
- [WPF CheckBox 文档](https://learn.microsoft.com/dotnet/desktop/wpf/controls/checkbox) 介绍原生选择模型。
