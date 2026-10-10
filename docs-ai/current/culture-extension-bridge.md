# Optional Culture bridges

## Current contract

The Blazor bridge targets net10.0 and integrates through `IFrameworkBuilder.ConfigureCulture`, supplied by `Flourish.Extensions.Culture.Blazor`. It references the current Blazor Framework and Essential.Culture.Blazor package. Framework remains independent of Essential; the optional adapter owns session registration, request negotiation, browser persistence, renderer subscriptions and LanguagePicker.

Register embedded or loaded catalogs through CultureBuilder, or register multilingual file modules with `AddCatalogFiles(catalogId, paths, fallbackCulture, options)`. Generated `CultureResources.Files` and `FallbackCulture` keep deployment and runtime loading aligned. Relative file paths resolve against AppContext.BaseDirectory. Essential owns key uniqueness, fallback and retained-language validation.

The shared Essential package pin is 1.4.0. Gallery.Blazor supplies seven functional multilingual modules; framework/extension caption resources remain embedded and independently owned. Immutable catalogs are shared; selected UI/format cultures and providers remain request/circuit scoped. Standard browser preferences survive refresh, and defaults come from appsettings.Flourish.json without rewriting it.

`AddFlourishCulture` and the former separate host registration/persistence APIs are retired. The complete current registration, file format, generator settings, failure semantics and preference boundaries are documented in [Culture Web integration](culture-web-integration.md).

## Native WPF

The native bridge targets net10.0-windows and adapts Essential.Culture.Wpf to the native text contract. Consumers configure `FrameworkBuilder.UseEssentialCulture` and own/dispose the returned provider. A loaded LocalizationCatalog supports isolated selection; the existing static-facade path remains available for consumers that intentionally share that facade. See [WPF integration](wpf-native-integration.md) for lifecycle and format-culture behavior.

## Evidence and scope

See [the Essential module report](bugfix-reports/2026-10-10_essential-modules-bridge.md), [dependency inventory](1_dependency.md), and [package verification guide](nuget-release-integration.md). Static SSR and Interactive Server are verified; WASM/Interactive Auto remain unverified. This documentation repair does not publish packages.

The [previous bridge guide](1_archived/2026-10-10_previous-culture-bridge-guide.md) retains obsolete API/version descriptions as history.
