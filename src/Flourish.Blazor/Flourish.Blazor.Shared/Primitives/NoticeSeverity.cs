namespace ArkheideSystem.Flourish.Blazor.Components.Primitives;

/// <summary>Presentation severity, independent of transport status codes and business rules.</summary>
public enum NoticeSeverity
{
    Error,
    Warning,
    Success,
    Information,
    Subtle
}

public static class NoticePresentation
{
    public static string CssClass(NoticeSeverity severity) => severity switch
    {
        NoticeSeverity.Error => "notice-error",
        NoticeSeverity.Warning => "notice-warning",
        NoticeSeverity.Success => "notice-success",
        NoticeSeverity.Information => "notice-info",
        NoticeSeverity.Subtle => "notice-subtle",
        _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, "Unknown notice severity.")
    };

    public static string Role(NoticeSeverity severity) => severity switch
    {
        NoticeSeverity.Error or NoticeSeverity.Warning => "alert",
        NoticeSeverity.Success or NoticeSeverity.Information or NoticeSeverity.Subtle => "status",
        _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, "Unknown notice severity.")
    };
}
