# Organize documentation and stabilize Gallery headings

Recorded using project local time: America/Sao_Paulo.

## Changes

- Replace Gallery's clock logo with the official Google Material Symbols Outlined browse SVG from the already selected repository revision and Apache 2.0 source. Its glyph is 36px within the existing 48px brand target, slightly smaller than the previous logo. Retain its favicon use and existing packaged license.
- Prevent short-page heading collapse/expansion feedback by testing the resulting scroll range and remembering unsuitable geometry. Share the implementation between ApplicationShell and NavigationSurface through the existing shell module. Keep long-page thresholds, compact typography and back-action behavior.
- Move IconSupport's aligned icon and text toward the middle of their equal halves, leaving 4px padding on each side of the join. Keep the grid layout effective under optional Design button styles.
- Remove the unused WPF favicon.png; resource, title-bar, tray and DocFX references use favicon.ico. Remove ignored caches from the obsolete monolithic Blazor project and the renamed WinUI Gallery. Keep the Native verification host, embedded samples and Material Symbols font/catalog.
- Consolidate active implementation, rendering, project-boundary and manual acceptance guidance. Delete blazor-render-review.md and blazor-project-boundaries.md after merging useful facts. Add current/index.md as the reading map, update the explanatory directory tree, and retain common standards, human DocFX content, historical bug reports and append-only change records.
- Replace culture-web-readiness.md with culture-web-integration.md. The new read-only source audit records the clean Essential.Culture checkout at e03775a, existing support, per-user isolation blockers, proposed optional adapter/contracts, resource packaging and staged acceptance. No Culture implementation or dependency was added.
- Preserve preceding interaction/semantic/style improvements and the user's existing skill deletions and .gitignore edit for the requested all-changes save. README files remain absent and archive remains absent.

## Verification

The normal fourteen-project root solution builds with zero warnings/errors. Blazor checks pass 105/105; every Node check script passes, including five new heading regressions. All 77 guides retain five sections/default columns; Framework-only HTTP checks pass. Fingerprinted shell, controls and surfaces assets decode correctly under identity/gzip/Brotli requests from normal Gallery output.

Isolated headless installed Edge verifies stable short-page heading geometry at three widths, normal long-page compaction/back action, equal grid halves with an 8px central gap, the browse asset and local NavigationSurface behavior without browser exceptions. Prior focused checks cover semantic notices, popup placement, theme/palette callbacks and form behavior. Temporary hosts use dynamic ports and stop only their own processes.

Essential.Culture's existing Release test artifacts pass 43/43. They were not rebuilt; binary/source identity and Server/SSR/WASM behavior are not established. Proposed localization APIs remain unimplemented. The original stopped localhost:5188 service could not provide its failed old-hash module response; the current normal build/module graph is verified. The bug report preserves that limit.

## Documentation and manual follow-up

Historical links to the removed active guides/readiness report remain unchanged to preserve append-only records. The current index and new integration plan identify their canonical replacements. Required documentation paths, local active links and maintained architecture-tree coverage are checked before saving; no README or AI-authored human documentation is introduced.

Restart VS Gallery after the rebuild, fully reload localhost:5188, scroll the short Home page and a long control page, and inspect IconSupport cells with short/wrapped titles. The full checklist remains in blazor-manual-tests.md. Git commit and remote push are authorized by the user after these reported issues are resolved; no further confirmation is required.
