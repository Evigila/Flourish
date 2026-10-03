using ApplicationTheme = ArkheideSystem.Flourish.Abstract.ApplicationTheme;
using ArkheideSystem.Flourish.Blazor.Abstract;

namespace ArkheideSystem.Flourish.Blazor.Hosting;

internal sealed record TitleBarOptions(bool Enabled = false, string Title = "Application", bool Search = false,
    string SearchLabel = "Search", bool NavigationToggle = true);
internal sealed record LayoutOptions(bool Fluid = true, int ContentWidth = 1180);
internal sealed record ApplicationOptions(TitleBarOptions TitleBar, LayoutOptions Layout,
    AppearanceState Appearance, IReadOnlyList<NavigationGroup> Navigation);

internal sealed class ApplicationBuilder : IApplicationBuilder, ITitleBarBuilder, INavigationBuilder, IAppearanceBuilder, ILayoutBuilder
{
    private bool completed;
    private TitleBarOptions titleBar = new();
    private LayoutOptions layout = new();
    private AppearanceState appearance = new("#153A32", "#16745F", "'Segoe UI', system-ui, sans-serif", ApplicationTheme.Light);
    private readonly List<NavigationGroup> navigation = [];
    private void Check() { if (completed) throw new InvalidOperationException("Configuration has already been completed."); }
    internal ApplicationOptions Complete()
    {
        Check();
        completed = true;
        return new(titleBar, layout, appearance, navigation.AsReadOnly());
    }
    public IApplicationBuilder UseTitleBar(Action<ITitleBarBuilder>? configure = null)
    {
        Check(); titleBar = titleBar with { Enabled = true }; configure?.Invoke(this); return this;
    }
    public IApplicationBuilder UseNavigation(Action<INavigationBuilder> configure) { Check(); ArgumentNullException.ThrowIfNull(configure); configure(this); return this; }
    public IApplicationBuilder ConfigureAppearance(Action<IAppearanceBuilder> configure) { Check(); ArgumentNullException.ThrowIfNull(configure); configure(this); return this; }
    public IApplicationBuilder ConfigureLayout(Action<ILayoutBuilder> configure) { Check(); ArgumentNullException.ThrowIfNull(configure); configure(this); return this; }
    public ITitleBarBuilder SetApplicationTitle(string title) { Check(); ArgumentException.ThrowIfNullOrWhiteSpace(title); titleBar = titleBar with { Title = title }; return this; }
    public ITitleBarBuilder SetSearch(bool enabled = true, string label = "Search") { Check(); ArgumentException.ThrowIfNullOrWhiteSpace(label); titleBar = titleBar with { Search = enabled, SearchLabel = label }; return this; }
    public ITitleBarBuilder SetNavigationToggle(bool enabled = true) { Check(); titleBar = titleBar with { NavigationToggle = enabled }; return this; }
    public INavigationBuilder AddGroup(string key, string label, string icon, Action<INavigationGroupBuilder> configure)
    {
        Check(); ArgumentException.ThrowIfNullOrWhiteSpace(key); ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentNullException.ThrowIfNull(configure);
        if (navigation.Any(group => group.Key == key)) throw new ArgumentException("Navigation keys must be unique.", nameof(key));
        var group = new NavigationGroupBuilder();
        configure(group);
        var items = group.Complete();
        if (items.Count == 0) throw new ArgumentException("A navigation group needs at least one item.");
        navigation.Add(new(key, label, icon, items, group.SecondaryNavigation)); return this;
    }
    public IAppearanceBuilder SetColors(string primary, string accent)
    {
        Check(); var palette = AppearancePalette.Create(primary, accent);
        appearance = appearance with { Primary = palette.Primary, Accent = palette.Accent }; return this;
    }
    public IAppearanceBuilder SetTheme(ApplicationTheme theme) { Check(); if (!Enum.IsDefined(theme)) throw new ArgumentOutOfRangeException(nameof(theme)); appearance = appearance with { Theme = theme }; return this; }
    public IAppearanceBuilder SetFont(string fontFamily)
    {
        Check(); ArgumentException.ThrowIfNullOrWhiteSpace(fontFamily);
        if (fontFamily.IndexOfAny([';', '{', '}', '<', '>', '\r', '\n']) >= 0) throw new ArgumentException("Use a CSS font family list, not a CSS declaration.", nameof(fontFamily));
        appearance = appearance with { FontFamily = fontFamily }; return this;
    }
    public ILayoutBuilder SetFluidContent(bool enabled = true) { Check(); layout = layout with { Fluid = enabled }; return this; }
    public ILayoutBuilder SetContentWidth(int maximumWidth) { Check(); if (maximumWidth < 320 || maximumWidth > 2400) throw new ArgumentOutOfRangeException(nameof(maximumWidth)); layout = layout with { ContentWidth = maximumWidth }; return this; }
}

internal sealed class NavigationGroupBuilder : INavigationGroupBuilder
{
    private bool completed;
    private readonly List<NavigationItem> items = [];
    internal bool SecondaryNavigation { get; private set; } = true;
    public INavigationGroupBuilder SetSecondaryNavigation(bool enabled = true)
    {
        if (completed) throw new InvalidOperationException("Navigation group configuration has been completed.");
        SecondaryNavigation = enabled; return this;
    }
    public INavigationGroupBuilder AddItem(string label, string href, string icon = "page", bool exact = false, bool disabled = false)
    {
        if (completed) throw new InvalidOperationException("Navigation group configuration has been completed.");
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        if (string.IsNullOrWhiteSpace(href) || !href.StartsWith('/') || href.StartsWith("//") || href.Contains('\\'))
            throw new ArgumentException("Navigation uses application-local absolute paths.", nameof(href));
        if (items.Any(item => item.Href == href)) throw new ArgumentException("Navigation routes must be unique within a group.", nameof(href));
        items.Add(new(label, href, icon, exact, disabled)); return this;
    }
    internal IReadOnlyList<NavigationItem> Complete() { completed = true; return items.AsReadOnly(); }
}