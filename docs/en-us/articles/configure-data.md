---
title: Application data
description: Identify the application and choose where Flourish stores preferences.
---

# Application data

Flourish uses application identity to scope saved preferences such as the selected theme and remembered profile state. Configure that identity when the application needs stable preference storage or a custom storage directory.

## Configure application identity

```csharp
builder.ConfigureData(data =>
{
    data
        .SetAppCompany("Example Company")
        .SetAppName("Foobar");
});
```

`SetAppName` supplies the name used for preference storage. If it is omitted, Flourish uses the title configured through [Title bar](configure-title-bar.md).

`SetAppCompany` supplies the company directory segment in the default preference path. Changing either identity value changes that path; Flourish does not move preferences from the previous path.

The default directory requires a company name and either an application name or a non-empty title. Use an explicit directory when the application should not derive storage from those values.

## Choose a custom directory

Use `SetAppPreferenceDataPath` when preferences must be portable or stored with a particular workspace.

```csharp
builder.ConfigureData(data =>
{
    data.SetAppPreferenceDataPath(preferenceDirectory);
});
```

The application is responsible for choosing an accessible, persistent directory. Otherwise, keep the default location derived from the company and application names.

## Related features

- [Themes](configure-themes.md) persist the selected theme for this application identity.
- [Profile](configure-profile.md) uses the same identity to scope remembered login state.
- [IFlourishBuilder](flourish-builder.md) explains when configuration callbacks are applied.
