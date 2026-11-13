# Place the Tutorial example in DisplayBoard

Wrapped the existing TutorialBoardSample in the standard DisplayBoard on the `/examples/tutorial` page. The Section heading remains outside the board; the executable controls, description and feedback use the shared background surface. The shared sample itself remains unchanged, because ComponentGuide already supplies its own boards. No custom background, nested board in the control guide, new renderer or interaction handler was introduced.

Source review confirms the direct InlineActions row retains the available board width and wrapping. TutorialBoard's preview and dialog use native Popover top-layer presentation, so the background board's scrolling viewport does not clip them. Browser visual and interaction verification remains manual; no Computer Use was performed.

Added a concise three-language 1.1.4-preview release note. Gallery Release build passed with zero warnings/errors; all 7,836 existing Gallery checks passed, including TutorialExample rendering across three cultures and 215 ChangeLog checks. Culture integrity passed 22,083 checks, ChangeLog validation passed 82 checks, and diff whitespace is clean.

Manual acceptance: verify the Tutorial controls appear on the standard dotted background in light/dark themes; hover/focus the tutorial trigger and open the panel to check visibility and keyboard return; exercise step selection, shortcuts, completion verification, skip and reset; narrow the viewport and confirm the action row wraps without overflow.
