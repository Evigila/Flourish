# Shared dynamic secondary navigation

Status: implemented in local `1.4.5-preview.4`; no public release.

## Symptom and boundary

ApplicationShell already rendered parent destinations and independently expandable child lists, but that renderer was private. A dynamic organization-scoped NavigationSurface host could not reuse it without duplicating library DOM, selection logic and styling. The authorized fix extracts that renderer as `NavigationTree`; it does not migrate the host into ApplicationShell or store per-user navigation in global ApplicationOptions.

`NavigationTree` is in `ArkheideSystem.Flourish.Blazor.Components` and consumes the existing `ArkheideSystem.Flourish.Blazor.Abstract.NavigationItem` contract. Parameters are `Items`, optional `CurrentPath`, optional `ItemLabel`, `ExpandLabel`, `CollapseLabel` and `Navigated`. Expand/collapse verbs retain the existing three-language text-provider fallback. Items may be replaced by new arrays and records on each render.

## Implemented behavior

- Shell and dynamic hosts use the same native nested-list renderer and SplitButton. Parent links retain their actual Href, including trailing slash/query/fragment. The independent triangle changes only expansion; it does not navigate or raise Navigated. A containing nav landmark remains host-owned.
- One internal NavigationSelection helper selects the most specific enabled destination and its ancestors, respecting segment boundaries and Exact. Matching ignores query, fragment and trailing slash. Standalone trees read the absolute-root Navigation.Uri path by default; Shell explicitly supplies its original base-relative path, preserving non-root base-href deployments.
- Ancestor-plus-Href identity preserves manual collapse and child-panel IDs across ordinary rerenders and equivalent fresh records. A changed selection path reveals current ancestors; query/fragment-only changes retain manual state. A parent may share its destination with its own landing child, as the pre-existing builder allows. Same-level duplicates, invalid local paths and cyclic item graphs are rejected.
- Disabled ancestry removes descendant navigation targets and disables branch controls. Each callback validates the current node identity/generation. Removed/replaced nodes, including a removed object later readded, cannot be expanded through old callbacks. Expansion, panel identity and route subscriptions are component-instance scoped.
- NavigationSurface receives a small library-owned narrow-layout adaptation only when its tree has child branches. The hierarchy remains vertical and scrollable, with the existing single rounded child-list background and no extra indentation or columns. Flat navigation keeps its previous responsive behavior.
- Gallery includes the public component guide, six parameter meanings, an executable sample in three languages, and updated actual Blazor Framework navigation guidance. No WPF or human documentation was edited.

## Diagnostic evidence

The first full Framework run passed 521 of 522 checks and exposed an established parent/landing-child same-Href contract. A whole-tree unique-Href identity was therefore incorrect. Hierarchical semantic keys now preserve that contract while rejecting sibling duplicates; the original regression was retained unchanged and the final 522 checks passed. Direct source review also preserved original Href output and Shell's base-relative path contract before package freezing.

The new independent interaction tests cover organization-prefixed absolute-root selection, untouched canonical Href, parent/leaf semantics, real SplitButton callbacks, collapse across fresh data and label changes, route/query changes, removed/readded node callbacks, inherited disabled state, specificity, separate instances, malformed/cyclic data and non-root-base Shell selection. Existing Shell tests remain part of the same run.

Actual expanded CSS was measured at 1280px and 600px using the existing optional installed headless-browser regression. Both sizes have a vertical nested list in the existing rail, zero child indentation, shared 12px-radius child background, and no extra column. This is automated verification, not Computer Use.

## Verification and delivery

- Twelve-project Blazor Release build: zero warnings/errors.
- Framework: 522/522; Gallery: 9023. Catalog: 87 components / 808 parameter rows / 381 documented defaults.
- Other Node regressions: 289/289; controller/actual-CSS geometry: 44/44, zero skipped.
- Culture integrity: 25787; changelog: 200; CSS bundle: 21; CSS SDK integration: 194.
- Six archives passed package identity, internal-version edge and expected-asset checks. Four fresh-cache package consumption modes compile and render NavigationTree, for 155 checks. All 33 external package/version pairs equal preview.3.
- Complete maintained tree: 731 files / 121 directories. The ordinary task did not renew documentation audit state.

Feed: `artifacts/navigation-tree-20261031/nuget`, local version `1.4.5-preview.4`. Source VersionPrefix remains 1.4.4; explicit CLI overrides select the candidate version. Parent `provenance.json`, `package-sha256.json`, `source-files/`, `source.patch` and `tree-verification.json` record baseline `7c6f4e2f939ba73e4d67ee532b38256c306dc4be`, 33 changed source/test/build hashes, six archive hashes and resolved closure. Framework/Design packaged CSS equal their actual Release bundles byte for byte. Eighteen prior archives from preview.1/.2/.3 retain their recorded hashes. No external dependency was installed/upgraded, no Colligere file was edited, and no commit/push/tag/public publication occurred.

## Manual acceptance checklist

1. In a named navigation landmark or NavigationSurface, pass dynamic organization-prefixed items and open a child URL directly. Confirm the child is current, its ancestors are selected, and its branch is expanded.
2. Click the triangle, then the parent name separately. The triangle only expands/collapses; the name uses the original configured URL. Repeat with keyboard activation.
3. Collapse the current branch and trigger an unrelated rerender or language change. Confirm collapse is retained and the visible/accessibility labels update. Navigate to another child path and confirm its ancestors reopen.
4. At wide and narrow widths, confirm child links remain in the same vertical rail, share one rounded background and acquire no extra indentation or independent column.
5. Remove or disable a branch through host permissions. Confirm unavailable descendants are absent or disabled, with no live link targets; restoring authorized data should not revive stale controls.
6. Check a Shell host under a non-root base href, and a separate NavigationSurface host with absolute organization paths. Both should select their corresponding current child using their documented path spaces.

Human visual, touch and assistive-technology acceptance remains a manual check; no Computer Use was performed.
