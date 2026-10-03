# Public Blazor contracts

Package: Arkheide.Flourish.Blazor.Abstract. Target: .NET 10.

Contains public shell/title/navigation/layout builder interfaces, optional appearance builder/service interfaces and table preference contracts. It references Shared metadata and does not implement runtime state, components or visual design.

Public namespace: ArkheideSystem.Flourish.Blazor.Abstract. Public record and enum namespaces remain stable even when their declaring assembly is Shared.

Implementations stay in Framework or Design. Appearance configuration is explicitly registered through AddFlourishDesign; the native application builder configures behavior and layout.
