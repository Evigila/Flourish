---
title: Projects
description: Manage the project identities, active selection, and title-bar requests displayed by a multi-project Flourish application.
---

# Projects

Project support gives the title bar a changing application-view identity, such as a solution, workspace, or document set. Flourish manages only in-memory display metadata and UI requests. The application remains responsible for creating, opening, saving, and deleting its data.

## Enable project mode

Enable the title bar and project mode, then configure the empty-selection text:

```csharp
builder
    .ConfigureShell(shell =>
        shell.UseTitleBar().UseMultiProject())
    .ConfigureTitleBar(titleBar =>
        titleBar
            .SetApplicationTitle("Foobar")
            .SetUnnamedProjectPlaceholder("Unnamed project")
            .SetLogo(showProjectTitle: true));
```

`UseMultiProject()` defaults to `true` when called and is omitted by default. Without project mode, the title button uses the application title and its menu contains only that title. With project mode, it uses the active project name or the unnamed-project placeholder.

## Project metadata

Resolve the singleton `IProjectService` through dependency injection. Each `FlourishProject` has a stable, case-sensitive ID, a display name, and an optional local storage path.

```csharp
public sealed class WorkspaceCatalog(IProjectService projects)
{
    public void Register()
    {
        projects.AddProject(
            new FlourishProject(
                "reports",
                "Reports",
                @"C:\Work\Reports"));

        projects.UpsertProject(
            new FlourishProject("samples", "Samples"),
            activate: false);
    }
}
```

The storage path is descriptive metadata. Flourish trims it but does not require it to exist and never reads, creates, moves, or deletes that location.

## Runtime operations

`IProjectService.Current` returns an immutable `FlourishProjectSnapshot` containing the ordered projects, active project, project-mode state, and version.

| Operation | Behavior |
| --- | --- |
| `AddProject(project, activate)` | Adds unique metadata and optionally makes it active. |
| `UpsertProject(project, activate)` | Adds or replaces metadata by ID. |
| `SetProjectMetadata(id, name, storagePath)` | Changes the name and optional path of an existing project. |
| `SetActiveProject(id)` | Changes the active identity; pass `null` to clear it. |
| `RemoveProject(id)` | Removes only the shell metadata. Removing the active project clears the selection. |
| `TryGetProject(id, out project)` | Queries one registered project. |
| `SetMultiProjectEnabled(enabled)` | Changes project-aware title-bar behavior at runtime. |

Observe `Changed` after metadata, active selection, or mode changes. The event identifies the mutation, affected project, and whether the active project changed.

## Handle title-bar requests

Selecting a project or **New project** in the title menu does not perform application work. The service raises a request event instead. Complete the business operation first, then update Flourish state:

```csharp
public sealed class ProjectRequests : IDisposable
{
    private readonly IProjectService projects;

    public ProjectRequests(IProjectService projects)
    {
        this.projects = projects;
        projects.ProjectActivationRequested += OnActivationRequested;
        projects.NewProjectRequested += OnNewProjectRequested;
    }

    private void OnActivationRequested(
        object? sender,
        FlourishProjectActivationRequestedEventArgs args)
    {
        OpenWorkspace(args.Project);
        projects.SetActiveProject(args.Project.Id);
    }

    private void OnNewProjectRequested(
        object? sender,
        FlourishNewProjectRequestedEventArgs args)
    {
        var project = CreateWorkspace();
        projects.AddProject(project);
    }

    public void Dispose()
    {
        projects.ProjectActivationRequested -= OnActivationRequested;
        projects.NewProjectRequested -= OnNewProjectRequested;
    }
}
```

Request events run on the thread that handled the title-bar interaction. Applications can defer asynchronous work and marshal their own UI updates as needed.

## Related features

- [Title bar](configure-title-bar.md) explains application identity, Logo details, and title-menu behavior.
- [Runtime APIs](runtime-apis.md) summarizes the complete runtime service surface.
- [Dependency injection](configure-services.md) explains service resolution and application registrations.
