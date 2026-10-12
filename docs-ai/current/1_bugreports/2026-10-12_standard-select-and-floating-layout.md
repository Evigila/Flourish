# Standard select and floating layout corrections

## Confirmed causes and fixes

An SSR DropdownSurface used min-width:100%, which became viewport-relative after the shared menu controller promoted it to a fixed top-layer popup. The controller now resolves percentage minimums against the actual trigger and restores original geometry on close. Native select pickers use intrinsic content width constrained to the viewport.

Colligere's language control had been changed from a standard selection component to ActionMenu. The correction extends SelectBox, StandaloneSelectBox and LanguagePicker with optional Icon/TriggerVariant, preserving native value/options/change/focus/cancellation semantics. Supporting engines enhance the standard native trigger; classic fallback remains readable. No separate language menu or selection controller is introduced.

PresentationProgress had circular links and switched to a static row after visible footers. It now reuses TutorialBoard's segment treatment, remains outside document flow, pairs its foreground with the band beneath it, and hides in footer-only or insufficient-space viewports. Footer exclusion and click alignment remain intact.

Login's in-flow TitleBar still reserved its 48px action height. ContentSurface now offers FloatingActions, positioned independently outside the header/stage. Its direct InlineActions margin reset removes an additional 16px margin without changing ordinary action rows. The sole access band fills from the top; long forms retain stage scrolling.

## Verification

Release compilation, six-package verification and 143 independent package-consumer checks passed. Source checks passed 451 Framework, 27 Culture and 8346 Gallery cases, all 19 maintained JavaScript suites, including 25 actual progress-module cases and 37 menu/native/browser cases with zero browser skips. Native trusted input verifies picker selection/cancellation and unchanged login viewport geometry; presentation compares actual TutorialBoard bar geometry and confirms no added footer height. A Gallery example's unsupported Razor namespace alias was replaced by the fully qualified component tag. A centering assertion was corrected to clientWidth because the scrollbar consumes 15px; its strict one-pixel tolerance remains.

The immutable .3 candidate is recorded in [the dependency inventory](../1_dependency.md#historical-local-popup-candidate-3-2026-10-12). Colligere's complete Web suite passed 2379, with zero failures and 132 existing database/shared-fixture skips. Both monthly audit states remain unchanged. [Manual acceptance](../popup-presentation-manual-checklist.md) covers live user sessions, older engines and viewport/zoom variations. No Computer Use, public package publication, application restart or Git commit was performed.
## Final chrome foreground follow-up, 2026-10-12

A real native picker comparison found that input-scoped black option text also colored the icon trigger black on blue ShellHeader chrome. Only the enabled customized Quiet select's chrome foreground inheritance was corrected in Design; picker color remains explicitly paired. The strict case verifies white ordinary Quiet/select/icon against the blue header and black-on-white popup/options. The actual browser file passed 38/38 with zero skips. This is delivered in the fresh immutable .4 candidate, with six-package/143-consumer verification and Colligere final 2379-pass Web proof. The original .3 evidence is retained as history in the dependency inventory; its bytes were not replaced. See [the follow-up record](../1_changelogs/2026-10-12_192640_preserve-native-quiet-select-chrome-colors.md).