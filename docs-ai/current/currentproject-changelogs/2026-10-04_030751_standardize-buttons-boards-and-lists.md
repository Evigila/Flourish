# Standardize buttons, display boards, split navigation and lists

Date: 2026-10-04 03:07:51, America/Sao_Paulo.

## Authorized scope and resulting source

The user requested a 50px page-title role, six standard button appearances, property-based icon/text/disabled states, silver display boards with code copying, list refinements and a visible split-navigation gap. These are Flourish changes; no consumer business page or new external dependency was added. Earlier uncommitted work and user-deleted skill packages remain untouched by Git staging. No commit was created.

- Design's page-title token and all extracted layout fallback sizes are 50px. Semantic H1/H2/H3/body remain 34/28/20/17px. This supersedes the earlier approved 42px title for Flourish only; portable common standards remain unchanged.
- Button exposes Filled, Outlined, Danger, Quiet, Underline and Elevated. Primary/Secondary retain compatibility. Icon, Text and optional Href replace the IconOnly flag; ChildContent remains available. Disabled/Busy are state properties. Missing text/content with a provided icon infers the icon-only shape. A disabled/busy link loses href, leaves the tab sequence and cannot recover navigation through splatted href attributes. Text is centered using flex and consistent line-height, with progressive text-box trimming.
- PageHeading and Primitives.RecordPageHeading compose the same Underline Button above the title. Their old custom visual rules were removed; only placement rules remain.
- DisplayBoard is a Framework component with Preview/Code roles. Design supplies a silver #e5e8eb surface and a dot pattern only for Preview. Code uses escaped CodeBlock text and a top-right Elevated copy action. clipboard.js loads on explicit copy, copies the exact entire text, handles a denied Clipboard API through a contained fallback and restores focus/selection without leaking the temporary node. Failure is visible instead of reported as success.
- ComponentGuide uses DisplayBoard for both demonstrations and both code sections; API's shared DataTable starts 24px below its invocation code. Other Gallery documentation code sections use the same board. No guide-specific backdrop or forced Dark scope remains. The catalog has 76 independent guides with five H2 sections.
- DataTable places previous and next actions before the current-page input. Gallery counters use 项. ActionMenu and display options share dropdown surface/item CSS; visible columns have a selected state. Sort-header hover is a border on transparent fill. Automatic ordinary data columns cap at 320px; the final visible data column does not, even when spacer/actions follow. Manual widths and Home reset semantics remain supported.
- SplitButton is a public generic component with independent native primary/secondary actions, shared Selected and controlled Expanded. It has a 3px gap and separate corner shapes. ApplicationShell uses it for a destination with children, preserving distinct route/disclosure semantics and existing keyboard focus behavior. Navigation labels retain left alignment and single-line truncation.
- Busy text now uses Framework screen-reader-only CSS and a button-local positioning context; display boards do not add a general overflow:auto wrapper. This addresses the code mechanism consistent with the temporary double-scrollbar symptom. Pixel-level reproduction remains manual acceptance, not an SSR claim.

## Verification

- Final Gallery and Framework-only native fixture builds: 0 warnings, 0 errors; temporary artifacts avoid the running VS host.
- .NET component/runtime suite: 78/78 passed, including new button/link, return-heading, split/disclosure, pager/dropdown/last-column and encoded board checks.
- Real data.js and clipboard.js mocked-DOM tests: 12/12 passed. Clipboard coverage includes multiline Unicode, fallback, failed copy, append/restore exceptions, cleanup and preserved selection.
- Existing menu/dialog/text-selection DOM checks: 23/23 passed.
- CSS parser/bundler suite: 21/21 passed. Final whitespace diff check passed.
- Independent temporary Development HTTP host: all 76 component guides, eight category directories, ten documentation-code routes, six button variants, shared dropdown markup, synchronized split selection, standard parent links, 50px title tokens, silver/dot CSS and clipboard module response passed. The temporary process was closed without stopping the user's host.
- All 76 displayed scenario sources were copied with the same removal of Gallery namespace/registration lines to a new Razor project referencing Framework and Design only; build passed with 0 warnings/errors. This exposed and repaired the missing Web import for the new DisplayBoard example's native selection binding.
- Typography scan found only inherited values and existing graphic-size exceptions (checkbox tick 24px, column drag glyph 22px), not extra reading sizes.
- The explanatory directory tree covers 876 maintained files and 132 directories with no missing, duplicate, extra or unexplained nodes.

Temporary evidence: `C:\Users\Evigila\AppData\Local\Temp\flourish-copy-controls-52ca3e20060c48a8b27e6a673fe958c0`; HTTP logs `flourish-control-http-2f0c7e9db7464061a95747255ed689dd.out.log` and `.err.log` in the same temp directory.

## Acceptance limits

No Computer Use was performed under repository rules. SSR, code inspection and mocked DOM cannot certify pixels, optical multilingual text centering, actual browser clipboard permission or absence of browser scrollbars throughout Busy. These checks, theme/viewport acceptance and a rebuilt VS Gallery session are listed in [blazor-manual-tests.md](../blazor-manual-tests.md). Functional library behavior remains separate from the optional Design skin and from host-owned business/session operations.
