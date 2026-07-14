using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;
using ButtonBase = ArkheideSystem.Flourish.Controls.Button;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal static class FlourishRegionElementFactory
{
    public static FrameworkElement CreateTitlebarActionButton(
        IServiceProvider services,
        string displayName,
        string iconGlyph,
        string? commandKey,
        Action<IServiceProvider>? action
    )
    {
        var button = new IconButton
        {
            Width = 38,
            Height = 32,
            MinWidth = 0,
            MinHeight = 0,
            Padding = new Thickness(),
            Margin = new Thickness(2, 4, 2, 4),
            Icon = CreateIconOrText(iconGlyph, displayName, "FlourishFontSizeTitlebarIcon"),
            Appearance = ButtonAppearance.Subtle,
            ToolTip = new FlourishToolTip { Content = displayName },
        };
        AttachClick(button, services, commandKey, action, CommandSource.TitleBar);
        return button;
    }

    public static FrameworkElement CreateFooterCommandButton(
        IServiceProvider services,
        string displayText,
        string iconGlyph,
        string? commandKey,
        Action<IServiceProvider>? action
    )
    {
        var button = new IconButton
        {
            Margin = new Thickness(8, -2, 0, -2),
            Height = 28,
            MinWidth = 28,
            MinHeight = 0,
            Padding = new Thickness(7, 0, 7, 0),
            Icon = string.IsNullOrWhiteSpace(iconGlyph) ? null : iconGlyph,
            Content = displayText,
            Appearance = ButtonAppearance.Subtle,
            ToolTip = new FlourishToolTip { Content = displayText },
        };
        AttachClick(button, services, commandKey, action, CommandSource.StatusBar);
        return button;
    }

    private static void AttachClick(
        ButtonBase button,
        IServiceProvider services,
        string? commandKey,
        Action<IServiceProvider>? action,
        CommandSource commandSource
    )
    {
        if (string.IsNullOrWhiteSpace(commandKey) && action is null)
        {
            button.IsEnabled = false;
            return;
        }

        button.Click += async (_, _) =>
        {
            action?.Invoke(services);
            if (!string.IsNullOrWhiteSpace(commandKey))
            {
                if (
                    services.GetService(typeof(ICommandDispatcher))
                    is ICommandDispatcher dispatcher
                )
                {
                    await dispatcher.ExecuteAsync(commandKey, source: commandSource);
                }
            }
        };
    }

    private static FlourishTextBlock CreateIconOrText(
        string iconGlyph,
        string fallbackText,
        string fontSizeResourceKey
    )
    {
        var text = new FlourishTextBlock
        {
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = string.IsNullOrWhiteSpace(iconGlyph) ? fallbackText : iconGlyph,
            TextAlignment = TextAlignment.Center,
        };
        if (!string.IsNullOrWhiteSpace(iconGlyph))
        {
            text.SetResourceReference(FlourishTextBlock.FontFamilyProperty, "FlourishIconFontFamily");
        }

        text.SetResourceReference(FlourishTextBlock.FontSizeProperty, fontSizeResourceKey);
        return text;
    }

}
