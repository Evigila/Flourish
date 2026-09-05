---
title: Flourish
description: Flourish Core 与 WPF 文档，以及未来 WinUI 3 实现路线图。
---

# Flourish

Flourish 是 Windows 应用 Shell 与控件库。平台无关契约和服务位于 Core，当前完整 UI 实现面向 WPF。WinUI 3 项目目前仅保留标准空白 Windows App SDK 基线，未来工作以路线图为准。

应用启动通常放在 `App.xaml.cs` 中，也可以放在应用配置的其他自定义入口中；视觉资源可以从 `App.xaml` 引入，Shell 行为则通过 fluent builder 配置。

> [!NOTE]
> WPF 是当前的 UI 实现。WinUI 3 尚未提供 Flourish Shell、控件库或可供使用的包。

## Flourish 提供什么

- 平台无关的命令、后台任务、本地化、设置、项目、通知、导航菜单与 Shell 状态
- WPF 中基于 `ApplicationBuilder` 和 `IApplicationRuntime` 的 Host 启动流程
- 由显式 `Flourish*` 自定义控件组成、不会改写原生 WPF 与第三方控件（包括其 Tooltip 模板）的可复用控件库
- 标题栏、导航栏、材质特效、字体和窗口尺寸等 Shell 配置
- 通过依赖注入注册页面并执行导航
- 连接到命令调度的页面专属动态工具栏项
- Host 管理的后台任务、运行状态图标、自定义状态项，以及合并的局域网和电源详情
- 页面过渡、导航栏动画和悬停揭示动效配置
- 可从 `App.xaml` 合并的主题资源

## 从这里开始

- [WPF 快速开始](articles/getting-started.md)
- [WinUI 3 实现路线图](https://github.com/Evigila/Flourish/blob/master/docs/roadmap.md)
- [控件库](articles/control-library.md)
- [Shell 配置](articles/shell-configuration.md)
- [导航](articles/navigation.md)
- [动态工具栏](articles/dynamic-toolbar.md)
- [后台任务](articles/background-tasks.md)
- [API 参考](xref:ArkheideSystem.Flourish.Abstract)

## 项目链接

- [GitHub 仓库](https://github.com/Evigila/Flourish)
- [Issues](https://github.com/Evigila/Flourish/issues)
- [Pull requests](https://github.com/Evigila/Flourish/pulls)

欢迎通过 issue 或 pull request 提交缺陷报告、文档修正、API 反馈和示例。
