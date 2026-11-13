# Popup placement and display-board alignment

## Symptoms and causes

ActionMenu click mode appears next to its button, but enabling hover mode can move it away. controls.js previously used the actual button rectangle for click mode and the outer f-action-menu rectangle for hover mode. A stretched or tall wrapper changes that rectangle without moving the button.

DataTable page-size options avoid viewport edges through the browser's native select picker. Its display selection only shared dropdown appearance: a separate details/summary panel remained absolutely positioned below the trigger. It lacked a viewport-aware behavior adapter, so it could extend beyond the visible area.

DisplayBoard ordinary content could remain start-aligned because its centered/start selectors expected a direct content child. The edge-scrollbar refactor introduced a viewport between the board and content. Direct Button also explicitly aligned itself to start.

## Correction

ActionMenu uses its button as the anchor in both modes; the wrapper only owns hover entry/exit. Shared menu positioning chooses above/below according to available space, aligns/clamps horizontally and limits height to the chosen side. Existing CSS/inline height limits remain effective and are restored after closing. Scroll and resize refresh position; hover panels meet their triggers without a gap.

DataTable keeps native disclosure/checkbox semantics and adds a manual popover using controls.js's shared disclosure adapter. data.js owns instance connection, rerender synchronization and disposal, including empty tables and old-browser portals. Multi-selection remains open; disabled inputs are excluded from arrow navigation. Escape, outside interaction and focus departure use the shared lifecycle. This closes a behavior-standardization gap; native select still uses browser positioning rather than the JavaScript adapter.

DisplayBoard selectors now traverse the viewport. Direct ordinary children and inline action groups center horizontally; required full-width layouts retain their width. CodeBlock content is excluded from ordinary centering and remains upper-left.

## Evidence and acceptance

Blazor 105 checks, controls DOM 38 checks, input-origin 5 checks and palette 9 checks pass. All 77 guides load. Builds have zero warnings/errors. Real headless installed Edge verifies oversized-wrapper hover anchoring, seamless panel entry and immediate exit, display above/below placement, persistent checkbox changes, outside/Escape dismissal and focus return, ordinary/direct-button centering and upper-left code. Browser exceptions are absent.

Mocked DOM also covers tall panels, scroll/resize, original height restoration, empty tables, listener cleanup and fallback portal rerenders. Manual acceptance should repeat those cases in supported browser engines, narrow/zoomed layouts and keyboard-only use; automated results do not claim complete assistive-technology acceptance. No new dependency, Computer Use test, Colligere change or user-process restart is involved.
