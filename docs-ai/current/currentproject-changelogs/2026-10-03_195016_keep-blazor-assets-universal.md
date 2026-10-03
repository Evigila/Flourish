# Keep Blazor UI universal and repair loading costs

The user rejected consumer-specific assets and compatibility entrypoints in Flourish. Framework/Design now contain generic UI assets only; consuming products own business pages, real isolated styles, role mappings and business scripts. Neutral Data* contracts, FormSurface and generic surface roots replace business-named APIs. Gallery's composition example is /patterns.

Modular CSS is flattened at build time into one primary CSS per library project. Installed SDK fingerprints, caching and gzip/Brotli alternatives work for ProjectReference and independent NuGet PackageReference consumers. Runtime imports and internal source CSS publication are removed. Framework remains usable without Design. Gallery/native request logging is configured locally; unused sheets initialize lazily and table widths avoid redundant probes while preserving invalidation and manual sizing.

Debug/Release builds passed with zero warnings/errors. Library renderer checks passed 49/49. CSS parser checks passed 21; expanded isolated SDK/HTTP checks passed 62. The consuming Web suite passed 1595, failed zero and skipped 85 environment-gated checks. Fresh independent package consumers built/published both modes with zero warnings/errors. Actual published styled CSS transfers total 219487 bytes uncompressed, 34298 with gzip or 28849 with Brotli; stable and fingerprint paths decompress identically and return 304 for matching ETags.

Temporary UI renaming mistakes in twelve host rate-limit identifiers were fully restored; no security policy changes remain. No external dependency, Computer Use, public-feed publication or Git commit was performed. The user's existing VS host remains running and requires a rebuild/restart for manual cold/warm browser timing.

See [diagnosis and implementation evidence](../bugfix-reports/2026-10-03_generic-blazor-assets-and-page-load.md), [current extraction boundaries](../blazor-extraction.md) and [manual acceptance](../blazor-manual-tests.md). Previous change records remain historical; this record supersedes the earlier consumer page-skin/compatibility proposal and earlier 47-check acceptance totals.

