---
title: ConfigureData
description: Configure Flourish application data and preference storage.
---

# ConfigureData

`ConfigureData` configures application identity and preference storage. A WPF application uses it when Flourish features need a stable application name, company name, or custom preference directory.

```csharp
builder.ConfigureData(data =>
{
    data
        .SetAppCompany("Acksheed System Team")
        .SetAppName("Flourish Gallery");
});
```

## Details

`SetAppName` provides the name used for preference storage. When no app name is configured, Flourish falls back to the title configured through [`ConfigureTitleBar`](configure-title-bar.md).

`SetAppCompany` provides the company folder segment used by the default preference path. It should be stable across releases so saved preferences stay discoverable after an update.

`SetAppPreferenceDataPath` overrides the default storage directory. Native WPF applications commonly keep the default path unless they need portable settings or a workspace-specific settings folder.

## Related APIs

- [`ConfigureThemes`](configure-themes.md) stores the selected theme in the preference store.
- [`ConfigureTitleBar`](configure-title-bar.md) can provide the fallback app name through `SetTitle`.
- [`IFlourishBuilder`](flourish-builder.md) explains when builder callbacks are applied.
