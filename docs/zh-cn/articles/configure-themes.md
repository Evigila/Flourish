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

## 选择初始模式

`FlourishTheme.System` 跟随 Windows 应用主题。`FlourishTheme.Light` 和 `FlourishTheme.Dark` 分别将亮色或暗色设为初始主题。省略参数时使用 `System`。

`SetThemeToggle` 的参数只在没有已保存偏好时生效。用户已经选择并保存主题时，保存的偏好优先。

## 主题偏好

Flourish 按 Host 的完整配置优先级读取 `Flourish:Preferences:Theme`。用户选择只写入 Host 的基础 `appsettings.json`；环境 appsettings、User Secrets、环境变量或命令行值仍可能在后续启动时优先。内容根目录必须保持可写，写入也会重新格式化 JSON 并移除注释。

不调用 `SetThemeToggle` 时，主题按钮和主题切换功能都不会启用。

## 相关功能

- [标题栏](configure-title-bar.md)配置主题切换入口及其他标题栏元素。
- [应用数据](configure-data.md)说明 Host 配置与主题键。
- [材质特效](configure-material-effect.md)配置与主题资源配合使用的窗口材质。
- [排版](configure-font.md)配置 Shell 文本使用的基础字体。
