namespace ArkheideSystem.Flourish.Blazor.Components;

/// <summary>A host-owned milestone. Completion is supplied, never inferred from UI actions.</summary>
/// <param name="Key">Stable, unique milestone identity.</param>
/// <param name="Title">The visible H1 and progress overview title.</param>
/// <param name="Description">Concise explanatory plain text.</param>
/// <param name="ActionText">The shortcut button label.</param>
/// <param name="ActionHref">Optional navigation destination; otherwise TutorialBoard.OnAction handles the shortcut.</param>
/// <param name="Completed">Verified completion supplied by the host.</param>
/// <param name="Disabled">Whether the host currently permits the shortcut.</param>
public sealed record TutorialStep(string Key, string Title, string Description, string ActionText,
    string? ActionHref = null, bool Completed = false, bool Disabled = false);
