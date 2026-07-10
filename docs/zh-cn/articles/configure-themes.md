---
title: 主题
description: 配置 Flourish 的默认主题、系统主题跟随和用户主题偏好。
---

# 主题

主题控制 Shell 使用的亮色和暗色资源。先在 [Shell 配置](shell-configuration.md)中启用 `UseThemes()`，再使用 `ConfigureThemes` 选择尚未保存用户偏好时采用的主题。

## 最小配置

```csharp
builder
    .ConfigureData(data =>
        data.SetAppCompany("示例公司").SetAppName("Foobar"))
    .ConfigureShell(shell => shell.UseThemes())
    .ConfigureThemes(FlourishTheme.System);
```

主题偏好需要显式偏好目录，或者公司名称与应用名称（也可以由非空标题提供应用名称）。具体存储选项参见[应用数据](configure-data.md)。

## 选择默认主题

`FlourishTheme.System` 跟随 Windows 应用主题。`FlourishTheme.Light` 和 `FlourishTheme.Dark` 分别将亮色或暗色设为初始主题。

`ConfigureThemes` 设置的是默认值。用户已经选择并保存主题时，保存的偏好优先于该默认值。

## 主题偏好

启用主题后，Flourish 会将用户选择写入应用偏好。偏好存储使用[应用数据](configure-data.md)中配置的公司名、应用名和可选存储路径。

更改应用标识会改变默认偏好位置，Flourish 不会自动迁移原位置中的主题偏好。

## 标题栏主题入口

标题栏可以显示主题切换按钮：

```csharp
builder
    .ConfigureShell(shell => shell.UseTitleBar().UseThemes())
    .ConfigureTitleBar(titleBar => titleBar.ShowThemeToggle());
```

主题按钮同时依赖 `UseTitleBar()`、`UseThemes()` 和 `ShowThemeToggle()`。不显示按钮时，应用仍可使用配置的默认主题和已保存偏好。

## 相关功能

- [标题栏](configure-title-bar.md)配置主题切换入口。
- [材质特效](configure-material-effect.md)配置与主题资源配合使用的窗口材质。
- [排版](configure-font.md)配置主题中 Shell 文本使用的基础字体。
