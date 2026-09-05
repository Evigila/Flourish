namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes the active window and page used to resolve scoped keyboard shortcuts.
/// </summary>
/// <remarks>
/// Initializes a shortcut resolution context.
/// </remarks>
/// <param name="windowKey">The optional active window key.</param>
/// <param name="pageKey">The optional active page key.</param>
public sealed class ShortcutResolutionContext(string? windowKey = null, string? pageKey = null)
{
    /// <summary>
    /// Gets the optional active window key.
    /// </summary>
    public string? WindowKey { get; } = NormalizeOptionalKey(windowKey);

    /// <summary>
    /// Gets the optional active page key.
    /// </summary>
    public string? PageKey { get; } = NormalizeOptionalKey(pageKey);

    private static string? NormalizeOptionalKey(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
