---
title: 材质特效
description: 选择 Flourish Shell 窗口使用的 Windows 材质。
---

# 材质特效

`UseMaterialEffect` 为 Shell 窗口选择背景材质。

```csharp
builder.ConfigShell(shell =>
    shell.UseMaterialEffect());
```

## 选择材质

| 值 | 行为 |
| --- | --- |
| `MaterialEffect.Auto` | Windows 11 使用 Mica，Windows 10 使用 Acrylic，不支持的系统不启用材质。 |
| `MaterialEffect.None` | 使用不带系统材质的不透明 Shell 背景。 |
| `MaterialEffect.Mica` | 在 Windows 11 上使用适合长时主窗口的 Mica。 |
| `MaterialEffect.Acrylic` | 通过 Windows 11 系统背景或 Windows 10 兼容后端使用 Desktop Acrylic。 |
| `MaterialEffect.MicaAlt` | 在 Windows 11 build 22621 及以上版本使用颜色更强的 Mica Alt。 |

`MaterialEffect.Auto` 是默认请求，并且材质效果默认启用。其实际结果为：Windows 11 build 22000 及以上使用 Mica，Windows 10 build 17134 及以上使用 Acrylic，其余平台使用 `None`。Windows 11 build 22621 及以上使用正式的 DWM 系统背景 API；Windows 11 初始版本使用旧版 Mica 属性，Windows 10 Acrylic 则使用隔离的 AccentPolicy 兼容后端。

传入 `enabled: false` 或选择 `MaterialEffect.None` 可恢复不透明 Shell 背景。`IsSupported` 用于判断具体材质是否可用；运行时通过 `SetEffect` 显式请求不支持的材质时，会在改变状态前抛出 `PlatformNotSupportedException`。`Auto` 不会抛出平台异常，必要时会安全解析为 `None`。

材质应用于 Shell 窗口。内置页面宿主保持透明，使背景材质可以连续显示在内容区域中；页面仍可在设计需要时添加局部背景。`CurrentEffect` 返回请求值，`EffectiveEffect` 返回当前平台解析后的具体效果。

运行时材质选择默认从所选 Flourish 设置文件恢复并写回。需要代码配置的材质始终优先时，传入 `usePersistedPreference: false`。已保存组合缺失、无效或不完整时，会完整保留代码参数。当具体材质偏好被迁移到不支持它的平台时，Flourish 会把请求安全回退为 `Auto`，使用新平台默认值，而不会令 Shell 启动失败。

> [!NOTE]
> 当用户关闭透明效果、启用节电模式或系统渲染策略要求回退时，Windows 可能将 Acrylic 呈现为纯色。原生调用成功并不保证所有系统状态下都能看到模糊效果。

## 相关功能

- [窗口](configure-window.md)配置承载材质的窗口。
- [主题](configure-themes.md)控制与材质配合使用的亮色和暗色资源。
- [DWM 系统背景类型](https://learn.microsoft.com/windows/win32/api/dwmapi/ne-dwmapi-dwm_systembackdrop_type)定义 Windows 11 的材质映射。
