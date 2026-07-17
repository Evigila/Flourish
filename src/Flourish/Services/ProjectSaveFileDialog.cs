using System.IO;
using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using Application = System.Windows.Application;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace ArkheideSystem.Flourish.Services;

internal sealed record ProjectSaveFileDialogRequest(string SuggestedFileName);

internal interface IProjectSaveFileDialog
{
    ValueTask<string?> ShowAsync(
        ProjectSaveFileDialogRequest request,
        CancellationToken cancellationToken = default
    );
}

internal sealed class ProjectSaveFileDialog(IFlourishLocalization localization)
    : IProjectSaveFileDialog
{
    private readonly IFlourishLocalization localization =
        localization ?? throw new ArgumentNullException(nameof(localization));

    public ValueTask<string?> ShowAsync(
        ProjectSaveFileDialogRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);
        return InvokeOnDispatcherAsync(() => ShowCore(request), cancellationToken);
    }

    private string? ShowCore(ProjectSaveFileDialogRequest request)
    {
        var dialog = new SaveFileDialog
        {
            AddExtension = true,
            CheckPathExists = true,
            DefaultExt = ".txt",
            FileName = GetSafeSuggestedFileName(request.SuggestedFileName),
            Filter = localization.Get(FlourishLocaleKeys.ProjectTextFileFilter),
            OverwritePrompt = true,
            RestoreDirectory = true,
            Title = localization.Get(FlourishLocaleKeys.ProjectSaveDialogTitle),
            ValidateNames = true,
        };
        var owner = GetActiveOwner();
        var accepted = owner is null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
        return accepted == true ? dialog.FileName : null;
    }

    private static string GetSafeSuggestedFileName(string suggestedFileName)
    {
        if (string.IsNullOrWhiteSpace(suggestedFileName))
        {
            return string.Empty;
        }

        var invalidCharacters = Path.GetInvalidFileNameChars();
        return string.Concat(
            suggestedFileName.Trim().Where(character => !invalidCharacters.Contains(character))
        );
    }

    private static Window? GetActiveOwner()
    {
        var application = Application.Current;
        if (application is null)
        {
            return null;
        }

        foreach (Window window in application.Windows)
        {
            if (window.IsActive)
            {
                return window;
            }
        }

        return application.MainWindow?.IsVisible == true ? application.MainWindow : null;
    }

    private static async ValueTask<TResult> InvokeOnDispatcherAsync<TResult>(
        Func<TResult> action,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            return action();
        }

        return await dispatcher
            .InvokeAsync(
                action,
                System.Windows.Threading.DispatcherPriority.Normal,
                cancellationToken
            )
            .Task.ConfigureAwait(false);
    }
}
