using System.ComponentModel;
using System.Windows;
using ArkheideSystem.Flourish.Themes;

namespace ArkheideSystem.Flourish.Styles;

/// <summary>
/// Compatibility resource entry point for applications written before
/// <see cref="FlourishThemeResources" /> became the canonical theme API.
/// </summary>
[Obsolete("Use FlourishThemeResources. FlourishStyles will be removed in a future major version.")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class FlourishStyles : ResourceDictionary
{
    /// <summary>
    /// Initializes a resource dictionary backed by the Flourish generic theme.
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
