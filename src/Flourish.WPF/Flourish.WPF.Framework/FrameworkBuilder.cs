using System.Windows.Media;
using ArkheideSystem.Flourish.WPF.Abstract;

namespace ArkheideSystem.Flourish.WPF;

/// <summary>Native equivalent of the Blazor project/top-bar/navigation composition; no host business services.</summary>
public sealed class FrameworkBuilder
{
    private readonly ProjectBuilder project = new();
    private readonly TopBarBuilder top = new();
    private readonly NavigationBuilder navigation = new();
    public ITextProvider? TextProvider { get; set; }
    public FrameworkBuilder ConfigureProject(Action<ProjectBuilder> configure) { ArgumentNullException.ThrowIfNull(configure); configure(project); return this; }
    public FrameworkBuilder ConfigureTopBar(Action<TopBarBuilder> configure) { ArgumentNullException.ThrowIfNull(configure); configure(top); return this; }
    public FrameworkBuilder ConfigureNavigation(Action<NavigationBuilder> configure) { ArgumentNullException.ThrowIfNull(configure); configure(navigation); return this; }
    public FrameworkOptions Build()
    {
        var entries = navigation.Entries.ToArray();
        var routes = Flatten(entries).Select(e => e.Route).ToArray();
        if (routes.Distinct(StringComparer.Ordinal).Count() != routes.Length) throw new ArgumentException("Navigation routes must be unique.");
        var provider = TextProvider;
        var placements = top.Right.Select<Func<ITextProvider?, object>, Func<object>>(factory => () => factory(provider)).ToArray();
        return new FrameworkOptions(project.Name, project.NameReference, project.Logo, Array.AsReadOnly(entries), Array.AsReadOnly(placements), provider);
    }
    internal static IEnumerable<NavigationEntry> Flatten(IEnumerable<NavigationEntry> entries)
    {
        foreach (var entry in entries) { yield return entry; foreach (var child in Flatten(entry.Children)) yield return child; }
    }
}
public sealed class ProjectBuilder
{
    internal string Name = "Application";
    internal TextReference? NameReference;
    internal ImageSource? Logo;
    public ProjectBuilder SetProjectName(string name) { ArgumentException.ThrowIfNullOrWhiteSpace(name); Name = name; NameReference = null; return this; }
    public ProjectBuilder SetProjectName(TextReference name) { ArgumentNullException.ThrowIfNull(name); NameReference = name; Name = name.FallbackText ?? name.Token; return this; }
    public ProjectBuilder SetLogo(ImageSource? logo) { Logo = logo; return this; }
}
public sealed class TopBarBuilder
{
    internal readonly List<Func<ITextProvider?, object>> Right = [];
    public TopBarBuilder InjectToRight(Func<object> content) { ArgumentNullException.ThrowIfNull(content); Right.Add(_ => content()); return this; }
    /// <summary>Composes the ordinary ActionMenu core. Consumers own action availability and execution.</summary>
    public TopBarBuilder AddMenu(string label, Func<IEnumerable<MenuAction>> actions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        return AddMenu(label, null, actions);
    }
    public TopBarBuilder AddMenu(TextReference label, Func<IEnumerable<MenuAction>> actions)
    {
        ArgumentNullException.ThrowIfNull(label);
        return AddMenu(label.FallbackText ?? label.Token, label, actions);
    }
    private TopBarBuilder AddMenu(string label, TextReference? reference, Func<IEnumerable<MenuAction>> actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        Right.Add(provider =>
        {
            var caption = new System.Windows.Controls.TextBlock { VerticalAlignment = System.Windows.VerticalAlignment.Center };
            var trigger = new System.Windows.Controls.StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };
            trigger.Children.Add(caption);
            trigger.Children.Add(new Controls.ExpansionIndicator { Margin = new System.Windows.Thickness(4, 0, 0, 0), VerticalAlignment = System.Windows.VerticalAlignment.Center });
            var menu = new Controls.ActionMenu { OpenOnHover = true, Icon = string.Empty, Content = trigger };
            menu.SetResourceReference(System.Windows.Controls.Control.ForegroundProperty, "Flourish.Brush.ChromeInk");
            var subscribed = false;
            void Update()
            {
                caption.Text = reference is not null && provider is not null ? provider.Get(reference) : label;
                menu.Actions = actions().ToArray();
                System.Windows.Automation.AutomationProperties.SetName(menu, caption.Text);
            }
            void Changed(object? sender, EventArgs args)
            {
                if (menu.Dispatcher.CheckAccess()) { if (subscribed) Update(); }
                else menu.Dispatcher.InvokeAsync(() => { if (subscribed) Update(); });
            }
            menu.Loaded += (_, _) => { if (provider is not null && !subscribed) { subscribed = true; provider.Changed += Changed; } Update(); };
            menu.Unloaded += (_, _) => { if (provider is not null && subscribed) { subscribed = false; provider.Changed -= Changed; } };
            Update();
            return menu;
        });
        return this;
    }
}
public class SubNavigationBuilder
{
    internal readonly List<NavigationEntry> Entries = [];
    public SubNavigationBuilder AddSubNav(string label, string icon, string route, Func<object> content, bool disabled = false, Action<SubNavigationBuilder>? children = null) => Add(label, null, icon, route, content, disabled, children);
    public SubNavigationBuilder AddSubNav(TextReference label, string icon, string route, Func<object> content, bool disabled = false, Action<SubNavigationBuilder>? children = null) => Add(label.FallbackText ?? label.Token, label, icon, route, content, disabled, children);
    protected SubNavigationBuilder Add(string label, TextReference? reference, string icon, string route, Func<object> content, bool disabled, Action<SubNavigationBuilder>? children)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label); ArgumentException.ThrowIfNullOrWhiteSpace(route); ArgumentNullException.ThrowIfNull(content);
        var sub = new SubNavigationBuilder(); children?.Invoke(sub);
        Entries.Add(new NavigationEntry(label, icon, route, content, Array.AsReadOnly(sub.Entries.ToArray()), disabled, reference)); return this;
    }
}
public sealed class NavigationBuilder : SubNavigationBuilder
{
    public NavigationBuilder AddNav(string label, string icon, string route, Func<object> content, Action<SubNavigationBuilder>? secondary = null, bool disabled = false)
    { Add(label, null, icon, route, content, disabled, secondary); return this; }
    public NavigationBuilder AddNav(TextReference label, string icon, string route, Func<object> content, Action<SubNavigationBuilder>? secondary = null, bool disabled = false)
    { Add(label.FallbackText ?? label.Token, label, icon, route, content, disabled, secondary); return this; }
}
