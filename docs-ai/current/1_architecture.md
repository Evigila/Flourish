# Project architecture and repository tree

**Stable 1.4.3 published, 2026-10-26:** Tag `v1.4.3` retains source commit `d4e8cd95c58e87646684903a0fdb2f9767994931`. Hosted build and Trusted Publishing succeeded; all six public package indexes and fresh-cache public-only consumers (**143/143**) verified. This record changes no source structure or dependency direction. See [completed release verification](release-verification.md#completed-flourish-143-release-on-2026-10-26); the preparation and preview observations below remain historical.

**Stable 1.4.3 preparation, 2026-10-26:** The root version is now **1.4.3** without a preview suffix, and the Gallery moves the reviewed preview changes into 1.4.3 with an empty 1.4.4-preview. The same six Core/Blazor packages and Essential 1.4.0 dependency remain. The internal notification service accepts an optional .NET `TimeProvider`, defaulting to `TimeProvider.System`, so replacement-expiration tests can advance time deterministically rather than race hosted-runner scheduling. No maintained file was added outside the excluded documentation/artifact areas, so the 701-file tree is unchanged. This is preparation, not evidence of public publication; see [release verification](release-verification.md#flourish-143-final-preparation-2026-10-26).

**Current Field-hover preview, 2026-10-25:** Root is **1.4.3-preview.portals.18**. A Design-owned empty direct-label ::before box bridges the existing Field gap, preserving native hover association and label measurements. The existing gap token drives both spacing and bridge size; actions/errors remain outside. No maintained source file, API, external dependency or production JavaScript was added, so tree and dependency direction are unchanged. Framework **474/474**, Node **35/35**, access spacing **4/4**, six package checks and consuming Release/Web verification passed (**2605 passed, 0 conditional skips**). [Delivery record](1_changelogs/2026-10-25_233308_bridge-field-label-hover-preview-eighteen.md) preserves evidence and manual limits.

**Previous unified-Notice preview, 2026-10-25:** Root is **1.4.3-preview.portals.17**. Information is white/blue/dark; warning orange/white, success green/white and error red/white. All Notices share body-sized title/body, 700/400 weights, a separate title row and existing control shadow; Subtle no longer selects a gray skin. Fixed semantic status paints remain stable across themes and host branding. Existing palette values are reused except a strong orange for readable white text; no external dependency or maintained source file was added, so tree and dependency direction are unchanged. Framework **474/474**, palette Node **33/33** (64 mode/brand/status/Subtle combinations), six package checks and consuming Release/Web verification passed (**2601 passed, 187 conditional skips**). See [delivery record](1_changelogs/2026-10-25_224053_unified-notice-colors-preview-seventeen.md) for evidence and manual checks.

**Previous Notice-title preview, 2026-10-25:** Root is **1.4.3-preview.portals.16**. Blazor Notice uses an explicit summary or a localized severity title, separate title/body rows, unchanged body font size and a bold semantic title, including subtle notices. Inline body fragments stay together. Framework **474/474**, CSS bundle **21/21**, six package verifications and consuming Colligere Release/Web checks passed (2601 passed, 187 conditional skips). No maintained source file or external dependency was added; the repository tree and dependency direction remain unchanged. Essential remains **1.4.0**. Six local archives have not been publicly published; [delivery record](1_changelogs/2026-10-25_222029_notice-titles-preview-sixteen.md) documents manual acceptance and evidence.
**Previous unframed-menu preview, 2026-10-25:** Root declares **1.4.3-preview.portals.15**. ActionMenu adds optional `Unframed=false` and `PanelColumns=null` (positive values only). Framework owns gated open-state grid layout and preserves direct tile centering, native disclosure/enhanced top-layer state, viewport bounds and scrolling. Design can remove panel decoration while standalone Elevated UniformGridButton children keep their own surface, radius and shadow. Default menus retain their prior surface and list layout. No maintained source file, dependency or production JavaScript was added; the repository tree is unchanged. Actual Framework **472/472** (83 components, 765 parameter rows, 355 documented defaults), focused CSS/placement Node **52/52** and ActionMenu DOM-only **28/28** passed; 14 physical browser checks were explicitly skipped. The [implementation/verification record](1_changelogs/2026-10-25_003131_unframed-action-menu-columns-preview-fifteen.md) preserves the first strict-fixture rejection and final pass. Consumer package/host verification belongs to the coordinating task. Audit state remains unchanged.

**Historical heading-loading preview, 2026-10-23:** Root declares **1.4.3-preview.portals.14**. PageHeading adds optional `Loading=false` and an absolute 4px progress strip aligned to body content gutters, flush with its lower divider. Framework CSS `:has()` links observable `data-f-loading=true` and `aria-busy=true` during the same style update. LoadingState, unfinished ProgressBar/ProgressRing and asynchronous Button actions participate without page-specific wiring. Page-level `body`/`.f-root` aggregation includes root-level reconnect dialogs; embedded PageBody/shell/surface scopes also participate. Several headings in one document may respond together, intentionally favoring early feedback. Semantic hidden/inert/inactive dialog and popover sources are excluded; manual Loading only adds feedback. Compact behavior remains, and DocumentFlow titles are relative and non-sticky. The standard 40px ProgressBar replaces nested transform animations with bounded logical positioning/background paint movement, addressing the screenshot's suspected compositing path without claiming a reproduced browser defect. Final Blazor Release built with zero warnings/errors; Framework **469/469** (83 components, 763 rows, 354 defaults), Gallery **8,585** (474 post-event, 379 ChangeLog), maintained Node entry points **193/193** rerun on the final body/.f-root source, plus focused heading checks **3/3**, CSS bundle/SDK **21/194**, preview **12** and ChangeLog **148** passed. Six exact local .14 packages passed identity/dependency/asset verification. Colligere's .14 debug-handler restart now has all eight ready probes at HTTP 200; actual HTTP Framework/Design CSS SHA-256 matches producer and package bytes. Consumer package/runtime provenance is in its implementation report and ignored runtime/inventory JSON. Physical animation and first-paint priority remain manual acceptance. The exact inventory now contains **701 files/121 directories**, adding the four CSS/regression files below; full audit completion is unchanged. See the [diagnosis and manual checklist](1_bugreports/2026-10-23_page-heading-loading-and-progress-rendering.md) and [change record](1_changelogs/2026-10-23_203946_add-heading-loading-and-stabilize-progress.md).

**Historical compact-choice preview, 2026-10-23:** Source base 734124f plus the compact-selector changes declares 1.4.3-preview.portals.12. Choice indicators use independent 28px sizing and 9px corners; option rows use a 40px minimum, 6px vertical padding and 12px corners. Ordinary controls and standalone label click areas remain 48px. Framework owns structure and native behavior; Design owns paint. No public component parameter, external dependency or maintained source file is added. Library component checks 460, focused Node 76, Gallery 8,565, CSS bundle/SDK checks 21/194, preview identity 12 and ChangeLog 142 passed. The [library change record](1_changelogs/2026-10-23_195028_compact-choice-indicators-preview-twelve.md) retains detailed scope and manual acceptance. Colligere generated and verified all six .12 packages, consumes five matching archives, passes full Web 2,444/153 conditional skips and dedicated PostgreSQL onboarding 26/26, and serves both verified CSS bundles over local HTTP. Consumer evidence is recorded in [its implementation report](../../../Colligere/docs-ai/current/1_bugreports/2026-10-23_account-onboarding-and-compact-selectors.md). Maintained file/tree scope and audit completion state are unchanged.

**Historical local popup preview, 2026-10-22:** Source **`c2207ec` plus the scoped popup update** declares **1.4.3-preview.portals.11**. Popup placement still adapts to the trigger edge and available vertical space; item content always aligns left. Native picker width is measured from an offscreen inert real-content copy before opening, and native choice markers share the standard 48px control geometry in a fixed column. No public parameter, third-party dependency or maintained source file is added. Blazor Release passed with zero warnings/errors; Framework **460/460**, Culture **27**, Gallery **8,557** (474 post-event and 367 ChangeLog included) and complete Node runner **169/169** passed; preview identity/publication-fence checks also passed **12/12**. The [popup alignment/width report](1_bugreports/2026-10-22_popup-left-alignment-and-native-picker-width.md) records the initial 168/169 fixture-only failure, strict fixture repair and targeted/full repeat. Six exact .11 packages and 17 delivered entries passed package/producer/cache verification; coordinated Colligere `db08924` plus package/test updates restores 17 graphs/168 identities and builds Release with zero warnings/errors. Consumer Web passed **2,328**, with **135 conditional PostgreSQL skips** and **zero failures** (2,463 total). Evidence captured **2026-10-22T20:55:28Z** belongs to [1_dependency.md](1_dependency.md). Physical native-picker first-frame acceptance and running Aspire version are not inferred from build/DOM tests. Maintained inventory remains 697 files/121 directories; audit state is unchanged.

**Verified earlier hidden-state preview, 2026-10-22:** Source **`07b64e0` plus the scoped visibility fix** declares **1.4.3-preview.portals.10**. The only production change is Framework's ordinary-hidden CSS contract across owned roots/descendants and standalone Notice/Loading; the new general guard excludes native `hidden="until-found"` case-insensitively. No public parameter, third-party dependency or maintained source file is added. Final Blazor solution Release build passed with zero warnings/errors; Framework 460/460 (83 components, 762 rows, 353 documented defaults), Gallery 8,549 (474 post-event and 363 ChangeLog), hidden/surface Node 35/35 (including 19 native cases), controls 77/77 and preview identity 12/12 passed. The new native group reproduced the old CSS defect at 16 passed/3 failed. An initial Framework 458/459 result exposed an existing narrow-CSS test parser splitting commas inside `:is`; its strict correction and new fail-closed regression passed without changing production CSS for that test issue. Six exact .10 packages passed identity/dependency/asset verification; 17 ZIP entries match final producer and consumer-cache bytes. Colligere base `b4e6e36` restored/built .10 with zero warnings/errors; final Web verification checked at `2026-10-22T20:16:13Z` passed 2,317 with 135 conditional PostgreSQL skips and zero failures (2,452 total). At that historical .10 verification, the running Aspire consumer served .9 and was not restarted; this is not a current runtime probe. See the [registration hidden-state report](1_bugreports/2026-10-22_registration-invitation-hidden-state.md) for source, verification and runtime/manual boundaries. Maintained inventory remains 697 files/121 directories; audit state is unchanged.

**Verified earlier selection/identity preview, 2026-10-22:** That source baseline `2a00a23` plus pending local changes declared **1.4.3-preview.portals.9**. Native option auto sizing removes the intrinsic percentage-width cycle, native checkmarks and multi-select checkboxes keep independent fixed tracks, and labels retain physical popup alignment. `LogoDisplayer.ContextPrimary` defaults false; with nonblank context it puts the smaller brand above the large context and, in Compact layout, aligns the image only to that context row. Native ownership, popup lifecycle and ordinary identity remain available. Final Blazor solution Release build passed with zero warnings/errors; Framework 459/459 (83 components, 762 parameter rows, 353 defaults), Gallery 8,541 (474 post-event and 359 ChangeLog), access Node 13/13, native/placement/palette 61/61, controls 77/77, split 5/5 and preview identity 12/12 passed. Six exact local .9 packages passed identity, dependency and required-asset verification. Seventeen delivered ZIP entries also match their final producer and consumer-cache bytes. See the [selection-column and service-identity report](1_bugreports/2026-10-22_selection-columns-and-service-identity.md) for source evidence, logs and manual acceptance; physical first-frame native picker appearance remains unverified. Coordinated Colligere final verification checked at `2026-10-22T13:02:03Z` also passed its 17-project Release build with zero warnings/errors and full Web result of 2,311 passed, 135 conditional PostgreSQL skips and zero failures; its report retains the earlier guard-only failure and correction. That maintained tree contained 697 files and 121 directories; audit state is unchanged.

**Verified earlier presentation preview, 2026-10-21:** That source baseline `2a00a23` plus pending local portal/popup/presentation changes declared **1.4.3-preview.portals.8**. PresentationBand and PresentationHero expose optional `BackgroundImageUrl`; Framework renders decorative, noninteractive cover media beneath content and Design supplies the existing Tone-role gradient with stronger narrow-screen coverage. Omitting media preserves ordinary band rendering. Compact LogoDisplayer keeps horizontal image/layout behavior while sharing the ordinary primary/context title scale; the old Compact 32px/24px primary and 18px context overrides are retired. Final source/C# Release build passed with zero warnings/errors; Framework 458/458 (83 components, 761 parameter rows, 352 defaults), Gallery 8,541 (474 post-event and 359 ChangeLog), focused Node 36/36 (10 access + 26 palette, including page-body checks) and preview identity 12/12 passed. Six exact local .8 packages passed identity, dependency and asset verification. The [product-media and access-title report](1_bugreports/2026-10-21_product-hero-media-and-access-titles.md) retains the first guard-only failure, corrected empty-alt/URL-encoding assertions, logs and manual acceptance. This round does not add third-party media or another external dependency. That maintained tree contained 697 files and 121 directories; audit state is unchanged.

**Verified earlier anchored-popup preview, 2026-10-21:** That source baseline `2a00a23` plus its pending portal/popup changes declared 1.4.3-preview.portals.7. Anchored menus, selection panels and search suggestions share content/longest-item width and physical left/right alignment; native select direction is synchronized before opening. ReferenceDropdown/SearchAutocomplete keep component-owned selection, keyboard and dismissal state while their panels reuse placement through manual native popovers. Modal dialog geometry and WPF behavior are separate. No external dependency or public parameter was added. Final complete Blazor solution Release build passed with zero warnings/errors; Framework 456/456, Gallery 8,529 (including 474 post-event and 355 ChangeLog), preview identity 12/12, focused Node 70/70, controls 77/77 and ActionMenu DOM-only 28/28 passed. The DOM-only run explicitly skipped 14 real-browser checks; physical first-open painting remains manual acceptance. Six exact local .7 packages passed identity, dependency and asset verification. See [anchored-popup report](1_bugreports/2026-10-21_anchored-popup-placement.md) for logs, the retained first Framework guard failure and manual checks. That maintained tree contained 697 files and 121 directories; audit state remains unchanged.

**Verified earlier control-state preview, 2026-10-21:** That source was 1.4.3-preview.portals.6. Native customized-select triggers reuse standard Button paint through the real select owner state, including chrome and unavailable states; native picker/menu surfaces and option roles are canonical while raw-select styles remain scoped separately. Sidebar navigation and details now share Surface. Optional LogoDisplayer.LogoSize sets positive image pixels independently of Compact, including an explicit 96px image with simple horizontal identity. Gallery/API metadata and focused regressions cover these public contracts. No external dependency changed. Final production/full solution Release builds passed with zero warnings/errors; Framework 456/456, Gallery 8,521, preview-version 12/12 and focused Node 41/41 passed. All six exact local packages passed identity, dependency and asset verification. See [final library verification](1_changelogs/2026-10-21_103800_native-select-preview-six-final-verification.md) for logs and scope; consumer results are maintained in its own report. See [implementation and manual checks](1_changelogs/2026-10-21_103624_native-select-state-and-identity-preview-six.md). That maintained tree contained 696 files and 121 directories; audit state remains unchanged.

**Verified earlier bounded-operation preview, 2026-10-20:** That source is 1.4.3-preview.portals.5. The .4 candidate failed the consumer minimum-text-scale guard; .5 removes its 16px short-height context override and retains the standard 18px context. AccessLayout fills a bounded ContentSurface stage with a stable Brand row and a remaining-height operation body. NavigationChoices.FillHeight is opt-in: the selected details keep natural height up to that body limit, then scroll without recentering the brand or sidebar. LogoDisplayer.Compact reuses identity semantics with a horizontal 48px logo and 32px/18px name/context; ordinary artistic identity is unchanged. Direct panel Sections suppress page-sized top padding even after a Notice. Narrow floating preferences have reserved clearance; at CSS viewport heights up to 360px the operation body scrolls as a reachability fallback. Five maintained files were added, for 695 files and 121 directories. Final coordinated verification passed: Release build with zero warnings/errors, six exact local packages, Framework 455/455, Gallery 8,513 and preview-version checks 12/12. The focused Node group passed 33 checks (7 layout plus 26 palette); Colligere restored/built all 17 projects and its full Web project passed 2,307 tests with 135 conditional PostgreSQL skips and zero failures. See [final .5 verification](1_changelogs/2026-10-20_213621_bounded-access-preview-five-final-verification.md) for evidence and explicit unperformed scopes. Audit state remains unchanged.

**Verified earlier local preview, 2026-10-19:** That source declared VersionPrefix 1.4.3 and VersionSuffix preview.portals.3; public 1.4.2 remains unchanged. NavigationChoices replaces its Compact boolean with NavigationChoicesLayout (Grid, Compact, Sidebar), supports optional item icons and a current-choice check, and retains native GET links and keyed hidden panels. Sidebar geometry belongs to Framework and semantic surfaces to Design. Selected links use their standard Button ActiveVariant without Sidebar paint overrides; the Gallery explicitly uses Primary. Gallery replaces its organization Hero/three-method scene with two native account/organization choices. One maintained Abstract enum and one local version-gate regression were added; that source tree had 690 files and 121 directories. The coordinating task ran and checked the final .3 Release build (zero warnings/errors), Framework 452/452, Gallery 8,422, palette 26/26 and the focused presentation/layout/spacing checks. All six local packages passed package-set verification, and Colligere restored and built them successfully. See [final local verification](1_changelogs/2026-10-19_162544_portal-preview-final-verification.md) for exact scopes, logs and consumer results; visual acceptance remains manual. Audit state remains unchanged.

**Release completed, 2026-10-19:** The completed public release is 1.4.2 for the unchanged six-package Core/Blazor manifest; Essential stays 1.4.0. Tag v1.4.2 identifies commit c8616f0f8712a5ff78b21612f6886004366fe414. Hosted verification/publication, all six public indexes and fresh-cache public-only consumers passed. The failed v1.4.1 tag remains as unpublished history. This recovery only changes the bounded browser-fixture wait and Gallery release records; no maintained source file or runtime responsibility was added. See [release verification](release-verification.md#completed-flourish-142-release-on-2026-10-19). The complete tree and audit timestamp remain unchanged; new records are in its excluded docs-ai subtree.

**Status:** Verified source and structure inventory, 2026-10-10 (America/Sao_Paulo, UTC-03:00). Inspected working tree based on commit `3e861b346a145240b8cdabbfe9b39cdd785fc819`, including the pending Essential.Culture 1.4.0 bridge changes and the installed AGENTS framework 1.0.0. This source audit does not certify application startup or visual behavior.

**Release update, 2026-10-10:** Flourish `VersionPrefix=1.4.0` is published from tag `v1.4.0`; Essential.Culture remains 1.4.0. The public manifest contains six Core/Blazor packages, excluding native WPF and its Culture bridge. Local/CI verification, public indexing and fresh public-only consumers passed; see [release verification](release-verification.md#completed-flourish-140-release-on-2026-10-10). The original audit baseline remains dated evidence; the separately dated inventory maintenance below does not renew the audit.

**Inventory maintenance, 2026-10-12:** The current working tree has been re-enumerated for the popup-strength and presentation-navigation changes. This maintenance updates the complete file tree and the source-owned API boundaries below; it leaves `AGENTS.ensure.json` and the 2026-10-10 full audit completion unchanged. Source inspection and inventory checks do not establish that new tests or browser/native interactions have passed.

## Inventory scope

The complete tree below includes every maintained file observed on 2026-10-23 in the repository outside the two documentation subtrees. The inventory contains **701 files and 121 directories below the root**, including **22 project files and 5 solution files**. It includes hidden maintained files and project-owned untracked files. Files were enumerated with `rg --files --hidden --no-ignore`; explicit generated/temporary exclusions were applied after identifying their roles. Git ignore rules were not used to discard maintained source, assets, configuration, or deployment definitions.

| Concrete excluded path or pattern | Reason |
| --- | --- |
| `docs/**` | Human-maintained documentation, outside this tree's assigned scope |
| `docs-ai/**` | AI documentation, outside this tree's assigned scope |
| Any `.git/**` | Version-control internals |
| Any `.vs/**`, `.idea/**` | IDE caches and personal workspace state |
| `artifacts/**`, `.packages/**` | Generated verification/package/publish artifacts and restored local packages |
| Any `bin/**`, `obj/**` | Compiler output, NuGet resolution files, generated source and intermediates |
| Any `node_modules/**`, `__pycache__/**`, `TestResults/**` | Dependency caches, Python bytecode and test output |
| `*.user`, `*.tmp`, `*.log`, `*.binlog`, `*.nupkg`, `*.snupkg` | Personal project settings, temporary files, logs and generated package archives; no maintained source matching these extensions was observed |
| Directories with no maintained descendants | Empty/residual physical folders are not maintained tree nodes; the concrete folders are listed below |

The physical scan also found eleven folders without maintained files after the output exclusions: `src/Flourish.Blazor/Flourish.Blazor.Design/Components`, `src/Flourish.Blazor/Flourish.Blazor.Shared`, `src/Flourish.WPF/BackgroundTasks`, `src/Flourish.WPF/Localization`, `src/Flourish.WPF/Shell`, `src/Flourish.WPF/Shell/StatusBar`, `src/Gallery.Flourish.Blazor/Components/Layout`, `src/Gallery.Flourish.WPF/Localization`, `src/Gallery.Flourish.WPF/Localization/Catalogs`, `tests/Flourish.Command.Test`, and `tests/LangKey.Test`. They contain no maintained files or directory markers; their physical presence does not retain a retired API or create an implemented module. They remain untouched.

No maintained source subtree was collapsed. In particular, `.agents/`, `.config/`, `.github/`, checked-in fonts, compressed icon data, Culture modules, and test scripts are included. The removed `script/preview-docs-*.ps1` files were absent and are not restored or represented as maintained files. There is no maintained root `Directory.Packages.props` or `.editorconfig` in this inspected tree.

## Complete repository tree

```text
Flourish/
|-- .agents/
|   \-- skills/
|       |-- audit-project-docs/
|       |   |-- agents/
|       |   |   \-- openai.yaml
|       |   \-- SKILL.md
|       \-- sync-agents-framework/
|           |-- agents/
|           |   \-- openai.yaml
|           \-- SKILL.md
|-- .config/
|   \-- dotnet-tools.json
|-- .github/
|   \-- workflows/
|       |-- build.yml
|       \-- docs.yml
|-- build/
|   |-- BundleCss.cs
|   |-- CssBundle.targets
|   |-- Test-BlazorPackageConsumers.ps1
|   |-- Test-ChangeLog.ps1
|   |-- Test-CssAssets.ps1
|   |-- Test-CssBundle.ps1
|   |-- Test-CultureCatalogs.ps1
|   |-- Test-CultureIntegration.cjs
|   |-- Test-CulturePreferences.ps1
|   |-- Test-GalleryCulture.ps1
|   |-- Test-GalleryNavigation.ps1
|   |-- Test-PreviewVersion.ps1
|   \-- Test-WpfPackageConsumers.ps1
|-- scripts/
|   |-- Convert-MaterialIcons.ps1
|   |-- fetch-agents.ps1
|   |-- Publish-Helper.ps1
|   |-- Release-Common.ps1
|   |-- ReleaseSettings.psd1
|   |-- Start-Common.ps1
|   |-- Start-Project.ps1
|   |-- StartSettings.json
|   |-- Test-Release.ps1
|   |-- Test-StartProject.ps1
|   \-- Verify-PackageSet.ps1
|-- src/
|   |-- Flourish.Blazor/
|   |   |-- Flourish.Blazor/
|   |   |   \-- Flourish.Blazor.csproj
|   |   |-- Flourish.Blazor.Abstract/
|   |   |   |-- Components/
|   |   |   |   |-- CenteredContainer.cs
|   |   |   |   |-- ControlContracts.cs
|   |   |   |   |-- NavigationChoiceItem.cs
|   |   |   |   |-- NavigationChoicesLayout.cs
|   |   |   |   |-- PopupStrength.cs
|   |   |   |   |-- PresentationTone.cs
|   |   |   |   |-- TableContracts.cs
|   |   |   |   |-- TutorialStep.cs
|   |   |   |   |-- UniformGridShape.cs
|   |   |   |   \-- UniformGridVariant.cs
|   |   |   |-- Primitives/
|   |   |   |   |-- GridContracts.cs
|   |   |   |   \-- SelectionContracts.cs
|   |   |   |-- ApplicationContracts.cs
|   |   |   |-- ApplicationData.cs
|   |   |   |-- Flourish.Blazor.Abstract.csproj
|   |   |   |-- ITablePreferences.cs
|   |   |   \-- TextContracts.cs
|   |   |-- Flourish.Blazor.Design/
|   |   |   |-- Hosting/
|   |   |   |   |-- AppearanceService.cs
|   |   |   |   \-- DesignOptions.cs
|   |   |   |-- wwwroot/
|   |   |   |   |-- patterns/
|   |   |   |   |   |-- content-surface.css
|   |   |   |   |   \-- navigation-surface.css
|   |   |   |   |-- primitives/
|   |   |   |   |   |-- AccessBrand.css
|   |   |   |   |   |-- DataPager.css
|   |   |   |   |   |-- EditingGrid.css
|   |   |   |   |   |-- FieldControl.css
|   |   |   |   |   |-- InteractionBoundary.css
|   |   |   |   |   |-- MaskedInput.css
|   |   |   |   |   |-- NoticeTrigger.css
|   |   |   |   |   |-- PrimaryNavigationItem.css
|   |   |   |   |   |-- primitives.css
|   |   |   |   |   |-- ReferenceDropdown.css
|   |   |   |   |   |-- SearchAutocomplete.css
|   |   |   |   |   |-- ServiceMenu.css
|   |   |   |   |   |-- ShellHeader.css
|   |   |   |   |   |-- StandaloneMaskedInput.css
|   |   |   |   |   \-- SystemNavigationMenu.css
|   |   |   |   |-- access-layout.css
|   |   |   |   |-- controls.css
|   |   |   |   |-- data-search.css
|   |   |   |   |-- data.css
|   |   |   |   |-- design.css
|   |   |   |   |-- display-board.css
|   |   |   |   |-- dropdown.css
|   |   |   |   |-- expansion-indicator.css
|   |   |   |   |-- form-inputs.css
|   |   |   |   |-- foundation.css
|   |   |   |   |-- layout.css
|   |   |   |   |-- line-chart.css
|   |   |   |   |-- multi-select-box.css
|   |   |   |   |-- native-overrides.css
|   |   |   |   |-- navigation-choices.css
|   |   |   |   |-- page-heading-loading.css
|   |   |   |   |-- presentation-progress.css
|   |   |   |   |-- presentation.css
|   |   |   |   |-- scrollbars.css
|   |   |   |   |-- section-navigator.css
|   |   |   |   |-- split-button.css
|   |   |   |   |-- static-surfaces.css
|   |   |   |   |-- tutorial-board.css
|   |   |   |   \-- uniform-grid.css
|   |   |   |-- _Imports.razor
|   |   |   |-- AppearancePalette.cs
|   |   |   |-- AssemblyInfo.cs
|   |   |   |-- DesignServiceCollectionExtensions.cs
|   |   |   \-- Flourish.Blazor.Design.csproj
|   |   |-- Flourish.Blazor.Framework/
|   |   |   |-- Components/
|   |   |   |   |-- Internal/
|   |   |   |   |   |-- SelectValueConversion.cs
|   |   |   |   |   |-- TableData.cs
|   |   |   |   |   \-- UniformGridContent.cs
|   |   |   |   |-- Patterns/
|   |   |   |   |   |-- ContentSurface.razor
|   |   |   |   |   \-- NavigationSurface.razor
|   |   |   |   |-- Primitives/
|   |   |   |   |   |-- _Imports.razor
|   |   |   |   |   |-- AccessBrand.razor
|   |   |   |   |   |-- DataPager.razor
|   |   |   |   |   |-- EditingGrid.razor
|   |   |   |   |   |-- GridInteractions.cs
|   |   |   |   |   |-- InputBehaviorBinding.cs
|   |   |   |   |   |-- InteractionBoundary.razor
|   |   |   |   |   |-- MaskedInput.razor
|   |   |   |   |   |-- NavigationGuard.razor
|   |   |   |   |   |-- NoticeTrigger.razor
|   |   |   |   |   |-- PrimaryNavigationItem.razor
|   |   |   |   |   |-- ReferenceDropdown.razor
|   |   |   |   |   |-- SearchAutocomplete.razor
|   |   |   |   |   |-- SecondaryNavigationItem.razor
|   |   |   |   |   |-- ServiceMenu.razor
|   |   |   |   |   |-- ShellHeader.razor
|   |   |   |   |   \-- StandaloneMaskedInput.razor
|   |   |   |   |-- AccessActions.razor
|   |   |   |   |-- AccessFormSurface.razor
|   |   |   |   |-- AccessLayout.razor
|   |   |   |   |-- AccessPanel.razor
|   |   |   |   |-- ActionMenu.razor
|   |   |   |   |-- ApplicationLayout.razor
|   |   |   |   |-- ApplicationShell.razor
|   |   |   |   |-- AttributionFooter.razor
|   |   |   |   |-- BackToTop.razor
|   |   |   |   |-- Button.razor
|   |   |   |   |-- Card.razor
|   |   |   |   |-- CheckBox.razor
|   |   |   |   |-- CodeBlock.razor
|   |   |   |   |-- ContentContainer.razor
|   |   |   |   |-- CopyText.razor
|   |   |   |   |-- DataSearch.razor
|   |   |   |   |-- DataTable.razor
|   |   |   |   |-- DateBox.razor
|   |   |   |   |-- Dialog.razor
|   |   |   |   |-- DialogView.cs
|   |   |   |   |-- Disclosure.razor
|   |   |   |   |-- DisplayBoard.razor
|   |   |   |   |-- DropdownSurface.razor
|   |   |   |   |-- EmptyState.razor
|   |   |   |   |-- ExpansionIndicator.razor
|   |   |   |   |-- Field.razor
|   |   |   |   |-- FilePicker.razor
|   |   |   |   |-- FormActions.razor
|   |   |   |   |-- FormGroup.razor
|   |   |   |   |-- FormLayout.razor
|   |   |   |   |-- Icon.razor
|   |   |   |   |-- ImagePreview.razor
|   |   |   |   |-- InlineActions.razor
|   |   |   |   |-- InputSemantics.cs
|   |   |   |   |-- LineChart.razor
|   |   |   |   |-- ListView.razor
|   |   |   |   |-- LoadingState.razor
|   |   |   |   |-- LogoDisplayer.razor
|   |   |   |   |-- MultiSelectBox.razor
|   |   |   |   |-- MultiSelectOption.cs
|   |   |   |   |-- NavigationChoices.razor
|   |   |   |   |-- Notice.razor
|   |   |   |   |-- NumberBox.razor
|   |   |   |   |-- OfferCard.razor
|   |   |   |   |-- OfferStage.razor
|   |   |   |   |-- PageBody.razor
|   |   |   |   |-- PageHeading.razor
|   |   |   |   |-- PresentationBand.razor
|   |   |   |   |-- PresentationFooter.razor
|   |   |   |   |-- PresentationHero.razor
|   |   |   |   |-- PresentationProgress.razor
|   |   |   |   |-- ProgressBar.razor
|   |   |   |   |-- ProgressRing.razor
|   |   |   |   |-- SearchBox.razor
|   |   |   |   |-- Section.razor
|   |   |   |   |-- SectionNavigator.razor
|   |   |   |   |-- SelectBox.razor
|   |   |   |   |-- SplitButton.razor
|   |   |   |   |-- StandaloneCheckBox.razor
|   |   |   |   |-- StandaloneSelectBox.razor
|   |   |   |   |-- StandaloneTextBox.razor
|   |   |   |   |-- TablePreferences.cs
|   |   |   |   |-- TextBox.razor
|   |   |   |   |-- TextComponentBase.cs
|   |   |   |   |-- TextInputBase.cs
|   |   |   |   |-- ToggleSection.razor
|   |   |   |   |-- ToggleSwitch.razor
|   |   |   |   |-- TutorialBoard.razor
|   |   |   |   |-- UniformGrid.razor
|   |   |   |   |-- UniformGridButton.razor
|   |   |   |   |-- UniformGridItem.razor
|   |   |   |   \-- ValidationMessages.razor
|   |   |   |-- Hosting/
|   |   |   |   |-- ApplicationOptions.cs
|   |   |   |   |-- CommandRuntime.cs
|   |   |   |   \-- LiteralTextProvider.cs
|   |   |   |-- Icons/
|   |   |   |   |-- IconCatalog.cs
|   |   |   |   \-- MaterialSymbolsOutlined.codepoints
|   |   |   |-- Localization/
|   |   |   |   \-- Culture.json
|   |   |   |-- Primitives/
|   |   |   |   |-- DataPagination.cs
|   |   |   |   |-- InputMaskFormatter.cs
|   |   |   |   \-- NoticePresentation.cs
|   |   |   |-- wwwroot/
|   |   |   |   |-- icons/
|   |   |   |   |   \-- material/
|   |   |   |   |       |-- LICENSE.txt
|   |   |   |   |       |-- MaterialSymbolsOutlined.woff2
|   |   |   |   |       \-- source.json
|   |   |   |   |-- patterns/
|   |   |   |   |   |-- content-surface.css
|   |   |   |   |   |-- navigation-surface.css
|   |   |   |   |   \-- surfaces.js
|   |   |   |   |-- presentation/
|   |   |   |   |   |-- layout.css
|   |   |   |   |   |-- offers.js
|   |   |   |   |   |-- progress.css
|   |   |   |   |   \-- progress.js
|   |   |   |   |-- primitives/
|   |   |   |   |   |-- behavior.css
|   |   |   |   |   |-- editing-grid-columns.js
|   |   |   |   |   |-- editing-grid.js
|   |   |   |   |   |-- EditingGrid.css
|   |   |   |   |   |-- input-behaviors.js
|   |   |   |   |   |-- interaction-origin.js
|   |   |   |   |   |-- native.css
|   |   |   |   |   |-- navigation-guard.js
|   |   |   |   |   |-- primary-navigation-item.js
|   |   |   |   |   \-- reference-dropdown.js
|   |   |   |   |-- access-layout.css
|   |   |   |   |-- back-to-top.css
|   |   |   |   |-- browse.svg
|   |   |   |   |-- clipboard.js
|   |   |   |   |-- controls.js
|   |   |   |   |-- data-search.css
|   |   |   |   |-- data-table.css
|   |   |   |   |-- data.js
|   |   |   |   |-- display-board.css
|   |   |   |   |-- expansion-indicator.css
|   |   |   |   |-- form-inputs.css
|   |   |   |   |-- framework.css
|   |   |   |   |-- layout.css
|   |   |   |   |-- line-chart.css
|   |   |   |   |-- multi-select-box.css
|   |   |   |   |-- multi-select-box.js
|   |   |   |   |-- navigation-choices.css
|   |   |   |   |-- page-heading-loading.css
|   |   |   |   |-- section-navigator.css
|   |   |   |   |-- section-navigator.js
|   |   |   |   |-- shell.js
|   |   |   |   |-- split-button.css
|   |   |   |   |-- tutorial-board.css
|   |   |   |   \-- uniform-grid.css
|   |   |   |-- _Imports.razor
|   |   |   |-- AssemblyInfo.cs
|   |   |   |-- ComponentUsageCatalog.cs
|   |   |   |-- Flourish.Blazor.Framework.csproj
|   |   |   \-- ServiceCollectionExtensions.cs
|   |   \-- Flourish.Blazor.slnx
|   |-- Flourish.Core/
|   |   |-- Abstract/
|   |   |   |-- ApplicationTheme.cs
|   |   |   |-- BackgroundTaskContracts.cs
|   |   |   |-- CommandContracts.cs
|   |   |   |-- ContentLayoutContracts.cs
|   |   |   |-- IDataBuilder.cs
|   |   |   |-- ISettingsEditor.cs
|   |   |   |-- ISettingsStore.cs
|   |   |   |-- IThemeService.cs
|   |   |   |-- LocalizationContracts.cs
|   |   |   |-- MotionContracts.cs
|   |   |   |-- NavigationMenuContracts.cs
|   |   |   |-- NavigationPanelDirection.cs
|   |   |   |-- NavigationPanelState.cs
|   |   |   |-- NotificationContracts.cs
|   |   |   |-- PageCacheMode.cs
|   |   |   |-- ProfileContracts.cs
|   |   |   |-- ProjectContracts.cs
|   |   |   |-- SettingsUpdateResult.cs
|   |   |   |-- ShellRegion.cs
|   |   |   |-- ShortcutConflictPolicy.cs
|   |   |   |-- ShortcutScope.cs
|   |   |   |-- StateContracts.cs
|   |   |   |-- StatusBarContracts.cs
|   |   |   |-- TitleBarContracts.cs
|   |   |   |-- ToolbarStateContracts.cs
|   |   |   |-- ToolTipContracts.cs
|   |   |   \-- WindowCloseContracts.cs
|   |   |-- Assets/
|   |   |   \-- FlourishCulture.Json
|   |   |-- BackgroundTasks/
|   |   |   \-- BackgroundTaskService.cs
|   |   |-- Commands/
|   |   |   |-- CommandDispatcher.cs
|   |   |   \-- CommandParserHostedService.cs
|   |   |-- Configuration/
|   |   |   |-- ApplicationConfigurationPath.cs
|   |   |   |-- ApplicationDataOptions.cs
|   |   |   |-- AppPreferenceService.cs
|   |   |   |-- AppSettingsConfigurationSource.cs
|   |   |   |-- BuilderMutationGuard.cs
|   |   |   |-- DataBuilder.cs
|   |   |   |-- PreferenceConfigurationKeys.cs
|   |   |   \-- ValueValidation.cs
|   |   |-- Hosting/
|   |   |   |-- CoreServiceCollectionExtensions.cs
|   |   |   \-- CoreServiceOptions.cs
|   |   |-- Layout/
|   |   |   |-- ContentLayoutService.cs
|   |   |   |-- LayoutBuilder.cs
|   |   |   \-- LayoutOptions.cs
|   |   |-- Localization/
|   |   |   |-- LocaleKeys.cs
|   |   |   \-- LocalizationService.cs
|   |   |-- Messaging/
|   |   |   \-- NotificationService.cs
|   |   |-- Motion/
|   |   |   |-- MotionBuilder.cs
|   |   |   \-- MotionOptions.cs
|   |   |-- Navigation/
|   |   |   |-- NavigationMenuStateService.cs
|   |   |   |-- NavigationStackEntry.cs
|   |   |   \-- PageHistoryService.cs
|   |   |-- Profile/
|   |   |   |-- IProfileCredentialStore.cs
|   |   |   |-- ProfileOptions.cs
|   |   |   |-- ProfileService.cs
|   |   |   |-- SimpleProfileAuthService.cs
|   |   |   \-- StoredProfileCredentials.cs
|   |   |-- Projects/
|   |   |   |-- ProjectBuilder.cs
|   |   |   |-- ProjectCatalogStore.cs
|   |   |   |-- ProjectOptions.cs
|   |   |   \-- ProjectService.cs
|   |   |-- Shell/
|   |   |   |-- StatusBar/
|   |   |   |   |-- StatusBarBuilder.cs
|   |   |   |   |-- StatusBarOptions.cs
|   |   |   |   \-- StatusBarService.cs
|   |   |   |-- TitleBar/
|   |   |   |   |-- TitleBarOptions.cs
|   |   |   |   |-- TitleBarRuntimeFacade.cs
|   |   |   |   |-- TitleBarSearchService.cs
|   |   |   |   |-- TitleBarSearchState.cs
|   |   |   |   \-- TitleBarService.cs
|   |   |   \-- Toolbar/
|   |   |       |-- ToolbarStateOptions.cs
|   |   |       \-- ToolbarStateService.cs
|   |   |-- ToolTips/
|   |   |   |-- ToolTipBuilder.cs
|   |   |   \-- ToolTipOptions.cs
|   |   |-- Windowing/
|   |   |   \-- WindowCloseService.cs
|   |   |-- AssemblyInfo.cs
|   |   \-- Flourish.Core.csproj
|   |-- Flourish.Extensions/
|   |   |-- Flourish.Extensions.Culture.Blazor/
|   |   |   |-- Localization/
|   |   |   |   \-- Culture.json
|   |   |   |-- wwwroot/
|   |   |   |   \-- browser-preferences.js
|   |   |   |-- _Imports.razor
|   |   |   |-- ComponentUsageCatalog.cs
|   |   |   |-- CultureBuilder.cs
|   |   |   |-- CultureFrameworkExtensions.cs
|   |   |   |-- CultureSession.cs
|   |   |   |-- Flourish.Extensions.Culture.Blazor.csproj
|   |   |   |-- LanguagePicker.razor
|   |   |   \-- LocalizedComponentBase.cs
|   |   |-- Flourish.Extensions.Culture.WPF/
|   |   |   |-- EssentialCultureBuilderExtensions.cs
|   |   |   |-- EssentialTextProvider.cs
|   |   |   \-- Flourish.Extensions.Culture.WPF.csproj
|   |   |-- Directory.Build.props
|   |   \-- Flourish.Extensions.slnx
|   |-- Flourish.WinUI3/
|   |   |-- Flourish.WinUI3.csproj
|   |   \-- Flourish.WinUI3.slnx
|   |-- Flourish.WPF/
|   |   |-- Flourish.WPF/
|   |   |   \-- Flourish.WPF.csproj
|   |   |-- Flourish.WPF.Abstract/
|   |   |   |-- ApplicationContracts.cs
|   |   |   |-- ControlContracts.cs
|   |   |   |-- Flourish.WPF.Abstract.csproj
|   |   |   |-- TableContracts.cs
|   |   |   \-- TextContracts.cs
|   |   |-- Flourish.WPF.Design/
|   |   |   |-- AppearancePalette.cs
|   |   |   |-- DesignResources.cs
|   |   |   |-- Flourish.WPF.Design.csproj
|   |   |   \-- ThemeSession.cs
|   |   |-- Flourish.WPF.Framework/
|   |   |   |-- Controls/
|   |   |   |   |-- Data/
|   |   |   |   |   |-- DataPager.cs
|   |   |   |   |   |-- DataPresentation.cs
|   |   |   |   |   |-- DataSearch.cs
|   |   |   |   |   |-- DataTable.cs
|   |   |   |   |   |-- DataText.cs
|   |   |   |   |   |-- EditingGrid.cs
|   |   |   |   |   |-- LineChart.cs
|   |   |   |   |   |-- ListView.cs
|   |   |   |   |   \-- TableData.cs
|   |   |   |   |-- ApplicationShell.cs
|   |   |   |   |-- Button.cs
|   |   |   |   |-- Content.cs
|   |   |   |   |-- Dialog.cs
|   |   |   |   |-- Forms.cs
|   |   |   |   |-- Icon.cs
|   |   |   |   |-- InputMaskFormatter.cs
|   |   |   |   |-- Inputs.cs
|   |   |   |   |-- Menus.cs
|   |   |   |   |-- OfferStage.cs
|   |   |   |   |-- PopupTrigger.cs
|   |   |   |   |-- Progress.cs
|   |   |   |   \-- UniformGrid.cs
|   |   |   |-- Icons/
|   |   |   |   |-- IconCatalog.cs
|   |   |   |   |-- LICENSE.txt
|   |   |   |   |-- MaterialSymbolsFilled.json.br
|   |   |   |   |-- MaterialSymbolsOutlined.codepoints
|   |   |   |   |-- MaterialSymbolsOutlined.ttf
|   |   |   |   \-- source.json
|   |   |   |-- Themes/
|   |   |   |   |-- Actions.xaml
|   |   |   |   |-- Content.xaml
|   |   |   |   |-- Data.xaml
|   |   |   |   |-- Generic.xaml
|   |   |   |   |-- Inputs.xaml
|   |   |   |   |-- Resources.xaml
|   |   |   |   \-- Shell.xaml
|   |   |   |-- AssemblyInfo.cs
|   |   |   |-- ComponentUsageCatalog.cs
|   |   |   |-- Converters.cs
|   |   |   |-- Flourish.WPF.Framework.csproj
|   |   |   |-- FrameworkBuilder.cs
|   |   |   |-- FrameworkResources.cs
|   |   |   \-- Motion.cs
|   |   \-- Flourish.WPF.slnx
|   |-- Gallery.Flourish.Blazor/
|   |   |-- Commands/
|   |   |   \-- GalleryCommandParser.cs
|   |   |-- Components/
|   |   |   |-- Catalog/
|   |   |   |   |-- ComponentGuide.razor
|   |   |   |   \-- ComponentGuide.razor.css
|   |   |   |-- Pages/
|   |   |   |   |-- AccessExamples.razor
|   |   |   |   |-- AccessMethodExample.razor
|   |   |   |   |-- Appearance.razor
|   |   |   |   |-- AppearanceRedirect.razor
|   |   |   |   |-- ChangeLog.razor
|   |   |   |   |-- Controls.razor
|   |   |   |   |-- DisplayExamples.razor
|   |   |   |   |-- Error.razor
|   |   |   |   |-- Examples.razor
|   |   |   |   |-- Forms.razor
|   |   |   |   |-- Framework.razor
|   |   |   |   |-- Icons.razor
|   |   |   |   |-- Localization.razor
|   |   |   |   |-- NotFound.razor
|   |   |   |   |-- Overview.razor
|   |   |   |   |-- ReconnectExample.razor
|   |   |   |   |-- RecordDetails.razor
|   |   |   |   |-- Records.razor
|   |   |   |   |-- ShellExample.razor
|   |   |   |   |-- SurfacePatterns.razor
|   |   |   |   |-- TutorialExample.razor
|   |   |   |   \-- WizardExample.razor
|   |   |   |-- Samples/
|   |   |   |   |-- Content/
|   |   |   |   |   |-- CardSample.razor
|   |   |   |   |   |-- CodeBlockSample.razor
|   |   |   |   |   |-- CopyTextSample.razor
|   |   |   |   |   |-- DisclosureSample.razor
|   |   |   |   |   |-- DisplayBoardSample.razor
|   |   |   |   |   |-- IconSample.razor
|   |   |   |   |   |-- ImagePreviewSample.razor
|   |   |   |   |   |-- InlineActionsSample.razor
|   |   |   |   |   |-- SectionSample.razor
|   |   |   |   |   |-- UniformGridItemSample.razor
|   |   |   |   |   \-- UniformGridSample.razor
|   |   |   |   |-- Data/
|   |   |   |   |   |-- ActionMenuSample.razor
|   |   |   |   |   |-- ButtonSample.razor
|   |   |   |   |   |-- CanonicalDataSearchSample.razor
|   |   |   |   |   |-- DataPagerSample.razor
|   |   |   |   |   |-- DataTableSample.razor
|   |   |   |   |   |-- DialogSample.razor
|   |   |   |   |   |-- DropdownSurfaceSample.razor
|   |   |   |   |   |-- EditingGridSample.razor
|   |   |   |   |   |-- EmptyStateSample.razor
|   |   |   |   |   |-- ExpansionIndicatorSample.razor
|   |   |   |   |   |-- LineChartSample.razor
|   |   |   |   |   |-- ListViewSample.razor
|   |   |   |   |   |-- LoadingStateSample.razor
|   |   |   |   |   |-- NoticeSample.razor
|   |   |   |   |   |-- NoticeTriggerSample.razor
|   |   |   |   |   |-- ProgressBarSample.razor
|   |   |   |   |   |-- ProgressRingSample.razor
|   |   |   |   |   |-- SplitButtonSample.razor
|   |   |   |   |   |-- TutorialBoardSample.razor
|   |   |   |   |   \-- UniformGridButtonSample.razor
|   |   |   |   |-- Display/
|   |   |   |   |   |-- AccessActionsSample.razor
|   |   |   |   |   |-- AccessFormSurfaceSample.razor
|   |   |   |   |   |-- AccessLayoutSample.razor
|   |   |   |   |   |-- AccessPanelSample.razor
|   |   |   |   |   |-- ContentContainerSample.razor
|   |   |   |   |   |-- LogoDisplayerSample.razor
|   |   |   |   |   |-- NavigationChoicesSample.razor
|   |   |   |   |   |-- OfferCardSample.razor
|   |   |   |   |   |-- OfferStageSample.razor
|   |   |   |   |   |-- PresentationBandSample.razor
|   |   |   |   |   |-- PresentationFooterSample.razor
|   |   |   |   |   |-- PresentationHeroSample.razor
|   |   |   |   |   \-- PresentationProgressSample.razor
|   |   |   |   |-- Inputs/
|   |   |   |   |   |-- CheckBoxSample.razor
|   |   |   |   |   |-- DateBoxSample.razor
|   |   |   |   |   |-- FieldSample.razor
|   |   |   |   |   |-- FilePickerSample.razor
|   |   |   |   |   |-- FormActionsSample.razor
|   |   |   |   |   |-- FormGroupSample.razor
|   |   |   |   |   |-- FormLayoutSample.razor
|   |   |   |   |   |-- LanguagePickerSample.razor
|   |   |   |   |   |-- MaskedInputSample.razor
|   |   |   |   |   |-- MultiSelectBoxSample.razor
|   |   |   |   |   |-- NumberBoxSample.razor
|   |   |   |   |   |-- ReferenceDropdownSample.razor
|   |   |   |   |   |-- SearchAutocompleteSample.razor
|   |   |   |   |   |-- SearchBoxSample.razor
|   |   |   |   |   |-- SelectBoxSample.razor
|   |   |   |   |   |-- StandaloneCheckBoxSample.razor
|   |   |   |   |   |-- StandaloneMaskedInputSample.razor
|   |   |   |   |   |-- StandaloneSelectBoxSample.razor
|   |   |   |   |   |-- StandaloneTextBoxSample.razor
|   |   |   |   |   |-- TextBoxSample.razor
|   |   |   |   |   |-- ToggleSectionSample.razor
|   |   |   |   |   |-- ToggleSwitchSample.razor
|   |   |   |   |   \-- ValidationMessagesSample.razor
|   |   |   |   \-- Layout/
|   |   |   |       |-- AccessBrandSample.razor
|   |   |   |       |-- ApplicationLayoutSample.razor
|   |   |   |       |-- ApplicationShellSample.razor
|   |   |   |       |-- AttributionFooterSample.razor
|   |   |   |       |-- BackToTopSample.razor
|   |   |   |       |-- ContentSurfaceSample.razor
|   |   |   |       |-- InteractionBoundarySample.razor
|   |   |   |       |-- NavigationGuardSample.razor
|   |   |   |       |-- NavigationSurfaceSample.razor
|   |   |   |       |-- PageBodySample.razor
|   |   |   |       |-- PageHeadingSample.razor
|   |   |   |       |-- PrimaryNavigationItemSample.razor
|   |   |   |       |-- SecondaryNavigationItemSample.razor
|   |   |   |       |-- SectionNavigatorSample.razor
|   |   |   |       |-- ServiceMenuSample.razor
|   |   |   |       \-- ShellHeaderSample.razor
|   |   |   |-- _Imports.razor
|   |   |   |-- AccessExampleLayout.razor
|   |   |   |-- App.razor
|   |   |   \-- Routes.razor
|   |   |-- Localization/
|   |   |   |-- Culture.Access.json
|   |   |   |-- Culture.ChangeLog.json
|   |   |   |-- Culture.Components.json
|   |   |   |-- Culture.Pages.json
|   |   |   |-- Culture.Parameters.json
|   |   |   |-- Culture.Samples.json
|   |   |   \-- Culture.Shell.json
|   |   |-- Models/
|   |   |   |-- AccessExampleState.cs
|   |   |   |-- BusinessRecord.cs
|   |   |   |-- CatalogSections.cs
|   |   |   |-- ChangeLog.json
|   |   |   |-- ChangeLogCatalog.cs
|   |   |   |-- ComponentCatalog.cs
|   |   |   |-- PaletteDraft.cs
|   |   |   |-- ParameterMeaning.cs
|   |   |   |-- SampleCatalog.cs
|   |   |   \-- SampleFor.cs
|   |   |-- Properties/
|   |   |   \-- launchSettings.json
|   |   |-- Services/
|   |   |   \-- RecordStore.cs
|   |   |-- wwwroot/
|   |   |   |-- gallery-favicon.svg
|   |   |   |-- gallery.svg
|   |   |   \-- reconnect-example.js
|   |   |-- appsettings.Flourish.json
|   |   |-- appsettings.json
|   |   |-- Gallery.Flourish.Blazor.csproj
|   |   \-- Program.cs
|   |-- Gallery.Flourish.WINUI3/
|   |   |-- App.xaml
|   |   |-- App.xaml.cs
|   |   |-- Gallery.Flourish.WinUI3.csproj
|   |   |-- MainWindow.xaml
|   |   \-- MainWindow.xaml.cs
|   \-- Gallery.Flourish.WPF/
|       |-- App.xaml
|       |-- App.xaml.cs
|       |-- Artwork.cs
|       |-- Culture.json
|       |-- Gallery.Flourish.WPF.csproj
|       |-- GalleryCatalog.cs
|       |-- MainWindow.cs
|       |-- Samples.cs
|       |-- Samples.Scenarios.cs
|       \-- SimpleSamples.cs
|-- tests/
|   |-- Tests.Flourish.Blazor/
|   |   |-- access-form-spacing-dom.mjs
|   |   |-- access-layout.test.mjs
|   |   |-- AccessExampleChecks.cs
|   |   |-- action-menu-dom.mjs
|   |   |-- ApiFinalAuditChecks.cs
|   |   |-- BrandingChecks.cs
|   |   |-- ButtonStructuredLabelChecks.cs
|   |   |-- ButtonUnavailableChecks.cs
|   |   |-- CatalogChecks.cs
|   |   |-- CenteredContentChecks.cs
|   |   |-- ChartToolbarChecks.cs
|   |   |-- clipboard-dom.mjs
|   |   |-- ComponentInventoryChecks.cs
|   |   |-- controls-dom.mjs
|   |   |-- ControlTextChecks.cs
|   |   |-- DataSearchChecks.cs
|   |   |-- DataTableChecks.cs
|   |   |-- DialogFixture.cs
|   |   |-- DialogResultChecks.cs
|   |   |-- DialogViewChecks.cs
|   |   |-- DisplayBoardChecks.cs
|   |   |-- DropdownChecks.cs
|   |   |-- EmptyStateChecks.cs
|   |   |-- FieldActionsChecks.cs
|   |   |-- FrameworkConfigurationChecks.cs
|   |   |-- GridChecks.cs
|   |   |-- heading-dom.mjs
|   |   |-- inline-toolbar-layout.test.mjs
|   |   |-- InlineActionsChecks.cs
|   |   |-- InputMigrationChecks.cs
|   |   |-- interaction-origin-dom.mjs
|   |   |-- InteropFinalAuditChecks.cs
|   |   |-- LifecycleChecks.cs
|   |   |-- ListViewChecks.cs
|   |   |-- measurement-lifecycle.test.mjs
|   |   |-- MultiSelectBoxChecks.cs
|   |   |-- native-select-trigger.test.mjs
|   |   |-- NavigationChoicesChecks.cs
|   |   |-- NavigationControlsChecks.cs
|   |   |-- NavigationGuardChecks.cs
|   |   |-- NoCompatibilityApiChecks.cs
|   |   |-- organization-access-presentation.test.mjs
|   |   |-- page-body-actions.test.mjs
|   |   |-- page-heading-loading.test.mjs
|   |   |-- palette-checks.mjs
|   |   |-- popup-placement.test.mjs
|   |   |-- presentation-offers-dom.mjs
|   |   |-- presentation-progress-dom.mjs
|   |   |-- PresentationChecks.cs
|   |   |-- PresentationIdentityChecks.cs
|   |   |-- PresentationProgressChecks.cs
|   |   |-- Program.cs
|   |   |-- progress-rendering.test.mjs
|   |   |-- section-navigator-dom.mjs
|   |   |-- SectionNavigatorChecks.cs
|   |   |-- SelectValueConversionChecks.cs
|   |   |-- split-menu-checks.mjs
|   |   |-- SplitButtonChecks.cs
|   |   |-- surface-form-layout.test.mjs
|   |   |-- Tests.Flourish.Blazor.csproj
|   |   |-- tests.js
|   |   |-- TextChecks.cs
|   |   |-- tutorial-board-dom.mjs
|   |   |-- TutorialBoardChecks.cs
|   |   |-- UniformGridChecks.cs
|   |   \-- ValidationMessagesChecks.cs
|   |-- Tests.Flourish.Blazor.Native/
|   |   |-- Components/
|   |   |   |-- Pages/
|   |   |   |   \-- Controls.razor
|   |   |   |-- _Imports.razor
|   |   |   |-- App.razor
|   |   |   \-- Routes.razor
|   |   |-- Properties/
|   |   |   \-- launchSettings.json
|   |   |-- appsettings.json
|   |   |-- Program.cs
|   |   \-- Tests.Flourish.Blazor.Native.csproj
|   |-- Tests.Flourish.Core/
|   |   |-- Abstract/
|   |   |   \-- PlatformNeutralContractTests.cs
|   |   |-- Architecture/
|   |   |   \-- CoreBoundaryTests.cs
|   |   |-- BackgroundTasks/
|   |   |   \-- BackgroundTaskServiceTests.cs
|   |   |-- Commands/
|   |   |   |-- CommandDispatcherRegressionTests.cs
|   |   |   |-- CommandDispatcherTests.cs
|   |   |   \-- CommandParserHostedServiceTests.cs
|   |   |-- Configuration/
|   |   |   |-- ApplicationDataOptionsTests.cs
|   |   |   |-- AppPreferenceServiceTests.cs
|   |   |   \-- ValueValidationTests.cs
|   |   |-- Hosting/
|   |   |   \-- CoreServiceCollectionExtensionsTests.cs
|   |   |-- Infrastructure/
|   |   |   \-- TemporaryDirectory.cs
|   |   |-- Layout/
|   |   |   |-- ContentLayoutServiceTests.cs
|   |   |   \-- LayoutBuilderTests.cs
|   |   |-- Localization/
|   |   |   |-- LocalizationServiceTests.cs
|   |   |   \-- TemporaryDirectory.cs
|   |   |-- Messaging/
|   |   |   \-- NotificationServiceTests.cs
|   |   |-- Motion/
|   |   |   \-- MotionBuilderTests.cs
|   |   |-- Navigation/
|   |   |   |-- NavigationMenuModelTests.cs
|   |   |   |-- NavigationMenuStateServiceTests.cs
|   |   |   |-- PageHistoryServiceBackStackTests.cs
|   |   |   \-- PageHistoryServiceTests.cs
|   |   |-- Profile/
|   |   |   |-- ProfileModelTests.cs
|   |   |   |-- ProfileServiceTests.cs
|   |   |   \-- StoredProfileCredentialsTests.cs
|   |   |-- Projects/
|   |   |   |-- ProjectCatalogPersistenceTests.cs
|   |   |   |-- ProjectServiceTests.cs
|   |   |   |-- ProjectTransactionTests.cs
|   |   |   \-- TemporaryDirectory.cs
|   |   |-- Shell/
|   |   |   |-- StatusBar/
|   |   |   |   |-- StatusBarBuilderTests.cs
|   |   |   |   \-- StatusBarServiceTests.cs
|   |   |   |-- TitleBar/
|   |   |   |   \-- TitleBarStateServiceTests.cs
|   |   |   \-- Toolbar/
|   |   |       |-- ToolbarItemTests.cs
|   |   |       |-- ToolbarStateServiceTests.cs
|   |   |       \-- ToolbarStateTransactionTests.cs
|   |   |-- ToolTips/
|   |   |   \-- ToolTipBuilderTests.cs
|   |   |-- Windowing/
|   |   |   \-- WindowCloseServiceTests.cs
|   |   \-- Tests.Flourish.Core.csproj
|   |-- Tests.Flourish.Extensions.Culture.Blazor/
|   |   |-- browser-preferences.test.mjs
|   |   |-- BrowserPreferenceChecks.cs
|   |   |-- CatalogModuleChecks.cs
|   |   |-- Program.cs
|   |   \-- Tests.Flourish.Extensions.Culture.Blazor.csproj
|   |-- Tests.Flourish.Extensions.Culture.WPF/
|   |   |-- Culture.json
|   |   |-- Culture.Records.json
|   |   |-- EssentialTextProviderTests.cs
|   |   |-- ModuleCatalogTests.cs
|   |   |-- TestAssembly.cs
|   |   \-- Tests.Flourish.Extensions.Culture.WPF.csproj
|   |-- Tests.Flourish.WPF/
|   |   |-- AppearanceTests.cs
|   |   |-- CatalogTests.cs
|   |   |-- ContentTests.cs
|   |   |-- ControlTests.cs
|   |   |-- DataTests.cs
|   |   |-- NativeTest.cs
|   |   |-- PopupTests.cs
|   |   |-- RenderTests.cs
|   |   |-- ShellTests.cs
|   |   |-- Tests.Flourish.WPF.csproj
|   |   \-- Usings.cs
|   \-- Tests.Gallery.Flourish.Blazor/
|       |-- ChangeLogChecks.cs
|       |-- DynamicSampleChecks.cs
|       |-- LocalizedValidationHarness.cs
|       |-- Program.cs
|       \-- Tests.Gallery.Flourish.Blazor.csproj
|-- .gitattributes
|-- .gitignore
|-- AGENTS.ensure.json
|-- AGENTS.framework.json
|-- AGENTS.lock.json
|-- AGENTS.md
|-- Directory.Build.props
|-- Directory.Build.targets
|-- fetch-agents.bat
|-- Flourish.slnx
|-- global.json
|-- LICENSE.txt
|-- publish-helper.bat
\-- start.bat
```

## Root files and module responsibilities

| Repository-relative path | Responsibility | Source evidence |
| --- | --- | --- |
| `Flourish.slnx` | Aggregate development solution containing all 22 projects; it is not a deployment orchestrator | Actual solution project entries and platform mappings |
| `global.json` | Requires .NET SDK 10.0.400 with latest-patch roll-forward and no prerelease SDK | SDK declaration |
| `Directory.Build.props` | Shared package metadata, deterministic output, Flourish 1.4.0 version prefix, Essential.Culture 1.4.0 version property, generated package output directory | Current shared MSBuild properties; user-authorized release preparation |
| `Directory.Build.targets` | Refuses architecture-specific pack output for managed libraries whose package assemblies are TFM-wide | `RequireArchitectureNeutralLibraryPackage` target |
| `AGENTS.md`, `AGENTS.ensure.json`, `AGENTS.framework.json`, `AGENTS.lock.json` | Instruction routing, separate audit state, installed managed-payload manifest and installation provenance | Current framework files; audit-state validity is evaluated by the coordinator |
| `.agents/` | Two project-owned workflow skills: documentation audit and shared framework synchronization | Each skill manifest and UI metadata |
| `.config/` | Repository-local .NET tool declarations for DocFX and CSharpier | `dotnet-tools.json` |
| `.github/` | Windows build/release verification, NuGet Trusted Publishing, and DocFX/GitHub Pages workflows | `workflows/build.yml`, `workflows/docs.yml` |
| `.gitignore`, `.gitattributes` | Generated/personal file exclusions and automatic text line-ending normalization | Actual tracked policies |
| `LICENSE.txt` | Repository license | Existing license file |
| `fetch-agents.bat`, `scripts/fetch-agents.ps1` | Shared AGENTS framework check/installation launcher and implementation | Installed manifest and scripts |
| `start.bat`, `scripts/Start-*.ps1`, `scripts/StartSettings.json` | Select and run the configured Gallery/diagnostic application; tests validate launcher behavior | Four configured application entries |
| `publish-helper.bat`, `scripts/Publish-Helper.ps1`, `scripts/Release-*.ps1`, `scripts/ReleaseSettings.psd1`, `scripts/Verify-PackageSet.ps1`, `scripts/Test-Release.ps1` | Release version/tag validation, focused builds/tests, packing, dependency-order verification and publication support | Release configuration and script implementations |
| `scripts/Convert-MaterialIcons.ps1` | Development-time conversion of maintained Material icon resources | Conversion script and icon provenance manifests |
| `build/` | Library CSS bundle MSBuild task plus CSS, Culture, ChangeLog, HTTP and independent package-consumer verification scripts | `CssBundle.targets`, `BundleCss.cs`, and focused script entry points |
| `src/Flourish.Core/` | Platform-neutral contracts and optional application services: settings, project catalogs, commands, navigation, layout, localization, messaging, profile, shell state, motion, tooltips and window-close decisions | `Abstract/`, functional folders and `Hosting/CoreServiceCollectionExtensions.cs` |
| `src/Flourish.Blazor/` | Abstract contracts, functional components/browser behavior, popup dismissal policy, semantic presentation bands/progress, explicit optional Design, and dependency-only convenience package | Four project files; `Components/PopupStrength.cs`, Framework controls and browser modules, optional Design CSS |
| `src/Flourish.WPF/` | Independent native WPF contracts, controls/templates/geometry, owned popup lifecycles and optional paint/theme resources, and dependency-only convenience package | Four project files; native `PopupTrigger`, menus/dialogs and themes; no WPF project reference to Core or Blazor assemblies |
| `src/Flourish.Extensions/` | Optional Blazor and WPF adapters to Essential.Culture, with each adapter referencing only its target platform Framework | Two bridge projects and their registration/provider APIs |
| `src/Flourish.WinUI3/` | Unpackable WinUI3 placeholder project with no maintained control implementation | Single project file; no other maintained source in this directory |
| `src/Gallery.Flourish.Blazor/` | Interactive-server Gallery, API/sample metadata, functional examples, module-based localization and circuit-local demonstration records | `Program.cs`, `Components/`, `Models/`, `Services/RecordStore.cs` |
| `src/Gallery.Flourish.WPF/` | Native Gallery that composes WPF production controls, theme resources and the Culture adapter | `App.xaml.cs`, `MainWindow.cs`, `GalleryCatalog.cs`, sample factories |
| `src/Gallery.Flourish.WINUI3/` | WinUI desktop placeholder displaying an empty Grid; it does not consume the Flourish WinUI library | `App.xaml.cs`, `MainWindow.xaml`, project with no project references |
| `tests/` | Core/WPF xUnit projects, executable Blazor/Gallery/bridge checks, framework-only web diagnostic host and maintained Node DOM/behavior checks | Seven test/diagnostic projects and their executable entry points |

## Solution and project boundaries

The five solutions are `Flourish.slnx`, `src/Flourish.Blazor/Flourish.Blazor.slnx`, `src/Flourish.WPF/Flourish.WPF.slnx`, `src/Flourish.WinUI3/Flourish.WinUI3.slnx`, and `src/Flourish.Extensions/Flourish.Extensions.slnx`. The root solution includes all projects. The platform/extension solutions select development graphs; they do not imply separate runtime services. All solution project paths and all 33 declared project-reference edges resolved on the inspected Windows workspace. The local project-reference graph has no cycle.

| Project file, relative to repository root | Kind and responsibility | Direct local project dependencies |
| --- | --- | --- |
| `src/Flourish.Blazor/Flourish.Blazor.Abstract/Flourish.Blazor.Abstract.csproj` | net10.0 contract library; shell, components, text and Design contracts | `Flourish.Core` |
| `src/Flourish.Blazor/Flourish.Blazor.Design/Flourish.Blazor.Design.csproj` | net10.0 Razor library; optional palette, typography, appearance state and stylesheet | `Flourish.Blazor.Framework`, `Flourish.Blazor.Abstract` |
| `src/Flourish.Blazor/Flourish.Blazor.Framework/Flourish.Blazor.Framework.csproj` | net10.0 Razor library; production controls, interaction, geometry and browser assets | `Flourish.Blazor.Abstract` |
| `src/Flourish.Blazor/Flourish.Blazor/Flourish.Blazor.csproj` | net10.0 dependency-only convenience package; no packaged assembly | `Flourish.Blazor.Abstract`, `Flourish.Blazor.Framework`, `Flourish.Blazor.Design`, `Flourish.Extensions.Culture.Blazor` |
| `src/Flourish.Core/Flourish.Core.csproj` | net10.0 shared library; platform-neutral contracts and application services | None |
| `src/Flourish.Extensions/Flourish.Extensions.Culture.Blazor/Flourish.Extensions.Culture.Blazor.csproj` | net10.0 Razor bridge; request negotiation, scoped text, language picker and browser Cookie preferences | `Flourish.Blazor.Framework` |
| `src/Flourish.Extensions/Flourish.Extensions.Culture.WPF/Flourish.Extensions.Culture.WPF.csproj` | net10.0-windows WPF bridge; provider-neutral text adapter with facade or isolated catalog/context | `Flourish.WPF.Framework` |
| `src/Flourish.WinUI3/Flourish.WinUI3.csproj` | net10.0-windows10.0.26100.0 unpackable WinUI placeholder library | None |
| `src/Flourish.WPF/Flourish.WPF.Abstract/Flourish.WPF.Abstract.csproj` | net10.0-windows contract library; native controls, shell, data and text contracts | None |
| `src/Flourish.WPF/Flourish.WPF.Design/Flourish.WPF.Design.csproj` | net10.0-windows WPF library; optional theme/palette resources and live ThemeSession | `Flourish.WPF.Abstract`, `Flourish.WPF.Framework` |
| `src/Flourish.WPF/Flourish.WPF.Framework/Flourish.WPF.Framework.csproj` | net10.0-windows WPF library; production controls, templates, interaction and icon resources | `Flourish.WPF.Abstract` |
| `src/Flourish.WPF/Flourish.WPF/Flourish.WPF.csproj` | net10.0-windows dependency-only convenience package; no packaged assembly | `Flourish.WPF.Abstract`, `Flourish.WPF.Framework`, `Flourish.WPF.Design` |
| `src/Gallery.Flourish.Blazor/Gallery.Flourish.Blazor.csproj` | net10.0 ASP.NET Core executable; Interactive Server Gallery | `Flourish.Blazor.Framework`, `Flourish.Blazor.Design`, `Flourish.Extensions.Culture.Blazor` |
| `src/Gallery.Flourish.WINUI3/Gallery.Flourish.WinUI3.csproj` | net10.0-windows10.0.26100.0 unpackaged WinExe; empty WinUI window placeholder | None |
| `src/Gallery.Flourish.WPF/Gallery.Flourish.WPF.csproj` | net10.0-windows WinExe; native Gallery | `Flourish.WPF`, `Flourish.Extensions.Culture.WPF` |
| `tests/Tests.Flourish.Blazor.Native/Tests.Flourish.Blazor.Native.csproj` | net10.0 ASP.NET Core diagnostic executable; Framework-only web sample without Design/Culture registration | `Flourish.Blazor.Framework` |
| `tests/Tests.Flourish.Blazor/Tests.Flourish.Blazor.csproj` | net10.0 executable checks; component APIs, render/event/lifecycle contracts; Node behavior fixtures | `Flourish.Blazor.Framework`, `Flourish.Blazor.Design`, `Flourish.Extensions.Culture.Blazor` |
| `tests/Tests.Flourish.Core/Tests.Flourish.Core.csproj` | net10.0 xUnit tests; platform-neutral behavior and assembly/public API boundaries | `Flourish.Core` |
| `tests/Tests.Flourish.Extensions.Culture.Blazor/Tests.Flourish.Extensions.Culture.Blazor.csproj` | net10.0 executable checks; scoped bridge, modules, preferences; Node Cookie fixture | `Flourish.Extensions.Culture.Blazor` |
| `tests/Tests.Flourish.Extensions.Culture.WPF/Tests.Flourish.Extensions.Culture.WPF.csproj` | net10.0-windows xUnit tests; WPF text bridge and generated module catalog integration | `Flourish.Extensions.Culture.WPF` |
| `tests/Tests.Flourish.WPF/Tests.Flourish.WPF.csproj` | net10.0-windows xUnit tests; native controls, offscreen templates/rendering and Gallery samples | `Flourish.WPF.Abstract`, `Flourish.WPF.Framework`, `Flourish.WPF.Design`, `Gallery.Flourish.WPF` |
| `tests/Tests.Gallery.Flourish.Blazor/Tests.Gallery.Flourish.Blazor.csproj` | net10.0 executable checks; Gallery routing, samples, API metadata and ChangeLog | `Gallery.Flourish.Blazor` |

Reference direction is explicit: Blazor Abstract uses Core contracts; Blazor Framework uses Abstract; Design uses Framework and Abstract; the Culture bridge uses Framework and Essential.Culture.Blazor. WPF Abstract is independent; WPF Framework uses Abstract; Design uses Framework and Abstract; the WPF Culture bridge uses Framework and Essential.Culture.Wpf. The Blazor convenience package includes its Culture bridge; the WPF convenience package does not, and the WPF Gallery registers the bridge explicitly. Neither platform Framework depends on its optional Design or its Culture bridge.

The WPF Framework's shared translation JSON and WPF Gallery's shared SVG are **source resource links** to Blazor-maintained files, not references to Blazor assemblies or a browser rendering dependency. WinUI projects appear beside Core in their solution but have no declared project-reference edge to Core.

## Runtime and resource boundaries

| Application or resource | Runtime/deployment boundary | Communication, state or persistence | Evidence |
| --- | --- | --- | --- |
| Blazor Gallery | One ASP.NET Core/Kestrel server process plus browser UI; SSR and Interactive Server rendering | Browser HTTP/static assets and Blazor circuit transport; `RecordStore` is scoped in-memory demonstration data, not a database | `Program.cs`, `Components/App.razor`, `Routes.razor`, `Services/RecordStore.cs` |
| Framework-only Blazor diagnostic sample | Separate optional ASP.NET Core executable, port 5189 in launcher configuration | Same framework assets/components, with no Design or Culture adapter registration | `tests/Tests.Flourish.Blazor.Native/Program.cs`, launch profile, `scripts/StartSettings.json` |
| WPF Gallery | Separate native desktop process/window; no WebView or ASP.NET server | MainWindow composes Framework and optional Design; native theme tracks Windows appearance; `Culture.json` copied beside the application for Essential's default facade | `App.xaml.cs`, `MainWindow.cs`, project resource/content declarations |
| WinUI Gallery | Separate unpackaged Windows desktop placeholder executable | `App.OnLaunched` activates MainWindow containing an empty Grid; no implemented Flourish controls | `App.xaml.cs`, `MainWindow.xaml`, project file |
| Framework defaults and browser Culture preference | Server configuration defaults are distinct from one browser's selection | `appsettings.Flourish.json` supplies defaults with host overrides taking precedence; `.AspNetCore.Culture` Cookie stores UI/format culture; middleware reads it before SSR; selection is scoped | Framework registration, `CultureFrameworkExtensions.cs`, `CultureSession.cs`, `wwwroot/browser-preferences.js` |
| Blazor appearance state | Optional per-circuit Design service | Primary/accent/theme changes are stored in scoped `AppearanceService`; refresh persistence for these personal selections is not implemented in that service | `DesignServiceCollectionExtensions.cs`, `Hosting/AppearanceService.cs` |
| Core application state | Optional library-hosted services, not a separate application process | Framework-owned settings descendants persist to configurable `appsettings.Flourish.json`; project catalogs default to `projects.json`; profile credentials use a caller-supplied optional credential-store contract | `Configuration/`, `Projects/ProjectCatalogStore.cs`, `Profile/`, `Hosting/CoreServiceCollectionExtensions.cs` |
| Gallery translation modules | Build/publish resource files owned by the Gallery host | Seven multilingual functional modules: Shell, Pages, Components, Parameters, Samples, Access and ChangeLog. Essential generator produces typed keys and the deployed file manifest; bridge eagerly loads/validates one logical Gallery catalog | Gallery project `CultureModule` items, `Localization/Culture.*.json`, `Program.cs` |
| Library translation resources | Embedded per-library catalogs | Blazor Framework and bridge embed their own catalogs; WPF Framework embeds the shared framework JSON under its native resource identity; adapters resolve through platform-neutral text contracts | Framework/bridge project resource declarations and providers |
| Icons and fonts | Checked-in assets loaded locally | Material Symbols WOFF2 for browser and TTF/Brotli icon geometry for WPF; provenance/license files included. Configured text font names are system/font-stack choices, not verified network font downloads | Both Framework icon directories, `source.json`, licenses, Design font configuration |
| CSS bundles | Build-time compilation output for two Razor libraries | `build/CssBundle.targets` and `BundleCss.cs` produce packaged Framework/Design CSS from maintained source CSS | Both Razor project imports and bundle entry properties |
| NuGet/GitHub/Pages infrastructure | Restore, release and documentation services used by tooling/CI | Release scripts verify and pack libraries; tag-triggered CI can publish NuGet packages; docs workflow builds human-maintained DocFX content and deploys GitHub Pages | Workflows, `.config/dotnet-tools.json`, `scripts/ReleaseSettings.psd1` |

No production database connection, remote application API client, message broker, distributed cache, or microservice orchestration was found in the inspected source/configuration. That is an observation about this repository, not a prohibition on consumers supplying such services. External links shown in Gallery samples are navigation/documentation links rather than application backend dependencies.

## Current public API and interaction ownership

PageHeading owns its loading strip. Optional `Loading` defaults false and is ORed through CSS with observable active sources across the document and embedded PageBody/shell/content/navigation scopes, including dialogs beside PageBody or outside Routes. Shared document aggregation may activate multiple headings together; it does not claim per-Gallery-example isolation. LoadingState publishes an active marker; completed ProgressBar/ProgressRing and stopped rings stop publishing it. Button automatically exposes pending asynchronous OnClick through its existing Busy semantics, prevents duplicates and releases its internal state in finally even on failure. The title does not publish a source marker, avoiding feedback loops. The absolute 4px strip occupies no document-flow space, follows centered/expanded/fluid gutters and bounds FullWidth titles to ContentContainer geometry. Compact title/action behavior remains; DocumentFlow uses relative positioning without making page titles sticky. Framework `wwwroot/page-heading-loading.css` owns state/geometry/animation; Design's same-named file owns theme/forced-color paint. The two Node files join the existing palette/page-body-actions entry points. No arbitrary fetch, native navigation or DOM-invisible Task is intercepted. See the [loading/progress report](1_bugreports/2026-10-23_page-heading-loading-and-progress-rendering.md) for confirmed source behavior and unconfirmed physical rendering diagnosis.

Framework owns ordinary HTML hidden visibility as well as control geometry. Its scoped rule covers owned roots themselves and descendants of `f-root`, shell/content/navigation surfaces, dialog and menu, plus standalone Notice/Loading components. `display:none!important` prevents ordinary hidden content from reappearing through component grid/flex declarations outside a shell. The new general guard excludes case-insensitive `hidden="until-found"`; existing component-specific contracts are retained. This is a visibility contract, with no invented validation result, registration transport, token use or host CSS workaround. The [registration hidden-state report](1_bugreports/2026-10-22_registration-invitation-hidden-state.md) distinguishes the invitation-page display defect from unconfirmed completion requests. The generated .10 package is not active in the existing .9 Aspire process until a separately controlled restart.

AccessLayout owns bounded access/simple-operation geometry inside an existing ContentSurface stage, never an additional main or authentication transport. Brand and return navigation stay outside its scrollable body. Wide selects 960px instead of 560px and Dotted uses existing semantic roles. NavigationChoices.FillHeight bounds each current panel without allowing hidden alternatives to affect layout; at <=760px its sidebar becomes a compact header, and at <=360px height AccessLayout permits body scrolling so operations remain reachable. LogoDisplayer.Compact is opt-in and keeps the configured identity, heading level and optional description with horizontal image/title placement. With default `ContextPrimary=false`, its primary title and context inherit ordinary LogoDisplayer typography: `clamp(56px,8vw,80px)` and semantic H2 context (28px default), without a Compact-only or short-height type override. Compact still has its own default image dimensions and geometry. Optional positive LogoSize controls only the image via actual dimensions and image-local --f-logo-size, independently of Compact or short-height defaults. The access-method Gallery demonstrates this actual composition; its catalog preview uses an explicit bounded 540px demonstration host.

Optional `LogoDisplayer.ContextPrimary` defaults false and takes effect only with nonblank ContextName. It promotes context to the ordinary fluid primary scale and places the smaller ProjectName above it within the same encoded semantic heading and existing TitleId/HeadingLevel contract. Compact uses a row subgrid so the image aligns only to the main context row; LogoSize remains independent and description follows the identity. Blank context falls back to the ordinary layout. Gallery samples and localized usage/default metadata describe this actual contract.

NavigationChoices exposes one Layout enum (Grid, Compact, Sidebar); the former Compact boolean is removed. Sidebar composes real Button/Icon entries, optional current-item check and retained keyed panels. Its two-column geometry stacks below 760px, and hidden panels cannot affect layout. The Gallery demonstrates only platform and organization methods; generic presentation/access controls remain available elsewhere.

Local source versions combine VersionPrefix and VersionSuffix. Get-PackageVersion returns the exact preview package identity, and Verify-PackageSet uses it by default. Stable release/tag/publication entry points use Get-ReleaseVersion and reject suffix-bearing source before remote/tag actions. Gallery keeps an X.Y.Z-preview aggregate, validated against the next stable patch; a preview prefix is never counted as an already released version. build/Test-PreviewVersion.ps1 exercises these boundaries using disposable local props only.


`PopupStrength` is a platform-specific public contract in Blazor Abstract (`Components/PopupStrength.cs`) and WPF Abstract (`ControlContracts.cs`). Its default is `Weak`: library-owned popups may dismiss through outside interaction, Escape, or repeated trigger activation. `Strong` requires selection or an explicit completion/cancellation action. Strength does not replace validation, disabled/busy state, `CanClose`/`Dismissible`, authorization, or disposal cleanup. Components validate the enum rather than silently interpreting unsupported values.

Blazor Framework owns the shared popup controller in `wwwroot/controls.js`; native SSR enhancement is bootstrapped by `wwwroot/primitives/input-behaviors.js`, while interactive components retain their own interop lifetimes. Actual control parameters forward strength into their owned menu, selection, dialog, tutorial, or notice surfaces. Native `details` remains the pre-enhancement disclosure boundary, so enhanced dismissal guarantees require the browser module. Browser-managed native select/date/file surfaces and platform tooltips retain their platform behavior; the library does not claim to impose its popup policy on those external surfaces.

The shared `positionPopup` function chooses a physical left/right edge from the actual trigger center relative to the viewport midpoint, writes `data-f-popup-align` before measuring, and retains viewport clamping, vertical flip, available-height scrolling and hover/action gaps. Menu, selection and search panels keep longest-item/content width, zero fixed minimum and a viewport cap. Their options, buttons and links always align content left; the edge attribute affects placement rather than text alignment. Search inputs fill the measured column. Theme copying excludes `--f-popup-align` so nested popups compute their own edge. Native select physical edge is synchronized during initialization and capture `pointerdown`/`focusin`/`keydown`, and after resize/scroll/content discovery. ReferenceDropdown and SearchAutocomplete retain manual-native-popover placement through `primitives/reference-dropdown.js` and their circuit-owned selection, keyboard, creation, dismissal and cleanup state. No host-side second selection controller is added. `popup-placement.test.mjs` remains registered in `scripts/ReleaseSettings.psd1`; simulated DOM/cascade results do not certify native first-frame painting. See the [popup alignment/width report](1_bugreports/2026-10-22_popup-left-alignment-and-native-picker-width.md).

Native option rows use automatic sizing with an independent `var(--f-control-height,48px)` square marker track and a flexible left-aligned label. Their marker paint follows the standard Checkbox border/radius/check-state roles without inserting a nested input or replacing native selection semantics. MultiSelectBox retains its standard checkbox track and flexible label. `initializeNativePickers` measures a private offscreen inert copy at initialization and before opening, copying actual option/rich content and computed typography, marker, gap, padding and border metrics. It writes `--f-picker-inline-size` before the default opening action; CSS constrains that fixed width to the viewport. Already-open native pickers are excluded from measurement. Edge and width properties are written only when values differ. There is no post-open frame resize, opening delay, simulated select, host patch or new public width parameter. Native browser form/keyboard/change ownership remains intact, and classic select fallback is retained where base-select is unavailable. The [popup alignment/width report](1_bugreports/2026-10-22_popup-left-alignment-and-native-picker-width.md) separates source/DOM evidence from manual physical first-frame acceptance.

WPF Framework implements strength on its owned `ActionMenu`, `SplitButton`, `MultiSelectBox`, `DropdownSurface`, `Dialog`, and `NoticeTrigger` paths. `ServiceMenu` inherits its menu policy. The internal `Controls/PopupTrigger.cs` guards the real toggle operation used by the dropdown and multi-selector templates, including keyboard/automation activation. Owned `Popup` instances use `StaysOpen` and routed event handling; explicit completion still follows the component's busy/close rules, and disabling or unloading releases transient UI. WPF `SelectBox`, `ReferenceDropdown`, and `SearchAutocomplete` retain the native ComboBox selection surface; `DateBox`, `FilePicker`, and ordinary tooltips retain native platform ownership. This is a Windows control boundary, not a browser implementation or a guarantee for operating-system dialogs.

Blazor `PresentationBand` and `PresentationHero` default to `FullHeight=true` and `MinHeight=0`. Full-height CSS establishes `max(100dvh, MinHeight)` with a `100vh` fallback, while content may grow beyond the viewport and scroll naturally. Explicit `FullHeight=false` retains natural height with zero minimum or a caller-specified nonnegative pixel minimum. Hero delegates one band and therefore produces one semantic progress marker. `PresentationFooter` has natural content height; footer boundaries remove the default viewport minimum from nested bands and exclude all footer content from navigation.

Both presentation components accept optional `BackgroundImageUrl`; Hero forwards it to its owned Band. A nonblank URL renders an empty-alt, `aria-hidden` image in a decorative layer before the content container, using a normal HTML-encoded image attribute rather than CSS URL interpolation. Framework owns absolute cover geometry, isolated stacking and `pointer-events:none`; the media adds no height, landmark, heading or accessible name. Optional Design overlays the existing Canvas/Surface/Primary role as a gradient that stays dense beneath copy and reveals the image toward the opposite edge, with stronger coverage at widths up to 760px. A missing/failed image leaves the underlying Tone surface; no media URL produces the prior ordinary structure. FullHeight, MinHeight, progress and natural footer behavior remain owned by the existing presentation contracts. Gallery samples use the existing local `gallery.svg`; the public API does not require a hosted media service or expose arbitrary mask colors/opacity.

`Patterns.ContentSurface.ShowPresentationProgress` defaults to `false`. Enabling it composes the reusable `PresentationProgress` scenario for that surface's main content and marks its Footer slot as an exclusion boundary. `PresentationBand.IncludeInProgress` and `PresentationHero.IncludeInProgress` default to `true`; explicit false excludes a destination without changing content or height. Only eligible outermost semantic bands are counted, rather than arbitrary sections or a footer's nested bands. One band retains one clickable tutorial-style segment, while zero bands hide the navigation. Existing `ContentId="application-content"` compatibility is retained; progress/nav-generated anchor identifiers are instance-specific and discovery is scoped to the actual surface main element.

The bottom-center tutorial segments navigate to the selected band's viewport position below actual sticky headers, reflect manual scrolling, and have accessible link names without hovering titles. `wwwroot/presentation/progress.js` owns scroll-container discovery, viewport/header geometry, reduced-motion handling, observers, route synchronization, and cleanup. Navigation stays outside document flow; it hides in a footer-only viewport or when its actual segment height cannot fit. Its foreground follows the band beneath its position, and footer actions remain available without an added tail row. Static SSR does not require `OnAfterRender`: the native browser entry initializes progress on document startup and enhanced navigation, sharing the same idempotent controller with component interop. Framework `presentation/progress.css` owns layout; optional Design `presentation-progress.css` owns paint. Business `SectionNavigator` remains the independent right-side content navigator.

`ComponentUsageCatalog` registers `PresentationProgress` as an explicit production scenario. Gallery `ComponentCatalog`, `ParameterMeaning`, and the three Components/Parameters/Samples Culture modules describe the actual defaults and scope. `PresentationProgressSample.razor` composes the public ContentSurface opt-in with two actual bands and a natural footer inside a bounded example scroll area. `PresentationProgressChecks.cs` and `presentation-progress-dom.mjs` exercise SSR/lifecycle and actual browser-module behavior; WPF `PopupTests.cs` exercises native popup boundaries. These are maintained verification entry points, not an assertion that this inventory maintenance executed them.

Blazor SelectBox, StandaloneSelectBox and the Culture LanguagePicker expose optional Icon and TriggerVariant (both default null). Customized native triggers project the real select owner states into the same standard Button declaration blocks; the inert child is not an independent interaction source. Native pickers use canonical menu surface/option roles, while raw-select rules explicitly exclude .f-select. Native popup dismissal remains browser-owned and is not assigned Strength. Customized triggers keep a real native select and standard option/value/change semantics, with base-select enhancement only where supported; classic rendering retains the text selector. ContentSurface.FloatingActions is a separate optional render slot at the upper inline end, outside both TitleBar and the scrolling stage. Its absolute layout and direct action-row spacing reserve no header height; ordinary TitleBar behavior is preserved.

## Active project constraints and exceptions

- Product project/solution names, organization-prefixed namespaces and existing `Arkheide.*` package IDs are separate identities. All inspected explicit C#/Razor namespace declarations use an accepted organization prefix. Dependency-only convenience projects omit `RootNamespace` but do not ship a compiled assembly or maintained source API.
- Core targets `net10.0` without a UI SDK, Windows target or platform assembly reference. `tests/Tests.Flourish.Core/Architecture/CoreBoundaryTests.cs` contains executable checks for its assembly, project and exported API boundaries; this audit inspected those checks without rerunning them.
- Optional Design is registered explicitly. Framework owns production controls/behavior and uses a neutral text contract; optional adapters register text services. This inventory describes source responsibilities rather than defining another UI specification.
- Existing Blazor `Primitives`, `Patterns` and `Internal` folders are source organization. Semantic usage is defined by `ComponentUsageCatalog` and production contracts rather than by assigning every folder a separate deployment/API family.
- Windows platform projects and their Gallery/test processes require the declared Windows capabilities. Presence in the aggregate solution does not certify an implementation: WinUI3 remains the placeholder described above.
- The current physical `src/Gallery.Flourish.WinUI3/` directory matches the case used by its project/solution paths. The earlier audit recorded `Gallery.Flourish.WINUI3` and a possible case-sensitive path risk; that observation is retained as dated evidence and does not describe the current physical inventory. This documentation refresh does not rename source files or certify WinUI runtime behavior.

## Verification and unresolved facts

The inspection used actual hidden/ignored physical-file enumeration, all maintained project/solution XML, shared MSBuild files, root SDK/tooling/release/launcher configuration, runtime entry points, framework registration, native resource ownership, Culture adapters, and representative executable architecture fixtures. The 2026-10-10 full audit found 678 maintained files, 121 non-root directories, 22 projects, 5 solutions, 33 local project-reference edges, no missing local project/solution references on Windows, and no project-reference cycle. All explicit C#/Razor namespace declarations inspected complied with the organization-prefix rule.

The 2026-10-12 maintenance re-enumerated all maintained files under the same exclusions, parsed the regenerated complete tree back into paths, and reconfirmed the 33 declared project-reference targets on Windows. That maintenance inventory contained 688 files and 121 directories, with 22 projects and 5 solutions. It adds ten maintained files to the audit tree and removes none: the Blazor popup enum, six progress component/style/module/test files, the Gallery progress sample, and WPF PopupTrigger/PopupTests. The eleven excluded empty/residual folders are unchanged. No build, test run, application startup, full documentation audit, framework synchronization, or audit-state update was performed by this inventory-maintenance task; coordinated validation is recorded separately by its owner.

Architecture inspection does not prove tests, application startup, browser appearance, Windows architecture support or release publication. This structure inventory did not rerun builds/tests or start applications. The coordinated review separately compiled the actual copyable Culture examples; see [the audit report](1_bugreports/2026-10-10_project-instruction-audit.md) for that focused verification. Independently recorded tests from earlier tasks remain separate evidence. The WinUI3 placeholder remains a current limitation; runtime certification for that platform is unknown. The earlier case-sensitive path concern is dated audit evidence; the current physical Gallery directory uses the matching `Gallery.Flourish.WinUI3` spelling.

This document supersedes the uninspected seed template. Documentation and installation state are intentionally outside this tree's runtime/source assertions; their audit completion and preserved history are managed by the coordinating audit task.
