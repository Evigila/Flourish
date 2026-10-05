using System.Collections.ObjectModel;
using ArkheideSystem.Flourish.Blazor.Abstract;
using Microsoft.AspNetCore.Components;
using ICommandParser = ArkheideSystem.Flourish.Abstract.ICommandParser;

#pragma warning disable CS0618 // Compatibility implementation for the previous public builder surface.

namespace ArkheideSystem.Flourish.Blazor.Hosting;

internal sealed record ProjectOptions(
    string Name = "Application",
    string LogoPath = ProjectOptions.DefaultLogoPath,
    string LogoAlternativeText = "",
    string? FaviconPath = null,
    TextReference? NameText = null)
{
    internal const string DefaultLogoPath = "_content/Arkheide.Flourish.Blazor.Framework/browse.svg";
    internal string BrowserIconPath => FaviconPath ?? LogoPath;
}

internal sealed record TopBarOptions(
    bool Enabled = false,
    bool DisplayLogo = true,
    bool DisplayProjectName = true,
    bool Search = false,
    string SearchLabel = "Search",
    bool NavigationToggle = true,
    TextReference? SearchText = null,
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
    ProjectOptions Project,
    TopBarOptions TopBar,
    LayoutOptions Layout,
    IReadOnlyList<NavigationEntry> PrimaryNavigation,
    IReadOnlyList<NavigationEntry> FixedNavigation,
    IReadOnlyList<NavigationGroup> LegacyNavigation,
    Type? CommandParserType,
    IReadOnlyDictionary<object, TextReference> LabelReferences);

internal sealed class ApplicationBuilder : IApplicationBuilder, IProjectBuilder, ITopBarBuilder, ITitleBarBuilder, INavigationBuilder, ILayoutBuilder
{
    private readonly Dictionary<object, TextReference> labelReferences = new(ReferenceEqualityComparer.Instance);
    private bool completed;
    private ProjectOptions project = new();
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
        return new(project, topBar, layout, primaryNavigation.AsReadOnly(), fixedNavigation.AsReadOnly(), legacyNavigation.AsReadOnly(), commandParserType,
            new ReadOnlyDictionary<object, TextReference>(new Dictionary<object, TextReference>(labelReferences, ReferenceEqualityComparer.Instance)));
    }

    public IFrameworkBuilder ConfigureProject(Action<IProjectBuilder> configure)
    {
        Check();
        ArgumentNullException.ThrowIfNull(configure);
        configure(this);
        return this;
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

    public IProjectBuilder SetProjectName(string projectName)
    {
        Check();
        ArgumentException.ThrowIfNullOrWhiteSpace(projectName);
        project = project with { Name = projectName, NameText = null };
        return this;
    }

    public IProjectBuilder SetProjectName(TextReference projectName)
    {
        ArgumentNullException.ThrowIfNull(projectName);
        SetProjectName(projectName.FallbackText ?? projectName.Token);
        project = project with { NameText = projectName };
        return this;
    }

    public ITopBarBuilder SetSearch(TextReference label, bool enabled = true)
    {
        ArgumentNullException.ThrowIfNull(label);
        SetSearch(enabled, label.FallbackText ?? label.Token);
        topBar = topBar with { SearchText = label };
        return this;
    }

    public IProjectBuilder SetLogo(string? logoPath = null, string? alternativeText = null)
    {
        Check();
        project = project with { LogoPath = logoPath is null ? ProjectOptions.DefaultLogoPath : ValidateAssetPath(logoPath), LogoAlternativeText = alternativeText ?? "" };
        return this;
    }

    public IProjectBuilder SetFavicon(string? faviconPath = null)
    {
        Check();
        project = project with { FaviconPath = string.IsNullOrEmpty(faviconPath) ? null : ValidateAssetPath(faviconPath) };
        return this;
    }

    public ITopBarBuilder DisplayLogo(bool display = true)
    {
        Check();
        topBar = topBar with { DisplayLogo = display };
        return this;
    }

    public ITopBarBuilder DisplayProjectName(bool display = true)
    {
        Check();
        topBar = topBar with { DisplayProjectName = display };
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
        var builder = new TopBarMenuBuilder(AttachText);
        configure(builder);
        var items = builder.Complete();
        if (items.Count == 0) throw new ArgumentException("A top bar menu needs at least one item.", nameof(configure));
        menus.Add(new(menuName, items));
        return this;
    }

    public ITopBarBuilder AddMenu(TextReference menuName, Action<ITopBarMenuBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(menuName);
        AddMenu(menuName.FallbackText ?? menuName.Token, configure);
        AttachText(menus[^1], menuName);
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
        SetProjectName(title);
        return this;
    }

    public ITitleBarBuilder SetSearch(bool enabled = true, string label = "Search")
    {
        Check();
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        topBar = topBar with { Search = enabled, SearchLabel = label, SearchText = null };
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
        var children = new SubNavigationBuilder(AddRoute, AttachText, route);
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

    private void AttachText(object owner, TextReference text)
    {
        Check();
        labelReferences.Add(owner, text);
    }

    public INavigationBuilder AddNav(TextReference label, string icon, string navTarget, Action<ISubNavigationBuilder>? configure = null, bool exact = false)
    {
        ArgumentNullException.ThrowIfNull(label);
        AddNav(label.FallbackText ?? label.Token, icon, navTarget, configure, exact);
        AttachText(primaryNavigation[^1], label);
        return this;
    }

    public INavigationBuilder AddNavButton(TextReference label, string icon, string commandKey)
    {
        ArgumentNullException.ThrowIfNull(label);
        AddNavButton(label.FallbackText ?? label.Token, icon, commandKey);
        AttachText(primaryNavigation[^1], label);
        return this;
    }

    public INavigationBuilder AddFixedNav(TextReference label, string icon, string navTarget, bool exact = false)
    {
        ArgumentNullException.ThrowIfNull(label);
        AddFixedNav(label.FallbackText ?? label.Token, icon, navTarget, exact);
        AttachText(fixedNavigation[^1], label);
        return this;
    }

    public INavigationBuilder AddFixedNavButton(TextReference label, string icon, string commandKey)
    {
        ArgumentNullException.ThrowIfNull(label);
        AddFixedNavButton(label.FallbackText ?? label.Token, icon, commandKey);
        AttachText(fixedNavigation[^1], label);
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

    internal static string NormalizeRoute(string route)
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
            throw new ArgumentException("Project icons use application-local asset paths.", nameof(path));
        return path;
    }

    private static void ValidateLabelAndIcon(string label, string icon)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(icon);
    }

    private static void ValidateCommandKey(string commandKey) => ArgumentException.ThrowIfNullOrWhiteSpace(commandKey);
}

