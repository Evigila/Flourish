namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Specifies whether page instances created for navigation should be cached.
/// </summary>
public enum PageCacheMode
{
    /// <summary>
    /// Reuses the page instance after it has been created.
    /// </summary>
    Enabled,

    /// <summary>
    /// Creates a new page instance for each navigation request.
    /// </summary>
    Disabled,
}
