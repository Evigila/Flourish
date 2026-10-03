# Rendered Workspace alignment

## Method and scope

On 2026-10-03, local Colligere was started through its existing Aspire AppHost. The user logged into an isolated, visible Edge window. Existing Account directory navigation opened an existing Workspace. The review visited dashboard, products, new product, customers, new customer, Workspace information and customization through GET navigation. No business create/update/delete action was performed. Gallery was rendered with installed Edge using the existing bundled Playwright tool, then opened in the same visible browser for final screenshot comparison. No browser, package, font or icon dependency was installed; Computer Use was not used.

## Findings and resulting behavior

| Area | Initial finding | Result |
| --- | --- | --- |
| Heading focus | H1 had a solid 3px field-like outline | H1 remains focused; outline-style is none; keyboard field remains solid 3px |
| Control cascade | Global font reset forced action cells to 17px/400 | Low-specificity reset preserves 40px/740 action type at 1440px and 28px/740 on narrow windows |
| Native enhanced select | Block layout put picker arrow beneath its text | Base-select uses a horizontal flex row; fallback remains native |
| Shell | Extra secondary group title; weak selected rail; bare title | Source-size selected rail, text-only secondary links, optional brand/service tracks and rail-only overview |
| Page rhythm | Title limited to padded content; fixed section gaps | Full-stage sticky title, 84/52/44px section rhythm and source-derived tinted surfaces/borders |
| Local list | Different search/view/pager geometry | 240px search selector, 16px gap, 36/14px spacing, 40px inset accent view switch and source-size pagination controls |
| Facts/identity | Native dd indentation, ungrouped dt/dd and lost primary scale | Shared FactList with continuous boundaries, correctly grouped identity facts and 40px/740 primary value |
| Dark/floating surfaces | Light Subtle values and dark progress text; partial sheet divider | Theme-scoped Subtle/progress roles, inherited popup tokens and full-width bottom-sheet divider |
| Dirty navigation | Discard cleared target before reading it | Discard follows the original requested page; keep-editing still preserves the draft |

## Verification

- Gallery/library Debug build: 0 warnings, 0 errors.
- C# contract/DI/SSR/data checks: 35 passed.
- Mocked controls DOM checks: 15 passed; data DOM checks: 6 passed.
- Installed Edge rendered checks: 79 passed, no page errors, six routes at 1440/980/760/520/320px; both document and main-stage horizontal overflow are zero. Wide table overflow stays inside its own region.
- Release package generated; all six packaged CSS/JS assets are nonempty and SHA-256-equal to final sources.
- Final visible Edge screenshots cover form, list and details alongside the authenticated Workspace. Captures and local runner scripts stay outside the repositories; browser profile/session files are not product assets.

## Remaining acceptance

Use blazor-manual-tests.md for OS scaling, browser zoom, wrapped long names, screen-reader/forced-colors use, reduced motion, Firefox/native select fallback, modal/menu keyboard acceptance and older-browser fallback. Real-browser sampling does not claim full cross-browser acceptance or authenticated Colligere adoption. Existing business/API/session boundaries remain consumer-owned.

The final visible-browser review additionally found parent and child secondary routes selected together. The shell now selects only the most specific enabled match; an unlisted record falls back to its parent. This is covered by a focused C# case and the rendered detail-page check. Odd final facts span the full definition track.
