---
title: Projects
description: Manage project identities, catalog persistence, title-bar selection, and replaceable project lifecycle behavior.
---

# Projects

Project support gives the title bar a changing application-view identity, such as a solution, workspace, or document set. `IProjectService` owns the ordered project catalog and active selection. `IProjectBehavior` owns the user-facing create, save, activate, delete, and close workflow and can be replaced by the application.

These APIs provide Shell state and a default placeholder-file workflow. Activating a project updates the active metadata and title; it does not load, unload, or switch application business content.

## Enable project mode

Enable the title bar and project mode, then configure the text used to display an unpersisted project:

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

`UseMultiProject()` defaults to `true` when called and is omitted by default. Without project mode, the title selector displays and lists only the application title; Flourish does not expose project-title, project-save, or project-close semantics. With project mode, the selector displays the active project name, or the unnamed-project placeholder for an unpersisted or missing selection, and lists every project plus **New project**.

## Project metadata and persistence

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
                @"C:\Work\Reports.txt"));

        projects.UpsertProject(
            new FlourishProject("samples", "Samples", @"C:\Work\Samples.txt"),
            activate: false);
    }
}
```

`StoragePath == null` means that a project is unpersisted. Unpersisted projects may exist in the current process, but they are excluded from `projects.json`. The unnamed-project placeholder is display text only; do not compare a project name with the placeholder to determine persistence. Names are not required to be unique and the placeholder can be changed or localized.

Flourish loads the ordered catalog and active project ID from `projects.json`. The file is stored beside `IAppSettingsStore.FilePath`, normally beside the base `appsettings.json`. Only mappings whose `StoragePath` refers to an existing local file are retained. On startup, entries with an empty path or a missing target file are removed and the repaired catalog is written back atomically. Every catalog mutation through `IProjectService` also rewrites the valid mappings atomically. If the write fails, the in-memory mutation is rolled back and the change event is not published.

Catalog persistence belongs to `IProjectService` and remains active when the application replaces `IProjectBehavior`. The catalog stores metadata only; `IProjectService` checks whether the represented file exists but does not read or write its contents.

When no persisted catalog entries exist, Flourish creates and activates one process-local, unpersisted project and displays it with the configured placeholder. That temporary identity receives a new ID after an application restart unless it is saved to a local file first.

## Runtime catalog operations

`IProjectService.Current` returns an immutable `FlourishProjectSnapshot` containing the ordered projects, active project, project-mode state, and version.

| Operation | Behavior |
| --- | --- |
| `AddProject(project, activate)` | Adds unique metadata and optionally makes it active. |
| `UpsertProject(project, activate)` | Adds or replaces metadata by ID. |
| `SetProjectMetadata(id, name, storagePath)` | Changes the name and optional path of an existing project. |
| `SetActiveProject(id)` | Changes only the active Shell identity; pass `null` to clear it. |
| `RemoveProject(id)` | Removes only the catalog entry. Removing the active project clears the selection. |
| `TryGetProject(id, out project)` | Queries one registered project. |
| `SetMultiProjectEnabled(enabled)` | Changes project-aware title-bar behavior at runtime. |

Observe `Changed` after metadata, active selection, or mode changes. The event identifies the mutation, affected project, and whether the active project changed. Direct `SetActiveProject` and `RemoveProject` calls do not run lifecycle prompts or touch project files; use `IProjectBehavior` when the user operation requires those behaviors.

## Default project behavior

Flourish registers a default `IProjectBehavior` when the application does not provide one. It manages `.txt` placeholder files and Shell metadata, not application document data.

| User operation | Default behavior |
| --- | --- |
| **New project** | Resolves an unpersisted active project first, then opens a Save dialog with `NewProject` as the suggested file name, creates a `.txt` placeholder when the selected file does not exist, adds its metadata, and activates it. Canceling either step prevents creation. |
| Ctrl+S | In project mode, saves the active project metadata. An unpersisted project, or one whose mapped file was deleted, opens the Save dialog with `NewProject` as the suggested file name, then receives the selected file name and path. An existing selected file is mapped without changing its contents, and saving a project whose mapped file still exists is a no-op. Outside project mode Flourish does not handle Ctrl+S, so the application owns its save semantics. The built-in command and shortcut use low priority so application registrations can take precedence. |
| Select another project | If the active project is unpersisted, offers only **Save** or **Cancel**. Activation continues only after saving succeeds. |
| Close the application | In project mode, an unpersisted active project offers **Save**, **Don't save**, and **Cancel** before an actual close. **Save** must complete, **Don't save** allows closing without a project file, and **Cancel** prevents the close. Outside project mode no project-save prompt is shown. |
| Right-click a project | Requests confirmation, removes the catalog entry, and deletes a managed `.txt` file when no other project references the same path. A catalog-write failure restores the isolated file and leaves the catalog unchanged. |

The default behavior only treats a path whose extension is `.txt` as a managed file. It never deletes a different file type. A direct `IProjectService.RemoveProject` call removes metadata only and does not show confirmation.

The placeholder files do not contain application data. An application that needs to serialize documents, open storage, or coordinate a domain workspace should replace the lifecycle behavior.

## Replace project behavior

Register one singleton `IProjectBehavior` through `ConfigureServices`. Flourish supplies its default only when no application registration exists.

```csharp
builder.ConfigureServices((_, services) =>
    services.AddSingleton<IProjectBehavior, WorkspaceProjectBehavior>());
```

While project mode is enabled, the Shell routes lifecycle entry points to five asynchronous methods:

| Method | Shell entry point |
| --- | --- |
| `CreateProjectAsync` | **New project** in the dropdown. |
| `SaveActiveProjectAsync` | The built-in Ctrl+S command. |
| `ActivateProjectAsync` | Selecting another project. |
| `DeleteProjectAsync` | Right-click deletion. |
| `CanCloseAsync` | The project close guard. |

Each method returns `true` when the requested operation may continue and `false` when it is canceled or cannot be completed. A replacement owns its dialogs and project-file lifecycle. It should use `IProjectService` to publish metadata and active-selection changes; those catalog mutations continue to be written atomically to `projects.json` by Flourish.

## Related features

- [Title bar](configure-title-bar.md) explains application identity, Logo details, and project-dropdown behavior.
- [Runtime APIs](runtime-apis.md) summarizes the complete runtime service surface.
- [Dependency injection](configure-services.md) explains how to replace `IProjectBehavior`.
- [Application data](configure-data.md) explains the shared appsettings location used by the project catalog.
