# Gallery localization

Gallery localization is application code. It is intentionally kept in the Gallery
assembly and does not add application strings or keys to Flourish.

`GalleryLocalizationService` observes the locale selected through
`IFlourishLocalization`, loads Gallery-owned embedded catalogs, and refreshes literal
page copy when cached pages or later popup content are displayed. English XAML text is
the fallback source. Chinese translations are split by application, shell-page, and
control-page domains under `Catalogs`.

`GalleryShellLocalizationCoordinator` separately republishes application-owned shell
labels such as navigation groups, navigation items, the search placeholder, and toolbar
commands. Stable IDs, route keys, command keys, API identifiers, code samples, and user
input are never translated.

Dynamic application messages should call `IGalleryLocalization.Get` or `Format` before
they are presented. Data-bound `ControlMemberRow` descriptions refresh through the same
client service. A locale without a Gallery catalog falls back to the original English
source even when Flourish itself has translations for that locale.

## Ownership boundary

- `src/Flourish/Assets/lang_*.json`: framework-owned built-in interface text.
- `src/Gallery/Localization/Catalogs/*.json`: Gallery-owned client text.
- `IFlourishLocalization`: shared locale selection and framework localization API.
- `IGalleryLocalization`: Gallery lookup, formatting, and page refresh behavior.