internal sealed class TopBarMenuBuilder(Action<object, TextReference> attachText) : ITopBarMenuBuilder
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

    public ITopBarMenuBuilder AddMenuItem(TextReference menuItemName, string commandKey, bool disabled = false, bool destructive = false)
    {
        ArgumentNullException.ThrowIfNull(menuItemName);
        AddMenuItem(menuItemName.FallbackText ?? menuItemName.Token, commandKey, disabled, destructive);
        attachText(items[^1], menuItemName);
        return this;
    }

    internal IReadOnlyList<TopBarMenuItem> Complete()
    {
        completed = true;
        return items.AsReadOnly();
    }
}

internal sealed class SubNavigationBuilder(Func<string, string> addRoute, Action<object, TextReference> attachText, string? parentRoute = null) : ISubNavigationBuilder
{
    private bool completed;
    private readonly List<NavigationItem> items = [];
    private readonly HashSet<string> routes = new(StringComparer.OrdinalIgnoreCase);

    public ISubNavigationBuilder AddSubNav(string label, string icon, string navTarget, bool exact = false, bool disabled = false)
    {
        var route = AddItemRoute(label, icon, navTarget);
        items.Add(new(label, route, icon, exact, disabled));
        return this;
    }

    public ISubNavigationBuilder AddSubNav(string label, string icon, string navTarget, Action<ISubNavigationBuilder> configure, bool exact = false, bool disabled = false)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var route = AddItemRoute(label, icon, navTarget);
        var children = new SubNavigationBuilder(addRoute, attachText, route);
        configure(children);
        items.Add(new(label, route, icon, exact, disabled, children.Complete()));
        return this;
    }

    public ISubNavigationBuilder AddSubNav(TextReference label, string icon, string navTarget, bool exact = false, bool disabled = false)
    {
        ArgumentNullException.ThrowIfNull(label);
        AddSubNav(label.FallbackText ?? label.Token, icon, navTarget, exact, disabled);
        attachText(items[^1], label);
        return this;
    }

    public ISubNavigationBuilder AddSubNav(TextReference label, string icon, string navTarget, Action<ISubNavigationBuilder> configure, bool exact = false, bool disabled = false)
    {
        ArgumentNullException.ThrowIfNull(label);
        AddSubNav(label.FallbackText ?? label.Token, icon, navTarget, configure, exact, disabled);
        attachText(items[^1], label);
        return this;
    }

    private string AddItemRoute(string label, string icon, string navTarget)
    {
        if (completed) throw new InvalidOperationException("Sub-navigation configuration has already been completed.");
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(icon);
        var route = ApplicationBuilder.NormalizeRoute(navTarget);
        if (!routes.Add(route)) throw new ArgumentException($"Sub-navigation route '{route}' is already configured.", nameof(navTarget));
        // A destination may also be its own first child, as with a primary landing page.
        return string.Equals(route, parentRoute, StringComparison.OrdinalIgnoreCase) ? route : addRoute(route);
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
