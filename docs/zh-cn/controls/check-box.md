---
title: CheckBox
description: 使用紧凑横向布局或图标卡片布局呈现布尔值及可选三态选择。
---

# CheckBox

`CheckBox` 表示一项独立选择。它沿用原生 WPF `CheckBox` 的状态与事件模型，并增加固定的 Horizontal 和 Vertical 布局。

## 选择布局

`Variant="Horizontal"` 是默认选项，适用于普通设置。它将圆形状态标识符放在内容前方。未选中时显示空心圆；选中后，圆形会替换为更大的主色勾，同时高亮内容和控件边框。

```xml
<flourish:CheckBox
  Content="启用通知"
  IsChecked="{Binding NotificationsEnabled, Mode=TwoWay}" />
```

需要图标辅助表达的卡片式选择使用 `Variant="Vertical"`。图标位于左上方，内容位于图标下方，状态标识符位于右上方。未选中时保留空心圆；选中后，圆形替换为主色勾，内容、图标前景和边框也使用主色高亮。

```xml
<flourish:CheckBox
  Content="云端工作区"
  Icon="&#xE753;"
  IsChecked="{Binding UsesCloudWorkspace, Mode=TwoWay}"
  Variant="Vertical" />
```

`Icon` 接受任意内容，但只由 Vertical 布局呈现。文字及其他遵循前景色的图标内容会继承选择高亮；显式指定自身颜色的图像或图形保留其原有颜色。

## 悬停反馈

两种布局默认都参与公共 `HoverReveal` 行为。CheckBox 遵循 Outlined 按钮的交互颜色：悬停使用共享的弱化揭示色，按下使用共享的更深按下色。这些交互层绘制在图标、内容和状态标识符下方，因此不会遮盖选中或第三状态的高亮。关闭悬停动画或 Windows 请求减少动态效果时，控件会以无动画方式保留相同反馈。父级设置 `HoverReveal.IsEnabled="False"` 后会由子树继承，并使用该静态回退。

## 三态选择

只有当 `null` 明确表示继承、混合或未知值时才设置 `IsThreeState="True"`。第三状态会把勾选标记替换为主色圆角正方形轮廓，其余高亮行为与选中状态相同。

```xml
<flourish:CheckBox
  Content="使用继承的设置"
  IsChecked="{Binding InheritedSetting, Mode=TwoWay}"
  IsThreeState="True" />
```

| Variant | 未选中 | 选中 | 第三状态 |
| --- | --- | --- | --- |
| Horizontal | 空心圆标识符 | 主色勾，并高亮内容与边框 | 主色圆角正方形轮廓，并高亮内容与边框 |
| Vertical | 右上角空心圆标识符 | 右上角显示主色勾，并高亮图标、内容与边框 | 右上角显示主色圆角正方形轮廓，并高亮图标、内容与边框 |

## 相关内容

- [CheckBox API](xref:ArkheideSystem.Flourish.Controls.CheckBox) 列出继承成员与声明成员。
- [CheckBoxVariant API](xref:ArkheideSystem.Flourish.Controls.CheckBoxVariant) 列出固定布局。
- [动效](../articles/configure-motion.md)配置悬停揭示与减少动态效果行为。
- [WPF CheckBox 文档](https://learn.microsoft.com/dotnet/desktop/wpf/controls/checkbox) 介绍原生选择模型。
