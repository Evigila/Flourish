---
title: ConfigureData
description: 配置 Flourish 应用数据和偏好存储。
---

# ConfigureData

`ConfigureData` 配置应用标识和偏好存储。WPF 应用在需要稳定的应用名称、公司名称或自定义偏好目录时使用它。

```csharp
builder.ConfigureData(data =>
{
    data
        .SetAppCompany("Acksheed System Team")
        .SetAppName("Flourish Gallery");
});
```

## 细节

`SetAppName` 提供偏好存储使用的应用名称。没有配置应用名称时，Flourish 会回退到 [`ConfigureTitleBar`](configure-title-bar.md) 中配置的标题。

`SetAppCompany` 提供默认偏好路径中的公司目录片段。它应在版本之间保持稳定，避免升级后丢失已保存偏好。

`SetAppPreferenceDataPath` 会覆盖默认存储目录。原生 WPF 应用通常保留默认路径；便携式设置或工作区级设置才需要显式路径。

## 相关 API

- [`ConfigureThemes`](configure-themes.md) 会把用户选择的主题写入偏好存储。
- [`ConfigureTitleBar`](configure-title-bar.md) 可以通过 `SetTitle` 提供回退应用名称。
- [`IFlourishBuilder`](flourish-builder.md) 解释 builder 回调的应用时机。
