# Consolidate Blazor packages for the first release

## Scope and cause

The user approved retiring Shared, adding a full Blazor convenience package and limiting the first 1.1.0 release to Core and Blazor, including the optional Blazor Culture bridge. The old Abstract → Shared → Core direction mixed public contracts with processing implementations; merging Shared wholesale into Framework would create an Abstract/Framework cycle. The former eight-package release also included desktop packages outside the approved release scope.

## Changes

- Removed the Shared project and its empty source directories. Public navigation, appearance, control, table, grid, selection, identity and severity contracts now compile in Abstract, which directly references Core.
- Moved local filter, sort, pagination, mask and notice-presentation implementations into Framework, together with internal TableData. Public names, namespaces and signatures are preserved; existing binaries must rebuild because declaring assemblies changed.
- Added a dependency-only Blazor convenience package referencing Abstract, Framework and Design. Core restores transitively. Installing Design does not activate it; AddFlourishDesign remains explicit. Framework-only installation still omits Design.
- Kept the Culture bridge independent: Abstract plus Essential.Culture.Blazor 1.3.0. Framework, Design and the convenience package have no Essential dependency.
- Scoped release preparation to the Blazor solution and six packages: Core, Abstract, Framework, Design, Blazor and Extensions.Culture.Blazor. WPF and its bridge remain in maintained source, outside this release.
- Tightened package verification to the configured library dependency set and dependency-only meta output. Updated the root/focused solutions and package inventory regression expectations.
- Added four package-only consumer fixtures with isolated caches, explicit NuGet feeds and no inherited repository build configuration. They verify Framework-only, unthemed meta, themed meta and Culture bridge consumers through local publication, actual SSR and HTTP static assets.
- Bridged legacy page-gutter and field-label-gap to Design tokens and checkbox-size to the existing approved 48px control-height token. The existing 7px field gap is preserved; themed narrow-window gutter is 20px. Portable common visual rules were not edited.
- Updated active architecture, package, Culture and manual guides. Previous change records and human-maintained docs remain untouched.

## Verification

The final scripts/Test-Release.ps1 completed with exit code 0 after all source changes:

| Check | Result |
|---|---|
| Release build with warnings as errors | 0 warnings, 0 errors |
| Core tests | 367 passed |
| Blazor framework checks | 134/134 passed |
| Blazor Culture bridge checks | 12 passed |
| Node tests and existing mock suites | 37 tests passed; mock checks passed |
| CSS bundle tests | 21 passed |
| CSS SDK/HTTP integration | 194 passed |
| NuGet set/dependencies/assets | Exactly six 1.1.0 packages passed |
| Isolated package consumers | 67 checks passed across four modes |
| Git whitespace verification | diff --check passed |

Final consumer evidence is under artifacts/package-consumers/728f83c9c57140e1a850a87b67697989. Final CSS fixture evidence is under the system temporary css-bundle-088dd94062184a71968eb44d2931ce1c and css-assets-03bf42bd40514c52aeddf8bb36a608f1 directories. Generated artifacts are ignored by Git.

The consumer setup explicitly initializes its language so operating-system locale does not determine the assertion, and registers the packaged Framework catalog under identity Flourish alongside its application catalog. Omitting that required catalog causes ordinary Framework translated text to fail explicitly; the bridge does not create catalogs for hosts.

## Boundaries and manual regression

No Computer Use, commit, release tag, push, external package upload or production deployment was performed. Essential 1.3.0 is consumed from prepared packages. Local output does not establish NuGet.org availability. Desktop source was retained but its full functional suites were not rerun in this Blazor release scope.

1. Rebuild existing consumers and verify no Shared assembly/reference remains.
2. Install the convenience package, register Framework only and confirm native layout, masks, menus, tables and editing-grid behavior. Then register Design and confirm theme activation and stylesheet order.
3. Check generic label gaps, 48px themed checkboxes and 24px/20px gutters at desktop/narrow widths, keyboard navigation and 200% zoom.
4. Register both application and Framework Culture catalogs; switch language in two independent browser profiles and verify translation/formatting remain scoped.
5. After public release, repeat a clean consumer restore from NuGet.org without sibling feeds or source projects.

