---
title: Flourish
description: Flourish WPF Shell 组合库文档。
---

# Flourish

Flourish 是面向 WPF 的开源桌面应用组合库。它为需要标准 Shell 的应用提供简单的启动流程、窗口外观配置、导航、动态工具栏命令、状态栏集成、页面缓存、材质特效和动效选项。

Flourish 的 API 表面保持小而清晰。应用启动可以集中在 `Program` 中完成，视觉资源可以从 `App.xaml` 引入，Shell 行为则通过 fluent builder 配置。

> [!NOTE]
> Flourish 面向 WPF，因此仅支持 Windows 桌面应用。项目应使用 `net10.0-windows` 这类 Windows 目标框架，并启用 WPF。

## Flourish 提供什么

- 基于 `FlourishBuilder` 和 `IFlourish` 的 Host 启动流程
- 标题栏、导航栏、材质特效、字体和窗口尺寸等 Shell 配置
- 通过依赖注入注册页面并执行导航
- 连接到命令解析器的上下文动态工具栏项
- 状态栏文本、自定义状态项，以及内置局域网/电源状态
- 页面过渡、导航栏动画和 Hover Reveal 动效配置
- 可从 `App.xaml` 合并的主题资源

## 从这里开始

- [快速开始](articles/getting-started.md)
- [Shell 配置](articles/shell-configuration.md)
- [导航](articles/navigation.md)
- [动态工具栏](articles/dynamic-toolbar.md)
- [API 参考](xref:AckSS.Flourish.Abstract)

## 项目链接

- [GitHub 仓库](https://github.com/typoo910/Flourish)
- [Issues](https://github.com/typoo910/Flourish/issues)
- [Pull requests](https://github.com/typoo910/Flourish/pulls)

欢迎通过 issue 或 pull request 提交缺陷报告、文档修正、API 反馈和示例。
