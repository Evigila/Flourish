namespace AcksheedSys.Flourish.Abstract;

/// <summary>
/// Specifies whether Flourish should cache page instances created for navigation.
/// </summary>
/// <example>
/// <code><![CDATA[
/// services.AddNavigable<HomePage>(
///     displayName: "Home",
///     iconGlyph: "\uE80F",
///     cacheMode: FlourishPageCacheMode.Enabled);
/// ]]></code>
/// </example>
public enum FlourishPageCacheMode
{
    /// <summary>
    /// Reuses the page instance after it has been created.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// services.AddNavigable<HomePage>("Home", "\uE80F", cacheMode: FlourishPageCacheMode.Enabled);
    /// ]]></code>
    /// </example>
    Enabled,

    /// <summary>
    /// Creates a new page instance for each navigation request.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// services.AddNavigable<EditorPage>("Editor", "\uE70F", cacheMode: FlourishPageCacheMode.Disabled);
    /// ]]></code>
    /// </example>
    Disabled,
}
