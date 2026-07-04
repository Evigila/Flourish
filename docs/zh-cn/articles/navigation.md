---
title: 导航
description: 注册并导航到 Flourish 页面。
---

# 导航

在服务配置阶段使用 `AddNavigable` 注册页面。每个注册页面都会带有用于 Shell 导航 UI 的显示元数据，并且可以选择页面缓存模式。

```csharp
builder.ConfigureServices((_, services) =>
{
    services.AddNavigable<HomePage>(
        displayName: "Home",
        iconGlyph: "\uE80F",
        isInitial: true);

    services.AddNavigable<SettingsPage>(
        displayName: "Settings",
        iconGlyph: "\uE713",
        cacheMode: FlourishPageCacheMode.Enabled);
});
```

页面类型必须派生自 `System.Windows.Controls.Page`。Flourish 会把页面类型注册到服务集合中，并使用导航元数据构建 Shell 的导航界面。

运行时导航可以从依赖注入中获取 `INavigationService`，再按页面类型跳转：

```csharp
public sealed class HomeViewModel(INavigationService navigation)
{
    public void OpenSettings()
    {
        navigation.Navigate<SettingsPage>();
    }
}
```
