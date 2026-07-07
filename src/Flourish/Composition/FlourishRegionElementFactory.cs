using System.Windows;
using System.Windows.Controls;
using AckSS.Flourish.Services;
using Button = System.Windows.Controls.Button;
using Orientation = System.Windows.Controls.Orientation;

namespace AckSS.Flourish.Composition;

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
        var button = new Button
        {
            Width = 38,
            Height = 32,
            Margin = new Thickness(2, 4, 2, 4),
            Content = CreateIconOrText(iconGlyph, displayName, "FlourishFontSizeTitlebarIcon"),
            ToolTip = displayName,
        };
        button.SetResourceReference(FrameworkElement.StyleProperty, "FlourishIconButtonStyle");
        AttachClick(button, services, commandKey, action);
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
        var button = new Button
        {
            Margin = new Thickness(8, -2, 0, -2),
            Content = CreateIconTextContent(iconGlyph, displayText),
            ToolTip = displayText,
        };
        button.SetResourceReference(FrameworkElement.StyleProperty, "FlourishToolbarButtonStyle");
        AttachClick(button, services, commandKey, action);
        return button;
    }

    private static void AttachClick(
        Button button,
        IServiceProvider services,
        string? commandKey,
        Action<IServiceProvider>? action
    )
    {
        if (string.IsNullOrWhiteSpace(commandKey) && action is null)
        {
            button.IsEnabled = false;
            return;
        }

        button.Click += (_, _) =>
        {
            action?.Invoke(services);
            if (!string.IsNullOrWhiteSpace(commandKey))
            {
                var parser = services.GetService(typeof(CommandParser)) as CommandParser;
                parser?.Parse(commandKey);
            }
        };
    }

    private static TextBlock CreateIconOrText(
        string iconGlyph,
        string fallbackText,
        string fontSizeResourceKey
    )
    {
        var text = new TextBlock
        {
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = string.IsNullOrWhiteSpace(iconGlyph) ? fallbackText : iconGlyph,
            TextAlignment = TextAlignment.Center,
        };
        if (!string.IsNullOrWhiteSpace(iconGlyph))
        {
            text.SetResourceReference(TextBlock.FontFamilyProperty, "FlourishIconFontFamily");
        }

        text.SetResourceReference(TextBlock.FontSizeProperty, fontSizeResourceKey);
        return text;
    }

    private static StackPanel CreateIconTextContent(string iconGlyph, string label)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal };
        if (!string.IsNullOrWhiteSpace(iconGlyph))
        {
            var icon = new TextBlock
            {
                Margin = new Thickness(0, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Text = iconGlyph,
            };
            icon.SetResourceReference(TextBlock.FontFamilyProperty, "FlourishIconFontFamily");
            icon.SetResourceReference(TextBlock.FontSizeProperty, "FlourishFontSizeCaption");
            content.Children.Add(icon);
        }

        var text = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Text = label,
        };
        text.SetResourceReference(TextBlock.FontSizeProperty, "FlourishFontSizeCaption");
        content.Children.Add(text);
        return content;
    }
}
