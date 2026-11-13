# Current Flourish project documentation

Read the root instruction router, typed SAF configuration and [effective project constraints](project-constraints.md). Actual controls/styles/public contracts and executable Gallery determine UI facts; these documents are responsibility/reference maps, not another visual system.

| Need | Current entry |
| --- | --- |
| Platform/package layers, resources, runtime state and operations | [Architecture](1_architecture.md) |
| Project graph, declarations, full captured package closure and tools/assets | [Dependency inventory](1_dependency.md) |
| Component responsibilities, host boundaries and manual acceptance | [Component contracts](component-api-organization.md) |
| Essential adapters, module generation, SSR and browser preferences | [Culture integration](culture-web-integration.md) |
| Native WPF resources, controls, lifecycle and Windows acceptance | [WPF integration](wpf-native-integration.md) |
| Gallery/diagnostic selection and restore/build/run | [Local startup](local-startup.md) |
| Six-package release gate, immutable publication and remaining limits | [Package release](nuget-release-integration.md) |
| Dated implementation/decisions | [Changes](1_changelogs/) |
| Diagnoses, fixes and regression evidence | [Bug investigations](1_bugreports/) |
| Superseded designs, full prior snapshots and release receipts | [Archives](1_archived/) |

The source default is stable 1.4.7 with Essential.Culture 1.4.0. Local 1.4.8-preview.14 evidence describes the explicit Colligere consumer candidate and does not change published version/tag identities. Native WPF/WinUI remain outside the six-package Core/Blazor publication set.

DocFX tooling/templates/site deployment are removed under explicit user authorization. The empty docs/.gitkeep marker preserves SAF's structural contract while a replacement documentation framework remains undecided. Gallery and maintained automated tests remain source; generated build/render/acceptance/consumer output is disposable. Human documentation ownership rules continue to apply to future content.

Current guides retain outstanding browser/native/platform/consumer acceptance and unknown metadata. Historical test/deployment/publication counts apply only to their recorded source and environment. A repository push is not a NuGet publication or new visual acceptance.

See the [SAF/DocFX cleanup and validation record](1_changelogs/2026-11-13_151115_migrate-saf-and-retire-docfx.md) and [selected-dropdown regression follow-up](1_bugreports/2026-11-13_selected-dropdown-regression-expectations.md) for the completed maintenance scope.
