# Development module fetch failed after adding build-time Brotli

## Symptom and evidence

The user reported Microsoft.JSInterop.JSException at ActionMenu.OnAfterRenderAsync while importing controls.js. The import map resolved it to /_content/Arkheide.Flourish.Blazor.Framework/controls.fd8scthqhi.js.

The original 5188 VS process was not listening when inspected. A separately rebuilt Gallery reproduced an invalid wire response under default Development static asset reload behavior: HTTP 200 with Content-Type text/javascript and Content-Encoding br, but bytes beginning 1F-8B-08 (gzip). Both declared gzip and declared br responses were 11540 bytes; identity was the original 11507 bytes. A successful HTTP status therefore did not prove the browser could decode and import the module.

The fingerprint endpoint and physical source existed. The JavaScript source, component call and route hash were correct; suppressing JSException or adding a fallback import would hide the resource delivery defect.

## Cause

The preceding CSS/package repair appended brotli to BuildCompressionFormats. That generated Brotli build endpoints for all compressible assets, including the library's browser modules, in every consuming ProjectReference Development host.

The installed development reload handler rewrites compressed responses through GZipStream while retaining the selected endpoint's Content-Encoding header. Brotli build endpoints can therefore deliver gzip bytes under br. This is consistent with the runtime source's compressed-response path. The previous protocol regression ran Production only; it missed Development wire decoding.

Official guidance separates build/development gzip from publish gzip plus Brotli. No runtime or SDK upgrade is required for this repair.

## Repair

- Restore the SDK's normal BuildCompressionFormats; do not append Brotli in ordinary library builds, including Release configurations used by a Development host.
- Preserve the CSS primary fingerprint RelativePath expression in generated NuGet props. Flattening it to a physical filename was the underlying reason consumer Publish previously lost the Brotli fingerprint alias.
- Retain the existing CSS gzip package asset and relocate its RelatedAsset path to the consumer's package directory. Let the SDK Publish pipeline generate Brotli and both route aliases.
- Keep ordinary development reload and cache defaults, optional Design, two stylesheet entrypoints and component behavior.
- Extend the permanent SDK regression to actually decode Development CSS and JS responses for stable/fingerprint routes, ProjectReference/PackageReference and identity/gzip/br/browser Accept-Encoding lists. Production still checks both compression formats and identical decoded bytes.

No component exception handling, business route, authentication/session policy, new dependency or global runtime workaround was added.

## Verification

- Blazor child solution Debug (normal VS output) and Release: zero warnings/errors.
- Actual Gallery import-map module responses: 56/56 checked across 13 fingerprinted modules plus the stable controls.js alias, with identity/gzip/br/browser Accept-Encoding lists. All return JavaScript MIME and decode exactly to source bytes; Development selects identity or gzip correctly.
- Permanent isolated SDK suite: 194 checks passed, including ProjectReference/PackageReference Development CSS/JS, no-build pack/publish, incremental CSS rebuilding and Production stable/fingerprint gzip/Brotli.
- Five existing packages repacked into a fresh local feed; native/styled PackageReference consumers restored with private fresh caches, built and published with zero warnings/errors.
- Actual published package CSS stable/fingerprint × gzip/Brotli: all eight responses decode identically and matching ETags return 304. Styled totals remain 34298 bytes gzip and 28849 Brotli.
- Current package assets: Framework one primary CSS + one gzip + 13 JS modules; Design one primary CSS + one gzip. Brotli is generated on publish, not exposed in development build manifests.
- Consuming Web static-resource/ownership regression: 15 passed, zero failures/skips.
- git diff --check passed; all diagnostic hosts were stopped. No Git commit or public-feed publication was performed.

## Manual regression and limits

Stop the existing VS Gallery process, rebuild and restart it, then use Ctrl+F5 to start a fresh document/circuit and module load. Visit /, /records and /controls; open row menus, inputs and dialogs. Network should show correct JavaScript MIME and successfully decodable identity/gzip development responses, with no ERR_CONTENT_DECODING_FAILED or dynamic-import exception. Visit /patterns to exercise demand-loaded primitive modules and styles. Verify published CSS still negotiates gzip/Brotli on stable and fingerprint URLs.

No Computer Use was used. HTTP byte decoding proves the reproduced transport defect is repaired; complete browser interaction remains a manual acceptance check.

## Primary sources

- [Microsoft static asset build and publish compression guidance](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-10.0).
- [ASP.NET Core development reload compressed-response source](https://github.com/dotnet/aspnetcore/blob/main/src/StaticAssets/src/Development/StaticAssetDevelopmentRuntimeHandler.cs).

## Evidence

External task artifact root: C:/Users/Evigila/.codex/visualizations/2026/10/03/01a0ff44-995d-7090-af88-8835158b1571/

- module-validation/module-http-before-fix.json contains the invalid br/gzip wire prefix.
- module-validation/module-http.json contains subsequent actual Gallery module decoding checks.
- css-dev-final/css-assets-6b3d11b4ac4b40b38d6d42dd2c2803ca/ contains the final 194-check isolated SDK evidence.
- Earlier repair history remains in 2026-10-03_generic-blazor-assets-and-page-load.md; its two-compression-alternatives package approach is superseded by this report.

