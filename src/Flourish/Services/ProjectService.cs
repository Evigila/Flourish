using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class ProjectService(FlourishShellOptions options) : IProjectService
{
    private readonly Lock gate = new();
    private readonly Dictionary<string, FlourishProject> projects = new(StringComparer.Ordinal);
    private readonly List<string> projectOrder = [];
    private string? activeProjectId;
    private long version;

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

            projects.Add(project.Id, project);
            projectOrder.Add(project.Id);
            activeChanged = activate && !StringComparer.Ordinal.Equals(activeProjectId, project.Id);
            if (activate)
            {
                activeProjectId = project.Id;
            }

            version++;
            snapshot = CreateSnapshot();
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
            snapshot = CreateSnapshot();
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

            projects[projectId] = current;
            activeProjectChanged = StringComparer.Ordinal.Equals(activeProjectId, projectId);
            version++;
            snapshot = CreateSnapshot();
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

            activeProjectId = projectId;
            version++;
            snapshot = CreateSnapshot();
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
            if (!projects.Remove(projectId))
            {
                return false;
            }

            projectOrder.Remove(projectId);
            activeChanged = StringComparer.Ordinal.Equals(activeProjectId, projectId);
            if (activeChanged)
            {
                activeProjectId = null;
            }

            version++;
            snapshot = CreateSnapshot();
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

    private FlourishProjectSnapshot CreateSnapshot()
    {
        var orderedProjects = Array.AsReadOnly(
            projectOrder.Select(id => projects[id]).ToArray()
        );
        var activeProject = activeProjectId is not null
            ? projects.GetValueOrDefault(activeProjectId)
            : null;
        return new FlourishProjectSnapshot(
            orderedProjects,
            activeProject,
            options.IsMultiProjectEnabled,
            version
        );
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
}
