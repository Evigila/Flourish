---
title: WPF 快速开始
description: 使用 Flourish 构建并运行基础 WPF 应用。
---

# WPF 快速开始

安装或引用 `Arkheide.Flourish.WPF` 后，导入共享契约命名空间即可。WPF 包依赖 `Arkheide.Flourish.Core`，用于提供共享契约和服务。

```csharp
using ArkheideSystem.Flourish.Abstract;
```

Flourish Shell 作为主窗口时不要设置 `StartupUri`。注册应用、配置标题栏与导航，再运行 Host：

```csharp
return ApplicationBuilder
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

`AddNavigable<TPage>` 注册页面、创建路由并添加导航项；需要层级时再用 `AddGroup` 或固定区域方法。

Flourish 文案默认使用 `en-US`。可在构建前选择其他内置语言或自定义文化文件：

```csharp
builder.ConfigureData(data =>
    data.SetLocale("zh-CN")
        .AddCultureFile("Locales/FlourishCulture.Json"));
```

启动后通过依赖注入解析 Service：Builder 定义初始状态，Service 修改实时状态，如 `theme.Current`、`theme.Changed` 和 `theme.SetTheme(...)`。

注入标准 Microsoft `IConfiguration` 读取最终配置；用 `ISettingsStore` 原子修改 `appsettings.Flourish.json` 的 `Flourish:` 节。

继续阅读 [IApplicationBuilder](flourish-builder.md)、[功能配置](shell-configuration.md)与[运行时 API](runtime-apis.md)。
