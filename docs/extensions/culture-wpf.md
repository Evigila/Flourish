# Flourish.Extensions.Culture.WPF

Connects the public Flourish localization service to Essential.Culture.

Install this package instead of installing `Arkheide.Essential.Culture.Wpf` directly. The Culture
runtime, WPF adapter, and source generator are provided transitively.

```csharp
using ArkheideSystem.Flourish.Extensions.Culture.WPF;

var application = ApplicationBuilder
    .CreateDefaultBuilder(args)
    .UseEssentialCulture()
    .ConfigureData(data => data.SetLocale("en-US"))
    .Build();
```

`ILocalizationService` remains the public culture endpoint:

```csharp
localization.SetLocale("zh-CN");
```

The extension synchronizes Essential Culture and localizes stable Culture tokens stored in the
Flourish navigation, title bar, search, toolbar, and status bar state. Application XAML and C# can
continue to use the generated Culture keys and localization syntax supplied transitively by the
Culture packages.
