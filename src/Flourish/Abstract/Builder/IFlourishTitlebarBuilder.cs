namespace ArkheideSystem.Flourish.Abstract.Builder;

/// <summary>
/// Configures the elements displayed in the Flourish shell title bar.
/// </summary>
/// <remarks>
/// When the shell title bar is enabled, elements with built-in defaults are enabled automatically.
/// Calling a configuration method overrides the corresponding default. Input-driven elements such
/// as search remain hidden until they are configured explicitly.
/// </remarks>
/// <example>
/// <code><![CDATA[
/// builder.ConfigTitleBar(titlebar =>
/// {
///     titlebar
///         .InitApplicationTitle("Foobar")
///         .InitApplicationSubTitle("Desktop workspace")
///         .UseNavigationToggle();
/// });
/// ]]></code>
/// </example>
public interface IFlourishTitlebarBuilder
{
    /// <summary>
    /// Configures and displays the search box with access to the application service provider.
    /// </summary>
    /// <param name="enabled">A value indicating whether search should be enabled initially.</param>
    /// <param name="placeholder">The placeholder text displayed in the search box.</param>
    /// <param name="handler">The callback invoked whenever the search text changes.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder UseSearch(
        bool enabled = true,
        string placeholder = "Search",
        Action<IServiceProvider, string>? handler = null
    );

    /// <summary>
    /// Configures the breadcrumb display behavior and enables the breadcrumb button.
    /// </summary>
    /// <param name="enabled">A value indicating whether breadcrumb navigation should be enabled initially.</param>
    /// <param name="option">The breadcrumb display behavior.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder UseBreadcrumb(
        bool enabled = true,
        BreadcrumbShowOption option = BreadcrumbShowOption.Auto
    );

    /// <summary>
    /// Displays the navigation panel toggle button.
    /// </summary>
    /// <param name="enabled">A value indicating whether the navigation toggle should be enabled initially.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder UseNavigationToggle(bool enabled = true);

    /// <summary>
    /// Configures and displays the built-in or application-provided logo button in the title bar,
    /// selects the identity fields shown in its information surface, and uses the same image as the
    /// shell window icon.
    /// </summary>
    /// <param name="enabled">A value indicating whether the logo should be enabled initially.</param>
    /// <param name="logoPath">
    /// A relative URI, absolute URI, or WPF pack URI for the logo image. When omitted,
    /// Flourish uses its built-in application icon.
    /// </param>
    /// <param name="showApplicationTitle">Whether the logo information surface displays the application title.</param>
    /// <param name="showApplicationSubTitle">Whether the logo information surface displays the application subtitle.</param>
    /// <param name="showProjectTitle">Whether the logo information surface displays the active project title.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>The same image is used for the shell window icon.</remarks>
    IFlourishTitlebarBuilder UseLogo(
        bool enabled = true,
        string? logoPath = null,
        bool showApplicationTitle = true,
        bool showApplicationSubTitle = true,
        bool showProjectTitle = false
    );

    /// <summary>
    /// Configures the application title and displays it in the title selector when multi-project mode is disabled.
    /// </summary>
    /// <param name="title">The application title.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder InitApplicationTitle(string title = "MyApp");

    /// <summary>
    /// Configures the application subtitle displayed in the logo information surface.
    /// </summary>
    /// <param name="subTitle">The application subtitle.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder InitApplicationSubTitle(string subTitle = "MyApp");

    /// <summary>
    /// Configures the display name used for a project that has no storage path or when no project is active.
    /// </summary>
    /// <param name="placeholder">The non-empty display name for an unpersisted project.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder InitUnnamedProjectPlaceholder(string placeholder = "Unnamed project");

    /// <summary>
    /// Configures and displays the profile trigger using the built-in default profile.
    /// </summary>
    /// <param name="enabled">A value indicating whether the profile trigger should be enabled initially.</param>
    /// <param name="nameOrder">The order used to display profile names and initials.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder UseProfile(
        bool enabled = true,
        NameOrder nameOrder = NameOrder.FirstLast
    );

    /// <summary>
    /// Configures and displays the theme toggle.
    /// </summary>
    /// <param name="enabled">A value indicating whether the theme toggle should be enabled initially.</param>
    /// <param name="mode">The theme used when no saved preference is available.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder UseThemeToggle(
        bool enabled = true,
        FlourishTheme mode = FlourishTheme.System
    );
}
