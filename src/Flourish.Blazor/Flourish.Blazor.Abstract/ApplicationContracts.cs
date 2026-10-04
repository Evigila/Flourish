using ArkheideSystem.Flourish.Abstract;
using Microsoft.AspNetCore.Components;

namespace ArkheideSystem.Flourish.Blazor.Abstract;

/// <summary>Configures the framework-owned application shell.</summary>
public interface IFrameworkBuilder
{
    IFrameworkBuilder ConfigureTopBar(Action<ITopBarBuilder> configure);
    IFrameworkBuilder ConfigureNavigation(Action<INavigationBuilder> configure);
    IFrameworkBuilder ConfigureLayout(Action<ILayoutBuilder> configure);
    IFrameworkBuilder SetCommandParser<TParser>() where TParser : class, ICommandParser;
}

/// <summary>Configures the framework-owned top bar.</summary>
public interface ITopBarBuilder
{
    ITopBarBuilder SetAppName(string displayName);
    ITopBarBuilder SetIcon(string iconPath, string? alternativeText = null);
    ITopBarBuilder SetSearch(bool enabled = true, string label = "Search");
    ITopBarBuilder SetNavigationToggle(bool enabled = true);
    ITopBarBuilder AddMenu(string menuName, Action<ITopBarMenuBuilder> configure);
    ITopBarBuilder InjectToLeft<TComponent>() where TComponent : IComponent;
    ITopBarBuilder InjectToCenter<TComponent>() where TComponent : IComponent;
    ITopBarBuilder InjectToRight<TComponent>() where TComponent : IComponent;
}

public interface ITopBarMenuBuilder
{
    ITopBarMenuBuilder AddMenuItem(string menuItemName, string commandKey, bool disabled = false, bool destructive = false);
}

public interface INavigationBuilder
{
    INavigationBuilder AddNav(string label, string icon, string navTarget, Action<ISubNavigationBuilder>? configure = null, bool exact = false);
    INavigationBuilder AddNavButton(string label, string icon, string commandKey);
    INavigationBuilder AddFixedNav(string label, string icon, string navTarget, bool exact = false);
    INavigationBuilder AddFixedNavButton(string label, string icon, string commandKey);

    [Obsolete("Use AddNav with an explicit primary navigation target.")]
    INavigationBuilder AddGroup(string key, string label, string icon, Action<INavigationGroupBuilder> configure);
}

public interface ISubNavigationBuilder
{
    ISubNavigationBuilder AddSubNav(string label, string icon, string navTarget, bool exact = false, bool disabled = false);
    /// <summary>Adds a destination with children displayed in the same navigation tree.</summary>
    ISubNavigationBuilder AddSubNav(string label, string icon, string navTarget, Action<ISubNavigationBuilder> configure, bool exact = false, bool disabled = false);
}

/// <summary>Legacy shell configuration retained for source migration.</summary>
[Obsolete("Use IFrameworkBuilder and AddFlourishFramework.")]
public interface IApplicationBuilder : IFrameworkBuilder
{
    IApplicationBuilder UseTitleBar(Action<ITitleBarBuilder>? configure = null);
    IApplicationBuilder UseNavigation(Action<INavigationBuilder> configure);
    new IApplicationBuilder ConfigureLayout(Action<ILayoutBuilder> configure);
}

[Obsolete("Use ITopBarBuilder.")]
public interface ITitleBarBuilder
{
    ITitleBarBuilder SetApplicationTitle(string title);
    ITitleBarBuilder SetSearch(bool enabled = true, string label = "Search");
    ITitleBarBuilder SetNavigationToggle(bool enabled = true);
}

[Obsolete("Use ISubNavigationBuilder.")]
public interface INavigationGroupBuilder
{
    INavigationGroupBuilder SetSecondaryNavigation(bool enabled = true);
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
