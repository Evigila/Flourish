# Header action row and primary-preview box alignment

Recorded 2026-10-28, America/Sao_Paulo (UTC-03:00). Follow-up to [header control-width accounting](2026-10-28_business-header-control-width-budget.md) and [weak primary-navigation preview](2026-10-28_weak-primary-navigation-preview.md). Those candidates retain their original evidence; successful mock checks do not constitute physical browser acceptance of this follow-up.

## Reported symptoms and source causes

The consuming Admin Center now places Services beside its brand, but identity and the language/POST-logout group can still occupy separate rows. This is not a second host identity. ShellHeader actions and the nested InlineActions both allow wrapping; actual InlineActions always supplies data-alignment, whose existing library rule gives it width:100%. The prior icon-width correction preserved 48px targets but its fixture omitted that attribute and did not constrain both wrapping levels. A correct content-width budget alone did not make the nested group an atomic content-width row.

Primary preview temporarily adds popover=manual to the original rail. The browser's popover default box adds padding/border. The preview previously reset margin and box-sizing, and optional Design cleared border, but NavigationSurface's aside had no author padding reset. This adds an outer inset around its already padded nav. Configured ApplicationShell already has author padding, so clearing every rail's padding would instead shift that shell's items. Preserve the collapsed rail's actual geometry when entering the top layer.

## Header correction

Only ShellHeader's direct action slots/groups use content width with a 100% ceiling. In measured aligned/fitted modes the outer action wing and those InlineActions are nowrap; stacked and no-script modes keep their ordinary wrapping fallback. Document, popup and dialog InlineActions keep their existing contracts. The geometry controller observes actual right-wing controls and identity text, and remeasures after native-select enhancement, with the existing differential observer set, idempotent property writes and disposal retained.

The regression uses real data-alignment=end, nested slots, the native POST form and its hidden antiforgery field. Complete CSS checks include both fitting modes, stacked/no-script release, narrow-to-fit recovery and unaffected nonheader/popup/dialog actions. These are source/mock checks, not physical-browser proof.

## Primary-preview correction

Capture the collapsed rail's block padding/border before adding popover; the expanded state explicitly neutralizes the UA box while retaining that block inset. Preserve each existing row's border-box height separately, rather than using only the first row for every command/link. Reset row padding/border in expanded layout and align each real icon using its old rectangle relative to the actual expanded row origin. This avoids assuming the item's outer-width midpoint is its icon center and accommodates command-button padding, rail borders and RTL scrollbars.

The controller removes its temporary per-row and rail properties on close, failed open and disposal. Collection checks compare all DOM item identities/order, including hidden entries, without layout measurement. Replacement, insertion, hiding or icon-node replacement closes the old preview; expanded geometry is never recaptured as collapsed geometry. The same original scroll owners, focus rules, links/buttons, primary-only captions and tooltip policy remain. Collapsed rules and public contracts are unchanged.

## Compatibility and manual acceptance

Keep the native links/buttons, POST form and antiforgery input, 48px targets, 68px growable header baseline, header/page/footer content ownership and existing preview lifecycle. No host control skin, new library, server hover callback, authorization change or fixed-height clipping is required.

1. Compare desktop Admin Center and presentation headers with identity/language/logout present. All controls should share a row when they fit; no action may be hidden or clipped.
2. Resize narrow then wide, change language/identity text, and exercise menus. Genuine overflow must retain a reachable fallback; fitted layout must recover and standalone headers must remain usable without enhancement.
3. Toggle primary previews repeatedly and compare icon centers before/after, including normal and RTL NavigationSurface, configured-shell main/fixed areas, nonzero author padding/borders and a scrolling rail.
4. Recheck expanded-label click, pointer leave, keyboard focus/Escape, touch, disabled explanations, route removal and failed Popover fallback. Content and header positions must not move.

## Frozen delivery and regression evidence

Candidate **1.4.4-preview.shell.20261027.11** is frozen in `artifacts/header-preview-alignment-20261028/nuget`. Both initial and frozen-source 12-project Release builds passed with zero warnings/errors. Framework **487/487**, Gallery **8653/8653**, focused Node **62/62** (preview **20**, header **9**, complete native CSS **33**) and CSS bundle **21/21** passed. The preview regressions include UA box defaults, route/command/disabled rows across RTL/gutter combinations, per-item icon coordinates, collection replacement and hidden-first-item handling; existing input/focus/scroll/failure checks remain. The shared CSS assertion's old uniform row-height expectation was synchronized to the new per-row fallback before the final run.

Six packages pass the existing identity/dependency/required-asset verifier. Eleven packaged CSS/JS/font/DLL resources equal maintained source or actual Release outputs; both fonts and every prior .10 archive were rehashed unchanged. The twelve assets graphs still resolve 33 external package/version pairs. No maintained source file or public parameter was added: the complete source tree remains 714 files/121 directories, 22 projects/5 solutions and 775 parameter rows/363 defaults.

Evidence logs, package-hashes.json and package-delivery.json are in the matching artifact directory; [current dependency inventory](../1_dependency.md#current-candidate-archive-and-asset-evidence) retains exact package/resource hashes. The existing explicit preview pack route was used; no old feed was replaced, external dependency installed/upgraded, public package published, commit made or service deployed. Independent read-only review found no remaining definite defect; consumer build/cache/regression evidence belongs to its coordinated record. No Computer Use or browser geometry measurement was performed.
