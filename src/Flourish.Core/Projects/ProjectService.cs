using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Projects;

internal sealed class ProjectService : IProjectService
{
    private readonly Lock gate = new();
    private readonly Dictionary<string, ProjectDescriptor> projects = new(
        StringComparer.Ordinal
    );
    private readonly List<string> projectOrder = [];
    private readonly ProjectOptions options;
    private readonly IProjectCatalogStore? catalogStore;
    private string? activeProjectId;
    private ProjectCatalogSnapshot current = null!;
    private long version;

    public ProjectService(ProjectOptions options, IProjectCatalogStore catalogStore)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.catalogStore = catalogStore ?? throw new ArgumentNullException(nameof(catalogStore));
        if (options.IsMultiProjectEnabled)
        {
            RestoreCatalog();
        }

        current = CreateSnapshot();
    }

    internal ProjectService(ProjectOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        this.options = options;
        current = CreateSnapshot();
    }

    public event EventHandler<ProjectCatalogChangedEventArgs>? Changed;

    public event EventHandler<ProjectCreationRequestedEventArgs>? NewProjectRequested;

    public event EventHandler<ProjectActivationRequestedEventArgs>?
        ProjectActivationRequested;

    public ProjectCatalogSnapshot Current => Volatile.Read(ref current);

    public void AddProject(ProjectDescriptor project, bool activate = true)
    {
        project = NormalizeProject(project);
        ProjectCatalogSnapshot snapshot;
        bool activeChanged;
        lock (gate)
        {
            if (projects.ContainsKey(project.Id))
            {
                throw new InvalidOperationException(
                    $"Project ID '{project.Id}' is already registered."
                );
            }

            activeChanged = activate && !StringComparer.Ordinal.Equals(activeProjectId, project.Id);
            snapshot = CommitCatalogMutation(() =>
            {
                projects.Add(project.Id, project);
                projectOrder.Add(project.Id);
                if (activate)
                {
                    activeProjectId = project.Id;
                }
            });
        }

        RaiseChanged(snapshot, CollectionChangeKind.Added, project.Id, activeChanged);
    }

    public void SetProject(ProjectDescriptor project, bool activate = true)
    {
        project = NormalizeProject(project);
        ProjectCatalogSnapshot snapshot;
        CollectionChangeKind changeKind;
        bool activeChanged;
        lock (gate)
        {
            bool exists = projects.ContainsKey(project.Id);
            ProjectDescriptor? previous = exists ? projects[project.Id] : null;
            bool wasActive = StringComparer.Ordinal.Equals(activeProjectId, project.Id);
            if (exists && previous == project && (!activate || wasActive))
            {
                return;
            }

            changeKind = exists ? CollectionChangeKind.Updated : CollectionChangeKind.Added;
            activeChanged =
                (exists && wasActive && previous != project) || (activate && !wasActive);
            snapshot = CommitCatalogMutation(() =>
            {
                if (!exists)
                {
                    projectOrder.Add(project.Id);
                }

                projects[project.Id] = project;
                if (activate)
                {
                    activeProjectId = project.Id;
                }
            });
        }

        RaiseChanged(snapshot, changeKind, project.Id, activeChanged);
    }

    public void SetProjectMetadata(string projectId, string name, string? storagePath = null)
    {
        projectId = ValidateRequired(projectId, nameof(projectId));
        name = ValidateRequired(name, nameof(name));
        storagePath = NormalizeOptional(storagePath);
        ProjectCatalogSnapshot snapshot;
        bool activeProjectChanged;
        lock (gate)
        {
            if (!projects.TryGetValue(projectId, out ProjectDescriptor? previous))
            {
                throw new KeyNotFoundException($"Project ID '{projectId}' is not registered.");
            }

            ProjectDescriptor updated = previous with { Name = name, StoragePath = storagePath };
            if (updated == previous)
            {
                return;
            }

            activeProjectChanged = StringComparer.Ordinal.Equals(activeProjectId, projectId);
            snapshot = CommitCatalogMutation(() => projects[projectId] = updated);
        }

        RaiseChanged(snapshot, CollectionChangeKind.Updated, projectId, activeProjectChanged);
    }

    public void SetActiveProject(string? projectId)
    {
        projectId = NormalizeOptional(projectId);
        ProjectCatalogSnapshot snapshot;
        lock (gate)
        {
            if (projectId is not null && !projects.ContainsKey(projectId))
            {
                throw new KeyNotFoundException($"Project ID '{projectId}' is not registered.");
            }

            if (StringComparer.Ordinal.Equals(activeProjectId, projectId))
            {
                return;
            }

            snapshot = CommitCatalogMutation(() => activeProjectId = projectId);
        }

        RaiseChanged(snapshot, CollectionChangeKind.Updated, projectId, activeProjectChanged: true);
    }

    public bool RemoveProject(string projectId)
    {
        projectId = ValidateRequired(projectId, nameof(projectId));
        ProjectCatalogSnapshot snapshot;
        bool activeChanged;
        lock (gate)
        {
            if (!projects.ContainsKey(projectId))
            {
                return false;
            }

            activeChanged = StringComparer.Ordinal.Equals(activeProjectId, projectId);
            snapshot = CommitCatalogMutation(() =>
            {
                projects.Remove(projectId);
                projectOrder.Remove(projectId);
                if (activeChanged)
                {
                    activeProjectId = null;
                }
            });
        }

        RaiseChanged(snapshot, CollectionChangeKind.Removed, projectId, activeChanged);
        return true;
    }

    internal bool RemoveProjectAndEnsureActive(string projectId)
    {
        projectId = ValidateRequired(projectId, nameof(projectId));
        ProjectCatalogSnapshot snapshot;
        bool activeChanged;
        lock (gate)
        {
            if (!projects.ContainsKey(projectId))
            {
                return false;
            }

            string? previousActiveProjectId = activeProjectId;
            snapshot = CommitCatalogMutation(() =>
            {
                projects.Remove(projectId);
                projectOrder.Remove(projectId);
                if (StringComparer.Ordinal.Equals(activeProjectId, projectId))
                {
                    activeProjectId = null;
                }

                if (activeProjectId is null)
                {
                    if (projectOrder.Count > 0)
                    {
                        activeProjectId = projectOrder[0];
                    }
                    else
                    {
                        ProjectDescriptor unnamedProject = CreateUnnamedProject();
                        projects.Add(unnamedProject.Id, unnamedProject);
                        projectOrder.Add(unnamedProject.Id);
                        activeProjectId = unnamedProject.Id;
                    }
                }
            });
            activeChanged = !StringComparer.Ordinal.Equals(
                previousActiveProjectId,
                activeProjectId
            );
        }

        RaiseChanged(snapshot, CollectionChangeKind.Removed, projectId, activeChanged);
        return true;
    }

    public ProjectDescriptor? GetProject(string projectId)
    {
        projectId = ValidateRequired(projectId, nameof(projectId));
        lock (gate)
        {
            return projects.GetValueOrDefault(projectId);
        }
    }

    public void SetMultiProjectEnabled(bool enabled)
    {
        ProjectCatalogSnapshot snapshot;
        lock (gate)
        {
            if (options.IsMultiProjectEnabled == enabled)
            {
                return;
            }

            options.IsMultiProjectEnabled = enabled;
            version++;
            snapshot = CreateSnapshot();
        }

        RaiseChanged(
            snapshot,
            CollectionChangeKind.Updated,
            projectId: null,
            activeProjectChanged: false
        );
    }

    internal void RequestNewProject()
    {
        ProjectCatalogSnapshot snapshot;
        lock (gate)
        {
            snapshot = CreateSnapshot();
        }

        NewProjectRequested?.Invoke(this, new ProjectCreationRequestedEventArgs(snapshot));
    }

    internal void RequestProjectActivation(string projectId)
    {
        if (TryRequestProjectActivation(projectId))
        {
            return;
        }

        throw new KeyNotFoundException($"Project ID '{projectId.Trim()}' is not registered.");
    }

    internal bool TryRequestProjectActivation(string projectId)
    {
        projectId = ValidateRequired(projectId, nameof(projectId));
        ProjectDescriptor project;
        ProjectCatalogSnapshot snapshot;
        lock (gate)
        {
            if (!projects.TryGetValue(projectId, out project!))
            {
                return false;
            }

            snapshot = CreateSnapshot();
        }

        ProjectActivationRequested?.Invoke(
            this,
            new ProjectActivationRequestedEventArgs(project, snapshot)
        );
        return true;
    }

    private void RestoreCatalog()
    {
        ProjectCatalog catalog = catalogStore!.Load();
        bool catalogChanged = false;
        foreach (ProjectDescriptor candidate in catalog.Projects)
        {
            ProjectDescriptor project = NormalizeProject(candidate);
            catalogChanged |= project != candidate;
            if (!IsPersistableProject(project))
            {
                catalogChanged = true;
                continue;
            }

            if (!projects.TryAdd(project.Id, project))
            {
                throw new InvalidDataException(
                    $"projects.json contains duplicate project ID '{project.Id}'."
                );
            }

            projectOrder.Add(project.Id);
        }

        string? requestedActiveId = NormalizeOptional(catalog.ActiveProjectId);
        catalogChanged |= !StringComparer.Ordinal.Equals(
            requestedActiveId,
            catalog.ActiveProjectId
        );
        if (requestedActiveId is not null && projects.ContainsKey(requestedActiveId))
        {
            activeProjectId = requestedActiveId;
        }
        else if (projectOrder.Count > 0)
        {
            activeProjectId = projectOrder[0];
            catalogChanged = true;
        }
        else
        {
            ProjectDescriptor project = CreateUnnamedProject();
            projects.Add(project.Id, project);
            projectOrder.Add(project.Id);
            activeProjectId = project.Id;
            catalogChanged |= requestedActiveId is not null;
        }

        if (catalogChanged)
        {
            PersistCatalog();
        }
    }

    private ProjectDescriptor CreateUnnamedProject()
    {
        string name = string.IsNullOrWhiteSpace(options.UnnamedProjectPlaceholder)
            ? "Unnamed project"
            : options.UnnamedProjectPlaceholder.Trim();
        return new ProjectDescriptor(Guid.NewGuid().ToString("N"), name);
    }

    private void PersistCatalog()
    {
        if (!options.IsMultiProjectEnabled)
        {
            return;
        }

        ProjectDescriptor[] persistedProjects = projectOrder
            .Select(id => projects[id])
            .Where(IsPersistableProject)
            .ToArray();
        string? persistedActiveProjectId =
            activeProjectId is not null
            && projects.TryGetValue(activeProjectId, out ProjectDescriptor? activeProject)
            && IsPersistableProject(activeProject)
                ? activeProjectId
                : null;
        catalogStore?.Save(new ProjectCatalog(persistedProjects, persistedActiveProjectId));
    }

    private static bool IsPersistableProject(ProjectDescriptor project)
    {
        return project.StoragePath is not null && File.Exists(project.StoragePath);
    }

    private ProjectCatalogSnapshot CommitCatalogMutation(Action mutation)
    {
        ProjectStateBackup backup = CaptureState();
        try
        {
            mutation();
            version++;
            PersistCatalog();
            return CreateSnapshot();
        }
        catch
        {
            RestoreState(backup);
            throw;
        }
    }

    private ProjectStateBackup CaptureState()
    {
        return new ProjectStateBackup(
            projects.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
            [.. projectOrder],
            activeProjectId,
            version
        );
    }

    private void RestoreState(ProjectStateBackup backup)
    {
        projects.Clear();
        foreach (KeyValuePair<string, ProjectDescriptor> pair in backup.Projects)
        {
            projects.Add(pair.Key, pair.Value);
        }

        projectOrder.Clear();
        projectOrder.AddRange(backup.ProjectOrder);
        activeProjectId = backup.ActiveProjectId;
        version = backup.Version;
    }

    private ProjectCatalogSnapshot CreateSnapshot()
    {
        ProjectCatalogSnapshot? published = current;
        if (published is not null && published.Version == version)
        {
            return published;
        }

        IReadOnlyList<ProjectDescriptor> orderedProjects = Array.AsReadOnly(
            projectOrder.Select(id => projects[id]).ToArray()
        );
        ProjectDescriptor? activeProject = activeProjectId is not null
            ? projects.GetValueOrDefault(activeProjectId)
            : null;
        var snapshot = new ProjectCatalogSnapshot(
            orderedProjects,
            activeProject,
            options.IsMultiProjectEnabled,
            version
        );
        Volatile.Write(ref current, snapshot);
        return snapshot;
    }

    private void RaiseChanged(
        ProjectCatalogSnapshot snapshot,
        CollectionChangeKind changeKind,
        string? projectId,
        bool activeProjectChanged
    )
    {
        Changed?.Invoke(
            this,
            new ProjectCatalogChangedEventArgs(
                snapshot,
                changeKind,
                projectId,
                activeProjectChanged
            )
        );
    }

    private static ProjectDescriptor NormalizeProject(ProjectDescriptor project)
    {
        ArgumentNullException.ThrowIfNull(project);
        return project with
        {
            Id = ValidateRequired(project.Id, nameof(project.Id)),
            Name = ValidateRequired(project.Name, nameof(project.Name)),
            StoragePath = NormalizeOptional(project.StoragePath),
        };
    }

    private static string ValidateRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed record ProjectStateBackup(
        IReadOnlyDictionary<string, ProjectDescriptor> Projects,
        IReadOnlyList<string> ProjectOrder,
        string? ActiveProjectId,
        long Version
    );
}
