using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Blazor.Abstract;

namespace ArkheideSystem.Flourish.Blazor.Hosting;

internal sealed record DesignOptions(AppearanceState Appearance);

internal sealed class AppearanceBuilder : IAppearanceBuilder
{
    private bool completed;
    private AppearanceState appearance = new("#153A32", "#16745F", "'Segoe UI', system-ui, sans-serif", ApplicationTheme.Light);

    private void Check()
    {
        if (completed) throw new InvalidOperationException("Appearance configuration has already been completed.");
    }

    internal DesignOptions Complete()
    {
        Check();
        completed = true;
        return new(appearance);
    }

    public IAppearanceBuilder SetColors(string primary, string accent)
    {
        Check();
        var palette = AppearancePalette.Create(primary, accent);
        appearance = appearance with { Primary = palette.Primary, Accent = palette.Accent };
        return this;
    }

    public IAppearanceBuilder SetTheme(ApplicationTheme theme)
    {
        Check();
        if (!Enum.IsDefined(theme)) throw new ArgumentOutOfRangeException(nameof(theme));
        appearance = appearance with { Theme = theme };
        return this;
    }

    public IAppearanceBuilder SetFont(string fontFamily)
    {
        Check();
        ArgumentException.ThrowIfNullOrWhiteSpace(fontFamily);
        if (fontFamily.IndexOfAny([';', '{', '}', '<', '>', '\r', '\n']) >= 0)
            throw new ArgumentException("Use a CSS font family list, not a CSS declaration.", nameof(fontFamily));
        appearance = appearance with { FontFamily = fontFamily };
        return this;
    }
}
