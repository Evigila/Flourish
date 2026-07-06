namespace AcksheedSys.Flourish.Abstract;

/// <summary>
/// Describes a custom option displayed by <see cref="IMessageService"/>.
/// </summary>
/// <remarks>
/// Options are rendered in the order provided by the caller. The last option appears on the
/// right side of the dialog footer.
/// </remarks>
public sealed record FlourishMessageOption
{
    /// <summary>
    /// Initializes a new message option.
    /// </summary>
    /// <param name="id">The stable value returned when this option is selected.</param>
    /// <param name="text">The text displayed on the option button.</param>
    public FlourishMessageOption(string id, string text)
    {
        Id = id;
        Text = text;
    }

    /// <summary>
    /// Gets the stable value returned when this option is selected.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Gets the text displayed on the option button.
    /// </summary>
    public string Text { get; init; }

    /// <summary>
    /// Gets a value indicating whether this option should be activated by Enter.
    /// </summary>
    public bool IsDefault { get; init; }

    /// <summary>
    /// Gets a value indicating whether this option should be returned when the dialog is
    /// dismissed by Escape or the titlebar close button.
    /// </summary>
    public bool IsCancel { get; init; }

    /// <summary>
    /// Gets a value indicating whether this option should use the primary accent button style.
    /// </summary>
    public bool IsPrimary { get; init; }
}
