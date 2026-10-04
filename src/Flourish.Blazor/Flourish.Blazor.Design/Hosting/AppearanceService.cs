using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Blazor.Abstract;

namespace ArkheideSystem.Flourish.Blazor.Hosting;

internal sealed class AppearanceService(DesignOptions options) : IAppearanceService, IThemeProvider
{
    public AppearanceState Current { get; private set; } = options.Appearance;
    public string CssClass => $"f-theme-{Current.Theme.ToString().ToLowerInvariant()}";
    public string CssVariables => AppearancePalette.Create(Current.Primary, Current.Accent).CssVariables + $";--f-font:{Current.FontFamily}";
    public string StylesheetHref => "_content/Arkheide.Flourish.Blazor.Design/design.css";
    public event EventHandler? Changed;
    public void SetColors(string primary, string accent)
    {
        var palette = AppearancePalette.Create(primary, accent);
        var next = Current with { Primary = palette.Primary, Accent = palette.Accent };
        if (Current == next) return;
        Current = next; Changed?.Invoke(this, EventArgs.Empty);
    }
    public void SetTheme(ApplicationTheme theme)
    {
        if (!Enum.IsDefined(theme)) throw new ArgumentOutOfRangeException(nameof(theme));
        if (Current.Theme == theme) return;
        Current = Current with { Theme = theme }; Changed?.Invoke(this, EventArgs.Empty);
    }
}
