using System.Windows;

namespace AcksheedSys.Flourish.Controls;

internal static class HoverReveal
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled",
        typeof(bool),
        typeof(HoverReveal),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits)
    );

    public static bool GetIsEnabled(DependencyObject element)
    {
        return (bool)element.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(DependencyObject element, bool value)
    {
        element.SetValue(IsEnabledProperty, value);
    }
}
