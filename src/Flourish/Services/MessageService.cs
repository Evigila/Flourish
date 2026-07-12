using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Themes;
using ArkheideSystem.Flourish.Views.Windows;
using Application = System.Windows.Application;
using MessageBoxOptions = System.Windows.MessageBoxOptions;

namespace ArkheideSystem.Flourish.Services;

internal sealed class MessageService(FlourishLocalizationService localizationService)
    : IMessageService
{
    public Task<MessageBoxResult> ShowAsync(
        string messageBoxText,
        string caption = "",
        MessageBoxButton button = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxResult defaultResult = MessageBoxResult.None,
        MessageBoxOptions options = MessageBoxOptions.None,
        CancellationToken cancellationToken = default
    )
    {
        return InvokeOnDispatcherAsync(
            () => Show(
                GetActiveOwner(),
                messageBoxText,
                caption,
                button,
                icon,
                defaultResult,
                options
            ),
            cancellationToken
        );
    }

    public Task<MessageBoxResult> ShowAsync(
        Window? owner,
        string messageBoxText,
        string caption = "",
        MessageBoxButton button = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxResult defaultResult = MessageBoxResult.None,
        MessageBoxOptions options = MessageBoxOptions.None,
        CancellationToken cancellationToken = default
    )
    {
        return InvokeOnDispatcherAsync(
            () => Show(owner, messageBoxText, caption, button, icon, defaultResult, options),
            cancellationToken
        );
    }

    public Task<FlourishMessageOption?> ShowAsync(
        string messageBoxText,
        string caption,
        IReadOnlyList<FlourishMessageOption> choices,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxOptions options = MessageBoxOptions.None,
        CancellationToken cancellationToken = default
    )
    {
        return InvokeOnDispatcherAsync(
            () => Show(GetActiveOwner(), messageBoxText, caption, choices, icon, options),
            cancellationToken
        );
    }

    public Task<FlourishMessageOption?> ShowAsync(
        Window? owner,
        string messageBoxText,
        string caption,
        IReadOnlyList<FlourishMessageOption> choices,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxOptions options = MessageBoxOptions.None,
        CancellationToken cancellationToken = default
    )
    {
        return InvokeOnDispatcherAsync(
            () => Show(owner, messageBoxText, caption, choices, icon, options),
            cancellationToken
        );
    }

    public MessageBoxResult Show(
        string messageBoxText,
        string caption = "",
        MessageBoxButton button = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxResult defaultResult = MessageBoxResult.None,
        MessageBoxOptions options = MessageBoxOptions.None
    )
    {
        return Show(GetActiveOwner(), messageBoxText, caption, button, icon, defaultResult, options);
    }

    public MessageBoxResult Show(
        Window? owner,
        string messageBoxText,
        string caption = "",
        MessageBoxButton button = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxResult defaultResult = MessageBoxResult.None,
        MessageBoxOptions options = MessageBoxOptions.None
    )
    {
        EnsureApplicationResources();

        var dialog = new FlourishMessageBoxWindow(
            messageBoxText,
            caption,
            button,
            icon,
            defaultResult,
            options,
            localizationService
        );
        ConfigureOwner(dialog, owner);

        dialog.ShowDialog();
        return dialog.Result;
    }

    public FlourishMessageOption? Show(
        string messageBoxText,
        string caption,
        IReadOnlyList<FlourishMessageOption> choices,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxOptions options = MessageBoxOptions.None
    )
    {
        return Show(GetActiveOwner(), messageBoxText, caption, choices, icon, options);
    }

    public FlourishMessageOption? Show(
        Window? owner,
        string messageBoxText,
        string caption,
        IReadOnlyList<FlourishMessageOption> choices,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxOptions options = MessageBoxOptions.None
    )
    {
        EnsureApplicationResources();

        var dialog = new FlourishMessageBoxWindow(
            messageBoxText,
            caption,
            FlourishMessageOptionValidator.Validate(choices),
            icon,
            options,
            localizationService
        );
        ConfigureOwner(dialog, owner);

        dialog.ShowDialog();
        return dialog.SelectedOption;
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

    private static void ConfigureOwner(Window dialog, Window? owner)
    {
        if (owner is not null && owner.IsVisible)
        {
            dialog.Owner = owner;
            return;
        }

        dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        dialog.ShowInTaskbar = true;
    }

    private static void EnsureApplicationResources()
    {
        var application = Application.Current;
        if (application is null)
        {
            return;
        }

        FlourishThemeResources.EnsureMerged(application.Resources);
    }

    private static async Task<TResult> InvokeOnDispatcherAsync<TResult>(
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
