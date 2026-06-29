namespace AcksheedSys.Flourish.Abstract;

public sealed record FlourishToolbarItem(
    string DisplayName,
    string IconGlyph,
    string? CommandKey = null
);
