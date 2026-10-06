# Fix navigation paint and document controller lifetime

SecondaryNavigationItem uses the canonical is-selected class; Foundation owns selected interaction paint and Surface's duplicate skin is removed. NavigationSurface keys SectionNavigator/BackToTop with its DocumentKey, replacing their bindings alongside the actual main region and retaining same-document instances.

LineChart.ControlsContent composes business filters beside the library display selector. InlineActions supports direct input plus natural Button inside Field without host geometry. Gallery, catalog, parameter meanings and real rendering/CSS/DOM regressions cover the production capabilities.

Console checks pass 367, selected layout/menu suites pass 28 and directory/top DOM checks pass 26. Gallery builds cleanly in isolated Release output. Framework/Design are repacked locally, not published; Colligere confirms NuGet hashes and current control output. See [the report](../bugfix-reports/2026-10-06_navigation-selection-and-content-lifetime.md) for causes, consumer test limits and manual checks. No version/dependency change, compatibility renderer, runtime stop, browser automation or commit occurs.
