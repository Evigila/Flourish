# Refine Gallery explanations and localization layout

Local timestamp: 2026-10-06 12:44:52, America/Sao_Paulo.

## Cause and implementation

Gallery explanations were authored directly in each page rather than through a shared prose renderer. Overview placed its minimum-setup explanation after the code board; comparable form/search, Design configuration and icon notes also followed their code. Framework already placed the main explanation before code, but several independent sentences shared one paragraph. HTML source whitespace cannot create visible paragraph or line boundaries.

Overview, Examples, Appearance and Icons now place their code explanations before the corresponding DisplayBoard. Appearance color and typography explanations and the Examples directory session note precede their demonstration content. Framework top-bar and navigation supplementary notes also precede their code. Explicit semantic line breaks separate static explanatory sentences on Framework, Examples, Appearance and Icons. Dynamic result/status text remains alongside its running example.

The existing English and Chinese Home_SetupDescription, Localization_Introduction and Localization_Scope catalog entries now contain intentional paragraph boundaries. Overview and Localization render those boundaries as encoded paragraphs while retaining scoped language refresh. There is no global punctuation splitter, CSS whitespace override, new renderer or dependency.

Localization is an explicit secondary navigation page, so its PageHeading no longer supplies ParentHref or ParentLabel. Its control-example language picker is bounded to 16rem. The numeric/date format Field moves inside the same DisplayBoard as the formatted output and is bounded to 20rem. Both widths cap at the available container width. LanguagePicker's optional Gallery-only width configuration leaves existing shell usage unchanged. Language and independent format-culture callbacks, table formatting and dialogs retain their existing behavior; no public library control contract changes.

## Verification and limitations

The Gallery Release build succeeded with zero warnings and zero errors in artifacts/gallery-prose-verification. A normal-output attempt encountered the running Gallery process locking its executable; the successful separate output avoided interrupting that process. The currently running instance must be restarted through the user's normal workflow to display the rebuilt pages.

Existing Blazor regressions passed 351/351, including the component catalog audit (90 exported controls, 726 parameter rows and 320 defaults). Evidence: artifacts/gallery-prose-regressions.log. Culture.json parses successfully, and the whitespace diff check reports no errors. Source and build checks do not certify browser geometry or the visual result at narrow widths.

## Manual acceptance

- Restart Gallery and check Home in Chinese and English: minimum-setup explanation precedes the code board and paragraphs remain separate after switching language.
- Check every Framework topic, especially application integration, top bar and navigation: independent explanations have visible line boundaries and accompanying notes precede their code.
- Check form/search examples, Design colors/typography/configuration, and Icons: explanation precedes demonstration content while live result/status feedback remains with its controls.
- Open language and localization directly and through secondary navigation: the large title has no parent/back action.
- At desktop and narrow widths, confirm the language dropdown is bounded, and the format label/dropdown appears inside the dotted board without horizontal overflow.
- Switch Chinese/English, open the dialog, and select Chinese, US and Brazilian numeric/date formats. Confirm translated controls and formatted text/table amounts still update, with format culture independent from language.
- Copy representative code blocks and confirm copied code is unchanged by surrounding prose layout.

No Computer Use, dependency update, deployment, process termination, Git commit or push occurred. Existing uncommitted refactor changes and append-only history were preserved.