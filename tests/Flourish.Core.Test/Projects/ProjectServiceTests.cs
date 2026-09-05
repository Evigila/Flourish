using System;
using System.Collections.Generic;
using System.Linq;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Projects;

using Xunit;

namespace ArkheideSystem.Flourish.Core.Test.Projects;

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
    public void AddProject_NormalizesMetadataPreservesOrderAndPublishesChanges()
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
        IList<ProjectDescriptor> projects = Assert.IsAssignableFrom<IList<ProjectDescriptor>>(
            sut.Current.Projects
        );
        Assert.Throws<NotSupportedException>(() =>
            projects[0] = new ProjectDescriptor("replacement", "Replacement")
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
    public void AddProject_UsesCaseSensitiveIdsAndRejectsExactDuplicates()
    {
        IProjectService sut = new ProjectService(new ProjectOptions());
        sut.AddProject(new ProjectDescriptor("project", "Lowercase"));

        Assert.Throws<InvalidOperationException>(() =>
            sut.AddProject(new ProjectDescriptor("project", "Duplicate"))
        );
        sut.AddProject(new ProjectDescriptor("PROJECT", "Uppercase"), activate: false);

        Assert.Equal(
            ["project", "PROJECT"],
            sut.Current.Projects.Select(project => project.Id)
        );
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
        ProjectDescriptor activeUpdate = updated with { Name = "Active rename" };
        sut.SetProject(activeUpdate, activate: false);
        sut.SetProject(activeUpdate, activate: false);

        Assert.Equal(activeUpdate, Assert.Single(sut.Current.Projects));
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
    }

    [Fact]
    public void MetadataAndActivation_TrackActiveIdentityAndSuppressEquivalentValues()
    {
        IProjectService sut = new ProjectService(new ProjectOptions());
        sut.AddProject(new ProjectDescriptor("first", "First", @"C:\Work\First"));
        sut.AddProject(new ProjectDescriptor("second", "Second"), activate: false);
        var changes = new List<ProjectCatalogChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);

        sut.SetProjectMetadata(" first ", " First ", @" C:\Work\First ");
        sut.SetProjectMetadata("first", "Renamed", "   ");
        sut.SetActiveProject("second");
        sut.SetActiveProject("second");
        sut.SetActiveProject(null);
        sut.SetActiveProject(null);

        Assert.Null(sut.Current.ActiveProject);
        Assert.Equal("Renamed", sut.GetProject("first")?.Name);
        Assert.Null(sut.GetProject("first")?.StoragePath);
        Assert.Equal(5, sut.Current.Version);
        Assert.Equal(["first", "second", null], changes.Select(change => change.ProjectId));
        Assert.All(changes, change => Assert.True(change.ActiveProjectChanged));
        Assert.Throws<KeyNotFoundException>(() => sut.SetActiveProject("missing"));
        Assert.Throws<KeyNotFoundException>(() =>
            sut.SetProjectMetadata("missing", "Missing")
        );
    }

    [Fact]
    public void RemoveProject_UpdatesLookupAndClearsTheActiveProject()
    {
        IProjectService sut = new ProjectService(new ProjectOptions());
        sut.AddProject(new ProjectDescriptor("first", "First"));
        sut.AddProject(new ProjectDescriptor("second", "Second"), activate: false);
        var changes = new List<ProjectCatalogChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);

        Assert.True(sut.RemoveProject("second"));
        Assert.False(sut.RemoveProject("second"));
        Assert.Null(sut.GetProject("second"));
        Assert.True(sut.RemoveProject("first"));

        Assert.Empty(sut.Current.Projects);
        Assert.Null(sut.Current.ActiveProject);
        Assert.Equal(4, sut.Current.Version);
        Assert.Equal(["second", "first"], changes.Select(change => change.ProjectId));
        Assert.Equal([false, true], changes.Select(change => change.ActiveProjectChanged));
    }

    [Fact]
    public void RemoveProjectAndEnsureActive_UsesFirstRemainingThenCreatesUnnamedReplacement()
    {
        var sut = new ProjectService(
            new ProjectOptions { UnnamedProjectPlaceholder = " Untitled " }
        );
        sut.AddProject(new ProjectDescriptor("first", "First"));
        sut.AddProject(new ProjectDescriptor("second", "Second"), activate: false);
        sut.AddProject(new ProjectDescriptor("third", "Third"), activate: false);

        Assert.True(sut.RemoveProjectAndEnsureActive("first"));
        Assert.Equal("second", sut.Current.ActiveProject?.Id);
        Assert.Equal(["second", "third"], sut.Current.Projects.Select(project => project.Id));

        Assert.True(sut.RemoveProjectAndEnsureActive("second"));
        Assert.True(sut.RemoveProjectAndEnsureActive("third"));

        ProjectDescriptor replacement = Assert.Single(sut.Current.Projects);
        Assert.Equal("Untitled", replacement.Name);
        Assert.Null(replacement.StoragePath);
        Assert.Equal(replacement, sut.Current.ActiveProject);
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
    }

    [Fact]
    public void ProjectSelectorRequests_RaiseIntentEventsWithoutMutatingState()
    {
        var sut = new ProjectService(new ProjectOptions());
        sut.AddProject(new ProjectDescriptor("first", "First"));
        sut.AddProject(new ProjectDescriptor("second", "Second"), activate: false);
        int changedCount = 0;
        ProjectCreationRequestedEventArgs? creationRequest = null;
        ProjectActivationRequestedEventArgs? activationRequest = null;
        sut.Changed += (_, _) => changedCount++;
        sut.NewProjectRequested += (_, args) => creationRequest = args;
        sut.ProjectActivationRequested += (_, args) => activationRequest = args;

        sut.RequestNewProject();
        sut.RequestProjectActivation("second");
        Assert.False(sut.TryRequestProjectActivation("missing"));

        Assert.Equal(2, creationRequest?.Current.Version);
        Assert.Equal("second", activationRequest?.Project.Id);
        Assert.Equal(2, activationRequest?.Current.Version);
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
                .Throws<ArgumentException>(() =>
                    sut.AddProject(new ProjectDescriptor("id", " "))
                )
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
}
