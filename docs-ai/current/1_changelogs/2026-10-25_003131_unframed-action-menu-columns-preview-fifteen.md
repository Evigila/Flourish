# Unframed multi-column action menus

Time: 2026-10-25 00:31:31, America/Sao_Paulo (UTC−03:00); UTC: 2026-10-25T03:31:31Z.
Status: implemented and verified through component and DOM/source checks. Physical browser acceptance remains manual. Source version is `1.4.3-preview.portals.15`; no public publication or Git commit was performed.

## Problem and supported composition

A host composed the portal trigger through DropdownSurface.TriggerClass using standard Button classes. DropdownSurface always prepends its fixed-size `f-menu-trigger`, so the text trigger simultaneously acquired icon-menu geometry. Its popup also wrapped three already interactive UniformGridButton controls in a decorated menu surface, a nav and a grouped UniformGrid.

Existing ActionMenu.TriggerVariant already provides an ordinary text-sized Button trigger without nesting another button inside summary. Existing menus did not provide an undecorated multi-column panel. The user explicitly approved the small Flourish extension and a new local preview so the host can compose direct standalone action tiles.

## Implemented public contracts and ownership

- `ActionMenu.Unframed` is a bool, default **false**. It removes the menu panel's background, border, padding, overall rounding and shared shadow. The normal popup controller, viewport bounds, scrolling, keyboard handling and dismissal remain in place.
- `ActionMenu.PanelColumns` is an optional int, default **null**. Positive values select equal-width columns; zero and negative values throw `ArgumentOutOfRangeException`. Null retains the normal list. The component renders its own validated column variable; hosts do not need CSS overrides.
- Both the native ChildContent and generated Actions branches expose the same panel options. Clearing options removes the related class and inline column variable.
- Framework applies grid display only in the already-supported open states, preserving native SSR disclosure, enhanced top-layer behavior and the native hidden contract. Its more specific tile selector preserves centered content even when accessible menuitem roles add list-row classes.
- Design removes only the requested panel decoration. Direct standalone Elevated UniformGridButton controls retain their own **110px minimum height**, **12px control radius**, padding and control shadow; long text may grow naturally. The three-column layout stays explicit on narrow screens instead of silently collapsing to a list.
- Colligere consumes the new API as `ActionMenu TriggerVariant=Quiet Unframed=true PanelColumns=3`, with three direct `UniformGridButton Variant=Elevated role=menuitem` links. There is no host nav, UniformGrid, card wrapper, control replacement, custom CSS or nested button. Portal targets, decorative image semantics and full navigation behavior are retained.
- Gallery parameter meanings are documented in English, Chinese and Portuguese. No maintained source file, third-party package, SDK, runtime, service, font, icon or media asset was added. No production JavaScript changed.

## Actual verification

- Framework Console runner: **472/472**. Catalog audit: **83 components, 765 parameter rows, 355 documented defaults**. New actual-component checks cover the defaults, native disclosure adoption without duplicated controllers, direct accessible tiles, generated disabled actions, resetting options, and invalid column counts.
- Focused complete CSS cascade/native-popup placement Node checks: **52/52**. Coverage includes closed/native-open/enhanced-open/hidden states, ordinary-menu preservation, independent tile geometry/elevation, viewport constraints and native-picker behavior.
- ActionMenu controller DOM-only checks: **28/28**; **14 real-browser checks explicitly skipped** with `--dom-only`. Existing selection dispatch, outside dismissal, disabled actions, Escape focus restoration, native SSR takeover and teardown remain covered. No Computer Use, browser or application startup occurred.
- The first Framework run was **471/472**. Its bounded SplitButton panel-display fixture deliberately rejected the new column selector as unreviewed. The fixture now models optional column configuration explicitly, asserts native and enhanced grid states plus closed-state hiding, and continues to reject unknown hidden or host-open selectors. No production change was used to bypass this failure; the final complete runner passes.
- Logs are retained in the consumer's ignored `artifacts/onboarding-refinement-20261025/`: `flourish-framework-15.log` (first result), `flourish-framework-15-final.log` (final result), and `flourish-popup-dom-15.log`. DOM-only controller stdout is retained in the task tool output.
- The coordinating task owns six-package identity/cache verification, the complete Colligere restore/build, host rendering tests and any runtime restart. Those outcomes are separate from these library checks.

## Manual acceptance

1. Switch Portuguese, English and Chinese; confirm the portal trigger grows with its actual label, remains one standard Button and never clips into an icon-sized square.
2. Open the popup and verify one row of three independent Elevated portal tiles, with no shared background, border, large padding or group shadow.
3. At a narrow viewport such as 320px, confirm all three columns remain visible, labels wrap readably and links retain their targets; open near either screen edge to verify viewport placement.
4. Use Tab/Arrow keys, Enter and Escape; confirm navigation works, Escape restores trigger focus and outside clicks dismiss the same popup.
5. Recheck an ordinary ActionMenu with default parameters; its existing surface and list layout must remain unchanged.

Audit gate: `SKIP_AUDIT`. Installed framework and audited version remain **1.0.0** with completion **2026-10-10T03:26:06Z**; the old repository rules do not require the newer debug-handler entry point. Audit state was not modified.
