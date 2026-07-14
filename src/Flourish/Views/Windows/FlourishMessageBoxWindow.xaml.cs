using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Services;
using Button = ArkheideSystem.Flourish.Controls.Button;
using Key = System.Windows.Input.Key;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBoxOptions = System.Windows.MessageBoxOptions;
using WpfFlowDirection = System.Windows.FlowDirection;

namespace ArkheideSystem.Flourish.Views.Windows;

internal partial class FlourishMessageBoxWindow : Window
{
    private readonly object? closeSelection;
    private readonly FlourishLocalizationService localizationService;
    private readonly WindowFrameFixService windowFrameFixService = new();
    private object? selection;

    public FlourishMessageBoxWindow(
        string messageBoxText,
        string caption,
        MessageBoxButton buttons,
        MessageBoxImage icon,
        MessageBoxResult defaultResult,
        MessageBoxOptions options,
        FlourishLocalizationService localizationService
    )
    {
        this.localizationService = localizationService;
        InitializeComponent();
        windowFrameFixService.Attach(this);

        closeSelection = GetCloseResult(buttons);
        ConfigureDialog(messageBoxText, caption, options);
        ConfigureIcon(icon);
        ConfigureButtons(CreateStandardButtons(buttons, defaultResult));
    }

    public FlourishMessageBoxWindow(
        string messageBoxText,
        string caption,
        IReadOnlyList<FlourishMessageOption> choices,
        MessageBoxImage icon,
        MessageBoxOptions options,
        FlourishLocalizationService localizationService
    )
    {
        this.localizationService = localizationService;
        InitializeComponent();
        windowFrameFixService.Attach(this);

        closeSelection = choices.FirstOrDefault(choice => choice.IsCancel);
        ConfigureDialog(messageBoxText, caption, options);
        ConfigureIcon(icon);
        ConfigureButtons(CreateCustomButtons(choices));
    }

    public MessageBoxResult Result =>
        selection is MessageBoxResult result ? result : MessageBoxResult.None;

    public FlourishMessageOption? SelectedOption => selection as FlourishMessageOption;

