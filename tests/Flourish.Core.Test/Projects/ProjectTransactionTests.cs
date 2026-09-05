using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Projects;

using Xunit;

namespace ArkheideSystem.Flourish.Core.Test.Projects;

public sealed class ProjectTransactionTests
{
    [Theory]
    [InlineData(ProjectMutation.Add)]
    [InlineData(ProjectMutation.Replace)]
    [InlineData(ProjectMutation.Metadata)]
    [InlineData(ProjectMutation.Activate)]
    [InlineData(ProjectMutation.Remove)]
    [InlineData(ProjectMutation.RemoveAndEnsureActive)]
    public void PersistentMutation_WhenSaveFails_RollsBackStateVersionAndEvents(
        ProjectMutation mutation
    )
    {
        using var directory = new TemporaryDirectory();
        string firstPath = Path.Combine(directory.Path, "First.txt");
        string secondPath = Path.Combine(directory.Path, "Second.txt");
        string addedPath = Path.Combine(directory.Path, "Added.txt");
        File.WriteAllText(firstPath, string.Empty);
        File.WriteAllText(secondPath, string.Empty);
        File.WriteAllText(addedPath, string.Empty);
        var first = new ProjectDescriptor("first", "First", firstPath);
        var second = new ProjectDescriptor("second", "Second", secondPath);
        var store = new ThrowingProjectCatalogStore(
            new ProjectCatalog([first, second], first.Id)
        );
        var sut = new ProjectService(
            new ProjectOptions { IsMultiProjectEnabled = true },
            store
        );
        ProjectCatalogSnapshot before = sut.Current;
        int changedCount = 0;
        sut.Changed += (_, _) => changedCount++;

        Assert.Throws<IOException>(() => ApplyMutation(sut, mutation, addedPath));

        ProjectCatalogSnapshot after = sut.Current;
        Assert.Equal(before.Version, after.Version);
        Assert.Equal(before.Projects.ToArray(), after.Projects.ToArray());
        Assert.Equal(before.ActiveProject, after.ActiveProject);
        Assert.Equal(0, changedCount);
        Assert.Equal(1, store.SaveCallCount);
        Assert.Null(sut.GetProject("added"));
        Assert.Equal("First", sut.GetProject("first")?.Name);
        Assert.Equal(firstPath, sut.GetProject("first")?.StoragePath);
        Assert.Equal("first", sut.Current.ActiveProject?.Id);
    }

    [Fact]
    public void RestoreRepair_WhenSaveFails_DoesNotPublishPartiallyRestoredState()
    {
        using var directory = new TemporaryDirectory();
        string existingPath = Path.Combine(directory.Path, "Existing.txt");
        File.WriteAllText(existingPath, string.Empty);
        var existing = new ProjectDescriptor("existing", "Existing", existingPath);
        var missing = new ProjectDescriptor(
            "missing",
            "Missing",
            Path.Combine(directory.Path, "Missing.txt")
        );
        var store = new ThrowingProjectCatalogStore(
            new ProjectCatalog([existing, missing], missing.Id)
        );

        Assert.Throws<IOException>(() =>
            new ProjectService(
                new ProjectOptions { IsMultiProjectEnabled = true },
                store
            )
        );

        Assert.Equal(1, store.SaveCallCount);
    }

    private static void ApplyMutation(
        ProjectService service,
        ProjectMutation mutation,
        string addedPath
    )
    {
        switch (mutation)
        {
            case ProjectMutation.Add:
                service.AddProject(new ProjectDescriptor("added", "Added", addedPath));
                break;
            case ProjectMutation.Replace:
                service.SetProject(
                    new ProjectDescriptor("first", "Replaced", addedPath),
                    activate: false
                );
                break;
            case ProjectMutation.Metadata:
                service.SetProjectMetadata("first", "Renamed", addedPath);
                break;
            case ProjectMutation.Activate:
                service.SetActiveProject("second");
                break;
            case ProjectMutation.Remove:
                service.RemoveProject("first");
                break;
            case ProjectMutation.RemoveAndEnsureActive:
                service.RemoveProjectAndEnsureActive("first");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mutation), mutation, null);
        }
    }

    public enum ProjectMutation
    {
        Add,
        Replace,
        Metadata,
        Activate,
        Remove,
        RemoveAndEnsureActive,
    }

    private sealed class ThrowingProjectCatalogStore(ProjectCatalog catalog)
        : IProjectCatalogStore
    {
        public int SaveCallCount { get; private set; }

        public ProjectCatalog Load()
        {
            return catalog;
        }

        public void Save(ProjectCatalog catalog)
        {
            SaveCallCount++;
            throw new IOException("Catalog save failed.");
        }
    }
}
