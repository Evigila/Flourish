# Local launcher and shared presentation defaults

## Scope and commit boundary

The previously completed UI/control baseline was committed first as `1d7f941` (`Unify Workspace controls and presentation scenarios`). Its Colligere consumer baseline is `7bf1f09`. This record covers subsequent changes, which remain uncommitted pending a separate user answer. Neither commit was pushed, and no public NuGet release or deployment occurred.

## Changes

Root `start.bat` delegates to `scripts/Start-Project.ps1` through built-in Windows PowerShell. `StartSettings.json` defines four actual runnable projects: the default Blazor Gallery, existing WPF Gallery, existing WinUI3 placeholder and Framework-only Native diagnostic Web host. `Start-Common.ps1` validates selections and composes argument arrays for restore, Release build and no-build run. Arbitrary paths/library projects are not selections. Help/list work without an SDK; non-interactive use requires an explicit project. Native failures stop later steps and preserve the exit code. The script honors `global.json` from its own root, including when called from another directory or a path containing spaces.

No VS UI, SDK/workload installation, cache purge, package publication, browser opening or unrelated-process termination is performed. Web selections retain their existing HTTP profiles. WPF/WinUI3 require Windows, and the latter retains its existing Windows/XAML prerequisites and placeholder status. Desktop execution is not certified. `Test-StartProject.ps1` checks plans and CLI failure/discovery cases without building or opening windows, and is now included in the existing Release gate. See [local startup](../local-startup.md).

PresentationBand now owns a single internal 720px default minimum, and PresentationHero references that value. Framework normal/full-height CSS fallbacks, component usage descriptions, reflected Gallery metadata, parameter explanations and samples use 720px consistently. The inspected baseline was 450px, not 400px; older dated records remain unchanged. Explicit consumer values such as 450px remain valid. Zero permits natural height; negative values still fail. FullHeight retains `max(100dvh, MinHeight)` and natural content growth, rather than a fixed/clipping 720px height. Dots remain opt-in. Product/Pricing consumers without overrides and access Banner composition inherit the same default.

Pricing's personalized action already declared Elevated, but the generic unavailable-button selector cleared its surface and shadow. Design now preserves those two properties for disabled/aria-disabled Elevated actions, without changing disabled text/border/cursor, callback guards or available hover/press paint. Other disabled variants remain unchanged. New CSS cascade checks cover specificity, source order, both disabled forms and hover/active combinations; two C# cases check actual submit/link SSR, Busy/Disabled, reserved attributes, no callbacks/navigation and re-enabled actions. See [the defect report](../bugfix-reports/2026-10-06_unavailable-elevated-presentation.md).

The display Pricing Gallery retains its personalized-license title and adds localized support copy in en-US/zh-CN. Its real Elevated action remains disabled and fictional. Colligere uses its own scoped Portuguese copy and the same library entry; no contact, checkout or license authority is invented.

## Verification

- `scripts/Test-Release.ps1` exits zero: Release build has zero warnings/errors; Core 367, Blazor 303/303 and Culture bridge 12 pass. Evidence: `artifacts/launcher-banner-release-accepted.log`.
- All ten configured JavaScript files pass, including 54 Node-runner checks. CSS bundle 21 and SDK asset integration 194 pass. Catalog audit remains 101 components, 799 parameter rows and 355 documented defaults.
- Windows PowerShell 5.1 and PowerShell 7 each pass 95 launcher checks. These are command-plan/CLI checks, not 95 actual application startups. The Release gate also executes the launcher suite.
- Six local 1.1.0 candidate packages pass dependency/asset verification; isolated FrameworkOnly, MetaNative, MetaDesign and MetaCulture consumers pass 95 checks. Evidence: `artifacts/package-consumers/de0db99a2d6042c590273ddf798cda41`. Existing package README advisories remain; no upload occurred.
- Framework archive SHA256: `65D0EE46B8F77E774B5A7F80532B56ABA7D2E086CF623FD6C31CF2BE1CA02327`; Design: `3FDE17CA55B9A3746FB58CA1CF5C829F31CE076CCF5D4B01A6456F12AF959E68`.
- The actual root batch starts Blazor and Native in Release from the sibling Colligere directory using SDK 10.0.400. Both build with zero warnings/errors and listen on their declared ports. Ten final Gallery HTTP checks pass for product/pricing/login, 720 defaults, viewport/dots composition, localized support copy, unavailable Elevated action and served Framework/Design rules. Two Native HTTP checks pass for its Framework-only root and actual fingerprinted stylesheet. Both task-owned instances were stopped; the user's Colligere instance was not touched.
- Colligere restores the actual local candidate set into content-addressed cache `27256c4e00b0f41a9827848e961baaebe`, verifies archive hashes and builds Release with zero warnings/errors. Full final tests: 2245 passed, two already-recorded order-contract failures, 87 database skips. Pricing/native-rendering/localization regressions pass; the application suite is not completely green.

Initial new CTA assertions incorrectly required child text to touch its tags; Button's template preserves whitespace. Assertions now allow surrounding whitespace while checking the complete label, real native disabled/type, Elevated class and correct section. Initial HTTP probes likewise used a generic access route and a fixed Native asset filename; final probes use the actual login route/class and manifest-fingerprinted URL. These were verification assumptions, not production defects or reasons to alter business behavior.

## Manual acceptance and remaining boundaries

Test the menu, Enter/default, explicit aliases, invalid selections, occupied ports and Ctrl+C. WPF/WinUI3 window startup/rendering and installed desktop prerequisites require separate user acceptance; no desktop window or Computer Use was invoked.

In Gallery and the restarted consuming host, check Light/Dark, narrow widths and 200 percent zoom. Confirm each ordinary banner has a 720px minimum while long content grows, login remains viewport-aware/dotted, and centered content is independent of full-width backgrounds. Confirm the personalized card displays its title/support copy and Elevated surface/shadow while remaining keyboard/pointer unavailable. Other disabled variants and enabled hover/press states must remain unchanged.

Essential source, SDK/dependency declarations, desktop source, authorization, business rules, native authentication protocols and databases are unchanged. All tests/builds are Release-only. Generated local package/cache outputs are not staged. Updated documentation records implementation contracts rather than creating a host UI standard.
