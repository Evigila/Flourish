using System.Windows;

namespace ArkheideSystem.Flourish.Themes;

/// <summary>
/// Loads the complete Flourish control and theme resource graph.
/// </summary>
/// <remarks>
/// Add one instance to <see cref="ResourceDictionary.MergedDictionaries" /> when Flourish
/// controls must be available in the WPF designer, before the shell starts, or independently
/// from the Flourish shell.
/// </remarks>
public sealed class FlourishThemeResources : ResourceDictionary
{
    internal const string GenericThemeSource =
        "/Flourish;component/Themes/Generic.xaml";

    /// <summary>
    /// Initializes a resource dictionary backed by the Flourish generic theme.
    /// </summary>
    public FlourishThemeResources()
    {
        Source = new Uri(GenericThemeSource, UriKind.Relative);
    }

    internal static void EnsureMerged(ResourceDictionary resources)
    {
        ArgumentNullException.ThrowIfNull(resources);
        if (resources.MergedDictionaries.Any(IsFlourishDictionary))
        {
            return;
        }

        resources.MergedDictionaries.Add(new FlourishThemeResources());
    }

    private static bool IsFlourishDictionary(ResourceDictionary dictionary)
    {
        if (
            dictionary is FlourishThemeResources
            || dictionary.GetType().FullName
                is "ArkheideSystem.Flourish.Styles.FlourishStyles"
                    or "ArkheideSystem.Flourish.Controls.FlourishControlResources"
        )
        {
            return true;
        }

        var source = dictionary.Source?.OriginalString.Replace('\\', '/');
        return source?.EndsWith(GenericThemeSource, StringComparison.OrdinalIgnoreCase) == true;
    }
}
