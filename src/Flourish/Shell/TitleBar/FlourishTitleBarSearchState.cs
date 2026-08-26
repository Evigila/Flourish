namespace ArkheideSystem.Flourish.Shell.TitleBar;

/// <summary>
/// Represents an immutable title bar search-box snapshot.
/// </summary>
/// <param name="Text">The current query text.</param>
/// <param name="Placeholder">The text displayed when the query is empty.</param>
/// <param name="IsVisible">Whether the search box is visible.</param>
/// <param name="FocusRequested">Whether the shell should move keyboard focus to the search box.</param>
/// <param name="Version">A monotonically increasing state version.</param>
internal sealed record FlourishTitleBarSearchState(
    string Text,
    string Placeholder,
    bool IsVisible,
    bool FocusRequested,
    long Version
);
