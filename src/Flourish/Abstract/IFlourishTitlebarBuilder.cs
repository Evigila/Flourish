namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Configures the elements displayed in the Flourish shell title bar.
/// </summary>
/// <remarks>
/// Calling a configuration method enables its corresponding title bar element.
/// Elements that are not configured remain hidden.
/// </remarks>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureTitleBar(titlebar =>
/// {
///     titlebar
///         .SetApplicationTitle("Foobar")
///         .SetApplicationSubTitle("Desktop workspace")
///         .SetNavToggle();
/// });
/// ]]></code>
/// </example>
public interface IFlourishTitlebarBuilder
{
    /// <summary>
    /// Configures and displays the search box with access to the application service provider.
    /// </summary>
    /// <param name="placeholder">The placeholder text displayed in the search box.</param>
    /// <param name="handler">The callback invoked whenever the search text changes.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder SetSearch(
        string placeholder,
        Action<IServiceProvider, string> handler
    );

    /// <summary>
    /// Configures the breadcrumb display behavior and enables the breadcrumb button.
    /// </summary>
    /// <param name="option">The breadcrumb display behavior.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder SetBreadcrumbButton(
        BreadcrumbShowOption option = BreadcrumbShowOption.Auto
    );

    /// <summary>
    /// Displays the navigation panel toggle button.
    /// </summary>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder SetNavToggle();

    /// <summary>
    /// Configures and displays the built-in or application-provided logo button in the title bar,
    /// selects the identity fields shown in its information surface, and uses the same image as the
    /// shell window icon.
    /// </summary>
    /// <param name="logoPath">
    /// A relative URI, absolute URI, or WPF pack URI for the logo image. When omitted,
    /// Flourish uses its built-in application icon.
    /// </param>
    /// <param name="showApplicationTitle">Whether the logo information surface displays the application title.</param>
    /// <param name="showApplicationSubTitle">Whether the logo information surface displays the application subtitle.</param>
    /// <param name="showProjectTitle">Whether the logo information surface displays the active project title.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>The same image is used for the shell window icon.</remarks>
    IFlourishTitlebarBuilder SetLogo(
        string? logoPath = null,
        bool showApplicationTitle = true,
        bool showApplicationSubTitle = true,
        bool showProjectTitle = false
    );

    /// <summary>
    /// Configures the application title and displays the title button when multi-project mode is disabled.
    /// </summary>
    /// <param name="title">The application title.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder SetApplicationTitle(string title);

    /// <summary>
    /// Configures the application subtitle displayed in the logo information surface.
    /// </summary>
    /// <param name="subTitle">The application subtitle.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder SetApplicationSubTitle(string subTitle);

    /// <summary>
    /// Configures the text shown by the title button when multi-project mode has no active project.
    /// </summary>
    /// <param name="placeholder">The non-empty unnamed-project placeholder.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder SetUnnamedProjectPlaceholder(string placeholder = "Unnamed project");

    /// <summary>
    /// Configures and displays the profile trigger using the built-in default profile.
    /// </summary>
    /// <param name="nameOrder">The order used to display profile names and initials.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder SetProfile(NameOrder nameOrder = NameOrder.FirstLast);

    /// <summary>
    /// Configures and displays the theme toggle.
    /// </summary>
    /// <param name="mode">The theme used when no saved preference is available.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder SetThemeToggle(
        FlourishTheme mode = FlourishTheme.System
    );
}