    protected override void OnClosing(CancelEventArgs e)
    {
        selection ??= closeSelection;

        base.OnClosing(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            selection = closeSelection;
            DialogResult = false;
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private void ConfigureDialog(
        string messageBoxText,
        string caption,
        MessageBoxOptions options
    )
    {
        Title = caption;
        CaptionText.Text = caption;
        CaptionText.Visibility = string.IsNullOrWhiteSpace(caption)
            ? Visibility.Collapsed
            : Visibility.Visible;
        MessageText.Text = messageBoxText;
        MessageText.TextAlignment = options.HasFlag(MessageBoxOptions.RightAlign)
            ? TextAlignment.Right
            : TextAlignment.Left;
        FlowDirection = options.HasFlag(MessageBoxOptions.RtlReading)
            ? WpfFlowDirection.RightToLeft
            : WpfFlowDirection.LeftToRight;
    }

    private void ConfigureIcon(MessageBoxImage icon)
    {
        var iconGlyph = GetIconGlyph(icon);
        if (iconGlyph is null)
        {
            IconHost.Visibility = Visibility.Collapsed;
            return;
        }

        IconText.Text = iconGlyph;
        var (backgroundKey, foregroundKey) = GetIconBrushKeys(icon);
        IconHost.SetResourceReference(Border.BackgroundProperty, backgroundKey);
        IconText.SetResourceReference(TextBlock.ForegroundProperty, foregroundKey);
    }

    private void ConfigureButtons(IReadOnlyList<MessageDialogButton> buttonDefinitions)
    {
        foreach (var buttonDefinition in buttonDefinitions)
        {
            var button = new Button
            {
                Appearance = buttonDefinition.IsPrimary
                    ? ButtonAppearance.Primary
                    : ButtonAppearance.Standard,
                MinWidth = 72,
                Height = 30,
                Padding = new Thickness(14, 0, 14, 0),
                Content = buttonDefinition.Text,
                IsDefault = buttonDefinition.IsDefault,
                IsCancel = buttonDefinition.IsCancel,
                Margin =
                    ButtonsHost.Children.Count > 0
                        ? new Thickness(8, 0, 0, 0)
                        : new Thickness(),
                Tag = buttonDefinition.Selection,
            };
            button.Click += ResultButton_Click;
            ButtonsHost.Children.Add(button);
        }
    }

    private void ResultButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: { } selectedResult })
        {
            selection = selectedResult;
            DialogResult = true;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        selection = closeSelection;
        DialogResult = false;
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private static IReadOnlyList<MessageBoxResult> GetButtonResults(MessageBoxButton buttons)
    {
        return buttons switch
        {
            MessageBoxButton.OKCancel => [MessageBoxResult.OK, MessageBoxResult.Cancel],
            MessageBoxButton.YesNo => [MessageBoxResult.No, MessageBoxResult.Yes],
            MessageBoxButton.YesNoCancel => [
                MessageBoxResult.Cancel,
                MessageBoxResult.No,
                MessageBoxResult.Yes,
            ],
            _ => [MessageBoxResult.OK],
        };
    }

    private static MessageBoxResult GetEffectiveDefaultResult(
        MessageBoxButton buttons,
        MessageBoxResult defaultResult
    )
    {
        var availableResults = GetButtonResults(buttons);
        if (defaultResult != MessageBoxResult.None && availableResults.Contains(defaultResult))
        {
            return defaultResult;
        }

        return buttons switch
        {
            MessageBoxButton.YesNo or MessageBoxButton.YesNoCancel => MessageBoxResult.Yes,
            _ => availableResults[0],
        };
    }

    private IReadOnlyList<MessageDialogButton> CreateStandardButtons(
        MessageBoxButton buttons,
        MessageBoxResult defaultResult
    )
    {
        var buttonResults = GetButtonResults(buttons);
        var effectiveDefaultResult = GetEffectiveDefaultResult(buttons, defaultResult);

        return buttonResults
            .Select(buttonResult => new MessageDialogButton(
                GetButtonText(buttonResult),
                buttonResult,
                buttonResult == effectiveDefaultResult,
                IsCancelResult(buttons, buttonResult),
                buttonResult == effectiveDefaultResult
            ))
            .ToArray();
    }

    private static IReadOnlyList<MessageDialogButton> CreateCustomButtons(
        IReadOnlyList<FlourishMessageOption> choices
    )
    {
        var defaultChoice = choices.FirstOrDefault(choice => choice.IsDefault) ?? choices[0];
        var primaryChoice = choices.FirstOrDefault(choice => choice.IsPrimary) ?? defaultChoice;

        return choices
            .Select(choice => new MessageDialogButton(
                choice.Text,
                choice,
                choice.Id == defaultChoice.Id,
                choice.IsCancel,
                choice.Id == primaryChoice.Id
            ))
            .ToArray();
    }

    private static bool IsCancelResult(MessageBoxButton buttons, MessageBoxResult result)
    {
        return buttons switch
        {
            MessageBoxButton.OK => result == MessageBoxResult.OK,
            MessageBoxButton.OKCancel or MessageBoxButton.YesNoCancel =>
                result == MessageBoxResult.Cancel,
            _ => false,
        };
    }

    private static MessageBoxResult GetCloseResult(MessageBoxButton buttons)
    {
        return buttons switch
        {
            MessageBoxButton.OK => MessageBoxResult.OK,
            MessageBoxButton.OKCancel or MessageBoxButton.YesNoCancel => MessageBoxResult.Cancel,
            MessageBoxButton.YesNo => MessageBoxResult.No,
            _ => MessageBoxResult.None,
        };
    }

    private string GetButtonText(MessageBoxResult result)
    {
        return result switch
        {
            MessageBoxResult.OK => localizationService.Get(FlourishLocaleKeys.MessageBoxOk),
            MessageBoxResult.Cancel => localizationService.Get(
                FlourishLocaleKeys.MessageBoxCancel
            ),
            MessageBoxResult.Yes => localizationService.Get(FlourishLocaleKeys.MessageBoxYes),
            MessageBoxResult.No => localizationService.Get(FlourishLocaleKeys.MessageBoxNo),
            _ => result.ToString(),
        };
    }

    private static string? GetIconGlyph(MessageBoxImage icon)
    {
        if (icon == MessageBoxImage.None)
        {
            return null;
        }

        if (icon == MessageBoxImage.Hand)
        {
            return "\uEA39";
        }

        if (icon == MessageBoxImage.Question)
        {
            return "\uE897";
        }

        if (icon == MessageBoxImage.Exclamation)
        {
            return "\uE7BA";
        }

        return "\uE946";
    }

    private static (string BackgroundKey, string ForegroundKey) GetIconBrushKeys(
        MessageBoxImage icon
    )
    {
        if (icon == MessageBoxImage.Hand)
        {
            return (
                "FlourishMessageBoxErrorIconBackgroundBrush",
                "FlourishMessageBoxErrorIconForegroundBrush"
            );
        }

        if (icon == MessageBoxImage.Question)
        {
            return (
                "FlourishMessageBoxQuestionIconBackgroundBrush",
                "FlourishMessageBoxQuestionIconForegroundBrush"
            );
        }

        if (icon == MessageBoxImage.Exclamation)
        {
            return (
                "FlourishMessageBoxWarningIconBackgroundBrush",
                "FlourishMessageBoxWarningIconForegroundBrush"
            );
        }

        return (
            "FlourishMessageBoxInfoIconBackgroundBrush",
            "FlourishMessageBoxInfoIconForegroundBrush"
        );
    }

    private sealed record MessageDialogButton(
        string Text,
        object Selection,
        bool IsDefault,
        bool IsCancel,
        bool IsPrimary
    );
}
