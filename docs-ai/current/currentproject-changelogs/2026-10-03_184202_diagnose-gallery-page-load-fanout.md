# Diagnose Gallery page-load resource fan-out

The user reported tens-of-seconds root refresh in Visual Studio, provided a single-page-open console capture and confirmed no subsequent interaction. Diagnosis completed; application code is unchanged and no performance fix is claimed.

The capture contains 120 unique started requests, 119 completions, 110 CSS responses (all 200), 594 Information records and one HTTPS Warning, totaling 1190 lines. CSS request median/P95/max are 529.5313/930.0849/1282.6880 ms; root HTML is 223.8245 ms. Blazor initialization starts after the CSS requests finish. The only 404 is favicon; the retained Blazor connection accounts for the unmatched completion. No repeated root navigation or business request is present.

Source audit confirms runtime imports expose all 110 CSS URLs on every page, including 48 page-specific URLs. The host lacks normal ASP.NET category filtering. Development asset reprocessing adds content/cache cost, confirmed by separate same-build HTTP comparison. Debugger/log-output amplification is a supported candidate but not independently quantified. Summed parallel request durations do not establish the actual page-load elapsed time.

Detailed symptoms, evidence, confirmed causes, secondary component work, prioritized mitigations and a manual acceptance checklist are in [the performance diagnosis](../bugfix-reports/2026-10-03_blazor-page-load-resource-fanout.md). Generated statistics and HTTP evidence remain outside the repository in the task artifact directory. No Computer Use, domain change, new external dependency or Git commit was performed. The user's VS service was not restarted; a temporary isolated diagnostic process is no longer listening.

This supplements the earlier functional extraction acceptance: successful component/render/HTTP suites did not cover cold-page stylesheet fan-out and debugger startup performance. Proposed fixes remain implementation work.
