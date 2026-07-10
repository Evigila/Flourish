---
title: 提示浮层
description: 配置 Flourish 提示浮层的显示延迟和窗口边界距离。
---

# 提示浮层

提示浮层为图标按钮和紧凑命令提供补充说明。先在 [Shell 配置](shell-configuration.md)中启用 `UseTips()`，再使用 `ConfigureTips` 调整显示时机和可用区域。

## 最小配置

```csharp
builder
    .ConfigureShell(shell => shell.UseTips())
    .ConfigureTips(tips =>
    {
        tips
            .SetDelay(200)
            .SetSpawnableMargin(5);
    });
```

## 显示延迟

`SetDelay` 设置指针悬停后到提示出现前的时间，单位为毫秒。较短延迟会更快显示提示；较长延迟可减少指针经过控件时的意外显示。

## 窗口边界

`SetSpawnableMargin` 设置提示浮层与 Shell 窗口边缘之间保留的最小距离。该约束适用于靠近标题栏、折叠导航栏、动态工具栏和状态栏边缘的提示。

边距必须是有限非负数，延迟不能为负数。

## 启用关系

调用 `UseTips(false)` 会禁用提示浮层，即使已经提供 `ConfigureTips` 设置。重新启用后，已配置的延迟和边距会继续生效。

## 相关功能

- [标题栏](configure-title-bar.md)、[导航](navigation.md)、[动态工具栏](dynamic-toolbar.md)和[状态栏（Footer）](status-bar.md)包含可显示提示的 Shell 控件。
- [动效](configure-motion.md)配置与提示浮层相互独立的过渡和悬停动画。
