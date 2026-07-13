---
title: 主题
description: 配置 Flourish 的主题切换、系统主题跟随和用户主题偏好。
---

# 主题

标题栏的主题按钮让用户在系统、亮色和暗色主题之间切换。`SetThemeToggle` 会启用主题功能、显示按钮，并指定没有已保存偏好时使用的模式。

## 最小配置

```csharp
builder
    .ConfigureShell(shell => shell.UseTitleBar())
    .ConfigureTitleBar(titleBar =>
        titleBar.SetThemeToggle(FlourishTheme.System));
```

参数是在 Host 配置不包含 `Flourish:Preferences:Theme` 时使用的回退值。[应用数据](configure-data.md)说明对应的 `appsettings.json` 设置。

## 自定义品牌颜色与圆角

Shell Builder 可以在启动时覆盖三种独立的品牌色和全局圆角：

```csharp
using System.Windows.Media;

builder.ConfigureShell(shell =>
    shell
        .UseThemeColors(new FlourishThemeColors(
            primary: Color.FromRgb(15, 108, 189),
            secondary: Color.FromRgb(92, 46, 145),
            accent: Color.FromRgb(216, 59, 1)))
        .UseCornerRadius(5));
```

`Primary` 用于品牌与主要操作，`Secondary` 用于辅助层级，`Accent` 用于焦点和强调细节。三者必须是不透明颜色。Flourish 会针对当前亮色或暗色主题派生 hover、pressed、surface 与可读前景，并在主题切换后重新计算。颜色覆盖位于可切换调色板之外，因此不会在切换时丢失；仍应在两种主题下验证最终的品牌效果。

`UseCornerRadius` 接受有限的非负 DIP 值。调用后会统一覆盖控件、卡片、浮层与对话框圆角；不调用时保留轻型的 4/6/8/10 层级。头像、单选按钮和徽标等圆形元素不受这个值影响。

## 选择初始模式

`FlourishTheme.System` 跟随 Windows 应用主题。`FlourishTheme.Light` 和 `FlourishTheme.Dark` 分别将亮色或暗色设为初始主题。省略参数时使用 `System`。

`SetThemeToggle` 的参数只在没有已保存偏好时生效。用户已经选择并保存主题时，保存的偏好优先。

## 主题偏好

Flourish 按 Host 的完整配置优先级读取 `Flourish:Preferences:Theme`。用户选择只写入 Host 的基础 `appsettings.json`；环境 appsettings、User Secrets、环境变量或命令行值仍可能在后续启动时优先。内容根目录必须保持可写，写入也会重新格式化 JSON 并移除注释。

不调用 `SetThemeToggle` 时，主题按钮和主题切换功能都不会启用。

## 相关功能

- [控件库](control-library.md)说明显式 `Flourish*` 控件如何使用当前主题资源，而原生 WPF 控件保持不变。
- [标题栏](configure-title-bar.md)配置主题切换入口及其他标题栏元素。
- [应用数据](configure-data.md)说明 Host 配置与主题键。
- [材质特效](configure-material-effect.md)配置与主题资源配合使用的窗口材质。
- [排版](configure-font.md)配置 Shell 文本使用的基础字体。
