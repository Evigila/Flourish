using System.Windows;
using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Windows;
using Application = System.Windows.Application;
using MessageBoxOptions = System.Windows.MessageBoxOptions;

namespace AcksheedSys.Flourish.Services;

internal sealed class MessageService : IMessageService
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
            options
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
            ValidateChoices(choices),
            icon,
            options
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

    private static IReadOnlyList<FlourishMessageOption> ValidateChoices(
        IReadOnlyList<FlourishMessageOption> choices
    )
    {
        ArgumentNullException.ThrowIfNull(choices);

        if (choices.Count == 0)
        {
            throw new ArgumentException(
                "At least one message option must be provided.",
                nameof(choices)
            );
        }

        var normalizedChoices = choices.ToArray();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var defaultCount = 0;
        var cancelCount = 0;
        var primaryCount = 0;

        foreach (var choice in normalizedChoices)
        {
            if (choice is null)
            {
                throw new ArgumentException("Message options cannot contain null.", nameof(choices));
            }

            if (string.IsNullOrWhiteSpace(choice.Id))
            {
                throw new ArgumentException("Message option ids cannot be empty.", nameof(choices));
            }

            if (string.IsNullOrWhiteSpace(choice.Text))
            {
                throw new ArgumentException(
                    "Message option text cannot be empty.",
                    nameof(choices)
                );
            }

            if (!ids.Add(choice.Id))
            {
                throw new ArgumentException(
                    $"Message option ids must be unique. Duplicate id: '{choice.Id}'.",
                    nameof(choices)
                );
            }

            if (choice.IsDefault)
            {
                defaultCount++;
            }

            if (choice.IsCancel)
            {
                cancelCount++;
            }

            if (choice.IsPrimary)
            {
                primaryCount++;
            }
        }

        if (defaultCount > 1)
        {
            throw new ArgumentException(
                "Only one message option can be marked as the default option.",
                nameof(choices)
            );
        }

        if (cancelCount > 1)
        {
            throw new ArgumentException(
                "Only one message option can be marked as the cancel option.",
                nameof(choices)
            );
        }

        if (primaryCount > 1)
        {
            throw new ArgumentException(
                "Only one message option can be marked as the primary option.",
                nameof(choices)
            );
        }

        return normalizedChoices;
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
