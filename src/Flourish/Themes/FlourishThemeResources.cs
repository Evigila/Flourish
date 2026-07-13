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
        if (FindThemeRoot(resources) is not null)
        {
            return;
        }

        resources.MergedDictionaries.Add(new FlourishThemeResources());
    }

    internal static ResourceDictionary? FindThemeRoot(ResourceDictionary resources)
    {
        return FindInGraph(resources, IsFlourishDictionary);
    }

    internal static ResourceDictionary? FindInGraph(
        ResourceDictionary resources,
        Func<ResourceDictionary, bool> predicate
    )
    {
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(predicate);

        var pending = new Stack<ResourceDictionary>();
        var visited = new HashSet<ResourceDictionary>(ReferenceEqualityComparer.Instance);
        pending.Push(resources);

        while (pending.TryPop(out var current))
        {
            if (!visited.Add(current))
            {
                continue;
            }

            if (predicate(current))
            {
                return current;
            }

            // WPF resolves later merged dictionaries first, so traverse the graph in the
            // same effective precedence order when more than one theme root is present.
            for (var index = 0; index < current.MergedDictionaries.Count; index++)
            {
                pending.Push(current.MergedDictionaries[index]);
            }
        }

        return null;
    }

    private static bool IsFlourishDictionary(ResourceDictionary dictionary)
    {
        if (dictionary is FlourishThemeResources)
        {
            return true;
        }

        return IsCanonicalThemeSource(dictionary.Source);
    }

    internal static bool IsCanonicalThemeSource(Uri? source)
    {
        const string applicationPackPrefix = "pack://application:,,,";
        var normalized = source?.OriginalString.Replace('\\', '/');
        return string.Equals(
                normalized,
                GenericThemeSource,
                StringComparison.OrdinalIgnoreCase
            )
            || string.Equals(
                normalized,
                applicationPackPrefix + GenericThemeSource,
                StringComparison.OrdinalIgnoreCase
            );
    }
}
