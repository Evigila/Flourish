# Shared metadata and processing

Package: Arkheide.Flourish.Blazor.Shared. Target: .NET 10.

Contains immutable navigation/appearance metadata, typed control/table metadata and the internal local filter/sort/page processing helper. Existing public namespaces are preserved: ArkheideSystem.Flourish.Blazor.Abstract and ArkheideSystem.Flourish.Blazor.Components.

This project has no Razor components, browser assets, host lifecycle or service registrations. It references the existing Core project to preserve the ApplicationTheme and NotificationSeverity enum identities; Core's registrations are not invoked. This retains the existing transitive Core package dependencies.

Framework and Abstract consume this package transitively. Business authorization, persistence, remote queries and data DTOs belong to the host.
