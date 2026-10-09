# Keep preference-save failures recoverable

Follow-up to the same task's 191144 culture-persistence record. Review identified that propagating a storage exception through an actual Interactive Server event could terminate the circuit. LanguagePicker and the Localization format handler now catch browser/disconnection/cancellation failures, preserve the previous language/format pair and display the existing Dialog/Notice warning. A successful retry clears the failure. Added complete three-language messages and real control-event regressions for failure rendering and continued interaction; no alternate controls or persistence core were introduced.

Final Gallery Release build: zero warnings/errors. Gallery executable suite: 8,202 checks, including 469 post-event and 223 ChangeLog checks. Culture catalogs: 22,096. ChangeLog: 88. Blazor 443/443, Node preference 4/4 and actual loopback HTTP 8 remain the verified storage baseline. Diff whitespace is clean. No Computer Use, dependency/version change, Git commit, tag or publication.

The earlier same-task verification counts remain historical evidence. See [the report's follow-up](../bugfix-reports/2026-10-09_culture-preference-persistence.md#same-task-ui-failure-follow-up) for the corrected UI boundary and manual acceptance checklist.
