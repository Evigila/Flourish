using ArkheideSystem.Flourish.Abstract;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArkheideSystem.Flourish.Blazor.Abstract;

/// <summary>Configures the framework-owned application shell.</summary>
public interface IFrameworkBuilder
{
    /// <summary>Registers an optional integration using the resolved framework defaults and host overrides.</summary>
    IFrameworkBuilder ConfigureServices(Action<IServiceCollection, IConfiguration> configure);
    IFrameworkBuilder ConfigureProject(Action<IProjectBuilder> configure);
    IFrameworkBuilder ConfigureTopBar(Action<ITopBarBuilder> configure);
    IFrameworkBuilder ConfigureNavigation(Action<INavigationBuilder> configure);
    IFrameworkBuilder ConfigureLayout(Action<ILayoutBuilder> configure);
    IFrameworkBuilder SetCommandParser<TParser>() where TParser : class, ICommandParser;
}

/// <summary>Configures project identity independently of where it is displayed.</summary>
public interface IProjectBuilder
{
    IProjectBuilder SetProjectName(string projectName);
    IProjectBuilder SetProjectName(TextReference projectName);
    /// <summary>Sets the shared logo. Null restores the built-in browse logo.</summary>
    IProjectBuilder SetLogo(string? logoPath = null, string? alternativeText = null);
    /// <summary>Sets the browser tab icon. Null or empty falls back to the project logo.</summary>
    IProjectBuilder SetFavicon(string? faviconPath = null);
}

/// <summary>Configures the framework-owned top bar.</summary>
public interface ITopBarBuilder
{
    ITopBarBuilder DisplayLogo(bool display = true);
    ITopBarBuilder DisplayProjectName(bool display = true);
    ITopBarBuilder SetSearch(bool enabled = true, string label = "Search");
    ITopBarBuilder SetSearch(TextReference label, bool enabled = true);
    ITopBarBuilder SetNavigationToggle(bool enabled = true);
    ITopBarBuilder AddMenu(string menuName, Action<ITopBarMenuBuilder> configure);
    ITopBarBuilder AddMenu(TextReference menuName, Action<ITopBarMenuBuilder> configure);
    ITopBarBuilder InjectToLeft<TComponent>() where TComponent : IComponent;
    ITopBarBuilder InjectToCenter<TComponent>() where TComponent : IComponent;
    ITopBarBuilder InjectToRight<TComponent>() where TComponent : IComponent;
}

public interface ITopBarMenuBuilder
{
    ITopBarMenuBuilder AddMenuItem(string menuItemName, string commandKey, bool disabled = false, bool destructive = false);
    ITopBarMenuBuilder AddMenuItem(TextReference menuItemName, string commandKey, bool disabled = false, bool destructive = false);
}

public interface INavigationBuilder
{
    INavigationBuilder AddNav(string label, string icon, string navTarget, Action<ISubNavigationBuilder>? configure = null, bool exact = false);
    INavigationBuilder AddNav(TextReference label, string icon, string navTarget, Action<ISubNavigationBuilder>? configure = null, bool exact = false);
    INavigationBuilder AddNavButton(string label, string icon, string commandKey);
    INavigationBuilder AddNavButton(TextReference label, string icon, string commandKey);
    INavigationBuilder AddFixedNav(string label, string icon, string navTarget, bool exact = false);
    INavigationBuilder AddFixedNav(TextReference label, string icon, string navTarget, bool exact = false);
    INavigationBuilder AddFixedNavButton(string label, string icon, string commandKey);
    INavigationBuilder AddFixedNavButton(TextReference label, string icon, string commandKey);
}

public interface ISubNavigationBuilder
{
    ISubNavigationBuilder AddSubNav(string label, string icon, string navTarget, bool exact = false, bool disabled = false);
    ISubNavigationBuilder AddSubNav(TextReference label, string icon, string navTarget, bool exact = false, bool disabled = false);
    /// <summary>Adds a destination with children displayed in the same navigation tree.</summary>
    ISubNavigationBuilder AddSubNav(string label, string icon, string navTarget, Action<ISubNavigationBuilder> configure, bool exact = false, bool disabled = false);
    ISubNavigationBuilder AddSubNav(TextReference label, string icon, string navTarget, Action<ISubNavigationBuilder> configure, bool exact = false, bool disabled = false);
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

/// <summary>Per-user runtime appearance. State is scoped to a server circuit or client host.</summary>
public interface IAppearanceService
{
    AppearanceState Current { get; }
    event EventHandler? Changed;
    void SetColors(string primary, string accent);
    void SetTheme(ApplicationTheme theme);
}

/// <summary>Optional visual state consumed by the framework without depending on a design package.</summary>
public interface IThemeProvider
{
    string CssClass { get; }
    string CssVariables { get; }
    string StylesheetHref { get; }
    event EventHandler? Changed;
}
