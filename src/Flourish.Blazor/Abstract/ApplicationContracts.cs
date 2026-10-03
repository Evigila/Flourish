using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Blazor.Abstract;

/// <summary>Configures the library-owned shell without exposing its implementation.</summary>
public interface IApplicationBuilder
{
    IApplicationBuilder UseTitleBar(Action<ITitleBarBuilder>? configure = null);
    IApplicationBuilder UseNavigation(Action<INavigationBuilder> configure);
    IApplicationBuilder ConfigureAppearance(Action<IAppearanceBuilder> configure);
    IApplicationBuilder ConfigureLayout(Action<ILayoutBuilder> configure);
}

public interface ITitleBarBuilder
{
    ITitleBarBuilder SetApplicationTitle(string title);
    ITitleBarBuilder SetSearch(bool enabled = true, string label = "Search");
    ITitleBarBuilder SetNavigationToggle(bool enabled = true);
}

public interface INavigationBuilder
{
    INavigationBuilder AddGroup(string key, string label, string icon, Action<INavigationGroupBuilder> configure);
}

public interface INavigationGroupBuilder
{
    INavigationGroupBuilder AddItem(string label, string href, string icon = "page", bool exact = false, bool disabled = false);
}

public interface IAppearanceBuilder
{
    IAppearanceBuilder SetColors(string primary, string accent);
    IAppearanceBuilder SetTheme(ApplicationTheme theme);
    /// <summary>Sets a CSS font stack. Does not download or install fonts.</summary>
    IAppearanceBuilder SetFont(string fontFamily);
}

public interface ILayoutBuilder
{
    ILayoutBuilder SetFluidContent(bool enabled = true);
    ILayoutBuilder SetContentWidth(int maximumWidth);
}

public sealed record NavigationItem(string Label, string Href, string Icon = "page", bool Exact = false, bool Disabled = false);
public sealed record NavigationGroup(string Key, string Label, string Icon, IReadOnlyList<NavigationItem> Items);
public sealed record AppearanceState(string Primary, string Accent, string FontFamily, ApplicationTheme Theme);

/// <summary>Per-user runtime appearance. State is scoped to a server circuit or client host.</summary>
public interface IAppearanceService
{
    AppearanceState Current { get; }
    event EventHandler? Changed;
    void SetColors(string primary, string accent);
    void SetTheme(ApplicationTheme theme);
}