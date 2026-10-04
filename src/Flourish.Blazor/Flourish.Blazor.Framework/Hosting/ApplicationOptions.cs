using ArkheideSystem.Flourish.Blazor.Abstract;
using Microsoft.AspNetCore.Components;
using ICommandParser = ArkheideSystem.Flourish.Abstract.ICommandParser;

#pragma warning disable CS0618 // Compatibility implementation for the previous public builder surface.

namespace ArkheideSystem.Flourish.Blazor.Hosting;

internal sealed record TopBarOptions(
    bool Enabled = false,
    string AppName = "Application",
    string? IconPath = null,
    string? IconAlternativeText = null,
    bool Search = false,
    string SearchLabel = "Search",
    bool NavigationToggle = true,
    IReadOnlyList<TopBarMenu>? Menus = null,
    IReadOnlyList<ComponentPlacement>? Left = null,
    IReadOnlyList<ComponentPlacement>? Center = null,
    IReadOnlyList<ComponentPlacement>? Right = null)
{
    internal IReadOnlyList<TopBarMenu> MenuItems => Menus ?? Array.Empty<TopBarMenu>();
    internal IReadOnlyList<ComponentPlacement> LeftItems => Left ?? Array.Empty<ComponentPlacement>();
    internal IReadOnlyList<ComponentPlacement> CenterItems => Center ?? Array.Empty<ComponentPlacement>();
    internal IReadOnlyList<ComponentPlacement> RightItems => Right ?? Array.Empty<ComponentPlacement>();
}

internal sealed record LayoutOptions(bool Fluid = true, int ContentWidth = 1180);

internal sealed record ApplicationOptions(
    TopBarOptions TopBar,
    LayoutOptions Layout,
    IReadOnlyList<NavigationEntry> PrimaryNavigation,
    IReadOnlyList<NavigationEntry> FixedNavigation,
    IReadOnlyList<NavigationGroup> LegacyNavigation,
    Type? CommandParserType);

internal sealed class ApplicationBuilder : IApplicationBuilder, ITopBarBuilder, ITitleBarBuilder, INavigationBuilder, ILayoutBuilder
{
    private bool completed;
    private TopBarOptions topBar = new();
    private LayoutOptions layout = new();
    private readonly List<TopBarMenu> menus = [];
    private readonly List<ComponentPlacement> left = [];
    private readonly List<ComponentPlacement> center = [];
    private readonly List<ComponentPlacement> right = [];
    private readonly List<NavigationEntry> primaryNavigation = [];
    private readonly List<NavigationEntry> fixedNavigation = [];
    private readonly List<NavigationGroup> legacyNavigation = [];
    private readonly HashSet<string> routes = new(StringComparer.OrdinalIgnoreCase);
    private Type? commandParserType;

    private void Check()
    {
        if (completed) throw new InvalidOperationException("Configuration has already been completed.");
    }

    internal ApplicationOptions Complete()
    {
        Check();
        completed = true;
        topBar = topBar with
        {
            Menus = menus.AsReadOnly(),
            Left = left.AsReadOnly(),
            Center = center.AsReadOnly(),
            Right = right.AsReadOnly(),
        };
        return new(topBar, layout, primaryNavigation.AsReadOnly(), fixedNavigation.AsReadOnly(), legacyNavigation.AsReadOnly(), commandParserType);
    }

    public IFrameworkBuilder ConfigureTopBar(Action<ITopBarBuilder> configure)
    {
        Check();
        ArgumentNullException.ThrowIfNull(configure);
        topBar = topBar with { Enabled = true };
        configure(this);
        return this;
    }

    public IFrameworkBuilder ConfigureNavigation(Action<INavigationBuilder> configure)
    {
        Check();
        ArgumentNullException.ThrowIfNull(configure);
        configure(this);
        return this;
    }

    IFrameworkBuilder IFrameworkBuilder.ConfigureLayout(Action<ILayoutBuilder> configure)
    {
        ConfigureLayout(configure);
        return this;
    }

    public IApplicationBuilder ConfigureLayout(Action<ILayoutBuilder> configure)
    {
        Check();
        ArgumentNullException.ThrowIfNull(configure);
        configure(this);
        return this;
    }

    public IFrameworkBuilder SetCommandParser<TParser>() where TParser : class, ICommandParser
    {
        Check();
        if (commandParserType is not null) throw new InvalidOperationException("Only one command parser can be configured for an application shell.");
        commandParserType = typeof(TParser);
        return this;
    }

