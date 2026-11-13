# Table widths and organization portal composition

## Symptoms and verified causes

Colligere Products used the actual DataTable and EditingGrid, not replacement renderers. DataTable's automatic width loop measured the entire bulk-editor row, concatenating select option captions and the apply command. The last visible data column intentionally has no ordinary 320px automatic text cap, so entering bulk mode inflated its width. Selection-column width was also omitted from the table minimum. This is a library sizing defect, not a consumer width override.

EditingGrid explicitly rendered a filler col/header/cell after the actual columns. Its min-width:100% forced that blank column to absorb viewport space. Removing only the filler would spread extra space over real columns; therefore the table must retain the sum of real column widths without a forced viewport minimum.

Organization entry selected ContentSurface's fixed-height application mode instead of document flow. Its footer sat outside the independently scrolling main. The dotted DisplayBoard was also applied to the operation panel rather than the page background.

## Repairs and ownership

- DataTable excludes bulk-editor rows from natural record sizing and includes selection chrome in its minimum width. Manual resizing, last-column text behavior and Home reset remain library-owned and unchanged.
- EditingGrid removes the filler DOM/styles and its obsolete sizing exclusion. Empty-state colspan matches the actual column count. The viewport still fills available space, but unused background is not a column. Manual widths, hidden columns and natural measurement remain supported.
- Gallery demonstrates a select in the last bulk column and a narrow EditingGrid with no synthetic tail column. Organization access uses a full-height dotted Surface hero and the existing Primary AccessFormSurface; ContentSurface uses document flow with an organization-only footer outside main.
- Stable 1.1.3 includes the previously validated centered-gutter, Dialog alignment and square EditingGrid repairs. Essential stays 1.3.0; WPF is not in the release manifest or commit scope.

## Verification and limitations

Initial focused JavaScript checks pass: DataTable 8/8, including enter/exit bulk, ordinary/final columns, manual widths and Home reset; EditingGrid measurement 7/7. Source Blazor checks initially pass 402/402. Final release preparation, public indexing and public-only consumption are independent gates; their outcomes are recorded in the release guide after execution. No Computer Use or authenticated live-session acceptance is claimed.

## User-operated regressions

1. Products: compare Situação before/during/after bulk editing; resize manually and restore with Home.
2. Table editing: scroll to Imagem and verify there is no following header/cell; resize the viewport and final column, then test copy/paste and links.
3. Organization portal: scroll a short/narrow viewport; footer follows content, not the viewport. Background dots cover the page; login method/actions retain primary fill in light/dark themes.

Unrelated WPF reconstruction and Colligere inventory/performance work remain outside this UI release.

## Completed release evidence

Source 6b5dc255fb97d4313da7e11bce9bab820cc06256 is tagged v1.1.3. [Run 37653211373](https://github.com/Evigila/Flourish/actions/runs/37653211373) successfully uploaded all six packages, with Created responses from 16:38:42 to 16:38:47 UTC. Complete isolated preparation passed with zero build warnings/errors: Core 367, Blazor 403/403, bridge 12, Gallery 7,266, Node 102, CSS 21/194, launcher 95, catalog 21,454 and package consumers 129. Independent public-only consumers pass 129 using fresh package and HTTP caches; all six cached fresh index responses contain 1.1.3 and their package provenance names NuGet.org.

Evidence remains under C:/Users/RC_Auditoria/AppData/Local/Temp/f113/artifacts: release-1.1.3-final.log, public-release-1.1.3.log and package-consumers/372b5d2e51504cd591b7f12110139733. Initial inherited Colligere central package configuration and Windows path-length failures were fixed by a short isolated source path, not by changing production dependencies or bypassing verification. Transient network/index propagation attempts are distinct from the final successful public verification. Public 1.1.2 is not overwritten; Essential stays 1.3.0; the WPF reconstruction remains outside this commit/release.
