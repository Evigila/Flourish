using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Configures the optional multi-project shell behavior.</summary>
public interface IProjectBuilder
{
    /// <summary>Enables or disables project-aware shell behavior.</summary>
    IProjectBuilder SetMultiProjectEnabled(bool enabled = true);
}

/// <summary>Coordinates the user-facing project lifecycle used by an application shell.</summary>
/// <remarks>
/// Platform packages can provide the default dialog and file-picker implementation. Applications
/// can replace the service when project creation, persistence, activation, deletion, or close
/// behavior is application-owned.
/// </remarks>
public interface IProjectBehavior
{
    /// <summary>Creates and activates a project.</summary>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>
    /// <see langword="true" /> when the project was created; otherwise,
    /// <see langword="false" /> when the operation was canceled.
    /// </returns>
    ValueTask<bool> CreateProjectAsync(CancellationToken cancellationToken = default);

    /// <summary>Saves the active project when it requires framework-managed persistence.</summary>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>
    /// <see langword="true" /> when saving completed or no framework-managed save was required;
    /// otherwise, <see langword="false" /> when the operation was canceled or no project is active.
    /// </returns>
    ValueTask<bool> SaveActiveProjectAsync(CancellationToken cancellationToken = default);

    /// <summary>Activates a project after resolving any unsaved active project.</summary>
    /// <param name="projectId">The case-sensitive project ID to activate.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>
    /// <see langword="true" /> when the project was activated; otherwise,
    /// <see langword="false" /> when the operation was canceled or the project no longer exists.
    /// </returns>
    ValueTask<bool> ActivateProjectAsync(
        string projectId,
        CancellationToken cancellationToken = default
    );

    /// <summary>Deletes a project after confirmation.</summary>
    /// <param name="projectId">The case-sensitive project ID to delete.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>
    /// <see langword="true" /> when the project was deleted; otherwise,
    /// <see langword="false" /> when the operation was canceled or the project no longer exists.
    /// </returns>
    ValueTask<bool> DeleteProjectAsync(
        string projectId,
        CancellationToken cancellationToken = default
    );

    /// <summary>Determines whether the active project allows the shell to close.</summary>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>
    /// <see langword="true" /> when closing may continue; otherwise,
    /// <see langword="false" /> when the user canceled the required save.
    /// </returns>
    ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken = default);
}

/// <summary>Manages the project catalog displayed by an application shell.</summary>
/// <remarks>
/// A configured catalog stores projects whose local storage files exist, together with their active
/// project ID. Unpersisted projects remain process-local, and stale mappings are removed when the
/// catalog is loaded. Catalog mutations use atomic file replacement. Project-file creation, saving,
/// activation policy, deletion, and close checks are coordinated separately by
/// <see cref="IProjectBehavior" />.
/// </remarks>
public interface IProjectService
{
    /// <summary>Occurs after project metadata, selection, or multi-project mode changes.</summary>
    event EventHandler<ProjectCatalogChangedEventArgs>? Changed;

    /// <summary>Occurs when a project selector requests creation of a project.</summary>
    event EventHandler<ProjectCreationRequestedEventArgs>? NewProjectRequested;

    /// <summary>Occurs when a project selector requests activation of a project.</summary>
    event EventHandler<ProjectActivationRequestedEventArgs>? ProjectActivationRequested;

    /// <summary>Gets an immutable snapshot of the current project display state.</summary>
    ProjectCatalogSnapshot Current { get; }

    /// <summary>Appends a project identity to the project catalog.</summary>
    /// <param name="project">The project metadata to append.</param>
    /// <param name="activate">Whether the appended project becomes active.</param>
    /// <exception cref="ArgumentNullException"><paramref name="project" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">The project ID or name is empty or whitespace.</exception>
    /// <exception cref="InvalidOperationException">A project with the same case-sensitive ID already exists.</exception>
    void AddProject(ProjectDescriptor project, bool activate = true);

    /// <summary>Adds a project or replaces the project with the same case-sensitive ID.</summary>
    /// <param name="project">The project metadata to add or replace.</param>
    /// <param name="activate">Whether the project becomes active.</param>
    /// <exception cref="ArgumentNullException"><paramref name="project" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">The project ID or name is empty or whitespace.</exception>
    void SetProject(ProjectDescriptor project, bool activate = true);

    /// <summary>Changes the display metadata for an existing project.</summary>
    /// <param name="projectId">The case-sensitive project ID.</param>
    /// <param name="name">The non-empty project display name.</param>
    /// <param name="storagePath">
    /// The optional local storage path represented by the project. A missing path identifies an
    /// unpersisted project.
    /// </param>
    /// <exception cref="ArgumentException"><paramref name="projectId" /> or <paramref name="name" /> is empty or whitespace.</exception>
    /// <exception cref="KeyNotFoundException">The project is not registered.</exception>
    void SetProjectMetadata(string projectId, string name, string? storagePath = null);

