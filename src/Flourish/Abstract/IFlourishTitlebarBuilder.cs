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
///         .SetTitle("Foobar")
///         .SetSubTitle("Workspace")
///         .SetNavToggle();
/// });
/// ]]></code>
/// </example>
public interface IFlourishTitlebarBuilder
{
    /// <summary>
    /// Configures and displays the search box.
    /// </summary>
    /// <param name="placeholder">The placeholder text displayed in the search box.</param>
    /// <param name="handler">The callback invoked whenever the search text changes.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder SetSearch(string placeholder, Action<string> handler);

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
    /// Configures and displays the logo image.
    /// </summary>
    /// <param name="logoPath">A relative URI, absolute URI, or WPF pack URI for the logo image.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder SetLogo(string logoPath);

    /// <summary>
    /// Configures and displays the title text.
    /// </summary>
    /// <param name="title">The title displayed in the title bar.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder SetTitle(string title);

    /// <summary>
    /// Configures and displays the subtitle text.
    /// </summary>
    /// <param name="subTitle">The subtitle displayed next to the title.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder SetSubTitle(string subTitle);

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
