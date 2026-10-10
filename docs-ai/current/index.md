# Current project documentation

Use [AGENTS.md](../../AGENTS.md) for the task router and [rules.md](../common/rules.md) for execution, ownership, audit and delivery requirements. Current guides describe implemented source; dated records preserve evidence and superseded decisions.

| Need | Canonical document |
| --- | --- |
| Complete repository tree, modules and runtime boundaries | [Architecture](1_architecture.md) |
| Declared/resolved packages, SDKs, tools, services and assets | [Dependencies](1_dependency.md) |
| Controls, styles, public APIs and executable usage | Actual source under src/Flourish.Blazor, native WPF source, Gallery examples and focused tests |
| Component classification, production scenarios and convergence boundaries | [Component API organization](component-api-organization.md) |
| Web Culture registration, modules and browser preferences | [Culture Web integration](culture-web-integration.md) |
| Optional bridge ownership and lifetimes | [Culture bridges](culture-extension-bridge.md) |
| Native WPF package layers and manual acceptance | [WPF integration](wpf-native-integration.md) |
| Repository/solution ownership | [Solution organization](solution-organization.md) |
| Package verification and Trusted Publishing | [NuGet release and integration](nuget-release-integration.md) |
| Dated package/test/HTTP results | [Release verification](release-verification.md) |
| Command-line application startup | [Local startup](local-startup.md) |
| New implementation/decision records | [Append-only changes](1_changelogs/) |
| New diagnoses and regression evidence | [Bug reports](1_bugreports/) |
| Superseded or abandoned design material | [Archived designs](1_archived/) |

## Reading and maintenance

For UI work, read [UIUX.md](../common/UIUX.md) and actual Flourish components/styles/contracts. Runtime/localization/package guides are technical references, not a competing visual specification. Hosts retain business/protocol responsibilities and supported brand/color configuration; library controls own their presentation and interaction.

Keep current project facts synchronized in 1_architecture.md and 1_dependency.md. Place other active guides directly in current/. Use the named 1_ history directories for new records and mark archived designs with status, reason, affected scope and replacement.

The existing [legacy change records](currentproject-changelogs/) and [legacy bug reports](bugfix-reports/) remain at their original paths to preserve append-only content and links. New records use the router's current paths. Earlier statements prohibiting the former archive directory are historical; the current contract requires 1_archived/.

Human-maintained material in docs/ remains under its ownership boundary. Preserve it and report stale content rather than editing it as part of this audit.
