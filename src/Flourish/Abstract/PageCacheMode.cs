namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Specifies whether Flourish should cache page instances created for navigation.
/// </summary>
/// <example>
/// <code><![CDATA[
/// services.AddNavigable<HomePage>(
///     displayName: "Home",
///     iconGlyph: "\uE80F",
///     cacheMode: PageCacheMode.Enabled);
/// ]]></code>
/// </example>
public enum PageCacheMode
{
    /// <summary>
    /// Reuses the page instance after it has been created.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// services.AddNavigable<HomePage>("Home", "\uE80F", cacheMode: PageCacheMode.Enabled);
    /// ]]></code>
    /// </example>
    Enabled,

    /// <summary>
    /// Creates a new page instance for each navigation request.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// services.AddNavigable<EditorPage>("Editor", "\uE70F", cacheMode: PageCacheMode.Disabled);
    /// ]]></code>
    /// </example>
    Disabled,
}
