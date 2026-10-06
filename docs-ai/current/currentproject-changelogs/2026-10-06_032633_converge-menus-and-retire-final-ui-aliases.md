# Complete the unique menu and UI contract convergence

Local timestamp: 2026-10-06 03:26:33, America/Sao_Paulo.

## Follow-up to the compatibility retirement

This append-only record supplements the earlier 02:50 retirement entry. Fix Icon/NavigationItem defaults to the official description name; match both actual primary-navigation production entries to the shared 24px role while retaining ordinary 26px icons. Delete dead table selector arms and IE-only scrollbar adapters.

ActionMenu now has all required generated/native command behavior, so remove RowActionMenu completely rather than retaining its former scenario distinction. Migrate its lifecycle/DOM coverage to actual ActionMenu, adding mode/element-change and honest error-propagation checks. The one current controller rejects unavailable actions, preserves native dispatch ordering, delayed Dialog invoker focus and idempotent cleanup. Native multi-selection disclosures deliberately remain open, without a second controller. Disabled menu links retain noninteractive paint and focus accessibility.

Remove the Surface arbitrary-button parameter and scrollToTop export. Current Patterns use two synchronization arguments; production BackToTop exclusively owns threshold, region scroll, reduced motion, focus and disposal. Update current architecture/API guidance and Gallery/catalog registrations; do not edit historical records or human docs.

## Accepted evidence

Complete release gate: zero-warning/error Release builds, Core 367, Blazor 330/330, bridge 12, 87 controls/705 rows/308 defaults, eleven JavaScript files/62 Node-runner checks, CSS 21/194, launcher 95, six local NuGet candidates and four isolated consumers/95 checks. Final log: artifacts/breaking-ui-release-final-2026-10-06_032000.log. Hashes and suite accounting are in [the audit report](../bugfix-reports/2026-10-06_blazor-compatibility-retirement.md).

Colligere restores the final 7522d254a8bd154b206aae2b810af502 cache; Framework/Design DLLs exactly match this build and its Release output. Its Release build, eight real-package JavaScript suites, 22 cache checks and UI tests pass. Complete consumer .NET results are 2251 pass/2 known order-contract failures/87 skips, not a fully green suite.

Seventy tracked retired source/style/sample/test files are removed or renamed and Git-recoverable. User data, database history, authentication, external dependencies and desktop projects are unchanged. Existing uncommitted launcher/720px/Pricing work is preserved. No Computer Use, public publication, commit/push, deployment or user-process restart occurred. Visual acceptance requires restarting the old Colligere instance and performing the recorded manual checks; nonvisual old-data retirement awaits the user's migration strategy.
