---
title: 排版
description: 配置字体系列和 Flourish 六种字号层级；未显式选择层级时默认使用 Standard。
---

# 排版

在 `ConfigureShell` 中调用 `UseGlobalFont`，可以同时设置 Shell 区域、已导航页面和 Profile 页面使用的字体系列与六种字号层级。

## 配置全局字体

```csharp
builder.ConfigureShell(shell =>
    shell.UseGlobalFont("Segoe UI", 12, 14, 16, 16, 24, 32));
```

七个参数依次是字体系列、Small、Standard、Icon、Large、ExtraLarge 与 HeaderSize。每个字号都必须是有限正数，各档彼此独立，可以使用相同数值；Flourish 不限制它们的相对大小。未调用 `UseGlobalFont` 时，Flourish 默认使用 `Segoe UI` 与 `12`、`14`、`16`、`16`、`24`、`32` DIP。

## 字号层级角色

文本元素或控件没有显式选择字号层级时，使用 Standard。应将它视为通用默认值，不要仅为了视觉强调而选择其他层级。

| 层级 | 角色 |
| --- | --- |
| `Small` | 导航栏分组标签、OutputCard 输出，以及由控件管理的其他紧凑状态或说明文字。 |
| `Standard` | 所有普通正文和控件文字，包括未指定层级的文本。 |
| `Icon` | 通用图标字形。专用图标控件可根据自身几何应用局部校正。 |
| `Large` | 卡片标题和标题栏当前标题。 |
| `ExtraLarge` | 区块标题一族，包括 `Chunk.Title`。 |
| `HeaderSize` | 仅保留给 `ChunkHero` 中的页面标题。 |

`Paragraph` 和 `CodeSpace` 显式使用 Large 层级，因此会跟随全局和页面级 Large 设置变化，而不会从 Standard 派生额外字号。

Large、ExtraLarge 和 HeaderSize 标题角色使用 `Bold`。标题下拉选项与 Logo 信息视图中的内置文本使用 Standard。应用向 `TitlebarApplicationInfo` 提供的内容仍保留自身的 WPF 排版设置。

Small 与 Standard 使用紧凑行高和最小下方空间，Large、ExtraLarge 与 HeaderSize 逐级增大，Icon 不增加下方空间。

Icon 是默认图标字号。由于 Segoe MDL2 的不同字形拥有不同的天然边界，Flourish 会进行固定的视觉校正：导航栏 `18`、标题栏命令 `16`、窗口标题按钮 `12`、搜索图标 `14`、状态栏项目 `14`、状态栏后台任务 `12`、后台任务详情 `16`、系统状态详情 `16` DIP。这些场景校正不会改变配置的默认 Icon 值。

字体系列应覆盖应用显示的全部语言，并提供 `Regular` 与 `Bold` 字形。

主内容区域与 Profile 中显示的页面都会继承全局字体。显式设置了本地字体的子控件会按照 WPF 属性优先级保留本地值；图标槽位使用图标字体是有意的字形字体通道。

## 覆盖单个页面

`SetOverrideFont<TPage>` 可以覆盖指定页面的字体系列或字号。某一档传入 `null` 时，该档继续跟随全局值。

```csharp
builder.ConfigureShell(shell =>
    shell
        .UseGlobalFont("Segoe UI", 12, 14, 16, 16, 24, 32)
        .SetOverrideFont<CodeEditorPage>(
            "Cascadia Mono",
            null,
            null,
            null,
            null,
            null,
            null));

shell.SetOverrideFont<PresentationPage>(
    "Microsoft YaHei UI",
    14,
    16,
    19,
    22,
    26,
    32);
```

页面覆盖中提供的每一档都必须是有限正数；除此之外各档彼此独立，包括通过 `null` 继承的值。

## 在运行时修改

`IFontService` 在应用启动后提供相同的原子七参数模型。页面覆盖会按配置时的页面类型匹配，并在缓存页面或动态注册页面显示时重新应用。

```csharp
fontService.SetFont("Segoe UI", 12, 14, 16, 16, 24, 32);

fontService.SetOverrideFont<CodeEditorPage>(
    "Cascadia Mono",
    null,
    null,
    null,
    null,
    null,
    null);

fontService.SetOverrideFont(
    typeof(DiagnosticsPage),
    "Segoe UI",
    11,
    14,
    16,
    19,
    22,
    28);

IReadOnlyDictionary<Type, FlourishPageFontOverride> overrides =
    fontService.PageOverrides;

fontService.ClearOverrideFont<CodeEditorPage>();
```

清除覆盖后，当前页面会立即返回最新的全局字体。页面覆盖中的 `null` 档位会继续跟随后续全局变化。

## 相关功能

- [窗口](configure-window.md)定义排版需要适配的可用空间。
- [标题栏](configure-title-bar.md)、[导航](navigation.md)与[状态栏](status-bar.md)会显示受全局排版影响的 Shell 文字。
- [主题](configure-themes.md)提供文字与背景颜色资源。