    public IApplicationBuilder UseTitleBar(Action<ITitleBarBuilder>? configure = null)
    {
        Check();
        topBar = topBar with { Enabled = true };
        configure?.Invoke(this);
        return this;
    }

    public IApplicationBuilder UseNavigation(Action<INavigationBuilder> configure)
    {
        Check();
        ArgumentNullException.ThrowIfNull(configure);
        configure(this);
        return this;
    }

    public ITopBarBuilder SetAppName(string displayName)
    {
        Check();
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        topBar = topBar with { AppName = displayName };
        return this;
    }

    public ITopBarBuilder SetIcon(string iconPath, string? alternativeText = null)
    {
        Check();
        topBar = topBar with { IconPath = ValidateAssetPath(iconPath), IconAlternativeText = alternativeText };
        return this;
    }

    ITopBarBuilder ITopBarBuilder.SetSearch(bool enabled, string label)
    {
        SetSearch(enabled, label);
        return this;
    }

    ITopBarBuilder ITopBarBuilder.SetNavigationToggle(bool enabled)
    {
        SetNavigationToggle(enabled);
        return this;
    }

    public ITopBarBuilder AddMenu(string menuName, Action<ITopBarMenuBuilder> configure)
    {
        Check();
        ArgumentException.ThrowIfNullOrWhiteSpace(menuName);
        ArgumentNullException.ThrowIfNull(configure);
        var builder = new TopBarMenuBuilder();
        configure(builder);
        var items = builder.Complete();
        if (items.Count == 0) throw new ArgumentException("A top bar menu needs at least one item.", nameof(configure));
        menus.Add(new(menuName, items));
        return this;
    }

    public ITopBarBuilder InjectToLeft<TComponent>() where TComponent : IComponent => AddComponent<TComponent>(left);
    public ITopBarBuilder InjectToCenter<TComponent>() where TComponent : IComponent => AddComponent<TComponent>(center);
    public ITopBarBuilder InjectToRight<TComponent>() where TComponent : IComponent => AddComponent<TComponent>(right);

    private ITopBarBuilder AddComponent<TComponent>(List<ComponentPlacement> target) where TComponent : IComponent
    {
        Check();
        target.Add(new(typeof(TComponent)));
        return this;
    }

    public ITitleBarBuilder SetApplicationTitle(string title)
    {
        SetAppName(title);
        return this;
    }

    public ITitleBarBuilder SetSearch(bool enabled = true, string label = "Search")
    {
        Check();
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        topBar = topBar with { Search = enabled, SearchLabel = label };
        return this;
    }

    public ITitleBarBuilder SetNavigationToggle(bool enabled = true)
    {
        Check();
        topBar = topBar with { NavigationToggle = enabled };
        return this;
    }

    public INavigationBuilder AddNav(string label, string icon, string navTarget, Action<ISubNavigationBuilder>? configure = null, bool exact = false)
    {
        Check();
        ValidateLabelAndIcon(label, icon);
        var route = AddRoute(navTarget);
        var children = new SubNavigationBuilder(AddRoute);
        configure?.Invoke(children);
        primaryNavigation.Add(new(label, icon, NavigationEntryKind.Route, Href: route, Children: children.Complete(), Exact: exact));
        return this;
    }

    public INavigationBuilder AddNavButton(string label, string icon, string commandKey)
    {
        Check();
        ValidateLabelAndIcon(label, icon);
        ValidateCommandKey(commandKey);
        primaryNavigation.Add(new(label, icon, NavigationEntryKind.Command, CommandKey: commandKey));
        return this;
    }

    public INavigationBuilder AddFixedNav(string label, string icon, string navTarget, bool exact = false)
    {
        Check();
        ValidateLabelAndIcon(label, icon);
        fixedNavigation.Add(new(label, icon, NavigationEntryKind.Route, Href: AddRoute(navTarget), Exact: exact));
        return this;
    }

    public INavigationBuilder AddFixedNavButton(string label, string icon, string commandKey)
    {
        Check();
        ValidateLabelAndIcon(label, icon);
        ValidateCommandKey(commandKey);
        fixedNavigation.Add(new(label, icon, NavigationEntryKind.Command, CommandKey: commandKey));
        return this;
    }

