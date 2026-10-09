# Refine Button labels, pointer feedback and independent examples

- Control example now displays Filled, Outlined, Danger, Quiet, Underline and Elevated in text/icon variants and localized icon-only accessible captions. Primary/Secondary remain the existing underlying variant identities.
- Removed visible Quiet hover/press border paint from ordinary, primary-access and chrome contexts; retained pointer fill/ink, transparent border geometry and keyboard focus outlines. Outlined border paint is unchanged.
- Separated Example account selection from Save draft busy/count handling. The selected account has localized status and remains usable during draft saving. Draft reset affects only the save counter. This supersedes the preceding sample's shared SaveAsync action, while preserving its new responsive width bounds.
- Added concise three-language notes to 1.1.4-preview and updated focused Gallery event and CSS cascade regressions. No dependency or package-version change, commit, tag or publication occurred.

Verification: Release build passed with zero warnings/errors; 7,713 Gallery checks, 41 focused Node tests, 22,160 catalog checks, 58 ChangeLog checks and git diff --check passed. Detailed cause, mitigation and manual acceptance are in [the bug report](../bugfix-reports/2026-10-09_button-example-actions-and-quiet-border.md). Await user confirmation before committing the pending ChangeLog and Button changes.
