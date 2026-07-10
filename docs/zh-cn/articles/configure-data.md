---
title: 应用数据
description: 为 Flourish 功能提供稳定的应用标识和偏好存储位置。
---

# 应用数据

Flourish 使用应用名称和公司名称区分不同应用的偏好数据。主题、Profile 等需要持久化的功能会共享这份应用标识。使用 `ConfigureData` 可以设置标识，也可以在需要时指定自定义偏好目录。

```csharp
builder.ConfigureData(data =>
{
    data
        .SetAppCompany("Foobar Company")
        .SetAppName("Foobar");
});
```

## 设置应用标识

`SetAppName` 提供偏好存储使用的应用名称。没有显式配置应用名称时，Flourish 会使用[标题栏](configure-title-bar.md)中设置的标题。

`SetAppCompany` 提供默认偏好路径中的公司目录片段。更改应用名称或公司名称会改变默认存储位置，原位置中的偏好不会自动迁移。

默认目录需要公司名称，以及应用名称或非空标题。无需按这些值派生存储位置时，可以改用显式目录。

## 指定偏好目录

`SetAppPreferenceDataPath` 会覆盖默认存储目录。默认路径按应用标识隔离数据；便携式存储或工作区级存储可以通过自定义目录实现。

```csharp
builder.ConfigureData(data =>
    data.SetAppPreferenceDataPath(preferencePath));
```

应用应确保自定义目录可写，并在整个运行期间保持可用。

## 相关功能

- [主题](configure-themes.md)会把用户选择的主题写入偏好存储。
- [用户资料（Profile）](configure-profile.md)使用应用标识隔离记住的登录状态。
- [标题栏](configure-title-bar.md)可以通过 `SetTitle` 提供回退应用名称。
- [`IFlourishBuilder`](flourish-builder.md) 说明配置回调的应用时机。
