---
title: Navigation
description: Register and navigate between Flourish pages.
---

# Navigation

Register pages during service configuration with `AddNavigable`. Each registered page receives display metadata for shell navigation and can opt into page caching.

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

Pages must derive from `System.Windows.Controls.Page`. Flourish registers page types with the service collection and uses the navigation metadata to build the shell navigation surface.

For runtime navigation, request `INavigationService` from dependency injection and navigate by page type:

```csharp
public sealed class HomeViewModel(INavigationService navigation)
{
    public void OpenSettings()
    {
        navigation.Navigate<SettingsPage>();
    }
}
```
