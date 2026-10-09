using System.Windows;

namespace ArkheideSystem.Flourish.WPF;

/// <summary>Native animation policy bound to the optional Design scope and Windows preference.</summary>
public static class Motion
{
    public static readonly DependencyProperty ReducedProperty = DependencyProperty.RegisterAttached("Reduced", typeof(bool), typeof(Motion), new FrameworkPropertyMetadata(!SystemParameters.ClientAreaAnimation, FrameworkPropertyMetadataOptions.Inherits));
    public static bool GetReduced(DependencyObject target) => (bool)target.GetValue(ReducedProperty);
    public static void SetReduced(DependencyObject target, bool value) => target.SetValue(ReducedProperty, value);
}
