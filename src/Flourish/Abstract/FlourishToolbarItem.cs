namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes a toolbar item displayed by the Flourish shell.
/// </summary>
/// <example>
/// <code><![CDATA[
/// var item = new FlourishToolbarItem("Export", "\uE898", "reports.export");
/// ]]></code>
/// </example>
public sealed record FlourishToolbarItem
{
    /// <summary>
    /// Creates a toolbar item displayed by the Flourish shell.
    /// </summary>
    /// <param name="displayName">The text displayed for the toolbar item.</param>
    /// <param name="iconGlyph">The icon glyph displayed for the toolbar item.</param>
    /// <param name="commandKey">The optional command key dispatched through <see cref="ICommandDispatcher" />.</param>
    public FlourishToolbarItem(string displayName, string iconGlyph, string? commandKey = null)
    {
        DisplayName = displayName;
        IconGlyph = iconGlyph;
        CommandKey = commandKey;
        Id = CreateDefaultId(displayName, commandKey);
    }

    /// <summary>
    /// Gets the stable identifier used by <see cref="IToolbarService" /> runtime mutations.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Gets the text displayed for the toolbar item.
    /// </summary>
    public string DisplayName { get; init; }

    /// <summary>
    /// Gets the icon glyph displayed for the toolbar item.
    /// </summary>
    public string IconGlyph { get; init; }

    /// <summary>
    /// Gets the optional command key dispatched through <see cref="ICommandDispatcher" />.
    /// </summary>
    public string? CommandKey { get; init; }

    /// <summary>Gets whether the item is visible.</summary>
    public bool IsVisible { get; init; } = true;

    /// <summary>Gets whether the item accepts interaction.</summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// Deconstructs the toolbar item into display name, icon glyph, and command key.
    /// </summary>
    /// <param name="displayName">The text displayed for the toolbar item.</param>
    /// <param name="iconGlyph">The icon glyph displayed for the toolbar item.</param>
    /// <param name="commandKey">The optional command key dispatched through <see cref="ICommandDispatcher" />.</param>
    public void Deconstruct(out string displayName, out string iconGlyph, out string? commandKey)
    {
        displayName = DisplayName;
        iconGlyph = IconGlyph;
        commandKey = CommandKey;
    }

    private static string CreateDefaultId(string displayName, string? commandKey)
    {
        var candidate = string.IsNullOrWhiteSpace(commandKey) ? displayName : commandKey;
        candidate = string.IsNullOrWhiteSpace(candidate) ? "item" : candidate.Trim();
        return $"toolbar:{candidate}";
    }
}
