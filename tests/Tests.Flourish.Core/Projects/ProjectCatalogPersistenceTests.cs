using System;
using System.IO;
using System.Linq;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Projects;

using Xunit;

namespace ArkheideSystem.Tests.Flourish.Core.Projects;

public sealed class ProjectCatalogPersistenceTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Store_WithoutConfiguredPath_IsANoOp(string? path)
    {
        var sut = new ProjectCatalogStore(path);

        ProjectCatalog loaded = sut.Load();
        sut.Save(new ProjectCatalog([], null));

        Assert.Null(sut.FilePath);
        Assert.Empty(loaded.Projects);
        Assert.Null(loaded.ActiveProjectId);
    }

    [Fact]
    public void Store_SaveCreatesParentAndRoundTripsOrderMetadataAndActiveId()
    {
        using var directory = new TemporaryDirectory();
        string catalogPath = Path.Combine(directory.Path, "nested", "projects.json");
        var sut = new ProjectCatalogStore(catalogPath);
        var first = new ProjectDescriptor("first", "First", "First.project");
        var second = new ProjectDescriptor("second", "Second", "Second.project");

        sut.Save(new ProjectCatalog([first, second], second.Id));
        ProjectCatalog loaded = sut.Load();

        Assert.True(File.Exists(catalogPath));
        Assert.Equal([first, second], loaded.Projects);
        Assert.Equal(second.Id, loaded.ActiveProjectId);
        Assert.Empty(
            Directory.EnumerateFiles(Path.GetDirectoryName(catalogPath)!, ".projects.json.*.tmp")
        );
    }

    [Fact]
    public void Store_LoadReportsInvalidJsonWithoutLeavingTemporaryFiles()
    {
        using var directory = new TemporaryDirectory();
        string catalogPath = Path.Combine(directory.Path, "projects.json");
        File.WriteAllText(catalogPath, "{ not json }");
        var sut = new ProjectCatalogStore(catalogPath);

        InvalidDataException error = Assert.Throws<InvalidDataException>(() => sut.Load());

        Assert.Contains("projects.json", error.Message, StringComparison.Ordinal);
        Assert.IsType<System.Text.Json.JsonException>(error.InnerException);
        Assert.Empty(Directory.EnumerateFiles(directory.Path, ".projects.json.*.tmp"));
    }

    [Fact]
    public void PersistentService_DoesNotPersistActiveUnnamedProject()
    {
        using var directory = new TemporaryDirectory();
        string catalogPath = Path.Combine(directory.Path, "projects.json");
        var options = new ProjectOptions
        {
            IsMultiProjectEnabled = true,
            UnnamedProjectPlaceholder = "Configured unnamed project",
        };
        var store = new ProjectCatalogStore(catalogPath);

        var first = new ProjectService(options, store);
        ProjectDescriptor created = Assert.Single(first.Current.Projects);

        Assert.Equal("Configured unnamed project", created.Name);
        Assert.Null(created.StoragePath);
        Assert.Equal(created, first.Current.ActiveProject);
        Assert.False(File.Exists(catalogPath));

        var reloaded = new ProjectService(options, store);
        ProjectDescriptor replacement = Assert.Single(reloaded.Current.Projects);

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
        string catalogPath = Path.Combine(directory.Path, "projects.json");
        var options = new ProjectOptions { IsMultiProjectEnabled = true };
        var store = new ProjectCatalogStore(catalogPath);
        var first = new ProjectService(options, store);
        ProjectDescriptor unnamed = Assert.Single(first.Current.Projects);
        string firstPath = Path.Combine(directory.Path, "First.txt");
        string secondPath = Path.Combine(directory.Path, "Second.txt");
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
    public void PersistentService_LoadPrunesStaleMappingsAndRepairsActiveProject()
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
        ProjectCatalog repaired = Assert.Single(store.SavedCatalogs);
        Assert.Equal(existing, Assert.Single(repaired.Projects));
        Assert.Equal(existing.Id, repaired.ActiveProjectId);
    }

    [Fact]
    public void DisabledMultiProjectMode_DoesNotLoadOrSaveCatalog()
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

    private sealed class RecordingProjectCatalogStore(ProjectCatalog catalog)
        : IProjectCatalogStore
    {
        public int LoadCallCount { get; private set; }

        public System.Collections.Generic.List<ProjectCatalog> SavedCatalogs { get; } = [];

        public ProjectCatalog Load()
        {
            LoadCallCount++;
            return catalog;
        }

        public void Save(ProjectCatalog catalog)
        {
            SavedCatalogs.Add(catalog);
        }
    }
}
