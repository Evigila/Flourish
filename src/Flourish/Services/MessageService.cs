using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Windows;
using Application = System.Windows.Application;
using MessageBoxOptions = System.Windows.MessageBoxOptions;

namespace ArkheideSystem.Flourish.Services;

internal sealed class MessageService(FlourishLocalizationService localizationService)
    : IMessageService
{
    private const string GenericThemeSource = "/Flourish;component/Themes/Generic.xaml";

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

        if (
            application.Resources.MergedDictionaries.Any(dictionary =>
                dictionary.Source?.OriginalString == GenericThemeSource
            )
        )
        {
            return;
        }

        application.Resources.MergedDictionaries.Add(
            new ResourceDictionary { Source = new Uri(GenericThemeSource, UriKind.Relative) }
        );
    }
}
