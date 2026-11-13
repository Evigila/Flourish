# Executable component guides and larger Material icons

The user reiterated the Google material-design-icons repository, requested larger default icons and directly visible component sections, and specified five parts for each guide: introduction, blank preview/variants, API explanation, scenario and reproducible scenario-code explanation.

## Changes

- Verified that the pinned Material Icons Outlined font already comes from the requested Google repository. Its source revision, Apache-2.0 license and self-hosted loading remain intact. No additional icon package, font family or CDN was installed.
- Increased standard/tool glyph defaults to 26px, primary navigation to 24px, and search/information to 22px. Added public Design size variables and the Framework-only 26px fallback. Existing click-target and logo geometry remains separate. This is the user's authorized Flourish exception to the portable glyph sizes.
- Replaced the nested disclosure catalog and category-wide mixed demonstrations with directly visible titled guides. Each guide has all five requested parts and a reflected parameter table. A standalone route shows one guide while preserving its secondary category.
- Added 73 compiled Razor samples, each with empty/basic previews and an independent scenario. Examples include actual input binding, choices, validation, menu callbacks, Busy handling, table state/view/paging, overlays, progress, grid edits and local layout composition. Fictional data is explicitly identified.
- Each sample carries its own imports, model and callbacks. Compiled source is embedded and displayed after removing only Gallery metadata; the live scenario and shown code share one file. The source explanation identifies binding, simulated async work, selectors, preference keys and preview-layout CSS where applicable.
- Corrected existing namespace-alias markup in API snippets. Razor component tags require recognized component names or fully qualified names; C# namespace aliases remain usable in expressions.
- Loaded Gallery's isolated guide CSS as host-owned presentation. Nested shell and interaction previews are locally constrained. NavigationGuard is opt-in and explains that it protects the current page.
- Updated active implementation notes, manual acceptance and the directory map. Independent map validation found 859 files, 132 directories and root, with no missing, extra, duplicate or unexplained nodes.
- No changes to Essential.Culture or Colligere. Pre-existing working-tree changes and user skill deletions were preserved.

## Validation and repairs

- Gallery and its dependencies build with zero warnings/errors using an isolated temporary output directory.
- A normal-output build reached compilation but failed copying DLLs because the user's Visual Studio/Gallery process held them. That process was preserved; a temporary output allowed verification without interrupting it. The user must stop debugging, rebuild and restart to load updated code in their own process.
- Copied all 73 displayed scenario sources into a separate Razor project without a Gallery reference or Gallery imports. Build passed with zero warnings/errors. This caught and repaired one missing framework namespace.
- In that independent project, HtmlRenderer successfully rendered 73 blank previews and 73 scenario instances using only Framework/Design registration and mocked browser/navigation boundaries: 146 renders passed. Post-render browser interaction is not established by HtmlRenderer.
- All eight category routes and 73 standalone component routes returned HTTP 200. Category output included exactly 73 guides, all five headings per guide, and no unresolved namespace-alias component tags. Each standalone route contained one guide and kept secondary navigation selected.
- Repaired a missing PreferenceKey in the primitive Registry table discovered by HTTP rendering. Each preview/scenario now has an isolated stable key for its component lifetime.
- Repaired HTML-encoded child selectors in dynamic preview style text. The CSS is constructed only from internal rules and generated IDs, then rendered as trusted markup; no user input enters it.
- Guide CSS and the Google font returned HTTP 200. Served Design CSS contains enlarged defaults. Home still has no secondary rail.
- Git diff whitespace check passed. Temporary HTTP validation hosts were stopped. No Computer Use test, commit or package publication was performed.

## Acceptance boundary

The blank/control examples retain structural labels and triggers where needed. Behavior-only components explain why they have no visible surface. Dialog/sheet bodies are opened by example buttons rather than automatically overlaying the page. Internal composition primitives identify the recommended higher-level control.

Keyboard, pointer, focus, narrow layouts, zoom, theme, clipboard and interactive state transitions remain in blazor-manual-tests.md. EditingGrid demonstrates single-cell change, validation, undo/redo and local save; multi-cell paste and remote transactions are not pretended to be implemented by the sample.
