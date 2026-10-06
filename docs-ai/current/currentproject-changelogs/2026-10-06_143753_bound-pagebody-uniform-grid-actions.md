# Bound PageBody uniform grid actions

Corrected the combined PageBody gutter and full-width rectangle/FormActions sizing in Flourish Design. Direct children now use automatic available width; nested grids and square sizing keep their existing contracts. Added two independent CSS composition regressions and verified the existing Gallery wizard, library contract suite, interaction mocks and Release build.

Framework and Design were repacked locally at 1.1.0 for NuGet-only Colligere verification. No remote package publication or Aspire restart occurred. The user explicitly requested a narrow commit: prior uncommitted refactors are excluded. See [the cause and regression report](../bugfix-reports/2026-10-06_pagebody-uniform-grid-overflow.md).
