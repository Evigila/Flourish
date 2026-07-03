namespace AcksheedSys.Flourish.Abstract;

/// <summary>
/// Describes a toolbar item displayed by the Flourish shell.
/// </summary>
/// <param name="DisplayName">The text displayed for the toolbar item.</param>
/// <param name="IconGlyph">The icon glyph displayed for the toolbar item.</param>
/// <param name="CommandKey">The optional command key passed to an <see cref="ICommandParser" />.</param>
/// <example>
/// <code><![CDATA[
/// var item = new FlourishToolbarItem("Open", "\uE8E5", "gallery.open");
/// ]]></code>
/// </example>
public sealed record FlourishToolbarItem(
    string DisplayName,
    string IconGlyph,
    string? CommandKey = null
);
