# Converge uniform-grid action variants

- Replaced the Blazor Filled/Outlined grid appearances with FilledElevated/Elevated; only Elevated, FilledElevated and Danger remain, without aliases. Elevated remains the grid/standalone default.
- FilledElevated combines the former Filled primary color roles and Elevated shadow. Removed all variant hover/press border paint while preserving keyboard focus outlines, unavailable feedback and structural dimensions.
- Updated shared grid/item/button render mappings, FormActions inherited defaults, NavigationChoices selected appearance, Gallery callers and copied code. Primary actions now explicitly use FilledElevated with Elevated secondary actions. Preview tiles contain only the three current appearances and retained busy/unavailable demonstrations.
- Updated the active component API guide, three-language recommendations and accessible labels. Added these source changes to 1.1.4-preview; no package version, dependency, Git tag or publication changed.
- Updated C# rendering/contract tests, Gallery preview checks and complete-import CSS cascade regressions. The preceding structured-account source test now checks SelectAccount rather than the removed shared SaveAsync callback.

Verification: zero-warning/error Release builds; 427/427 Blazor checks; 7,745 Gallery checks; 37 focused Node tests; 22,135 catalog and 61 ChangeLog checks; git diff --check. See [the diagnosis and manual checklist](../bugfix-reports/2026-10-09_uniform-grid-variants-and-pointer-borders.md). Earlier recorded Filled/Outlined decisions are superseded for Blazor by this explicit user request; append-only history and the independent WPF implementation remain preserved. No Computer Use or Git commit was performed.
