using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class ProjectServiceTests
{
    [Fact]
    public void Current_StartsEmptyAndReflectsConfiguredMultiProjectMode()
    {
        IProjectService sut = new ProjectService(
            new FlourishShellOptions { IsMultiProjectEnabled = true }
        );

        Assert.Empty(sut.Current.Projects);
        Assert.Null(sut.Current.ActiveProject);
        Assert.True(sut.Current.IsMultiProjectEnabled);
        Assert.Equal(0, sut.Current.Version);
    }

    [Fact]
    public void AddProject_NormalizesMetadataPreservesOrderAndPublishesChanges()
    {
        IProjectService sut = new ProjectService(new FlourishShellOptions());
        var changes = new List<FlourishProjectsChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);

        sut.AddProject(new FlourishProject(" first ", " First ", @" C:\Work\First "));
        sut.AddProject(
            new FlourishProject("second", "Second", "   "),
            activate: false
        );

        Assert.Equal(
            ["first", "second"],
            sut.Current.Projects.Select(project => project.Id)
        );
        Assert.Equal("First", sut.Current.Projects[0].Name);
        Assert.Equal(@"C:\Work\First", sut.Current.Projects[0].StoragePath);
        Assert.Null(sut.Current.Projects[1].StoragePath);
        var readOnlyProjects = Assert.IsAssignableFrom<IList<FlourishProject>>(
            sut.Current.Projects
        );
        Assert.Throws<NotSupportedException>(() =>
            readOnlyProjects[0] = new FlourishProject("replacement", "Replacement")
        );
        Assert.Equal("first", sut.Current.ActiveProject?.Id);
        Assert.Equal(2, sut.Current.Version);
        Assert.Collection(
            changes,
            change =>
            {
                Assert.Equal(FlourishRuntimeChangeKind.Added, change.ChangeKind);
                Assert.Equal("first", change.ProjectId);
                Assert.True(change.ActiveProjectChanged);
                Assert.Equal(1, change.Current.Version);
            },
            change =>
            {
                Assert.Equal(FlourishRuntimeChangeKind.Added, change.ChangeKind);
                Assert.Equal("second", change.ProjectId);
                Assert.False(change.ActiveProjectChanged);
                Assert.Equal(2, change.Current.Version);
            }
        );
    }

    [Fact]
    public void AddProject_UsesCaseSensitiveIdsAndRejectsExactDuplicates()
    {
        IProjectService sut = new ProjectService(new FlourishShellOptions());
        sut.AddProject(new FlourishProject("project", "Lowercase"));

        Assert.Throws<InvalidOperationException>(() =>
            sut.AddProject(new FlourishProject("project", "Duplicate"))
        );
        sut.AddProject(
            new FlourishProject("PROJECT", "Uppercase"),
            activate: false
        );

        Assert.Equal(
            ["project", "PROJECT"],
            sut.Current.Projects.Select(project => project.Id)
        );
        Assert.Equal(2, sut.Current.Version);
    }

    [Fact]
    public void UpsertProject_UpdatesInPlaceActivatesOnRequestAndSuppressesNoOps()
    {
        IProjectService sut = new ProjectService(new FlourishShellOptions());
        var changes = new List<FlourishProjectsChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);
        var original = new FlourishProject("alpha", "Alpha");
        var updated = new FlourishProject("alpha", "Renamed", @"C:\Work\Alpha");

        sut.UpsertProject(original, activate: false);
        sut.UpsertProject(original, activate: false);
        sut.UpsertProject(updated, activate: false);
        sut.UpsertProject(updated);
        sut.UpsertProject(updated);
        var activeUpdate = updated with { Name = "Active rename" };
        sut.UpsertProject(activeUpdate, activate: false);
        sut.UpsertProject(activeUpdate, activate: false);

        var project = Assert.Single(sut.Current.Projects);
        Assert.Equal(activeUpdate, project);
        Assert.Equal(activeUpdate, sut.Current.ActiveProject);
        Assert.Equal(4, sut.Current.Version);
        Assert.Equal(
            [
                FlourishRuntimeChangeKind.Added,
                FlourishRuntimeChangeKind.Updated,
                FlourishRuntimeChangeKind.Updated,
                FlourishRuntimeChangeKind.Updated,
            ],
            changes.Select(change => change.ChangeKind)
        );
        Assert.Equal(
            [false, false, true, true],
            changes.Select(change => change.ActiveProjectChanged)
        );
        Assert.Equal(
            [1L, 2L, 3L, 4L],
            changes.Select(change => change.Current.Version)
        );
    }

    [Fact]
    public void SetProjectMetadata_UpdatesActiveProjectAndSuppressesEquivalentValues()
    {
        IProjectService sut = new ProjectService(new FlourishShellOptions());
        sut.AddProject(new FlourishProject("alpha", "Alpha", @"C:\Work\Alpha"));
        var changes = new List<FlourishProjectsChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);

        sut.SetProjectMetadata(" alpha ", " Alpha ", @" C:\Work\Alpha ");
        sut.SetProjectMetadata("alpha", "Renamed", "   ");

        var change = Assert.Single(changes);
        Assert.Equal(FlourishRuntimeChangeKind.Updated, change.ChangeKind);
        Assert.Equal("alpha", change.ProjectId);
        Assert.True(change.ActiveProjectChanged);
        Assert.Equal(2, change.Current.Version);
        Assert.Equal("Renamed", sut.Current.ActiveProject?.Name);
        Assert.Null(sut.Current.ActiveProject?.StoragePath);
        Assert.Throws<KeyNotFoundException>(() =>
            sut.SetProjectMetadata("missing", "Missing")
        );
    }

    [Fact]
    public void SetActiveProject_SwitchesClearsAndSuppressesNoOps()
    {
        IProjectService sut = new ProjectService(new FlourishShellOptions());
        sut.AddProject(new FlourishProject("first", "First"));
        sut.AddProject(new FlourishProject("second", "Second"), activate: false);
        var changes = new List<FlourishProjectsChangedEventArgs>();
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
        IProjectService sut = new ProjectService(new FlourishShellOptions());
        sut.AddProject(new FlourishProject("first", "First"));
        sut.AddProject(new FlourishProject("second", "Second"), activate: false);
        var changes = new List<FlourishProjectsChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);

        Assert.True(sut.TryGetProject("second", out var second));
        Assert.Equal("Second", second?.Name);
        Assert.True(sut.RemoveProject("second"));
        Assert.False(sut.RemoveProject("second"));
        Assert.False(sut.TryGetProject("second", out _));
        Assert.True(sut.RemoveProject("first"));

        Assert.Empty(sut.Current.Projects);
        Assert.Null(sut.Current.ActiveProject);
        Assert.Equal(4, sut.Current.Version);
        Assert.Equal(
            ["second", "first"],
            changes.Select(change => change.ProjectId)
        );
        Assert.Equal(
            [false, true],
            changes.Select(change => change.ActiveProjectChanged)
        );
        Assert.All(
            changes,
            change => Assert.Equal(FlourishRuntimeChangeKind.Removed, change.ChangeKind)
        );
    }

    [Fact]
    public void SetMultiProjectEnabled_PublishesOnlyMaterialChanges()
    {
        var options = new FlourishShellOptions();
        IProjectService sut = new ProjectService(options);
        var changes = new List<FlourishProjectsChangedEventArgs>();
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
            change => Assert.Equal(FlourishRuntimeChangeKind.Updated, change.ChangeKind)
        );
    }

    [Fact]
    public void TitleBarRequests_RaiseIntentEventsWithoutMutatingProjectState()
    {
        var sut = new ProjectService(new FlourishShellOptions());
        sut.AddProject(new FlourishProject("first", "First"));
        sut.AddProject(new FlourishProject("second", "Second"), activate: false);
        var changedCount = 0;
        FlourishNewProjectRequestedEventArgs? newProjectRequest = null;
        FlourishProjectActivationRequestedEventArgs? activationRequest = null;
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
        Assert.Throws<KeyNotFoundException>(() =>
            sut.RequestProjectActivation("missing")
        );
    }

    [Fact]
    public void ProjectMutations_ValidateRequiredMetadata()
    {
        IProjectService sut = new ProjectService(new FlourishShellOptions());

        Assert.Equal(
            "project",
            Assert.Throws<ArgumentNullException>(() => sut.AddProject(null!)).ParamName
        );
        Assert.Equal(
            "Id",
            Assert
                .Throws<ArgumentException>(() =>
                    sut.AddProject(new FlourishProject(" ", "Name"))
                )
                .ParamName
        );
        Assert.Equal(
            "Name",
            Assert
                .Throws<ArgumentException>(() =>
                    sut.AddProject(new FlourishProject("id", " "))
                )
                .ParamName
        );
        Assert.Equal(
            "projectId",
            Assert.Throws<ArgumentException>(() => sut.RemoveProject(" ")).ParamName
        );
        Assert.Equal(
            "projectId",
            Assert
                .Throws<ArgumentException>(() => sut.TryGetProject(" ", out _))
                .ParamName
        );
    }
}
