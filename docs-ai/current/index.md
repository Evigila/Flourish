# Current project documentation

Use this reading map for active decisions. Current guides describe the implemented source; change records and bug reports preserve the evidence/history behind it. Root AGENTS.md and common standards govern ownership and process.

| Need | Canonical document |
|---|---|
| Repository directory/file responsibilities | [Directory map](currentproject-architecture.md) |
| Controls, styles, public APIs and executable usage | Actual maintained source under `src/Flourish.Blazor/`, Gallery examples and focused tests |
| Component use classification, final API review, access scenarios and convergence boundaries | [Component API organization](component-api-organization.md) |
| Implemented local per-user Culture bridge, usage and rollout limits | [Culture Web integration](culture-web-integration.md) |
| Native WPF four-package reconstruction, usage and manual acceptance | [WPF integration](wpf-native-integration.md) |
| Two-repository ownership, solution entries and migration | [Solution organization](solution-organization.md) |
| Optional Culture bridge registration and lifetimes | [Extension bridge](culture-extension-bridge.md) |
| Package sets, local feed verification and user-confirmed Trusted Publishing | [NuGet release and integration](nuget-release-integration.md) |
| Final local package/test/HTTP results and remaining release configuration | [Local release verification](release-verification.md) |
| Dated implementation/verification evidence | [Append-only change records](currentproject-changelogs/) |
| Defect symptoms, causes, evidence and regression boundaries | [Historical bug reports](bugfix-reports/) |

## Reading and maintenance

For UI work, inspect actual Flourish component/style implementations and executable Gallery examples; they are the authority, not a parallel Markdown token/skin standard. For architecture work, read [common architecture](../common/architecturedesign.md), the directory map and affected project files. Read code/dependency/process standards in common/ when their subject is affected.

Contracts reside in Abstract, processing/rendering in Framework and visual implementation in Design. The convenience package installs all three layers without enabling Design. Package sets, opt-in service registration and release limits belong to the retained runtime/release guides. This does not establish WASM or public package support. Consumers use Flourish controls/styles, retain their business/protocol responsibilities and configure only approved brand/color-role values; missing controls are recorded and reported before custom work.

The user retired the parallel UI standard, implementation/palette guide and visual checklist on 2026-10-05. Their committed versions and deletion-time dirty differences remain recoverable through append-only history, not an archive or active visual authority. Culture Web integration retains runtime/localization scope; package consumption belongs in the release/integration guides. Historical links/counts and README removals remain unchanged.

Keep active material directly in current/. Record new changes in a new timestamped change record and preserve historical bug reports. Do not recreate archive/. Human-maintained DocFX content in docs/ remains under its ownership rules; no README or placeholder document is created outside docs-ai/.
