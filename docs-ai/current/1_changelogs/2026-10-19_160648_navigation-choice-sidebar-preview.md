# Native choice sidebar and local portal preview

Recorded 2026-10-19 16:06:48, America/Sao_Paulo (UTC-03:00). Source version: 1.4.3-preview.portals.1. Public 1.4.2 is not replaced or republished.

NavigationChoices now selects Grid, Compact or Sidebar through one NavigationChoicesLayout enum; the old Compact boolean is removed. Sidebar composes actual Button/Icon entries with native same-site GET links, optional icons and a current-item check. Its narrow navigation and details stack below 760px. Framework owns geometry and inactive-panel suppression; Design owns semantic surfaces. Keyed host panels remain mounted and preserve native forms, with only the active panel participating in layout. Authentication and POST behavior remain host-owned.

The Gallery replaces its organization Hero and three-method/Pass scene with two generic platform/organization methods. Shared Hero, AccessPanel and AccessFormSurface controls remain for other scenarios. Three-language sample, API, usage and preview release notes now describe the new contract. The architecture tree adds one Abstract enum and no external dependency is added.

Renderer regressions were adapted to Layout and extended for actual Button/Icon instances, current-state changes, optional indicators, disabled GET links, panel naming and retained native forms. Loopback Gallery checks now reject the retired Hero/Pass scene and validate both routes. Source inspection and required-path/version routing were completed; the recent matching audit allows SKIP_AUDIT. No build, pack, test run, Computer Use, commit, push or publication was performed by this bounded change. Coordinated verification and packaging remain with the parent task.

Manual acceptance after rebuilding:
1. Inspect long translated labels in both modes; desktop shows the narrow list beside one details panel and narrow screens stack naturally without horizontal overflow.
2. Switch both method links using normal activation, keyboard and a new tab; verify current-page state and decorative check, original GET addresses and unchanged native forms.
3. Exercise an unavailable choice and a hidden indicator; verify disabled activation and accessible current state remain correct.
4. Confirm the Gallery no longer shows the organization Hero, three-way choice or Pass form, while other shared access/marketing samples remain available.
