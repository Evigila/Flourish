using AcksheedSys.Flourish.Abstract;

namespace AcksheedSys.Flourish.Services;

internal sealed class FlourishThemeChangedEventArgs(
    FlourishTheme requestedTheme,
    FlourishTheme effectiveTheme
) : EventArgs
{
    public FlourishTheme RequestedTheme { get; } = requestedTheme;

    public FlourishTheme EffectiveTheme { get; } = effectiveTheme;

    public bool IsDark => EffectiveTheme == FlourishTheme.Dark;
}
