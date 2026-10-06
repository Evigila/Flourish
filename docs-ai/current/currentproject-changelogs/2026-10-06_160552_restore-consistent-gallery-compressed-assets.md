# Restore consistent compressed Gallery assets

The user reported widespread layout and Icon failures after the title-localization repair using the original launcher. The same normal Release/Development startup reproduced an empty HTTP 200 Framework CSS response with compressed negotiation, while identity CSS remained available.

Prior validation used OutDir alone and therefore shared the normal Web/Razor obj state. Normal runtime mappings still referenced the 58b0pj2wmg compressed CSS fingerprint, while generated assets used 2o7135cjvg. HTML-only title validation omitted this failure. See [the incident report](../bugfix-reports/2026-10-06_gallery-stale-compressed-assets.md) for preserved evidence and responsibility.

Rebuilt the normal Gallery output to restore matching bundles, compressed files and manifests. Added real identity/gzip/browser-negotiated resource checks for Framework/Design CSS, Gallery scoped CSS, Material Symbols WOFF2 and Blazor JS. Validation now uses --artifacts-path to isolate both output and intermediates; six normal manifest/bundle hashes were unchanged during the isolated title tests. Source theme/font configuration, the original launcher and title translations were preserved.

Verification: zero-warning/error normal Gallery Release rebuild; 502 HTTP checks and 42 heading refresh/disposal checks passed. The diagnostic host was stopped. No Computer Use, external dependency, Colligere write, commit or publication occurred.

Manual acceptance: start through the original launcher, refresh, verify layout and Icons on Home/Controls/Icons, then check language switching and title collapse/expand.
