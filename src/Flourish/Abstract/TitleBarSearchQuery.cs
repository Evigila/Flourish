namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes a title bar search query raised at runtime.
/// </summary>
/// <param name="Text">The complete user-entered query.</param>
/// <param name="Sequence">The monotonically increasing query sequence number.</param>
public readonly record struct TitleBarSearchQuery(string Text, long Sequence);
