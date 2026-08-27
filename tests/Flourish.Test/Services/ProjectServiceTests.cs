using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Projects;
using ArkheideSystem.Flourish.Test.Infrastructure;

using System.IO;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class ProjectServiceTests
{
    [Fact]
    public void Current_StartsEmptyAndReflectsConfiguredMultiProjectMode()
    {
        IProjectService sut = new ProjectService(
            new ProjectOptions { IsMultiProjectEnabled = true }
        );

        Assert.Empty(sut.Current.Projects);
        Assert.Null(sut.Current.ActiveProject);
        Assert.True(sut.Current.IsMultiProjectEnabled);
        Assert.Equal(0, sut.Current.Version);
    }

    [Fact]
    public void AppendProject_NormalizesMetadataPreservesOrderAndPublishesChanges()
    {
        IProjectService sut = new ProjectService(new ProjectOptions());
        var changes = new List<ProjectCatalogChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);

        sut.AddProject(new ProjectDescriptor(" first ", " First ", @" C:\Work\First "));
        sut.AddProject(new ProjectDescriptor("second", "Second", "   "), activate: false);

        Assert.Equal(["first", "second"], sut.Current.Projects.Select(project => project.Id));
        Assert.Equal("First", sut.Current.Projects[0].Name);
        Assert.Equal(@"C:\Work\First", sut.Current.Projects[0].StoragePath);
        Assert.Null(sut.Current.Projects[1].StoragePath);
        var readOnlyProjects = Assert.IsAssignableFrom<IList<ProjectDescriptor>>(
            sut.Current.Projects
        );
        Assert.Throws<NotSupportedException>(() =>
            readOnlyProjects[0] = new ProjectDescriptor("replacement", "Replacement")
        );
        Assert.Equal("first", sut.Current.ActiveProject?.Id);
        Assert.Equal(2, sut.Current.Version);
        Assert.Collection(
            changes,
            change =>
            {
                Assert.Equal(CollectionChangeKind.Added, change.ChangeKind);
                Assert.Equal("first", change.ProjectId);
                Assert.True(change.ActiveProjectChanged);
                Assert.Equal(1, change.Current.Version);
            },
            change =>
            {
                Assert.Equal(CollectionChangeKind.Added, change.ChangeKind);
                Assert.Equal("second", change.ProjectId);
                Assert.False(change.ActiveProjectChanged);
                Assert.Equal(2, change.Current.Version);
            }
        );
    }

    [Fact]
    public void AppendProject_UsesCaseSensitiveIdsAndRejectsExactDuplicates()
    {
        IProjectService sut = new ProjectService(new ProjectOptions());
        sut.AddProject(new ProjectDescriptor("project", "Lowercase"));

        Assert.Throws<InvalidOperationException>(() =>
            sut.AddProject(new ProjectDescriptor("project", "Duplicate"))
        );
        sut.AddProject(new ProjectDescriptor("PROJECT", "Uppercase"), activate: false);

        Assert.Equal(["project", "PROJECT"], sut.Current.Projects.Select(project => project.Id));
        Assert.Equal(2, sut.Current.Version);
    }

    [Fact]
    public void SetProject_UpdatesInPlaceActivatesOnRequestAndSuppressesNoOps()
    {
        IProjectService sut = new ProjectService(new ProjectOptions());
        var changes = new List<ProjectCatalogChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);
        var original = new ProjectDescriptor("alpha", "Alpha");
        var updated = new ProjectDescriptor("alpha", "Renamed", @"C:\Work\Alpha");

        sut.SetProject(original, activate: false);
        sut.SetProject(original, activate: false);
        sut.SetProject(updated, activate: false);
        sut.SetProject(updated);
        sut.SetProject(updated);
        var activeUpdate = updated with { Name = "Active rename" };
        sut.SetProject(activeUpdate, activate: false);
        sut.SetProject(activeUpdate, activate: false);

        var project = Assert.Single(sut.Current.Projects);
        Assert.Equal(activeUpdate, project);
        Assert.Equal(activeUpdate, sut.Current.ActiveProject);
        Assert.Equal(4, sut.Current.Version);
        Assert.Equal(
            [
                CollectionChangeKind.Added,
                CollectionChangeKind.Updated,
                CollectionChangeKind.Updated,
                CollectionChangeKind.Updated,
            ],
            changes.Select(change => change.ChangeKind)
        );
        Assert.Equal(
            [false, false, true, true],
            changes.Select(change => change.ActiveProjectChanged)
        );
        Assert.Equal([1L, 2L, 3L, 4L], changes.Select(change => change.Current.Version));
    }

    [Fact]
    public void SetProjectMetadata_UpdatesActiveProjectAndSuppressesEquivalentValues()
    {
        IProjectService sut = new ProjectService(new ProjectOptions());
        sut.AddProject(new ProjectDescriptor("alpha", "Alpha", @"C:\Work\Alpha"));
        var changes = new List<ProjectCatalogChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);

        sut.SetProjectMetadata(" alpha ", " Alpha ", @" C:\Work\Alpha ");
        sut.SetProjectMetadata("alpha", "Renamed", "   ");

        var change = Assert.Single(changes);
        Assert.Equal(CollectionChangeKind.Updated, change.ChangeKind);
        Assert.Equal("alpha", change.ProjectId);
        Assert.True(change.ActiveProjectChanged);
        Assert.Equal(2, change.Current.Version);
        Assert.Equal("Renamed", sut.Current.ActiveProject?.Name);
        Assert.Null(sut.Current.ActiveProject?.StoragePath);
        Assert.Throws<KeyNotFoundException>(() => sut.SetProjectMetadata("missing", "Missing"));
    }

    [Fact]
    public void SetActiveProject_SwitchesClearsAndSuppressesNoOps()
    {
        IProjectService sut = new ProjectService(new ProjectOptions());
        sut.AddProject(new ProjectDescriptor("first", "First"));
        sut.AddProject(new ProjectDescriptor("second", "Second"), activate: false);
        var changes = new List<ProjectCatalogChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);

        sut.SetActiveProject("second");
        sut.SetActiveProject("second");
        sut.SetActiveProject(null);
        sut.SetActiveProject(null);

        Assert.Null(sut.Current.ActiveProject);
        Assert.Equal(4, sut.Current.Version);
        Assert.Equal(["second", null], changes.Select(change => change.ProjectId));
        Assert.All(changes, change => Assert.True(change.ActiveProjectChanged));
        Assert.Equal([3L, 4L], changes.Select(change => change.Current.Version));
        Assert.Throws<KeyNotFoundException>(() => sut.SetActiveProject("missing"));
    }

    [Fact]
    public void RemoveProject_UpdatesLookupAndClearsTheActiveProject()
    {
        IProjectService sut = new ProjectService(new ProjectOptions());
        sut.AddProject(new ProjectDescriptor("first", "First"));
        sut.AddProject(new ProjectDescriptor("second", "Second"), activate: false);
        var changes = new List<ProjectCatalogChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);

        var second = sut.GetProject("second");
        Assert.NotNull(second);
        Assert.Equal("Second", second?.Name);
        Assert.True(sut.RemoveProject("second"));
        Assert.False(sut.RemoveProject("second"));
        Assert.Null(sut.GetProject("second"));
        Assert.True(sut.RemoveProject("first"));

        Assert.Empty(sut.Current.Projects);
        Assert.Null(sut.Current.ActiveProject);
        Assert.Equal(4, sut.Current.Version);
        Assert.Equal(["second", "first"], changes.Select(change => change.ProjectId));
        Assert.Equal([false, true], changes.Select(change => change.ActiveProjectChanged));
        Assert.All(
            changes,
            change => Assert.Equal(CollectionChangeKind.Removed, change.ChangeKind)
        );
    }

    [Fact]
    public void SetMultiProjectEnabled_PublishesOnlyMaterialChanges()
    {
        var options = new ProjectOptions();
        IProjectService sut = new ProjectService(options);
        var changes = new List<ProjectCatalogChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);

        sut.SetMultiProjectEnabled(false);
        sut.SetMultiProjectEnabled(true);
        sut.SetMultiProjectEnabled(true);
        sut.SetMultiProjectEnabled(false);

        Assert.False(sut.Current.IsMultiProjectEnabled);
        Assert.False(options.IsMultiProjectEnabled);
        Assert.Equal(2, sut.Current.Version);
        Assert.Equal([1L, 2L], changes.Select(change => change.Current.Version));
        Assert.All(changes, change => Assert.Null(change.ProjectId));
        Assert.All(changes, change => Assert.False(change.ActiveProjectChanged));
        Assert.All(
            changes,
            change => Assert.Equal(CollectionChangeKind.Updated, change.ChangeKind)
        );
    }

    [Fact]
    public void TitleBarRequests_RaiseIntentEventsWithoutMutatingProjectState()
    {
        var sut = new ProjectService(new ProjectOptions());
        sut.AddProject(new ProjectDescriptor("first", "First"));
        sut.AddProject(new ProjectDescriptor("second", "Second"), activate: false);
        var changedCount = 0;
        ProjectCreationRequestedEventArgs? newProjectRequest = null;
        ProjectActivationRequestedEventArgs? activationRequest = null;
        sut.Changed += (_, _) => changedCount++;
        sut.NewProjectRequested += (_, args) => newProjectRequest = args;
        sut.ProjectActivationRequested += (_, args) => activationRequest = args;

        sut.RequestNewProject();
        sut.RequestProjectActivation("second");
        Assert.False(sut.TryRequestProjectActivation("missing"));

        Assert.NotNull(newProjectRequest);
        Assert.Equal(2, newProjectRequest.Current.Version);
        Assert.NotNull(activationRequest);
        Assert.Equal("second", activationRequest.Project.Id);
        Assert.Equal(2, activationRequest.Current.Version);
        Assert.Equal("first", sut.Current.ActiveProject?.Id);
        Assert.Equal(2, sut.Current.Version);
        Assert.Equal(0, changedCount);
        Assert.Throws<KeyNotFoundException>(() => sut.RequestProjectActivation("missing"));
    }

    [Fact]
    public void ProjectMutations_ValidateRequiredMetadata()
    {
        IProjectService sut = new ProjectService(new ProjectOptions());

        Assert.Equal(
            "project",
            Assert.Throws<ArgumentNullException>(() => sut.AddProject(null!)).ParamName
        );
        Assert.Equal(
            "Id",
            Assert
                .Throws<ArgumentException>(() =>
                    sut.AddProject(new ProjectDescriptor(" ", "Name"))
                )
                .ParamName
        );
        Assert.Equal(
            "Name",
            Assert
                .Throws<ArgumentException>(() => sut.AddProject(new ProjectDescriptor("id", " ")))
                .ParamName
        );
        Assert.Equal(
            "projectId",
            Assert.Throws<ArgumentException>(() => sut.RemoveProject(" ")).ParamName
        );
        Assert.Equal(
            "projectId",
            Assert.Throws<ArgumentException>(() => sut.GetProject(" ")).ParamName
        );
    }

    [Fact]
    public void PersistentService_WhenMultiProjectIsDisabled_DoesNotTouchCatalogStore()
    {
        var persisted = new ProjectDescriptor("persisted", "Persisted", "persisted.txt");
        var store = new RecordingProjectCatalogStore(
            new ProjectCatalog([persisted], persisted.Id)
        );
        var sut = new ProjectService(new ProjectOptions(), store);

        sut.AddProject(new ProjectDescriptor("runtime", "Runtime", "runtime.txt"));

        Assert.Equal(0, store.LoadCallCount);
        Assert.Empty(store.SavedCatalogs);
        Assert.Equal("runtime", Assert.Single(sut.Current.Projects).Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ProjectCatalogStore_WithoutConfiguredPath_IsANoOp(string? path)
    {
        var sut = new ProjectCatalogStore(
            new ApplicationDataOptions { ProjectCatalogFilePath = path }
        );

        var loaded = sut.Load();
        sut.Save(new ProjectCatalog([], null));

        Assert.Null(sut.FilePath);
        Assert.Empty(loaded.Projects);
        Assert.Null(loaded.ActiveProjectId);
    }

    [Fact]
    public void PersistentService_DoesNotPersistActiveUnnamedProject()
    {
        using var directory = new TemporaryDirectory();
        var options = new ProjectOptions
        {
            IsMultiProjectEnabled = true,
            UnnamedProjectPlaceholder = "Configured unnamed project",
        };
        var store = new ProjectCatalogStore(
            new ApplicationDataOptions
            {
                ProjectCatalogFilePath = Path.Combine(directory.Path, "projects.json"),
            }
        );

        var first = new ProjectService(options, store);

        var created = Assert.Single(first.Current.Projects);
        Assert.Equal("Configured unnamed project", created.Name);
        Assert.Null(created.StoragePath);
        Assert.Equal(created, first.Current.ActiveProject);
        Assert.False(File.Exists(Path.Combine(directory.Path, "projects.json")));

        var reloaded = new ProjectService(options, store);

        var replacement = Assert.Single(reloaded.Current.Projects);
        Assert.Equal("Configured unnamed project", replacement.Name);
        Assert.Null(replacement.StoragePath);
        Assert.NotEqual(created.Id, replacement.Id);
        Assert.Equal(replacement, reloaded.Current.ActiveProject);
        Assert.Empty(Directory.EnumerateFiles(directory.Path, ".projects.json.*.tmp"));
    }

    [Fact]
    public void PersistentService_RoundTripsProjectOrderMetadataAndActiveId()
    {
        using var directory = new TemporaryDirectory();
        var options = new ProjectOptions { IsMultiProjectEnabled = true };
        var store = new ProjectCatalogStore(
            new ApplicationDataOptions
            {
                ProjectCatalogFilePath = Path.Combine(directory.Path, "projects.json"),
            }
        );
        var first = new ProjectService(options, store);
        var unnamed = Assert.Single(first.Current.Projects);
        var firstPath = Path.Combine(directory.Path, "First.txt");
        var secondPath = Path.Combine(directory.Path, "Second.txt");
        File.WriteAllText(firstPath, string.Empty);
        File.WriteAllText(secondPath, string.Empty);
        first.SetProjectMetadata(unnamed.Id, "First", firstPath);
        first.AddProject(new ProjectDescriptor("second", "Second", secondPath), activate: false);
        first.SetActiveProject("second");

        var reloaded = new ProjectService(options, store);

        Assert.Equal(
            [unnamed.Id, "second"],
            reloaded.Current.Projects.Select(project => project.Id)
        );
        Assert.Equal("First", reloaded.Current.Projects[0].Name);
        Assert.Equal("second", reloaded.Current.ActiveProject?.Id);
        Assert.Equal(0, reloaded.Current.Version);
    }

    [Fact]
    public void PersistentService_PersistsOnlyExistingStorageMappings()
    {
        using var directory = new TemporaryDirectory();
        var options = new ProjectOptions { IsMultiProjectEnabled = true };
        var store = new ProjectCatalogStore(
            new ApplicationDataOptions
            {
                ProjectCatalogFilePath = Path.Combine(directory.Path, "projects.json"),
            }
        );
        var existingPath = Path.Combine(directory.Path, "Existing.txt");
        File.WriteAllText(existingPath, string.Empty);
        var missingPath = Path.Combine(directory.Path, "Missing.txt");
        var first = new ProjectService(options, store);
        var initial = Assert.Single(first.Current.Projects);
        first.SetProjectMetadata(initial.Id, "Existing", existingPath);
        first.AddProject(
            new ProjectDescriptor("missing", "Missing", missingPath),
            activate: false
        );
        first.AddProject(new ProjectDescriptor("unmapped", "Unmapped"));

        var reloaded = new ProjectService(options, store);

        var remaining = Assert.Single(reloaded.Current.Projects);
        Assert.Equal(initial.Id, remaining.Id);
        Assert.Equal(Path.GetFullPath(existingPath), Path.GetFullPath(remaining.StoragePath!));
        Assert.Equal(remaining, reloaded.Current.ActiveProject);
        Assert.Null(reloaded.GetProject("missing"));
        Assert.Null(reloaded.GetProject("unmapped"));
    }

    [Fact]
    public void PersistentService_LoadPrunesStaleMappingsAndRepairsActiveProject()
    {
        using var directory = new TemporaryDirectory();
        var existingPath = Path.Combine(directory.Path, "Existing.txt");
        File.WriteAllText(existingPath, string.Empty);
        var existing = new ProjectDescriptor("existing", "Existing", existingPath);
        var missing = new ProjectDescriptor(
            "missing",
            "Missing",
            Path.Combine(directory.Path, "Missing.txt")
        );
        var unmapped = new ProjectDescriptor("unmapped", "Unmapped");
        var store = new RecordingProjectCatalogStore(
            new ProjectCatalog([existing, missing, unmapped], missing.Id)
        );

        var sut = new ProjectService(
            new ProjectOptions { IsMultiProjectEnabled = true },
            store
        );

        Assert.Equal(existing, Assert.Single(sut.Current.Projects));
        Assert.Equal(existing, sut.Current.ActiveProject);
        var repaired = Assert.Single(store.SavedCatalogs);
        Assert.Equal(existing, Assert.Single(repaired.Projects));
        Assert.Equal(existing.Id, repaired.ActiveProjectId);
    }

    [Fact]
    public void PersistentMutation_WhenCatalogSaveFails_RollsBackWithoutPublishingChange()
    {
        using var directory = new TemporaryDirectory();
        var existingPath = Path.Combine(directory.Path, "Existing.txt");
        var addedPath = Path.Combine(directory.Path, "Added.txt");
        File.WriteAllText(existingPath, string.Empty);
        File.WriteAllText(addedPath, string.Empty);
        var existing = new ProjectDescriptor("existing", "Existing", existingPath);
        var store = new ThrowingProjectCatalogStore(new ProjectCatalog([existing], existing.Id));
        var sut = new ProjectService(
            new ProjectOptions { IsMultiProjectEnabled = true },
            store
        );
        var before = sut.Current;
        var changedCount = 0;
        sut.Changed += (_, _) => changedCount++;

        Assert.Throws<IOException>(() =>
            sut.AddProject(new ProjectDescriptor("added", "Added", addedPath))
        );

        var after = sut.Current;
        Assert.Equal(before.Version, after.Version);
        Assert.Equal(before.Projects, after.Projects);
        Assert.Equal(before.ActiveProject, after.ActiveProject);
        Assert.Null(sut.GetProject("added"));
        Assert.Equal(0, changedCount);
    }

    private sealed class TestAppSettingsStore(string filePath) : ISettingsStore
    {
        public string FilePath { get; } = filePath;

        public ValueTask<SettingsUpdateResult> UpdateAsync(
            Action<ISettingsEditor> update,
            CancellationToken cancellationToken = default
        ) => throw new NotSupportedException();

        public ValueTask<SettingsUpdateResult> SetAsync<T>(
            string path,
            T value,
            CancellationToken cancellationToken = default
        ) => throw new NotSupportedException();

        public ValueTask<SettingsUpdateResult> RemoveAsync(
            string path,
            CancellationToken cancellationToken = default
        ) => throw new NotSupportedException();

        public ValueTask<SettingsUpdateResult> MergeAsync<T>(
            string path,
            T value,
            CancellationToken cancellationToken = default
        ) => throw new NotSupportedException();

        public ValueTask<SettingsUpdateResult> AppendAsync<T>(
            string path,
            T value,
            CancellationToken cancellationToken = default
        ) => throw new NotSupportedException();
    }

    private sealed class RecordingProjectCatalogStore(ProjectCatalog catalog) : IProjectCatalogStore
    {
        public int LoadCallCount { get; private set; }

        public List<ProjectCatalog> SavedCatalogs { get; } = [];

        public ProjectCatalog Load()
        {
            LoadCallCount++;
            return catalog;
        }

        public void Save(ProjectCatalog catalog) => SavedCatalogs.Add(catalog);
    }

    private sealed class ThrowingProjectCatalogStore(ProjectCatalog catalog) : IProjectCatalogStore
    {
        public ProjectCatalog Load() => catalog;

        public void Save(ProjectCatalog catalog) => throw new IOException("Catalog save failed.");
    }
}
