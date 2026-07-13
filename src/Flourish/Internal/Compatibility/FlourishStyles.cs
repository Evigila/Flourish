using System.ComponentModel;
using System.Windows;
using ArkheideSystem.Flourish.Themes;

namespace ArkheideSystem.Flourish.Styles;

/// <summary>
/// Obsolete resource entry point. Use <see cref="FlourishThemeResources" />.
/// </summary>
[Obsolete("Use FlourishThemeResources.")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class FlourishStyles : ResourceDictionary
{
    /// <summary>
    /// Initializes a resource dictionary containing Flourish control and theme resources.
    /// </summary>
    public FlourishStyles()
    {
        Source = new Uri(FlourishThemeResources.GenericThemeSource, UriKind.Relative);
    }

    internal static void EnsureMerged(ResourceDictionary resources)
    {
        FlourishThemeResources.EnsureMerged(resources);
    }
}
