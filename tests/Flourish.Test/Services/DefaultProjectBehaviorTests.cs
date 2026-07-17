using System.IO;
using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;
using Moq;
using MessageBoxOptions = System.Windows.MessageBoxOptions;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class DefaultProjectBehaviorTests
{
    [Fact]
    public async Task CreateProjectAsync_CreatesTextFileAddsProjectAndRaisesRequest()
    {
        using var directory = new TemporaryDirectory();
        var selectedPath = Path.Combine(directory.Path, "Created project");
        var dialog = new RecordingSaveFileDialog(selectedPath);
        var projects = new ProjectService(new FlourishShellOptions());
        var requestCount = 0;
        projects.NewProjectRequested += (_, _) => requestCount++;
        var sut = CreateBehavior(projects, dialog, new Mock<IMessageService>().Object);

        var result = await sut.CreateProjectAsync();

        var expectedPath = Path.GetFullPath(selectedPath + ".txt");
        Assert.True(result);
        Assert.Equal(1, requestCount);
        Assert.True(File.Exists(expectedPath));
        Assert.Equal(0, new FileInfo(expectedPath).Length);
        var project = Assert.Single(projects.Current.Projects);
        Assert.Equal("Created project", project.Name);
        Assert.Equal(expectedPath, project.StoragePath);
        Assert.Equal(project, projects.Current.ActiveProject);
    }

    [Fact]
    public async Task SaveActiveProjectAsync_WhenPathIsMissing_SavesAndUpdatesMetadata()
    {
        using var directory = new TemporaryDirectory();
        var selectedPath = Path.Combine(directory.Path, "Saved draft.txt");
        var dialog = new RecordingSaveFileDialog(selectedPath);
        var projects = new ProjectService(new FlourishShellOptions());
        projects.AddProject(new FlourishProject("draft", "Draft"));
        var sut = CreateBehavior(projects, dialog, new Mock<IMessageService>().Object);

        var result = await sut.SaveActiveProjectAsync();

        Assert.True(result);
        Assert.Equal("Draft", Assert.Single(dialog.Requests).SuggestedFileName);
        Assert.True(File.Exists(selectedPath));
        Assert.Equal("Saved draft", projects.Current.ActiveProject?.Name);
        Assert.Equal(Path.GetFullPath(selectedPath), projects.Current.ActiveProject?.StoragePath);
    }

    [Fact]
    public async Task SaveActiveProjectAsync_WhenPathExists_DoesNotRewriteApplicationData()
    {
        using var directory = new TemporaryDirectory();
        var storagePath = Path.Combine(directory.Path, "Existing.txt");
        await File.WriteAllTextAsync(storagePath, "application-owned content");
        var projects = new ProjectService(new FlourishShellOptions());
        projects.AddProject(new FlourishProject("existing", "Existing", storagePath));
        var dialog = new RecordingSaveFileDialog();
        var sut = CreateBehavior(projects, dialog, new Mock<IMessageService>().Object);

        var result = await sut.SaveActiveProjectAsync();

        Assert.True(result);
        Assert.Equal("application-owned content", await File.ReadAllTextAsync(storagePath));
        Assert.Empty(dialog.Requests);
    }

    [Fact]
    public async Task SaveActiveProjectAsync_WhenCatalogPersistenceFails_PreservesExistingFile()
    {
        using var directory = new TemporaryDirectory();
        var storagePath = Path.Combine(directory.Path, "Existing.txt");
        await File.WriteAllTextAsync(storagePath, "existing content");
        var draft = new FlourishProject("draft", "Draft");
        var catalogStore = new FailingProjectCatalogStore(
            new ProjectCatalog([draft], draft.Id)
        );
        var projects = new ProjectService(new FlourishShellOptions(), catalogStore);
        var sut = CreateBehavior(
            projects,
            new RecordingSaveFileDialog(storagePath),
            new Mock<IMessageService>().Object
        );

        await Assert.ThrowsAsync<IOException>(() =>
            sut.SaveActiveProjectAsync().AsTask()
        );

        Assert.Equal("existing content", await File.ReadAllTextAsync(storagePath));
        Assert.Null(projects.Current.ActiveProject?.StoragePath);
    }

    [Fact]
    public async Task CreateProjectAsync_WhenCurrentProjectSaveIsCanceled_DoesNotOpenCreateDialog()
    {
        var projects = new ProjectService(new FlourishShellOptions());
        projects.AddProject(new FlourishProject("draft", "Draft"));
        var dialog = new RecordingSaveFileDialog();
        IReadOnlyList<FlourishMessageOption>? choices = null;
        var messages = CreateCustomPromptMessageService(
            "cancel",
            observedChoices => choices = observedChoices
        );
        var sut = CreateBehavior(projects, dialog, messages.Object);

        var result = await sut.CreateProjectAsync();

        Assert.False(result);
        Assert.Equal("draft", Assert.Single(projects.Current.Projects).Id);
        Assert.Equal("draft", projects.Current.ActiveProject?.Id);
        Assert.Empty(dialog.Requests);
        Assert.NotNull(choices);
        Assert.Equal(["cancel", "save"], choices.Select(choice => choice.Id));
    }

    [Fact]
    public async Task ActivateProjectAsync_WhenUnsavedSaveIsCanceled_LeavesActiveProject()
    {
        var projects = new ProjectService(new FlourishShellOptions());
        projects.AddProject(new FlourishProject("first", "First"));
        projects.AddProject(
            new FlourishProject("second", "Second", "second.external"),
            activate: false
        );
        var dialog = new RecordingSaveFileDialog();
        IReadOnlyList<FlourishMessageOption>? choices = null;
        var messages = CreateCustomPromptMessageService(
            "cancel",
            observedChoices => choices = observedChoices
        );
        var activationRequestCount = 0;
        projects.ProjectActivationRequested += (_, _) => activationRequestCount++;
        var sut = CreateBehavior(projects, dialog, messages.Object);

        var result = await sut.ActivateProjectAsync("second");

        Assert.False(result);
        Assert.Equal("first", projects.Current.ActiveProject?.Id);
        Assert.Equal(0, activationRequestCount);
        Assert.Empty(dialog.Requests);
        Assert.NotNull(choices);
        Assert.Equal(["cancel", "save"], choices.Select(choice => choice.Id));
    }

    [Fact]
    public async Task ActivateProjectAsync_SavesUnsavedProjectRaisesRequestAndActivatesTarget()
    {
        using var directory = new TemporaryDirectory();
        var projects = new ProjectService(new FlourishShellOptions());
        projects.AddProject(new FlourishProject("first", "First"));
        projects.AddProject(
            new FlourishProject("second", "Second", "second.external"),
            activate: false
        );
        FlourishProjectActivationRequestedEventArgs? activationRequest = null;
        projects.ProjectActivationRequested += (_, args) => activationRequest = args;
        var savedPath = Path.Combine(directory.Path, "First.txt");
        var dialog = new RecordingSaveFileDialog(savedPath);
        var messages = CreateSavePromptMessageService(save: true);
        var sut = CreateBehavior(projects, dialog, messages.Object);

        var result = await sut.ActivateProjectAsync("second");

        Assert.True(result);
        Assert.True(File.Exists(savedPath));
        Assert.Equal("second", activationRequest?.Project.Id);
        Assert.Equal("second", projects.Current.ActiveProject?.Id);
        Assert.True(projects.TryGetProject("first", out var saved));
        Assert.Equal(Path.GetFullPath(savedPath), saved?.StoragePath);
    }

    [Fact]
    public async Task DeleteProjectAsync_DeletesManagedTextFileAndActivatesFirstRemainingProject()
    {
        using var directory = new TemporaryDirectory();
        var managedPath = Path.Combine(directory.Path, "Managed.txt");
        await File.WriteAllTextAsync(managedPath, "placeholder");
        var projects = new ProjectService(new FlourishShellOptions());
        projects.AddProject(new FlourishProject("first", "First", managedPath));
        projects.AddProject(
            new FlourishProject("second", "Second", "second.external"),
            activate: false
        );
        var messages = CreateStandardMessageService(MessageBoxResult.Yes);
        var sut = CreateBehavior(
            projects,
            new RecordingSaveFileDialog(),
            messages.Object
        );

        var result = await sut.DeleteProjectAsync("first");

        Assert.True(result);
        Assert.False(File.Exists(managedPath));
        Assert.Equal("second", Assert.Single(projects.Current.Projects).Id);
        Assert.Equal("second", projects.Current.ActiveProject?.Id);
    }

    [Fact]
    public async Task DeleteProjectAsync_PreservesNonTextFileAndCreatesUnnamedReplacement()
    {
        using var directory = new TemporaryDirectory();
        var externalPath = Path.Combine(directory.Path, "External.project");
        await File.WriteAllTextAsync(externalPath, "application-owned");
        var projects = new ProjectService(
            new FlourishShellOptions { UnnamedProjectPlaceholder = "Untitled" }
        );
        projects.AddProject(new FlourishProject("only", "Only", externalPath));
        var messages = CreateStandardMessageService(MessageBoxResult.Yes);
        var sut = CreateBehavior(
            projects,
            new RecordingSaveFileDialog(),
            messages.Object
        );

        var result = await sut.DeleteProjectAsync("only");

        Assert.True(result);
        Assert.True(File.Exists(externalPath));
        var replacement = Assert.Single(projects.Current.Projects);
        Assert.Equal("Untitled", replacement.Name);
        Assert.Null(replacement.StoragePath);
        Assert.Equal(replacement, projects.Current.ActiveProject);
    }

    [Fact]
    public async Task DeleteProjectAsync_WhenStoragePathIsShared_PreservesReferencedFile()
    {
        using var directory = new TemporaryDirectory();
        var sharedPath = Path.Combine(directory.Path, "Shared.txt");
        await File.WriteAllTextAsync(sharedPath, "shared content");
        var projects = new ProjectService(new FlourishShellOptions());
        projects.AddProject(new FlourishProject("first", "First", sharedPath));
        projects.AddProject(
            new FlourishProject("second", "Second", sharedPath),
            activate: false
        );
        var sut = CreateBehavior(
            projects,
            new RecordingSaveFileDialog(),
            CreateStandardMessageService(MessageBoxResult.Yes).Object
        );

        var result = await sut.DeleteProjectAsync("first");

        Assert.True(result);
        Assert.Equal("shared content", await File.ReadAllTextAsync(sharedPath));
        Assert.Equal("second", Assert.Single(projects.Current.Projects).Id);
        Assert.Equal("second", projects.Current.ActiveProject?.Id);
    }

    [Fact]
    public async Task DeleteProjectAsync_WhenCatalogPersistenceFails_RestoresManagedFileAndState()
    {
        using var directory = new TemporaryDirectory();
        var managedPath = Path.Combine(directory.Path, "Managed.txt");
        await File.WriteAllTextAsync(managedPath, "recoverable content");
        var project = new FlourishProject("only", "Only", managedPath);
        var catalogStore = new FailingProjectCatalogStore(
            new ProjectCatalog([project], project.Id)
        );
        var projects = new ProjectService(new FlourishShellOptions(), catalogStore);
        var messages = CreateStandardMessageService(MessageBoxResult.Yes);
        var sut = CreateBehavior(
            projects,
            new RecordingSaveFileDialog(),
            messages.Object
        );

        await Assert.ThrowsAsync<IOException>(() =>
            sut.DeleteProjectAsync(project.Id).AsTask()
        );

        Assert.Equal("recoverable content", await File.ReadAllTextAsync(managedPath));
        Assert.Equal(project, Assert.Single(projects.Current.Projects));
        Assert.Equal(project, projects.Current.ActiveProject);
        Assert.Equal(1, catalogStore.SaveCallCount);
        Assert.Empty(Directory.EnumerateFiles(directory.Path, ".*.delete"));
    }

    [Fact]
    public async Task CreateProjectAsync_WhenChangedObserverThrows_KeepsCommittedProjectFile()
    {
        using var directory = new TemporaryDirectory();
        var selectedPath = Path.Combine(directory.Path, "Committed.txt");
        var projects = new ProjectService(new FlourishShellOptions());
        projects.Changed += (_, _) => throw new InvalidOperationException("observer failed");
        var sut = CreateBehavior(
            projects,
            new RecordingSaveFileDialog(selectedPath),
            new Mock<IMessageService>().Object
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.CreateProjectAsync().AsTask()
        );

        Assert.True(File.Exists(selectedPath));
        Assert.Equal(Path.GetFullPath(selectedPath), projects.Current.ActiveProject?.StoragePath);
    }

    [Fact]
    public async Task CanCloseAsync_WhenUnsavedSaveIsCanceled_ReturnsFalse()
    {
        var projects = new ProjectService(new FlourishShellOptions());
        projects.AddProject(new FlourishProject("draft", "Draft"));
        IReadOnlyList<FlourishMessageOption>? choices = null;
        var messages = CreateCustomPromptMessageService(
            "cancel",
            observedChoices => choices = observedChoices
        );
        var sut = CreateBehavior(
            projects,
            new RecordingSaveFileDialog(),
            messages.Object
        );

        var result = await sut.CanCloseAsync();

        Assert.False(result);
        Assert.Null(projects.Current.ActiveProject?.StoragePath);
        Assert.NotNull(choices);
        Assert.Equal(
            ["cancel", "dont-save", "save"],
            choices.Select(choice => choice.Id)
        );
        Assert.Equal("Don't save", choices.Single(choice => choice.Id == "dont-save").Text);
        Assert.True(choices.Single(choice => choice.Id == "cancel").IsCancel);
        Assert.True(choices.Single(choice => choice.Id == "save").IsDefault);
        Assert.True(choices.Single(choice => choice.Id == "save").IsPrimary);
    }

    [Fact]
    public async Task CanCloseAsync_WhenDontSaveIsSelected_AllowsCloseWithoutPersistingProject()
    {
        var projects = new ProjectService(new FlourishShellOptions());
        projects.AddProject(new FlourishProject("draft", "Draft"));
        var dialog = new RecordingSaveFileDialog();
        var messages = CreateCustomPromptMessageService("dont-save");
        var sut = CreateBehavior(projects, dialog, messages.Object);

        var result = await sut.CanCloseAsync();

        Assert.True(result);
        Assert.Empty(dialog.Requests);
        Assert.Equal("Draft", projects.Current.ActiveProject?.Name);
        Assert.Null(projects.Current.ActiveProject?.StoragePath);
    }

    [Fact]
    public async Task CanCloseAsync_WhenSaveIsSelected_SavesProjectAndAllowsClose()
    {
        using var directory = new TemporaryDirectory();
        var storagePath = Path.Combine(directory.Path, "Saved before close.txt");
        var projects = new ProjectService(new FlourishShellOptions());
        projects.AddProject(new FlourishProject("draft", "Draft"));
        var dialog = new RecordingSaveFileDialog(storagePath);
        var messages = CreateCustomPromptMessageService("save");
        var sut = CreateBehavior(projects, dialog, messages.Object);

        var result = await sut.CanCloseAsync();

        Assert.True(result);
        Assert.True(File.Exists(storagePath));
        Assert.Equal("Saved before close", projects.Current.ActiveProject?.Name);
        Assert.Equal(Path.GetFullPath(storagePath), projects.Current.ActiveProject?.StoragePath);
    }

    private static DefaultProjectBehavior CreateBehavior(
        ProjectService projects,
        IProjectSaveFileDialog dialog,
        IMessageService messages
    ) =>
        new(
            projects,
            dialog,
            messages,
            new FlourishLocalizationService(new FlourishDataOptions())
        );

    private static Mock<IMessageService> CreateStandardMessageService(MessageBoxResult result)
    {
        var messages = new Mock<IMessageService>(MockBehavior.Strict);
        messages
            .Setup(service =>
                service.ShowAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<MessageBoxButton>(),
                    It.IsAny<MessageBoxImage>(),
                    It.IsAny<MessageBoxResult>(),
                    It.IsAny<MessageBoxOptions>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(result);
        return messages;
    }

    private static Mock<IMessageService> CreateSavePromptMessageService(bool save)
        => CreateCustomPromptMessageService(save ? "save" : "cancel");

    private static Mock<IMessageService> CreateCustomPromptMessageService(
        string selectedOptionId,
        Action<IReadOnlyList<FlourishMessageOption>>? observeChoices = null
    )
    {
        var messages = new Mock<IMessageService>(MockBehavior.Strict);
        messages
            .Setup(service =>
                service.ShowAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyList<FlourishMessageOption>>(),
                    It.IsAny<MessageBoxImage>(),
                    It.IsAny<MessageBoxOptions>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback(
                (
                    string _,
                    string _,
                    IReadOnlyList<FlourishMessageOption> choices,
                    MessageBoxImage _,
                    MessageBoxOptions _,
                    CancellationToken _
                ) => observeChoices?.Invoke(choices)
            )
            .ReturnsAsync(new FlourishMessageOption(selectedOptionId, selectedOptionId));
        return messages;
    }

    private sealed class RecordingSaveFileDialog(params string?[] results)
        : IProjectSaveFileDialog
    {
        private readonly Queue<string?> results = new(results);

        public List<ProjectSaveFileDialogRequest> Requests { get; } = [];

        public ValueTask<string?> ShowAsync(
            ProjectSaveFileDialogRequest request,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests.Add(request);
            return ValueTask.FromResult(results.Dequeue());
        }
    }

    private sealed class FailingProjectCatalogStore(ProjectCatalog catalog)
        : IProjectCatalogStore
    {
        public int SaveCallCount { get; private set; }

        public ProjectCatalog Load() => catalog;

        public void Save(ProjectCatalog catalog)
        {
            SaveCallCount++;
            throw new IOException("catalog persistence failed");
        }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"flourish-project-behavior-{Guid.NewGuid():N}"
            );
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
