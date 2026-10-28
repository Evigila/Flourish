# Compact brand Button and header height

Recorded 2026-10-27, America/Sao_Paulo (UTC-03:00). Local candidate: **1.4.4-preview.shell.20261027.6**. Follow-up to [information grids and header consistency](2026-10-27_information-grids-and-header-consistency.md).

The preceding outer-spacing change retained a 48px logo. With standard Button vertical padding/borders and 16px outer block padding, that wing required about 86px before the header border. ShellHeader also inherited --f-topbar-height:auto from natural-row surface hosts, so its former var fallback did not establish an effective 68px minimum.

The logo now uses 24px native attributes, CSS dimensions and flex basis on all widths. Standard Quiet Button paint, 48px minimum height, 10px/13px padding and 10px gap remain unchanged. Outer 8px/10px padding (8px inline on narrow screens) remains noninteractive. Fallback wing minima shrink to 82px/78px and the existing controller still measures actual content requirements.

Framework and Design establish a component-owned 68px minimum rather than inheriting the surface's auto row token. Default branding requires about 48+16+1=65px and fits that floor. There is no fixed-height or maximum-height cap: stacked navigation, zoom and long actions retain natural growth. No public API, dependency type or maintained file was added. These are source/CSS calculations, not a physical browser measurement.

Verification: producer 12-project and consumer 17-project Release builds pass with zero warnings/errors. Framework checks pass **485/485**, header mocks **7/7**, native-select/CSS checks **31/31**, and five focused consumer header classes **95/95**. Default size calculations use actual Button typography, padding/border and logo values, including inherited auto and responsive layouts. Six .6 packages were packed and five restored. Gallery JSON parses and compiles; Gallery and whole-solution suites were not rerun. Earlier .5 results remain historical.

A unified consumer restart selects five .6 Debug dependencies, returns eight HTTP-200 readiness responses and serves six byte-matching resources. Four public SSR headers render the real Quiet brand with native 24×24 image dimensions, retaining service rows and the management destination. Captures are under the consumer's ignored `artifacts/compact-brand-20261027/`; they establish delivery rather than measured browser height.

Manual acceptance: compare brand/ordinary Button height at normal wide layouts, retain outer gaps and standard feedback, and check narrow/two-row/zoomed content for clipping and preserved header/footer alignment. No Computer Use is used.
