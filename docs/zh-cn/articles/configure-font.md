---
title: 排版
description: 配置 Flourish Shell 与应用页面使用的全局字体，并按页面提供可选覆盖。
---

# 排版

`UseGlobalFont` 可以统一设置完整 Flourish 可视树的字体系列和基础字号。它会影响标题栏、导航栏、动态工具栏、状态栏、Shell 对话框、导航内容页以及 Profile 页面。

## 最小配置

```csharp
builder.ConfigureShell(shell =>
    shell.UseGlobalFont("Microsoft YaHei UI", 14));
```

只需更改字体系列时可以省略字号，基础字号默认为 `14`。

## 字体覆盖范围

所选字体应覆盖应用显示的全部语言。包含中文与拉丁文字的界面应选择同时提供这些字形的字体；缺少字形时，WPF 会使用字体回退，实际外观可能与 Shell 的其他文字不同。

WPF 的 `Frame` 导航通常会截断字体属性继承。Flourish 会在主内容页或 Profile 页面显示时主动跨越该边界：普通页面文字继承全局值，Flourish 控件则通过页面感知的动态资源解析相同值。明确设置了局部字体的子控件仍保留自己的值，例如使用 `Consolas` 的代码段或使用图标字体的图标。

## 覆盖单个页面

当某个页面需要不同的文字字体时，使用 `SetOverrideFont<TPage>`。省略可选字号后，该页面仍会跟随全局基础字号。

```csharp
builder.ConfigureShell(shell =>
    shell
        .UseGlobalFont("Microsoft YaHei UI", 14)
        .SetOverrideFont<CodeEditorPage>("Cascadia Mono"));
```

如果页面还需要独立的排版比例，可以同时指定字号：

```csharp
shell.SetOverrideFont<PresentationPage>("Microsoft YaHei UI", 16);
```

## 运行时修改覆盖

`IFontService` 在启动后提供相同模型。覆盖按配置的页面类型匹配，并会在缓存页面或动态注册页面再次显示时重新应用。

```csharp
fontService.SetOverrideFont<CodeEditorPage>("Cascadia Mono");
fontService.SetOverrideFont(typeof(DiagnosticsPage), "Microsoft YaHei UI", 15);

IReadOnlyDictionary<Type, FlourishPageFontOverride> overrides =
    fontService.PageOverrides;

fontService.ClearOverrideFont<CodeEditorPage>();
```

清除覆盖后，当前页面会立即返回最新的全局字体。覆盖字号为 `null` 时，页面会继续跟随后续的全局字号变化。

## 字号约束

字号必须是有限正数。Flourish 会从基础字号派生多个文本尺寸，因此调整该值会同时改变不同 Shell 区域和页面控件的文字比例。

## 相关功能

- [窗口](configure-window.md)定义排版需要适配的可用尺寸。
- [标题栏](configure-title-bar.md)、[导航](navigation.md)和[状态栏](status-bar.md)会显示受全局排版影响的 Shell 文本。
- [主题](configure-themes.md)提供与文字颜色和背景相关的资源。
