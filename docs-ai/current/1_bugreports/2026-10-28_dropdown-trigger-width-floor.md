# Dropdown width floor follows the actual trigger

Recorded 2026-10-28, America/Sao_Paulo (UTC-03:00), following committed source `aaac88c`. The requested rule is the larger of the actual trigger border-box width and natural option content width, subject to the existing viewport cap. Small menus retain content-driven expansion; wide form controls no longer open a narrower panel.

## Source boundary

Shared positionPopup owns ActionMenu/ServiceMenu, native-select hover enhancement/LanguagePicker, MultiSelect and the reference/search panel path. Native base-select has a separate option-probe measurement path. The correction must cover both while preserving natural max-content sizing, author style/priority, alignment/flip, height/scroll, disabled/hidden rules, existing filtering and keyboard/touch/hover behavior.

ReferenceDropdown already has a private full-items natural-width sizer, which remains. MultiSelect retains its existing filtered-visible-content sizing; this change adds the trigger floor without adding an interactive clone or changing search semantics. The panel must shrink again after a narrower trigger/content replaces a prior wide opening, and close/detach must restore owned variables.

## Implementation boundary

The shared positioning routine writes an owned trigger-width custom property only on actual dropdown panel classes. CSS retains max-content width and uses that property as a viewport-bounded minimum; the existing computed author maximum remains a bound. Theme copying excludes this geometry property, and menu/reference release restores its prior inline value and priority. Tutorial surfaces retain their existing sizing.

Native base-select keeps option-probe natural width separate from the trigger floor, so an earlier wide trigger does not contaminate the longest-option measurement. Trigger ResizeObserver updates follow container changes, while shared-menu/reference teardown and document-native discovery release disconnected nodes. Existing list sizers, filtering, alignment, height and input semantics remain.

## Manual acceptance

1. Open long form triggers with short choices, and short menu/language triggers with long choices. The larger required width should win, including padding, borders, icons and selection markers.
2. Resize/reopen, change content/culture and use native/base-select, hover-enhanced select, reference/search and multiple selection. No old trigger width should remain after a narrower opening.
3. Check viewport-narrower-than-trigger, right-edge/RTL, opening above, scrolling, groups/hidden/disabled options and filtering. Preserve the existing usable viewport limits and interaction behavior.
4. Close/remove/rebind a control and verify no stale inline width variable, priority, observer or popup state remains. Header and primary-preview motion remain unchanged.

## Frozen implementation and verification

Candidate **1.4.4-preview.shell.20261027.13** is frozen in `artifacts/dropdown-width-and-admin-navigation-20261028/nuget`. Four existing production files change: controls.js, framework.css, form-inputs.css and primitives/reference-dropdown.js. The full Design cascade required a more specific actual dropdown-owner selector so a later min-width:0 rule could not erase the floor. The shared helper also bounds the floor by document.clientWidth minus the existing viewport margin, including scrollbar-versus-100vw differences; the existing long-content cap remains.

Final twelve-project Release passed with zero warnings/errors; Framework **487/487**, Gallery **8653/8653**, CSS bundle **21/21** and scoped DOM/CSS **176/176** passed: popup/native **66**, controls DOM **82**, action-menu DOM-only **28**. Fourteen physical-browser checks were intentionally not executed. Two legacy SSR data: test loaders initially failed to resolve the already-existing header/preview imports; their real dependency URLs were rewritten without stubbing behavior, then all DOM checks passed. Both production JS syntax checks and diff checks passed.

Six packages pass identity/dependency/required-asset verification. Thirteen packaged CSS/JS/font/DLL resources equal maintained source or Release output; this evidence adds controls.js/reference-dropdown.js to the preceding asset capture. Header/primary-preview JS, both fonts and previous .12 archives remain unchanged. Twelve graphs retain 33 external package/version pairs. No maintained source file/public API is added: tree 714 files/121 directories, 22 projects/5 solutions and 775 parameter rows/363 defaults remain.

Evidence is under the matching artifact directory, including build.log/build-final.log, Framework/Gallery/DOM/CSS logs, pack/verification logs and package-hashes.json/package-delivery.json. [Dependency inventory](../1_dependency.md#current-candidate-archive-and-asset-evidence) retains exact hashes. Independent read-only review found no remaining definite defect. No Computer Use, browser geometry measurement, external dependency installation, cache/old-feed replacement, public publication, commit, push or deployment occurred. Physical functional acceptance remains manual.
