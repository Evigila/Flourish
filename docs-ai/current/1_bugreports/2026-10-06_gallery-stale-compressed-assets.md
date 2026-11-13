# Gallery lost layout and Icons after stale compressed-asset mapping

## Symptoms and confirmed reproduction

After the collapsible-title repair, the user reported extensive Gallery layout failures and missing Icons, using the original project launcher/start.bat. A diagnostic instance using the same Release output and Development launch profile reproduced the failure without Computer Use: identity Framework CSS returned 65,045 bytes, while gzip/browser-style compressed negotiation returned HTTP 200 with an empty body. Design CSS and the WOFF2 font file themselves remained available.

Framework CSS contains functional layout rules and the Material Symbols font-face/icon rules. Losing that stylesheet explains both symptoms even when the font file is not damaged.

## Cause and responsibility

Earlier validation changed only OutDir to avoid locking the running Gallery assemblies. This did not isolate the source projects' normal obj directories. Subsequent validation regenerated shared bundles/manifests while normal output retained older static-resource mappings. The previous title tests checked HTML and real component language changes but omitted browser-style compressed asset delivery. Calling that verification sufficient for Gallery was an error.

Fault evidence was preserved in artifacts/gallery-assets-incident-before. The normal runtime manifest mapped framework.css.gz to the 58b0pj2wmg fingerprint, while the current generated bundle/compressed asset used 2o7135cjvg. The old endpoint declared 64,886 uncompressed bytes; the current bundle contained 65,045. The stale compressed mapping produced the empty CSS response.

The audit found no heading-task modifications to App.razor, Gallery project settings, ApplicationLayout, Icon, AppearanceService, Directory.Build.props/targets, theme registration or font paths. Title lifecycle changes only subscribe/unsubscribe to scoped language events. The solution additions are test-project entries; the temporary solution-tool normalization of unrelated platform mappings had already been corrected. The defect was generated build state and the unsafe validation isolation boundary, not a reason to remove the title translations.

## Repair and prevention

1. Stopped only the diagnostic instance, preserved the user's source changes, and rebuilt Gallery with its normal Release output. The rebuild regenerated matching CSS, compressed files and manifests.
2. Extended Test-GalleryCulture.ps1 to request Framework CSS, Design CSS, the WOFF2 font, Gallery scoped CSS and Blazor startup JS using identity, gzip and browser gzip/br negotiation. It checks MIME, nonempty decoded size, SHA-256 equivalence and necessary CSS rules.
3. Switched validation to --artifacts-path so both outputs and intermediates are separated by project. Do not use OutDir alone as isolation for Web/Razor builds.
4. Verified six normal output/intermediate manifest/bundle hashes were unchanged during an isolated heading-regression run.

The existing launcher and production UI/style/font configuration were preserved. No external dependency, business behavior, Colligere change, automated browser test or Git commit was introduced.

## Verification and limits

- Normal Gallery Release rebuild with TreatWarningsAsErrors: zero warnings/errors.
- Updated real HTTP regression: 502 checks passed, including all five resource classes and three encoding negotiations.
- Framework CSS decoded to 65,045 bytes in each negotiation, Design CSS to 120,408 and WOFF2 to 4,003,088; per-resource decoded hashes matched.
- Real scoped heading-language/disposal regression: 42 checks passed under the isolated artifacts path.
- Six normal generated-file hashes stayed unchanged during that validation.
- The temporary 127.0.0.1:5272 instance was stopped after checks.

The normal launch build now has consistent resources. Manual visual acceptance remains with the user: start Gallery through the original launcher, refresh once, check layout/Icons on Home, Controls and Icons, then switch languages and exercise title collapse/expand. If a previously open page retains an earlier stylesheet, force-refresh after restarting; browser caching was not used as the diagnosis.
