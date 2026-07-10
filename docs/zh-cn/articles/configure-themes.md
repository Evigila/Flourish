---
title: 主题
description: 配置 Flourish 的主题切换、系统主题跟随和用户主题偏好。
---

# 主题

标题栏的主题按钮让用户在系统、亮色和暗色主题之间切换。`SetThemeToggle` 会启用主题功能、显示按钮，并指定没有已保存偏好时使用的模式。

## 最小配置

```csharp
builder
    .ConfigureData(data =>
        data.SetAppCompany("示例公司").SetAppName("Foobar"))
    .ConfigureShell(shell => shell.UseTitleBar())
    .ConfigureTitleBar(titleBar =>
        titleBar.SetThemeToggle(FlourishTheme.System));
```

主题偏好需要显式偏好目录，或者公司名称与应用名称（也可以由非空标题提供应用名称）。具体存储选项参见[应用数据](configure-data.md)。

## 选择初始模式

`FlourishTheme.System` 跟随 Windows 应用主题。`FlourishTheme.Light` 和 `FlourishTheme.Dark` 分别将亮色或暗色设为初始主题。省略参数时使用 `System`。

`SetThemeToggle` 的参数只在没有已保存偏好时生效。用户已经选择并保存主题时，保存的偏好优先。

## 主题偏好

主题按钮会将用户选择写入应用偏好。更改应用标识会改变默认偏好位置，Flourish 不会自动迁移原位置中的主题偏好。

不调用 `SetThemeToggle` 时，主题按钮和主题切换功能都不会启用。

## 相关功能

- [标题栏](configure-title-bar.md)配置主题切换入口及其他标题栏元素。
- [材质特效](configure-material-effect.md)配置与主题资源配合使用的窗口材质。
- [排版](configure-font.md)配置 Shell 文本使用的基础字体。
