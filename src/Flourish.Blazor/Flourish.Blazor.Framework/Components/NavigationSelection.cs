using ArkheideSystem.Flourish.Blazor.Abstract;

namespace ArkheideSystem.Flourish.Blazor.Components;

internal static class NavigationSelection
{
    internal static string NormalizePath(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https") value = uri.AbsolutePath;
        return "/" + value.Split('?', '#')[0].Trim('/');
    }

    internal static bool Matches(string currentPath, string href, bool exact)
    {
        var path = NormalizePath(href);
        return string.Equals(currentPath, path, StringComparison.OrdinalIgnoreCase)
            || (!exact && path != "/" && currentPath.StartsWith(path + "/", StringComparison.OrdinalIgnoreCase));
    }

    internal static IReadOnlyList<NavigationItem> FindActivePath(IReadOnlyList<NavigationItem> items, string currentPath)
    {
        IReadOnlyList<NavigationItem> best = [];
        foreach (var item in items)
        {
            if (item.Disabled) continue;
            var children = FindActivePath(item.ChildItems, currentPath);
            IReadOnlyList<NavigationItem> candidate = Matches(currentPath, item.Href, item.Exact) ? [item] : [];
            if (children.Count > 0 && (candidate.Count == 0 || Specificity(children[^1]) >= Specificity(item)))
                candidate = [item, .. children];
            if (candidate.Count > 0 && (best.Count == 0 || Specificity(candidate[^1]) > Specificity(best[^1]))) best = candidate;
        }
        return best;
    }

    internal static int Specificity(NavigationItem item) => NormalizePath(item.Href).Length;
}
