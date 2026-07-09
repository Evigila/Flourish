---
title: ConfigureTips
description: 配置 Flourish 提示浮层时机和边界约束。
---

# ConfigureTips

`ConfigureTips` 配置提示浮层行为。提示浮层只有在 [`ConfigureShell`](configure-shell.md) 启用 `UseTips()` 后才会生效。

```csharp
builder
    .ConfigureShell(shell => shell.UseTips())
    .ConfigureTips(tips =>
    {
        tips.SetDelay(200).SetSpawnableMargin(5);
    });
```

## 细节

`SetDelay` 控制初始悬浮延迟，单位为毫秒。密集型 WPF 工具需要避免提示过于频繁，同时也要让图标按钮容易发现。

`SetSpawnableMargin` 让提示浮层避开 Shell 边缘。导航栏折叠、Footer 命令靠近底部、工具栏命令靠近窗口边框时，这个设置尤其有用。

通过 `UseTips(false)` 关闭提示会覆盖这些详细设置。

## 相关 API

- [`ConfigureShell`](configure-shell.md) 拥有 `UseTips` 开关。
- [`ConfigureTitleBar`](configure-title-bar.md)、[`ConfigureNavigation`](configure-navigation.md)、[`ConfigureFooter`](configure-footer.md) 都包含可能显示提示的内置控件。