    public INavigationBuilder AddGroup(string key, string label, string icon, Action<INavigationGroupBuilder> configure)
    {
        Check();
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ValidateLabelAndIcon(label, icon);
        ArgumentNullException.ThrowIfNull(configure);
        if (legacyNavigation.Any(group => group.Key == key)) throw new ArgumentException("Navigation keys must be unique.", nameof(key));
        var group = new NavigationGroupBuilder(AddRoute);
        configure(group);
        var items = group.Complete();
        if (items.Count == 0) throw new ArgumentException("A navigation group needs at least one item.", nameof(configure));
        legacyNavigation.Add(new(key, label, icon, items, group.SecondaryNavigation));
        return this;
    }

    public ILayoutBuilder SetFluidContent(bool enabled = true)
    {
        Check();
        layout = layout with { Fluid = enabled };
        return this;
    }

    public ILayoutBuilder SetContentWidth(int maximumWidth)
    {
        Check();
        if (maximumWidth < 320 || maximumWidth > 2400) throw new ArgumentOutOfRangeException(nameof(maximumWidth));
        layout = layout with { ContentWidth = maximumWidth };
        return this;
    }

    private string AddRoute(string route)
    {
        var normalized = NormalizeRoute(route);
        if (!routes.Add(normalized)) throw new ArgumentException($"Navigation route '{normalized}' is already configured.", nameof(route));
        return normalized;
    }

    private static string NormalizeRoute(string route)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        route = route.Trim();
        if (route.StartsWith("//", StringComparison.Ordinal) || route.Contains('\\') || route.Contains(':'))
            throw new ArgumentException("Navigation uses application-local paths.", nameof(route));
        route = "/" + route.TrimStart('/');
        return route.Length > 1 ? route.TrimEnd('/') : route;
    }

    private static string ValidateAssetPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        path = path.Trim();
        if (path.StartsWith("//", StringComparison.Ordinal) || path.Contains('\\') || path.Contains(':'))
            throw new ArgumentException("Top bar icons use application-local asset paths.", nameof(path));
        return path;
    }

    private static void ValidateLabelAndIcon(string label, string icon)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(icon);
    }

    private static void ValidateCommandKey(string commandKey) => ArgumentException.ThrowIfNullOrWhiteSpace(commandKey);
}

internal sealed class TopBarMenuBuilder : ITopBarMenuBuilder
{
    private bool completed;
    private readonly List<TopBarMenuItem> items = [];

    public ITopBarMenuBuilder AddMenuItem(string menuItemName, string commandKey, bool disabled = false, bool destructive = false)
    {
        if (completed) throw new InvalidOperationException("Menu configuration has already been completed.");
        ArgumentException.ThrowIfNullOrWhiteSpace(menuItemName);
        ArgumentException.ThrowIfNullOrWhiteSpace(commandKey);
        items.Add(new(menuItemName, commandKey, disabled, destructive));
        return this;
    }

    internal IReadOnlyList<TopBarMenuItem> Complete()
    {
        completed = true;
        return items.AsReadOnly();
    }
}

internal sealed class SubNavigationBuilder(Func<string, string> addRoute) : ISubNavigationBuilder
{
    private bool completed;
    private readonly List<NavigationItem> items = [];

    public ISubNavigationBuilder AddSubNav(string label, string icon, string navTarget, bool exact = false, bool disabled = false)
    {
        if (completed) throw new InvalidOperationException("Secondary navigation configuration has already been completed.");
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(icon);
        items.Add(new(label, addRoute(navTarget), icon, exact, disabled));
        return this;
    }

    internal IReadOnlyList<NavigationItem> Complete()
    {
        completed = true;
        return items.AsReadOnly();
    }
}

internal sealed class NavigationGroupBuilder(Func<string, string> addRoute) : INavigationGroupBuilder
{
    private bool completed;
    private readonly List<NavigationItem> items = [];
    internal bool SecondaryNavigation { get; private set; } = true;

    public INavigationGroupBuilder SetSecondaryNavigation(bool enabled = true)
    {
        if (completed) throw new InvalidOperationException("Navigation group configuration has been completed.");
        SecondaryNavigation = enabled;
        return this;
    }

    public INavigationGroupBuilder AddItem(string label, string href, string icon = "page", bool exact = false, bool disabled = false)
    {
        if (completed) throw new InvalidOperationException("Navigation group configuration has been completed.");
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(icon);
        items.Add(new(label, addRoute(href), icon, exact, disabled));
        return this;
    }

    internal IReadOnlyList<NavigationItem> Complete()
    {
        completed = true;
        return items.AsReadOnly();
    }
}
#pragma warning restore CS0618
