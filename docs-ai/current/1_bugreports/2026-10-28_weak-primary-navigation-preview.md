# Weak primary-navigation preview with one real navigation tree

Recorded 2026-10-28, America/Sao_Paulo (UTC-03:00). Local **1.4.4-preview.shell.20261027.10**. Prior .9 header/control budgets, .8 fitted menus/Information24px and .7 icon-font compatibility remain. No publication, deployment or commit is involved.

## Requirement and chosen boundary

The primary rail previously showed icons and individual descriptions. It now weakly expands the same primary items to Icon + Label when the pointer enters the entire rail, with no content/secondary-column reflow. Both composed NavigationSurface and configured ApplicationShell use **PrimaryNavigationPreview=true** by default; false preserves the narrow icon behavior. This does not show another module's secondary links or change configured mobile/persistent secondary navigation.

A cloned RenderFragment was rejected because it would duplicate links, IDs, focus/assistive content and interop initialization. The existing aside/nav and its actual scroll owners are temporarily shown as a **manual native popover**, with a noninteractive slot retaining the original grid column. They return to normal flow on close. The top layer avoids transformed/paint-contained Gallery clipping; geometry is bounded to the owning root and viewport, in LTR/RTL. A clipped top offset is applied to the original scroller while expanded and reversed only after a successful presentation. Original main/fixed scrolling regions remain, without transferring navigation state between duplicate elements.

The [HTML popover algorithms](https://html.spec.whatwg.org/multipage/popover.html) distinguish manual from auto/hint focus restoration. The controller does not request autofocus or trap Tab. It preserves an existing focus only if the rendering transition cleared it to body and the prior element is still connected, and does not move an outside focus. Popover failure restores the normal rail, reservation and original scroll.

## Interaction and accessibility

- Mouse entry over the rail opens; the outer label region remains the same interactive rail. Leaving the whole region closes. Mouse-click focus does not pin it. Native GET links and command callbacks are untouched; no click is synthesized or intercepted.
- Keyboard focus-visible previews the labels. Escape closes; it is consumed only when focus belongs to that rail. A hover-only preview closes without consuming a foreground dialog/editor's Escape. Touch does not auto-open and retains ordinary icon navigation.
- The same full aria-label, current/disabled state and unique IDs remain. Added visual labels are aria-hidden. Long labels ellipsize in the original row height/target width instead of increasing row height. DisabledMessage remains inline and in its existing aria-describedby tooltip; ordinary duplicate label tooltips are suppressed while previewing. Tooltip attachment rebinds when a render replaces an enabled anchor with a disabled span, removing the old trigger listeners.
- Resize/blur release the reservation, and the next genuine entry works immediately. Escape alone retains its dismissal latch until a deliberate new entry/Tab interaction. Opt-out, removed roots and disposal cancel frames, observers/listeners, popover attributes and owned CSS properties.
- The old shell's layout observer measures its narrow slot, preserving secondary/backdrop offsets; mobile placement rules are scoped to the old shell's direct slot so an embedded NavigationSurface with no secondary rail does not inherit them.

Framework owns the shared controller/layout; optional Design uses existing Chrome/ink/shadow tokens. The normal icon targets, selected/unavailable paint, existing scrolling and source link semantics remain. Gallery provides a localized toggle and disabled-reason example; public parameter metadata/localization covers English, Chinese and Portuguese. No external package or polyfill is introduced.

## Verification and corrected development boundaries

The initial Gallery build used Razor component namespace aliases that tag resolution did not accept; using the existing fully qualified tag convention corrected that source error. A first new CSS assertion assumed desktop-only 55px while the static parser includes narrow 45px rules; it now tests the complete bounded cascade. Final production/C# test sources predate the successful frozen build.

Code review and focused regressions also caught and fixed: a legacy-shell mobile slot selector leaking into nested composed surfaces, resize/blur incorrectly swallowing the first hover, applying a clipped-scroll reversal when showPopover failed, taking a hidden first item as the rail target, and consuming an outside editor/dialog Escape. These are retained here as development evidence, not unresolved defects.

- Final twelve-project Release: **zero warnings/errors**.
- Framework **487/487**, including same-link/ID, safe label encoding, disabled description, opt-out/default and all catalog parameters. Catalog: 83 public components, 775 parameter rows and 363 documented defaults.
- Gallery **8,653/8,653**, including 474 post-event sample checks and 443 ChangeLog checks.
- Focused Node **76/76**: preview lifecycle/input **10**, complete CSS **33**, header **9**, plus existing heading/surface/navigation cases. The section/top-control script additionally reports its 26 internal checks. CSS bundle **21/21**.
- Six new archives pass the existing package-set/dependency/asset verifier. Eleven CSS/JS/font/assembly resources equal source or actual Release output. Header-layout.js/common/full fonts match .9 byte for byte; twelve assets graphs retain 33 external package/version pairs.

Immutable local feed: `artifacts/primary-preview-20261028/nuget`. It follows the authorized explicit preview build/pack path because no debug-handler exists and Prepare is stable-only. Earlier feeds were not replaced. The package verifier explicitly requires the controller module; new CSS partials are inlined in the existing CSS bundles. Evidence logs and package-hashes/package-delivery JSON are in the adjacent artifact directory. Pack retains the existing missing-package-readme informational notices. No remote tag/publication, dependency installation, deployment or Git commit occurred. [Exact package identities](../1_dependency.md#current-candidate-archive-and-asset-evidence) retain the hashes.

Four maintained files were added: Framework/Design patterns/primary-navigation-preview.css, Framework patterns/primary-navigation-preview.js and tests/Tests.Flourish.Blazor/primary-navigation-preview.test.mjs. The complete tree is **714 files/121 directories**, with 22 projects/5 solutions. No Computer Use, browser automation or physical UI measurement ran. Consumer integration belongs to its coordinated record.

## Manual acceptance

1. In a business portal, enter the empty primary-rail area and move across the expanded labels. Confirm the same links are clickable; secondary/content boundaries and header height stay fixed. Current and disabled items retain their expected paint.
2. Click a navigation item, then move away. Confirm click focus does not pin the preview and native navigation/commands execute once. Repeat configured-shell fixed commands and route entries.
3. Tab/Shift+Tab through the rail; labels and focus must remain readable. Escape closes without losing the focused link. With focus in an outside editor or newly opened dialog, Escape must still reach that control while dismissing only the hover preview.
4. Scroll a long rail and, for configured shell, its main/fixed areas. Expand, scroll, collapse and reopen without a jump to the top. Repeat an embedded Gallery sample partly outside the viewport, RTL and narrow/zoomed layouts.
5. Change language and use long labels/custom disabled explanations. Row/icon positions should remain stable; full accessible names and disabled descriptions should remain available. No duplicate ordinary tooltip should appear over the expanded label.
6. On touch, use ordinary icon navigation; no hover-dependent step is required. Toggle PrimaryNavigationPreview off/on, resize, blur/refocus the window, change routes and remove an embedded surface. Confirm the first fresh hover works and no stale floating rail remains.
