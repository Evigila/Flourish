using System;

using ArkheideSystem.Flourish.Abstract;
using System.Windows;
using ArkheideSystem.Flourish.Controls;
using ButtonBase = ArkheideSystem.Flourish.Controls.Button;

namespace ArkheideSystem.Flourish.Views.Windows;

internal static class ShellRegionElementFactory
{
    public static FrameworkElement CreateTitlebarActionButton(
        IServiceProvider services,
        string displayName,
        string iconGlyph,
        string? commandKey,
        Action<IServiceProvider>? action
    )
    {
        var hasIcon = !string.IsNullOrWhiteSpace(iconGlyph);
        ButtonBase button = hasIcon
            ? new ButtonBase
            {
                Width = 38,
                MinWidth = 0,
                Padding = new Thickness(),
                Icon = CreateIconOrText(iconGlyph, displayName, "FlourishIconFontSizeTitlebar"),
            }
            : new ButtonBase
            {
                MinWidth = 38,
                Padding = new Thickness(10, 0, 10, 0),
                Content = displayName,
            };
        button.Height = 32;
        button.MinHeight = 0;
        button.Margin = new Thickness(2, 4, 2, 4);
        button.Variant = ButtonVariant.Text;
        button.ToolTip = displayName;
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
        ButtonBase button = string.IsNullOrWhiteSpace(iconGlyph)
            ? new ButtonBase { Content = displayText }
            : new ButtonBase { Icon = iconGlyph, Content = displayText };
        if (!string.IsNullOrWhiteSpace(iconGlyph))
        {
            button.SetResourceReference(ButtonBase.IconSizeProperty, "FlourishIconFontSizeStatusBar");
        }
        button.Margin = new Thickness(8, -2, 0, -2);
        button.Height = 28;
        button.MinWidth = 28;
        button.MinHeight = 0;
        button.Padding = new Thickness(7, 0, 7, 0);
        button.Variant = ButtonVariant.Text;
        button.ToolTip = displayText;
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
                    services.GetService(typeof(ICommandDispatcher)) is ICommandDispatcher dispatcher
                )
                {
                    await dispatcher.ExecuteAsync(commandKey, source: commandSource);
                }
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
            LayoutMode = TextLayoutMode.Control,
            VerticalAlignment = VerticalAlignment.Center,
            Text = string.IsNullOrWhiteSpace(iconGlyph) ? fallbackText : iconGlyph,
            TextAlignment = TextAlignment.Center,
        };
        if (!string.IsNullOrWhiteSpace(iconGlyph))
        {
            text.Role = TextRole.Icon;
            text.SetResourceReference(
                TextBlock.FontFamilyProperty,
                "FlourishIconFontFamily"
            );
        }

        text.SetResourceReference(TextBlock.FontSizeProperty, fontSizeResourceKey);
        return text;
    }
}
