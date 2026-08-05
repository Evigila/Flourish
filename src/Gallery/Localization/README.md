# Gallery localization

Gallery localization is application code. It remains in the Gallery assembly and
does not add application strings or keys to Flourish.

Every localized value is addressed by a stable `Gallery.*` resource key. The
embedded catalogs under `Catalogs` provide explicit `en-US` and `zh-CN` values for
the same key set. English is the fallback catalog: selecting a locale for which the
Gallery has no catalog still renders the English value instead of a resource key.

`GalleryLocalizationService` observes the locale selected through
`IFlourishLocalization`, resolves Gallery keys, and refreshes cached pages and later
popup content. XAML display properties contain stable keys, for example:

```xml
<flourish:Chunk
  Title="Gallery.Shell.Explore_3B73900B"
  Content="Gallery.Shell.ChooseAFeatureAreaToOpenItsInteractiveGuideStartWithConfiguratio_E9A7BC4A"
/>
```

The loaded-element integration resolves those keys into the selected catalog. Code
should use constants from `GalleryLocaleKeys` with `IGalleryLocalization.Get` or
`Format` instead of embedding a key or display text directly:

```csharp
localization.Get(GalleryLocaleKeys.ApplicationOverview_D4B1EA57);
localization.Format(GalleryLocaleKeys.DynamicLocaleChangedTo0_1C2A91ED, locale);
```

`GalleryShellLocalizationCoordinator` republishes application-owned shell labels
such as navigation groups, navigation items, the search placeholder, and toolbar
commands after a locale change. Stable IDs, route keys, command keys, API
identifiers, code samples, and user input are not localization resources.

## Catalog contract

- A resource key represents meaning and ownership; changing English wording must not
  require changing the key.
- All supported Gallery catalogs contain the same key set.
- Composite-format placeholder indexes are identical between locale values.
- Missing requested-locale entries fall back to `en-US`; an unknown key remains
  visible as the key so omissions can be diagnosed.
- New runtime messages use `Get` or `Format`. New XAML display text uses an existing
  stable key or adds that key to every Gallery catalog.

## Relationship to Flourish

This follows the framework's localization pattern: stable semantic keys, explicit
English and translated catalogs, English fallback, and a key inventory that tests
can compare with catalog contents. The ownership boundary stays separate:

- `src/Flourish/Assets/lang_*.json`: framework-owned built-in interface text.
- `src/Gallery/Localization/Catalogs/*.json`: Gallery-owned client text.
- `IFlourishLocalization`: shared locale selection and framework localization API.
- `IGalleryLocalization`: Gallery lookup, formatting, and page refresh behavior.

Do not move `Gallery.*` keys into Flourish assets or register Gallery catalogs with
Flourish. Sharing the selected locale does not make application text part of the
framework's public localization surface.
