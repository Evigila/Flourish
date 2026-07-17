namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Manages the in-memory project identities displayed by the Flourish shell.
/// </summary>
/// <remarks>
/// Flourish does not open, save, or close application data. Applications use this service to keep
/// the shell display synchronized with their own project lifecycle.
/// </remarks>
public interface IProjectService
{
    /// <summary>Occurs after project metadata, selection, or multi-project mode changes.</summary>
    event EventHandler<FlourishProjectsChangedEventArgs>? Changed;

    /// <summary>Occurs when the title-bar project menu requests creation of a project.</summary>
    event EventHandler<FlourishNewProjectRequestedEventArgs>? NewProjectRequested;

    /// <summary>Occurs when the title-bar project menu requests activation of a project.</summary>
    event EventHandler<FlourishProjectActivationRequestedEventArgs>? ProjectActivationRequested;

    /// <summary>Gets an immutable snapshot of the current project display state.</summary>
    FlourishProjectSnapshot Current { get; }

    /// <summary>Adds a project identity.</summary>
    /// <param name="project">The project metadata to add.</param>
    /// <param name="activate">Whether the added project becomes active.</param>
    /// <exception cref="ArgumentNullException"><paramref name="project" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">The project ID or name is empty or whitespace.</exception>
    /// <exception cref="InvalidOperationException">A project with the same case-sensitive ID already exists.</exception>
    void AddProject(FlourishProject project, bool activate = true);

    /// <summary>Adds a project or replaces the project with the same case-sensitive ID.</summary>
    /// <param name="project">The project metadata to add or replace.</param>
    /// <param name="activate">Whether the project becomes active.</param>
    /// <exception cref="ArgumentNullException"><paramref name="project" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">The project ID or name is empty or whitespace.</exception>
    void UpsertProject(FlourishProject project, bool activate = true);

    /// <summary>Changes the display metadata for an existing project.</summary>
    /// <param name="projectId">The case-sensitive project ID.</param>
    /// <param name="name">The non-empty project display name.</param>
    /// <param name="storagePath">The optional local storage path represented by the project.</param>
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

    /// <summary>Tries to get a project by its case-sensitive ID.</summary>
    /// <param name="projectId">The case-sensitive project ID.</param>
    /// <param name="project">The matching project, or <see langword="null" /> when it is not registered.</param>
    /// <returns><see langword="true" /> when a matching project is registered.</returns>
    /// <exception cref="ArgumentException"><paramref name="projectId" /> is empty or whitespace.</exception>
    bool TryGetProject(string projectId, out FlourishProject? project);

    /// <summary>Enables or disables the project-aware title-bar display at runtime.</summary>
    /// <param name="enabled">Whether the title bar uses the active project identity.</param>
    void SetMultiProjectEnabled(bool enabled);
}

/// <summary>Describes one application project represented by the Flourish shell.</summary>
public sealed record FlourishProject
{
    /// <summary>Creates project display metadata.</summary>
    /// <param name="id">The stable, case-sensitive project ID.</param>
    /// <param name="name">The project display name.</param>
    /// <param name="storagePath">The optional local storage path represented by the project.</param>
    public FlourishProject(string id, string name, string? storagePath = null)
    {
        Id = id;
        Name = name;
        StoragePath = storagePath;
    }

    /// <summary>Gets the stable, case-sensitive project ID.</summary>
    public string Id { get; init; }

    /// <summary>Gets the project display name.</summary>
    public string Name { get; init; }

    /// <summary>Gets the optional local storage path represented by this project.</summary>
    public string? StoragePath { get; init; }
}

/// <summary>Represents the current project identities and selection.</summary>
/// <param name="Projects">The registered projects in insertion order.</param>
/// <param name="ActiveProject">The active project, or <see langword="null" /> when none is selected.</param>
/// <param name="IsMultiProjectEnabled">Whether the title bar uses project-aware display semantics.</param>
/// <param name="Version">The monotonically increasing project-state version.</param>
public sealed record FlourishProjectSnapshot(
    IReadOnlyList<FlourishProject> Projects,
    FlourishProject? ActiveProject,
    bool IsMultiProjectEnabled,
    long Version
);

/// <summary>Provides data after the project display state changes.</summary>
/// <param name="current">The state after the change.</param>
/// <param name="changeKind">The kind of mutation.</param>
/// <param name="projectId">The affected project ID, if applicable.</param>
/// <param name="activeProjectChanged">Whether the active identity or its displayed metadata changed.</param>
public sealed class FlourishProjectsChangedEventArgs(
    FlourishProjectSnapshot current,
    FlourishRuntimeChangeKind changeKind,
    string? projectId,
    bool activeProjectChanged
) : EventArgs
{
    /// <summary>Gets the state after the change.</summary>
    public FlourishProjectSnapshot Current { get; } = current;

    /// <summary>Gets the mutation kind.</summary>
    public FlourishRuntimeChangeKind ChangeKind { get; } = changeKind;

    /// <summary>Gets the affected project ID, if applicable.</summary>
    public string? ProjectId { get; } = projectId;

    /// <summary>Gets whether the active project identity or its displayed metadata changed.</summary>
    public bool ActiveProjectChanged { get; } = activeProjectChanged;
}

/// <summary>Provides project state when the title bar requests a new project.</summary>
/// <param name="current">The project state at the time of the request.</param>
public sealed class FlourishNewProjectRequestedEventArgs(FlourishProjectSnapshot current)
    : EventArgs
{
    /// <summary>Gets the project state at the time of the request.</summary>
    public FlourishProjectSnapshot Current { get; } = current;
}

/// <summary>Provides the requested project and current state for a title-bar activation request.</summary>
/// <param name="project">The project selected by the user.</param>
/// <param name="current">The project state at the time of the request.</param>
public sealed class FlourishProjectActivationRequestedEventArgs(
    FlourishProject project,
    FlourishProjectSnapshot current
) : EventArgs
{
    /// <summary>Gets the project the user selected.</summary>
    public FlourishProject Project { get; } = project;

    /// <summary>Gets the project state at the time of the request.</summary>
    public FlourishProjectSnapshot Current { get; } = current;
}
