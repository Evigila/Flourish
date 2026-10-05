# Current project documentation

Use this reading map for active decisions. Current guides describe the implemented source; change records and bug reports preserve the evidence/history behind it. Root AGENTS.md and common standards govern ownership and process.

| Need | Canonical document |
|---|---|
| Repository directory/file responsibilities | [Directory map](currentproject-architecture.md) |
| Blazor projects, APIs, consumers and current component behavior | [Implementation and consumer boundaries](blazor-extraction.md) |
| Approved seventeen-role Design palette and color exceptions | [Color contract](blazor-color-roles.md) |
| Browser, keyboard, accessibility, package and host acceptance | [Manual checklist](blazor-manual-tests.md) |
| Future per-user Web/Blazor localization and optional adapter | [Culture Web integration plan](culture-web-integration.md) |
| Dated implementation/verification evidence | [Append-only change records](currentproject-changelogs/) |
| Defect symptoms, causes, evidence and regression boundaries | [Historical bug reports](bugfix-reports/) |

## Reading and maintenance

For product/UI work, read [common UI/UX](../common/uiuxdesign.md), then the implementation guide and color contract. For architecture work, read [common architecture](../common/architecturedesign.md), the directory map and the affected project files. Read code/dependency/process standards in common/ when their subject is affected.

The implementation guide includes approved Blazor exceptions: fixed reading sizes/compact title, Gallery font stack, larger icons/checkboxes/ring, application palette and navigation behavior. They do not silently redefine the portable common rules. The current Shared/Abstract migration and Culture bridge are future recommendations, not completed functionality or authorization to add dependencies.

The former blazor-render-review.md and blazor-project-boundaries.md were merged into the implementation/manual guides. culture-web-integration.md consolidates and replaces culture-web-readiness.md. Older historical links and counts remain traceable in their original records; new decisions use these canonical destinations. Historical README/package-instruction references predate the explicit README removal; current package consumption belongs in the implementation/manual guides.

Keep active material directly in current/. Record new changes in a new timestamped change record and preserve historical bug reports. Do not recreate archive/. Human-maintained DocFX content in docs/ remains under its ownership rules; no README or placeholder document is created outside docs-ai/.
