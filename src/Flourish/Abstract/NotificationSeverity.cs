namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Specifies the visual importance of a Flourish runtime notification.
/// </summary>
public enum NotificationSeverity
{
    /// <summary>
    /// Neutral information that does not require special attention.
    /// </summary>
    Information,

    /// <summary>
    /// Confirmation that an operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// A recoverable condition that may require attention.
    /// </summary>
    Warning,

    /// <summary>
    /// A failed operation or condition requiring attention.
    /// </summary>
    Error,
}
