# Pricing composition and root scroll

Flourish now demonstrates the standard Pricing composition through existing production controls. Additive Card and OfferStage parameters preserve their defaults; typography, role paint and document-scroll ownership remain library-owned. No new control family, package or external dependency was introduced.

## Changes

The Pricing example uses a standard PageHeading at 50px followed by five full-width 450px-minimum bands alternating Canvas/Surface. First/fourth section headings use left-aligned H2 at 28px. DocumentFlow keeps the page title static and aligns its padding with the existing centered ContentContainer, including the narrow-screen gutters. Business-stage compact headings remain unchanged.

Card adds Prominent=false, Text and Actions. Its default DOM remains compatible. Prominent uses left actions and right paragraph copy at the existing H1 34px scale, a 260px minimum and natural growth; at 760px and below it stacks vertically. It supports long copy and an absent Actions slot without inventing another heading or form. The Pricing free/custom bands use brief copy and real Elevated actions, while their example-only or unavailable behavior stays explicit.

OfferStage adds RotationControlIconOnly=false. Optional icon mode uses the standard Quiet Button, pause/play icon and scoped accessible label. The default text mode, initialization/SSR availability, reduced motion, focus, controller ownership and disposal are unchanged. Comparison uses the existing static ListView with a concise accessible caption rather than a redundant visible heading. Original pricing tiles and offer collection remain examples, not authentication or billing authority.

PresentationFooter examples use standard Underline navigation. Its Primary-background foreground and press roles are defined in the central Button stylesheet. An initial presentation-CSS placement was rejected by the existing palette test; the rules were moved to the correct ownership boundary without weakening that gate or adding a host skin.

Framework applies overscroll-behavior-y:none to html/body containing document-flow surfaces or independent access documents. Source analysis is consistent with the reported root bounce moving sticky top chrome. The fix does not lock body height, intercept gestures, reset scroll position or change internal business-stage overflow. It also suppresses vertical pull-to-refresh; browser/system support and physical touchpad behavior remain manual acceptance. Normal scrolling, native fragments/forms and horizontal overflow remain native.

Card, OfferStage and Footer guides demonstrate their current contracts. The catalog remains 98 controls with 737 parameter rows and 326 defaults. Existing broader API convergence debt remains documented; this task does not claim all compatibility renderers have merged.

## Verification

- Complete final scripts/Test-Release.ps1: exit 0; Release compilation zero warnings/errors; Core 367, Blazor 259, bridge 12, all ten JavaScript files, CSS bundle 21 and SDK integration 194 passed.
- Nine new Blazor checks cover Card compatibility, encoding, action/copy order, no-action and long-copy cases, responsive role/type ownership, document heading/root-scroll contracts, Footer Underline context and icon rotation labels/lifecycle.
- Six 1.1.0 candidates passed package validation. Four fresh isolated NuGet-only consumers passed 95 checks at artifacts/package-consumers/8068db7229dc44e58ae790505dbcfe11. Expected package README advisories remain separate from compilation.
- Gallery HTTP passed Pricing, product, login and Card, OfferStage and Footer guides, plus both actual bundled CSS contracts. The owned Gallery process was stopped.
- Colligere full Release solution and final test-project builds reported zero warnings/errors; 249 focused Web regressions passed with matching local candidates. Original five-category/seven-feature matrix, native price fragments and unavailable checkout/custom actions remain guarded.
- No Computer Use, Debug build, user-instance stop/restart, new commit, push, public NuGet upload, dependency change or business workflow expansion. Essential and desktop sources are unchanged.

## Manual acceptance

Inspect Pricing under Light/Dark/System, wide/320px widths and 200 percent zoom. Confirm the standard page title, left section headings, alternating full-width bands, centered content, unchanged tiles and brief prominent paragraphs. Cards must grow without clipping and stack at the documented narrow breakpoint. Footer Underline links must remain legible against Primary.

Use keyboard and pointer to pause/resume rotating offers; narrow/reduced-motion/static modes intentionally do not expose an active rotation control. Check localized accessible names and default text-mode compatibility. Exercise native price links and disabled example actions without treating them as checkout.

On the actual supported browsers, test top/bottom touchpad boundary gestures on document-flow marketing pages and independent access documents. Confirm normal scroll, keyboard navigation, anchors and horizontal tables remain usable, then compare internal business-stage scrolling and dialogs. Root policy is a source contract, not a claim that OS/browser chrome can be controlled.
