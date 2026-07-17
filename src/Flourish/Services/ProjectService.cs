using System.IO;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class ProjectService : IProjectService
{
    private readonly Lock gate = new();
    private readonly Dictionary<string, FlourishProject> projects = new(StringComparer.Ordinal);
    private readonly List<string> projectOrder = [];
    private readonly FlourishShellOptions options;
    private readonly IProjectCatalogStore? catalogStore;
    private string? activeProjectId;
    private FlourishProjectSnapshot? cachedSnapshot;
    private long version;

    public ProjectService(
        FlourishShellOptions options,
        IProjectCatalogStore catalogStore
    )
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.catalogStore = catalogStore
            ?? throw new ArgumentNullException(nameof(catalogStore));
        InitializeFromCatalog();
    }

    internal ProjectService(FlourishShellOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public event EventHandler<FlourishProjectsChangedEventArgs>? Changed;

    public event EventHandler<FlourishNewProjectRequestedEventArgs>? NewProjectRequested;

    public event EventHandler<FlourishProjectActivationRequestedEventArgs>?
        ProjectActivationRequested;

    public FlourishProjectSnapshot Current
    {
        get
        {
            lock (gate)
            {
                return CreateSnapshot();
            }
        }
    }

    public void AddProject(FlourishProject project, bool activate = true)
    {
        project = NormalizeProject(project);
        FlourishProjectSnapshot snapshot;
        bool activeChanged;
        lock (gate)
        {
            if (projects.ContainsKey(project.Id))
            {
                throw new InvalidOperationException(
                    $"Project ID '{project.Id}' is already registered."
                );
            }

            var backup = CaptureState();
            try
            {
                projects.Add(project.Id, project);
                projectOrder.Add(project.Id);
                activeChanged =
                    activate && !StringComparer.Ordinal.Equals(activeProjectId, project.Id);
                if (activate)
                {
                    activeProjectId = project.Id;
                }

                version++;
                PersistCatalog();
                snapshot = CreateSnapshot();
            }
            catch
            {
                RestoreState(backup);
                throw;
            }
        }

        RaiseChanged(
            snapshot,
            FlourishRuntimeChangeKind.Added,
            project.Id,
            activeChanged
        );
    }

    public void UpsertProject(FlourishProject project, bool activate = true)
    {
        project = NormalizeProject(project);
        FlourishProjectSnapshot snapshot;
        FlourishRuntimeChangeKind changeKind;
        bool activeChanged;
        lock (gate)
        {
            var exists = projects.ContainsKey(project.Id);
            var previous = exists ? projects[project.Id] : null;
            var wasActive = StringComparer.Ordinal.Equals(activeProjectId, project.Id);
            if (
                exists
                && previous == project
                && (!activate || wasActive)
            )
            {
                return;
            }

            var backup = CaptureState();
            try
            {
                if (!exists)
                {
                    projectOrder.Add(project.Id);
                }

                projects[project.Id] = project;
                changeKind = exists
                    ? FlourishRuntimeChangeKind.Updated
                    : FlourishRuntimeChangeKind.Added;
                activeChanged = (exists && wasActive && previous != project)
                    || (activate && !wasActive);
                if (activate)
                {
                    activeProjectId = project.Id;
                }

                version++;
                PersistCatalog();
                snapshot = CreateSnapshot();
            }
            catch
            {
                RestoreState(backup);
                throw;
            }
        }

        RaiseChanged(snapshot, changeKind, project.Id, activeChanged);
    }

    public void SetProjectMetadata(string projectId, string name, string? storagePath = null)
    {
        projectId = ValidateRequired(projectId, nameof(projectId));
        name = ValidateRequired(name, nameof(name));
        storagePath = NormalizeOptional(storagePath);
        FlourishProjectSnapshot snapshot;
        bool activeProjectChanged;
        lock (gate)
        {
            if (!projects.TryGetValue(projectId, out var previous))
            {
                throw new KeyNotFoundException($"Project ID '{projectId}' is not registered.");
            }

            var current = previous with { Name = name, StoragePath = storagePath };
            if (current == previous)
            {
                return;
            }

            var backup = CaptureState();
            try
            {
                projects[projectId] = current;
                activeProjectChanged = StringComparer.Ordinal.Equals(activeProjectId, projectId);
                version++;
                PersistCatalog();
                snapshot = CreateSnapshot();
            }
            catch
            {
                RestoreState(backup);
                throw;
            }
        }

        RaiseChanged(
            snapshot,
            FlourishRuntimeChangeKind.Updated,
            projectId,
            activeProjectChanged
        );
    }

    public void SetActiveProject(string? projectId)
    {
        projectId = NormalizeOptional(projectId);
        FlourishProjectSnapshot snapshot;
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

            var backup = CaptureState();
            try
            {
                activeProjectId = projectId;
                version++;
                PersistCatalog();
                snapshot = CreateSnapshot();
            }
            catch
            {
                RestoreState(backup);
                throw;
            }
        }

        RaiseChanged(
            snapshot,
            FlourishRuntimeChangeKind.Updated,
            projectId,
            activeProjectChanged: true
        );
    }

    public bool RemoveProject(string projectId)
    {
        projectId = ValidateRequired(projectId, nameof(projectId));
        FlourishProjectSnapshot snapshot;
        bool activeChanged;
        lock (gate)
        {
            if (!projects.ContainsKey(projectId))
            {
                return false;
            }

            var backup = CaptureState();
            try
            {
                projects.Remove(projectId);
                projectOrder.Remove(projectId);
                activeChanged = StringComparer.Ordinal.Equals(activeProjectId, projectId);
                if (activeChanged)
                {
                    activeProjectId = null;
                }

                version++;
                PersistCatalog();
                snapshot = CreateSnapshot();
            }
            catch
            {
                RestoreState(backup);
                throw;
            }
        }

        RaiseChanged(
            snapshot,
            FlourishRuntimeChangeKind.Removed,
            projectId,
            activeChanged
        );
        return true;
    }

    internal bool RemoveProjectAndEnsureActive(string projectId)
    {
        projectId = ValidateRequired(projectId, nameof(projectId));
        FlourishProjectSnapshot snapshot;
        bool activeChanged;
        lock (gate)
        {
            if (!projects.ContainsKey(projectId))
            {
                return false;
            }

            var backup = CaptureState();
            try
            {
                var previousActiveProjectId = activeProjectId;
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
                        var unnamedProject = CreateUnnamedProject();
                        projects.Add(unnamedProject.Id, unnamedProject);
                        projectOrder.Add(unnamedProject.Id);
                        activeProjectId = unnamedProject.Id;
                    }
                }

                activeChanged = !StringComparer.Ordinal.Equals(
                    previousActiveProjectId,
                    activeProjectId
                );
                version++;
                PersistCatalog();
                snapshot = CreateSnapshot();
            }
            catch
            {
                RestoreState(backup);
                throw;
            }
        }

        RaiseChanged(
            snapshot,
            FlourishRuntimeChangeKind.Removed,
            projectId,
            activeChanged
        );
        return true;
    }

    public bool TryGetProject(string projectId, out FlourishProject? project)
    {
        projectId = ValidateRequired(projectId, nameof(projectId));
        lock (gate)
        {
            return projects.TryGetValue(projectId, out project);
        }
    }

    public void SetMultiProjectEnabled(bool enabled)
    {
        FlourishProjectSnapshot snapshot;
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
            FlourishRuntimeChangeKind.Updated,
            projectId: null,
            activeProjectChanged: false
        );
    }

    internal void RequestNewProject()
    {
        FlourishProjectSnapshot snapshot;
        lock (gate)
        {
            snapshot = CreateSnapshot();
        }

        NewProjectRequested?.Invoke(this, new FlourishNewProjectRequestedEventArgs(snapshot));
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
        FlourishProject project;
        FlourishProjectSnapshot snapshot;
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
            new FlourishProjectActivationRequestedEventArgs(project, snapshot)
        );
        return true;
    }

    private void InitializeFromCatalog()
    {
        var catalog = catalogStore!.Load();
        var catalogChanged = false;
        foreach (var candidate in catalog.Projects)
        {
            var project = NormalizeProject(candidate);
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

        var requestedActiveId = NormalizeOptional(catalog.ActiveProjectId);
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
            var project = CreateUnnamedProject();
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

    private FlourishProject CreateUnnamedProject()
    {
        var name = string.IsNullOrWhiteSpace(options.UnnamedProjectPlaceholder)
            ? "Unnamed project"
            : options.UnnamedProjectPlaceholder.Trim();
        return new FlourishProject(Guid.NewGuid().ToString("N"), name);
    }

    private void PersistCatalog()
    {
        var persistedProjects = projectOrder
            .Select(id => projects[id])
            .Where(IsPersistableProject)
            .ToArray();
        var persistedActiveProjectId = activeProjectId is not null
            && projects.TryGetValue(activeProjectId, out var activeProject)
            && IsPersistableProject(activeProject)
                ? activeProjectId
                : null;
        catalogStore?.Save(
            new ProjectCatalog(
                persistedProjects,
                persistedActiveProjectId
            )
        );
    }

    private static bool IsPersistableProject(FlourishProject project) =>
        project.StoragePath is not null && File.Exists(project.StoragePath);

    private ProjectStateBackup CaptureState() =>
        new(
            projects.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
            [.. projectOrder],
            activeProjectId,
            version
        );

    private void RestoreState(ProjectStateBackup backup)
    {
        projects.Clear();
        foreach (var pair in backup.Projects)
        {
            projects.Add(pair.Key, pair.Value);
        }

        projectOrder.Clear();
        projectOrder.AddRange(backup.ProjectOrder);
        activeProjectId = backup.ActiveProjectId;
        version = backup.Version;
    }

    private FlourishProjectSnapshot CreateSnapshot()
    {
        if (cachedSnapshot?.Version == version)
        {
            return cachedSnapshot;
        }

        var orderedProjects = Array.AsReadOnly(
            projectOrder.Select(id => projects[id]).ToArray()
        );
        var activeProject = activeProjectId is not null
            ? projects.GetValueOrDefault(activeProjectId)
            : null;
        cachedSnapshot = new FlourishProjectSnapshot(
            orderedProjects,
            activeProject,
            options.IsMultiProjectEnabled,
            version
        );
        return cachedSnapshot;
    }

    private void RaiseChanged(
        FlourishProjectSnapshot snapshot,
        FlourishRuntimeChangeKind changeKind,
        string? projectId,
        bool activeProjectChanged
    )
    {
        Changed?.Invoke(
            this,
            new FlourishProjectsChangedEventArgs(
                snapshot,
                changeKind,
                projectId,
                activeProjectChanged
            )
        );
    }

    private static FlourishProject NormalizeProject(FlourishProject project)
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
        IReadOnlyDictionary<string, FlourishProject> Projects,
        IReadOnlyList<string> ProjectOrder,
        string? ActiveProjectId,
        long Version
    );
}
