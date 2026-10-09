using System.Windows;
using System.Windows.Media;

namespace ArkheideSystem.Flourish.WPF;

/// <summary>Explicit host resource registration; custom controls also resolve their own native default templates.</summary>
public static class FrameworkResources
{
    public static ResourceDictionary Load() => new() { Source = new Uri("/Flourish.WPF.Framework;component/Themes/Generic.xaml", UriKind.Relative) };
    public static void Apply(FrameworkElement scope)
    {
        ArgumentNullException.ThrowIfNull(scope);
        if (!scope.Resources.MergedDictionaries.Any(d => d.Source?.OriginalString.Contains("Flourish.WPF.Framework;component/Themes/Generic.xaml", StringComparison.Ordinal) == true))
            scope.Resources.MergedDictionaries.Insert(0, Load());
    }
    /// <summary>Native Popup and modal Window have independent trees; share the actual dictionaries, not color snapshots.</summary>
    public static void ShareResources(FrameworkElement origin, FrameworkElement destination)
    {
        destination.Resources.MergedDictionaries.Add(Load());
        var scopes = new Stack<ResourceDictionary>();
        for (DependencyObject? current = origin; current is not null; current = current is Visual ? VisualTreeHelper.GetParent(current) ?? LogicalTreeHelper.GetParent(current) : LogicalTreeHelper.GetParent(current))
            if (current is FrameworkElement element && element.Resources.Count + element.Resources.MergedDictionaries.Count > 0) scopes.Push(element.Resources);
        if (Application.Current is { } app) destination.Resources.MergedDictionaries.Add(app.Resources);
        foreach (var dictionary in scopes) destination.Resources.MergedDictionaries.Add(dictionary);
    }
}
