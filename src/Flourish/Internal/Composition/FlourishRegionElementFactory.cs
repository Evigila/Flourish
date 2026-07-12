using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Services;
using Orientation = System.Windows.Controls.Orientation;

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
        var button = new FlourishButton
        {
            Width = 38,
            Height = 32,
            Margin = new Thickness(2, 4, 2, 4),
            Content = CreateIconOrText(iconGlyph, displayName, "FlourishFontSizeTitlebarIcon"),
            Appearance = FlourishButtonAppearance.Subtle,
            Variant = FlourishButtonVariant.Icon,
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
        var button = new FlourishButton
        {
            Margin = new Thickness(8, -2, 0, -2),
            Content = CreateIconTextContent(iconGlyph, displayText),
            Appearance = FlourishButtonAppearance.Subtle,
            Variant = FlourishButtonVariant.Toolbar,
            ToolTip = new FlourishToolTip { Content = displayText },
        };
        AttachClick(button, services, commandKey, action, CommandSource.StatusBar);
        return button;
    }

    private static void AttachClick(
        FlourishButton button,
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
                else
                {
                    // Preserve custom service-provider scenarios that only register the historical
                    // concrete parser while applications transition to ICommandDispatcher.
                    var parser = services.GetService(typeof(CommandParser)) as CommandParser;
                    parser?.Parse(commandKey);
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

    private static StackPanel CreateIconTextContent(string iconGlyph, string label)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal };
        if (!string.IsNullOrWhiteSpace(iconGlyph))
        {
            var icon = new FlourishTextBlock
            {
                Margin = new Thickness(0, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Text = iconGlyph,
            };
            icon.SetResourceReference(FlourishTextBlock.FontFamilyProperty, "FlourishIconFontFamily");
            icon.SetResourceReference(FlourishTextBlock.FontSizeProperty, "FlourishFontSizeCaption");
            content.Children.Add(icon);
        }

        var text = new FlourishTextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Text = label,
        };
        text.SetResourceReference(FlourishTextBlock.FontSizeProperty, "FlourishFontSizeCaption");
        content.Children.Add(text);
        return content;
    }
}
