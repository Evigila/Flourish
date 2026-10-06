using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Blazor.Components.Primitives;

public static class NoticePresentation
{
    public static string CssClass(NotificationSeverity severity, bool subtle = false) => subtle ? "notice-subtle" : severity switch
    {
        NotificationSeverity.Error => "notice-error",
        NotificationSeverity.Warning => "notice-warning",
        NotificationSeverity.Success => "notice-success",
        NotificationSeverity.Information => "notice-info",
        _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, "Unknown notice severity.")
    };

}