    /// <summary>Changes the active project, or clears it when <paramref name="projectId" /> is empty.</summary>
    /// <param name="projectId">The case-sensitive project ID, or <see langword="null" /> or whitespace to clear the selection.</param>
    /// <exception cref="KeyNotFoundException">The project is not registered.</exception>
    void SetActiveProject(string? projectId);

    /// <summary>Removes a project. Removing the active project clears the active selection.</summary>
    /// <param name="projectId">The case-sensitive project ID.</param>
    /// <returns><see langword="true" /> when a project was removed.</returns>
    /// <exception cref="ArgumentException"><paramref name="projectId" /> is empty or whitespace.</exception>
    bool RemoveProject(string projectId);

    /// <summary>Gets a project by its case-sensitive ID.</summary>
    /// <param name="projectId">The case-sensitive project ID.</param>
    /// <returns>The matching project, or <see langword="null" /> when it is not registered.</returns>
    /// <exception cref="ArgumentException"><paramref name="projectId" /> is empty or whitespace.</exception>
    ProjectDescriptor? GetProject(string projectId);

    /// <summary>Enables or disables project-aware display at runtime.</summary>
    /// <param name="enabled">Whether the shell uses the active project identity.</param>
    void SetMultiProjectEnabled(bool enabled);
}

/// <summary>Describes one application project represented by a shell.</summary>
public sealed record ProjectDescriptor
{
    /// <summary>Creates project display metadata.</summary>
    /// <param name="id">The stable, case-sensitive project ID.</param>
    /// <param name="name">The project display name.</param>
    /// <param name="storagePath">
    /// The optional local storage path represented by the project. A missing path identifies an
    /// unpersisted project.
    /// </param>
    public ProjectDescriptor(string id, string name, string? storagePath = null)
    {
        Id = id;
        Name = name;
        StoragePath = storagePath;
    }

    /// <summary>Gets the stable, case-sensitive project ID.</summary>
    public string Id { get; init; }

    /// <summary>Gets the project display name.</summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the local storage path represented by this project, or <see langword="null" /> when the
    /// project has not been persisted.
    /// </summary>
    public string? StoragePath { get; init; }
}

/// <summary>Represents the current project identities and selection.</summary>
/// <param name="Projects">The registered projects in insertion order.</param>
/// <param name="ActiveProject">The active project, or <see langword="null" /> when none is selected.</param>
/// <param name="IsMultiProjectEnabled">Whether the shell uses project-aware display semantics.</param>
/// <param name="Version">The monotonically increasing project-state version.</param>
public sealed record ProjectCatalogSnapshot(
    IReadOnlyList<ProjectDescriptor> Projects,
    ProjectDescriptor? ActiveProject,
    bool IsMultiProjectEnabled,
    long Version
);

/// <summary>Provides data after the project display state changes.</summary>
public sealed class ProjectCatalogChangedEventArgs : EventArgs
{
    /// <summary>Initializes project catalog change data.</summary>
    public ProjectCatalogChangedEventArgs(
        ProjectCatalogSnapshot current,
        CollectionChangeKind changeKind,
        string? projectId,
        bool activeProjectChanged
    )
    {
        Current = current;
        ChangeKind = changeKind;
        ProjectId = projectId;
        ActiveProjectChanged = activeProjectChanged;
    }

    /// <summary>Gets the state after the change.</summary>
    public ProjectCatalogSnapshot Current { get; }

    /// <summary>Gets the mutation kind.</summary>
    public CollectionChangeKind ChangeKind { get; }

    /// <summary>Gets the affected project ID, if applicable.</summary>
    public string? ProjectId { get; }

    /// <summary>Gets whether the active project identity or displayed metadata changed.</summary>
    public bool ActiveProjectChanged { get; }
}

/// <summary>Provides project state when a project selector requests a new project.</summary>
public sealed class ProjectCreationRequestedEventArgs : EventArgs
{
    /// <summary>Initializes project creation request data.</summary>
    public ProjectCreationRequestedEventArgs(ProjectCatalogSnapshot current)
    {
        Current = current;
    }

    /// <summary>Gets the project state at the time of the request.</summary>
    public ProjectCatalogSnapshot Current { get; }
}

/// <summary>Provides the requested project and current state for an activation request.</summary>
public sealed class ProjectActivationRequestedEventArgs : EventArgs
{
    /// <summary>Initializes project activation request data.</summary>
    public ProjectActivationRequestedEventArgs(
        ProjectDescriptor project,
        ProjectCatalogSnapshot current
    )
    {
        Project = project;
        Current = current;
    }

    /// <summary>Gets the project selected by the user.</summary>
    public ProjectDescriptor Project { get; }

    /// <summary>Gets the project state at the time of the request.</summary>
    public ProjectCatalogSnapshot Current { get; }
}
