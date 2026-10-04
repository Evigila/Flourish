# Public Blazor contracts

Package: Arkheide.Flourish.Blazor.Abstract. Target: .NET 10.

Contains public framework/top-bar/navigation/layout builder interfaces, scoped command-parser contracts from Core, optional appearance/theme interfaces and table preference contracts. It references Shared metadata and does not implement runtime state, components or visual design.

Public namespace: ArkheideSystem.Flourish.Blazor.Abstract. Public record and enum namespaces remain stable even when their declaring assembly is Shared.

Implementations stay in Framework or Design. `AddFlourishFramework` configures behavior and shell structure; `AddFlourishDesign` explicitly opts into the visual skin.
