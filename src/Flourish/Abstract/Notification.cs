using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Defines a notification shown by the Flourish shell.
/// </summary>
/// <param name="Id">A non-empty identifier used for updates and dismissal.</param>
/// <param name="Title">The non-empty notification heading.</param>
/// <param name="Message">The non-empty notification body.</param>
/// <param name="Severity">The visual severity.</param>
/// <param name="IconGlyph">An optional icon glyph understood by the shell theme.</param>
/// <param name="CommandKey">An optional runtime command invoked when the notification is activated.</param>
/// <param name="Duration">An optional positive lifetime after which the notification is dismissed automatically.</param>
public sealed record Notification(
    string Id,
    string Title,
    string Message,
    NotificationSeverity Severity = NotificationSeverity.Information,
    string? IconGlyph = null,
    string? CommandKey = null,
    TimeSpan? Duration = null
);
