---
title: 主题
description: 配置主题选择、应用配色、共用圆角和偏好持久化。
---

# 主题

Flourish 提供跟随系统、亮色和暗色主题。`SetThemeToggle` 会启用主题选择、显示标题栏入口，并指定 Host 配置中没有已保存偏好时使用的回退模式。

## 配置主题选择

显示主题入口前需要启用标题栏：

```csharp
builder
    .ConfigureShell(shell => shell.UseTitleBar())
    .ConfigureTitleBar(titleBar =>
        titleBar.SetThemeToggle(FlourishTheme.System));
```

省略参数时使用 `FlourishTheme.System`。只有 Host 配置中不存在 `Flourish:Preferences:Theme` 时，回退值才会生效。[应用数据](configure-data.md)说明对应的 `appsettings.json` 设置。

不调用 `SetThemeToggle` 时，标题栏主题入口保持隐藏，Shell 以亮色主题初始化。应用仍可通过 `IThemeService` 在运行时更改主题。

## 配置应用配色与圆角

通过 `ConfigureShell` 设置主要色、辅助色、强调色与共用圆角：

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

三种颜色必须完全不透明。Flourish 会根据有效的亮色或暗色主题派生语义交互、表面和前景资源，并在主题变化后重新计算。

`UseCornerRadius` 接受以设备无关像素表示的有限非负值。`0` 会生成直角的共用几何形状；省略该方法时，控件和表面使用主题定义的圆角。

应用配色后，应在亮色和暗色主题下验证结果，并保持文字对比度。

## 主题模式与偏好

`FlourishTheme.System` 跟随 Windows 应用主题，`Light` 与 `Dark` 使用固定主题，直到用户选择其他模式。

Flourish 按 Host 的完整配置优先级读取 `Flourish:Preferences:Theme`。用户通过标题栏选择主题时，只写入 Host 内容根目录中的基础 `appsettings.json`；环境专用 appsettings、User Secrets、环境变量或命令行值仍可能在后续启动时优先。

内容根目录必须可写。写入偏好时会重新序列化完整 JSON 对象，因此文件会被重新格式化，注释也会被移除。

## 相关功能

- [控件库](control-library.md)说明显式 Flourish 控件与主题资源加载。
- [标题栏](configure-title-bar.md)配置主题入口。
- [应用数据](configure-data.md)说明 Host 配置与主题偏好键。
- [运行时 API](runtime-apis.md)说明应用运行期间通过 `IThemeService` 更改主题。
- [材质特效](configure-material-effect.md)配置与当前主题配合使用的窗口材质。
- [排版](configure-font.md)配置主题资源使用的字体。
