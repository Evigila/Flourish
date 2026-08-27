---
title: 排版
description: 配置字体系列和 Flourish 六种字号层级；未显式选择层级时默认使用 Standard。
---

# 排版

在 `ConfigureFont` 中调用 `SetFont`，设置 Shell、导航页面和 Profile 页面的字体系列与六种字号层级。

## 配置全局字体

```csharp
builder.ConfigureFont(font =>
    font.SetFont("Segoe UI", 11, 13, 14, 14, 18, 25));
```

全局文本字体族、图标字体族与完整的六档字号默认作为同一偏好组恢复和更新。需要代码配置的全局比例始终优先时，传入 `usePersistedPreference: false`。页面专属覆盖仍由应用拥有，不会持久化。

七个参数依次为字体系列、Small、Standard、StandardIcon、Large、ExtraLarge 和 HeaderSize。字号必须是有限正数，各档独立且可相同。未调用 `SetFont` 时，默认使用 `Segoe UI` 和 `11`、`13`、`14`、`14`、`18`、`25` DIP。

## 字号层级角色

文本元素或控件没有显式选择字号层级时，使用 Standard。应将它视为通用默认值，不要仅为了视觉强调而选择其他层级。

| 层级 | 角色 |
| --- | --- |
| `Small` | 导航栏分组标签、OutputCard 输出，以及由控件管理的其他紧凑状态或说明文字。 |
| `Standard` | 所有普通正文和控件文字，包括未指定层级的文本。 |
| `StandardIcon` | 普通图标字形，包括按钮图标。 |
| `Large` | 卡片标题和标题栏当前标题。 |
| `ExtraLarge` | 区块标题一族，包括 `Chunk.Title`。 |
| `HeaderSize` | 仅保留给 `HeaderChunk` 中的页面标题。 |

`Document` 中的 `Paragraph` 与 `CodeSpace` 显式使用 Large 层级，因此会跟随全局和页面级 Large 设置变化，而不会从 Standard 派生额外字号。

Large、ExtraLarge 和 HeaderSize 标题角色使用 `Bold`。标题下拉选项与 Logo 信息视图中的内置文本使用 Standard。应用向 `TitleBarApplicationInfo` 提供的内容仍保留自身的 WPF 排版设置。

Small 与 Standard 使用紧凑行高和最小下方空间，Large、ExtraLarge 与 HeaderSize 逐级增大，图标不增加下方空间。

StandardIcon 是可配置的默认图标字号，为 `14` DIP。卡片与展示图标使用固定的 LargeIcon `22` DIP。工具栏、导航栏、标题栏、搜索、状态栏和窗口控件继续使用针对几何的固定校正。

字体系列应覆盖应用显示的全部语言，并提供 `Regular` 与 `Bold` 字形。

主内容区域与 Profile 中显示的页面都会继承全局字体。显式设置了本地字体的子控件会按照 WPF 属性优先级保留本地值；图标槽位使用图标字体是有意的字形字体通道。

## 覆盖单个页面

`SetOverrideFont<TPage>` 可以设置指定页面的字体系列或字号。某一档传入 `null` 时，该档继续跟随全局值。

```csharp
builder.ConfigureFont(font =>
{
    font
        .SetFont("Segoe UI", 11, 13, 14, 14, 18, 25)
        .SetOverrideFont<CodeEditorPage>(
            "Cascadia Mono",
            null,
            null,
            null,
            null,
            null,
            null);

    font.SetOverrideFont<PresentationPage>(
        "Microsoft YaHei UI",
        14,
        16,
        19,
        22,
        26,
        32);
});
```

页面覆盖中提供的每一档都必须是有限正数；除此之外各档彼此独立，包括通过 `null` 继承的值。

## 在运行时修改

`IFontService` 在应用启动后提供相同的原子七参数模型。页面覆盖会按配置时的页面类型匹配，并在缓存页面或动态注册页面显示时重新应用。

```csharp
fontService.SetFont("Segoe UI", 11, 13, 14, 14, 18, 25);

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

IReadOnlyDictionary<Type, PageFontOverride> overrides =
    fontService.Current.PageOverrides;

fontService.RemoveOverrideFont<CodeEditorPage>();
```

清除覆盖后，当前页面会立即返回最新的全局字体。页面覆盖中的 `null` 档位会继续跟随后续全局变化。

## 相关功能

- [窗口](configure-window.md)
- [标题栏](configure-title-bar.md)、[导航](navigation.md)与[状态栏](status-bar.md)
- [主题](configure-themes.md)
