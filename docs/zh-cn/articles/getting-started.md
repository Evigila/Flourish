---
title: 快速开始
description: 使用 Flourish 构建并运行基础 WPF 应用。
---

# 快速开始

安装或引用 `Arkheide.Flourish` 后，只需导入唯一的公共命名空间：

```csharp
using ArkheideSystem.Flourish.Abstract;
```

Flourish Shell 作为主窗口时不要设置 `StartupUri`。注册 WPF 应用，配置标题栏与导航，然后运行 Host：

```csharp
return FlourishBuilder
    .CreateDefaultBuilder(args)
    .ConfigureServices((_, services) => services.AddSingleton<App>())
    .ConfigureTitleBar(titleBar =>
        titleBar
            .SetEnabled()
            .SetApplicationTitle("Foobar")
            .SetNavigationToggle())
    .ConfigureNavigation(navigation =>
        navigation
            .SetEnabled()
            .AddNavigable<HomePage>(
                displayName: "首页",
                iconGlyph: "\uE80F",
                isInitial: true))
    .Run<App>();
```

`AddNavigable<TPage>` 会同时注册页面、创建路由并添加可见导航项。需要明确层级时再使用 `AddGroup` 或固定区域方法。

Flourish 文案默认使用 `en-US`。可在构建前选择其他内置语言或自定义文化文件：

```csharp
builder.ConfigureData(data =>
    data.SetLocale("zh-CN")
        .AddCultureFile("Locales/FlourishCulture.Json"));
```

启动后通过依赖注入解析运行时 Service。Builder 定义初始状态，Service 修改实时状态。例如读取 `theme.Current`、监听 `theme.Changed`，并调用 `theme.SetTheme(...)`。

Flourish 使用标准 Microsoft `IConfiguration`。注入它即可读取最终配置；需要原子修改 `appsettings.Flourish.json` 中的 `Flourish:` 节时，注入 `IFlourishSettingsStore`。

继续阅读 [IFlourishBuilder](flourish-builder.md)、[功能配置](shell-configuration.md)与[运行时 API](runtime-apis.md)。
