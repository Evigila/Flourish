using System.IO;
using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using MessageBoxOptions = System.Windows.MessageBoxOptions;

namespace ArkheideSystem.Flourish.Services;

internal sealed class DefaultProjectBehavior(
    ProjectService projectService,
    IProjectSaveFileDialog saveFileDialog,
    IMessageService messageService,
    IFlourishLocalization localization
) : IProjectBehavior
{
    private const string DefaultProjectFileName = "NewProject";
    private const string SaveOptionId = "save";
    private const string DontSaveOptionId = "dont-save";
    private const string CancelOptionId = "cancel";
    private readonly ProjectService projectService =
        projectService ?? throw new ArgumentNullException(nameof(projectService));
    private readonly IProjectSaveFileDialog saveFileDialog =
        saveFileDialog ?? throw new ArgumentNullException(nameof(saveFileDialog));
    private readonly IMessageService messageService =
        messageService ?? throw new ArgumentNullException(nameof(messageService));
    private readonly IFlourishLocalization localization =
        localization ?? throw new ArgumentNullException(nameof(localization));
    private readonly SemaphoreSlim operationGate = new(1, 1);

    public async ValueTask<bool> CreateProjectAsync(
        CancellationToken cancellationToken = default
    )
    {
        await operationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!await ConfirmAndSaveActiveProjectAsync(cancellationToken).ConfigureAwait(false))
            {
                return false;
            }

            projectService.RequestNewProject();
            var selectedPath = await saveFileDialog
                .ShowAsync(
                    new ProjectSaveFileDialogRequest(DefaultProjectFileName),
                    cancellationToken
                )
                .ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(selectedPath))
            {
                return false;
            }

            var storagePath = NormalizeTextFilePath(selectedPath);
            var createdPlaceholder = false;
            try
            {
                createdPlaceholder = await EnsurePlaceholderFileExistsAsync(
                        storagePath,
                        cancellationToken
                    )
                    .ConfigureAwait(false);
                var project = new FlourishProject(
                    Guid.NewGuid().ToString("N"),
                    GetProjectName(storagePath),
                    storagePath
                );
                projectService.AddProject(project);
                return true;
            }
            catch
            {
                if (
                    createdPlaceholder
                    && !ProjectCatalogReferencesPath(projectService.Current, storagePath)
                )
                {
                    TryDeleteFile(storagePath);
                }

                throw;
            }
        }
        finally
        {
            operationGate.Release();
        }
    }

    public async ValueTask<bool> SaveActiveProjectAsync(
        CancellationToken cancellationToken = default
    )
    {
        await operationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await SaveActiveProjectCoreAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            operationGate.Release();
        }
    }

    public async ValueTask<bool> ActivateProjectAsync(
        string projectId,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(projectId))
        {
            return false;
        }

        projectId = projectId.Trim();
        await operationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!projectService.TryGetProject(projectId, out _))
            {
                return false;
            }

            var current = projectService.Current.ActiveProject;
            if (StringComparer.Ordinal.Equals(current?.Id, projectId))
            {
                return true;
            }

            if (!await ConfirmAndSaveActiveProjectAsync(cancellationToken).ConfigureAwait(false))
            {
                return false;
            }

            if (!projectService.TryRequestProjectActivation(projectId))
            {
                return false;
            }

            if (!projectService.TryGetProject(projectId, out _))
            {
                return false;
            }

            projectService.SetActiveProject(projectId);
            return true;
        }
        finally
        {
            operationGate.Release();
        }
    }

    public async ValueTask<bool> DeleteProjectAsync(
        string projectId,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(projectId))
        {
            return false;
        }

        projectId = projectId.Trim();
        await operationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!projectService.TryGetProject(projectId, out var project) || project is null)
            {
                return false;
            }

            var result = await messageService
                .ShowAsync(
                    localization.Format(
                        FlourishLocaleKeys.ProjectDeletePrompt,
                        project.Name,
                        project.StoragePath ?? string.Empty
                    ),
                    localization.Get(FlourishLocaleKeys.ProjectDeleteTitle),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No,
                    MessageBoxOptions.None,
                    cancellationToken
                )
                .ConfigureAwait(false);
            if (result != MessageBoxResult.Yes)
            {
                return false;
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (!projectService.TryGetProject(projectId, out project) || project is null)
            {
                return false;
            }

            string? isolatedFilePath = null;
            if (
                IsManagedTextFile(project.StoragePath)
                && File.Exists(project.StoragePath)
                && !ProjectCatalogReferencesPath(
                    projectService.Current,
                    project.StoragePath!,
                    project.Id
                )
            )
            {
                isolatedFilePath = IsolateManagedFile(project.StoragePath!);
            }

            try
            {
                if (!projectService.RemoveProjectAndEnsureActive(projectId))
                {
                    RestoreIsolatedFile(isolatedFilePath, project.StoragePath);
                    return false;
                }

                TryDeleteFile(isolatedFilePath);
                return true;
            }
            catch
            {
                if (projectService.TryGetProject(projectId, out _))
                {
                    RestoreIsolatedFile(isolatedFilePath, project.StoragePath);
                }
                else
                {
                    TryDeleteFile(isolatedFilePath);
                }

                throw;
            }
        }
        finally
        {
            operationGate.Release();
        }
    }

    public async ValueTask<bool> CanCloseAsync(
        CancellationToken cancellationToken = default
    )
    {
        await operationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await ConfirmCloseActiveProjectAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            operationGate.Release();
        }
    }

    private async ValueTask<bool> ConfirmCloseActiveProjectAsync(
        CancellationToken cancellationToken
    )
    {
        var activeProject = projectService.Current.ActiveProject;
        if (activeProject is null || IsPersistedProject(activeProject))
        {
            return true;
        }

        IReadOnlyList<FlourishMessageOption> choices =
        [
            new(CancelOptionId, localization.Get(FlourishLocaleKeys.MessageBoxCancel))
            {
                IsCancel = true,
            },
            new(DontSaveOptionId, localization.Get(FlourishLocaleKeys.ProjectDontSave)),
            new(SaveOptionId, localization.Get(FlourishLocaleKeys.ProjectSave))
            {
                IsDefault = true,
                IsPrimary = true,
            },
        ];
        var result = await messageService
            .ShowAsync(
                localization.Format(
                    FlourishLocaleKeys.ProjectUnsavedPrompt,
                    activeProject.Name
                ),
                localization.Get(FlourishLocaleKeys.ProjectUnsavedTitle),
                choices,
                MessageBoxImage.Warning,
                MessageBoxOptions.None,
                cancellationToken
            )
            .ConfigureAwait(false);
        if (StringComparer.Ordinal.Equals(result?.Id, DontSaveOptionId))
        {
            return true;
        }

        return StringComparer.Ordinal.Equals(result?.Id, SaveOptionId)
            && await SaveActiveProjectCoreAsync(cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<bool> ConfirmAndSaveActiveProjectAsync(
        CancellationToken cancellationToken
    )
    {
        var activeProject = projectService.Current.ActiveProject;
        if (activeProject is null || IsPersistedProject(activeProject))
        {
            return true;
        }

        IReadOnlyList<FlourishMessageOption> choices =
        [
            new(CancelOptionId, localization.Get(FlourishLocaleKeys.MessageBoxCancel))
            {
                IsCancel = true,
            },
            new(SaveOptionId, localization.Get(FlourishLocaleKeys.ProjectSave))
            {
                IsDefault = true,
                IsPrimary = true,
            },
        ];
        var result = await messageService
            .ShowAsync(
                localization.Format(
                    FlourishLocaleKeys.ProjectUnsavedPrompt,
                    activeProject.Name
                ),
                localization.Get(FlourishLocaleKeys.ProjectUnsavedTitle),
                choices,
                MessageBoxImage.Warning,
                MessageBoxOptions.None,
                cancellationToken
            )
            .ConfigureAwait(false);
        return StringComparer.Ordinal.Equals(result?.Id, SaveOptionId)
            && await SaveActiveProjectCoreAsync(cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<bool> SaveActiveProjectCoreAsync(
        CancellationToken cancellationToken
    )
    {
        var activeProject = projectService.Current.ActiveProject;
        if (activeProject is null)
        {
            return false;
        }

        if (IsPersistedProject(activeProject))
        {
            return true;
        }

        var selectedPath = await saveFileDialog
            .ShowAsync(
                new ProjectSaveFileDialogRequest(DefaultProjectFileName),
                cancellationToken
            )
            .ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            return false;
        }

        var storagePath = NormalizeTextFilePath(selectedPath);
        var createdPlaceholder = false;
        try
        {
            createdPlaceholder = await EnsurePlaceholderFileExistsAsync(
                    storagePath,
                    cancellationToken
                )
                .ConfigureAwait(false);
            projectService.SetProjectMetadata(
                activeProject.Id,
                GetProjectName(storagePath),
                storagePath
            );
            return true;
        }
        catch
        {
            if (
                createdPlaceholder
                && !ProjectCatalogReferencesPath(projectService.Current, storagePath)
            )
            {
                TryDeleteFile(storagePath);
            }

            throw;
        }
    }

    private static string NormalizeTextFilePath(string path)
    {
        var normalized = Path.GetFullPath(path.Trim());
        return string.Equals(
            Path.GetExtension(normalized),
            ".txt",
            StringComparison.OrdinalIgnoreCase
        )
            ? normalized
            : normalized + ".txt";
    }

    private static string GetProjectName(string storagePath)
    {
        var name = Path.GetFileNameWithoutExtension(storagePath);
        return string.IsNullOrWhiteSpace(name) ? "Unnamed project" : name;
    }

    private static bool IsManagedTextFile(string? storagePath) =>
        !string.IsNullOrWhiteSpace(storagePath)
        && string.Equals(
            Path.GetExtension(storagePath),
            ".txt",
            StringComparison.OrdinalIgnoreCase
        );

    private static bool IsPersistedProject(FlourishProject project) =>
        project.StoragePath is not null && File.Exists(project.StoragePath);

    private static async ValueTask<bool> EnsurePlaceholderFileExistsAsync(
        string storagePath,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (File.Exists(storagePath))
        {
            return false;
        }

        var directory = Path.GetDirectoryName(storagePath)
            ?? throw new InvalidOperationException("The project file has no parent directory.");
        Directory.CreateDirectory(directory);
        try
        {
            await using var stream = new FileStream(
                storagePath,
                new FileStreamOptions
                {
                    Mode = FileMode.CreateNew,
                    Access = FileAccess.Write,
                    Share = FileShare.Read,
                    Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
                }
            );
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (IOException) when (File.Exists(storagePath))
        {
            return false;
        }
    }

    private static bool ProjectCatalogReferencesPath(
        FlourishProjectSnapshot snapshot,
        string storagePath,
        string? excludedProjectId = null
    )
    {
        var fullPath = Path.GetFullPath(storagePath);
        return snapshot.Projects.Any(project =>
            !StringComparer.Ordinal.Equals(project.Id, excludedProjectId)
            && project.StoragePath is not null
            && StringComparer.OrdinalIgnoreCase.Equals(
                Path.GetFullPath(project.StoragePath),
                fullPath
            )
        );
    }

    private static string IsolateManagedFile(string storagePath)
    {
        var fullPath = Path.GetFullPath(storagePath);
        var directory = Path.GetDirectoryName(fullPath)
            ?? throw new InvalidOperationException("The project file has no parent directory.");
        var isolatedPath = Path.Combine(
            directory,
            $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.delete"
        );
        File.Move(fullPath, isolatedPath);
        return isolatedPath;
    }

    private static void RestoreIsolatedFile(string? isolatedPath, string? storagePath)
    {
        if (
            string.IsNullOrWhiteSpace(isolatedPath)
            || string.IsNullOrWhiteSpace(storagePath)
            || !File.Exists(isolatedPath)
        )
        {
            return;
        }

        File.Move(isolatedPath, Path.GetFullPath(storagePath), overwrite: true);
    }

    private static void TryDeleteFile(string? storagePath)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            return;
        }

        try
        {
            File.Delete(storagePath);
        }
        catch (Exception error) when (
            error is IOException or UnauthorizedAccessException
        )
        {
            // Best-effort cleanup must not hide the catalog operation's result.
        }
    }
}
