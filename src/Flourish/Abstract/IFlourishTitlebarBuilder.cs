using System.Windows.Media;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Configures the Flourish shell title bar.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureTitleBar(titlebar =>
/// {
///     titlebar.ShowTitle().SetTitle("Foobar");
/// });
/// ]]></code>
/// </example>
public interface IFlourishTitlebarBuilder
{
    /// <summary>
    /// Shows or hides the search box.
    /// </summary>
    /// <param name="enabled">A value indicating whether the search box should be shown.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// titlebar.ShowSearch();
    /// ]]></code>
    /// </example>
    IFlourishTitlebarBuilder ShowSearch(bool enabled = true);

    /// <summary>
    /// Shows or hides breadcrumb navigation.
    /// </summary>
    /// <param name="enabled">A value indicating whether breadcrumb navigation should be shown.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// titlebar.ShowBreadcrumb(false);
    /// ]]></code>
    /// </example>
    IFlourishTitlebarBuilder ShowBreadcrumb(bool enabled = true);

    /// <summary>
    /// Shows or hides the navigation panel toggle button.
    /// </summary>
    /// <param name="enabled">A value indicating whether the navigation toggle should be shown.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// titlebar.ShowNavToggle();
    /// ]]></code>
    /// </example>
    IFlourishTitlebarBuilder ShowNavToggle(bool enabled = true);

    /// <summary>
    /// Shows or hides the logo area.
    /// </summary>
    /// <param name="enabled">A value indicating whether the logo should be shown.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// titlebar.ShowLogo();
    /// ]]></code>
    /// </example>
    IFlourishTitlebarBuilder ShowLogo(bool enabled = true);

    /// <summary>
    /// Shows or hides the title text.
    /// </summary>
    /// <param name="enabled">A value indicating whether the title should be shown.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// titlebar.ShowTitle();
    /// ]]></code>
    /// </example>
    IFlourishTitlebarBuilder ShowTitle(bool enabled = true);

    /// <summary>
    /// Shows or hides the subtitle text.
    /// </summary>
    /// <param name="enabled">A value indicating whether the subtitle should be shown.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// titlebar.ShowSubTitle(false);
    /// ]]></code>
    /// </example>
    IFlourishTitlebarBuilder ShowSubTitle(bool enabled = true);

    /// <summary>
    /// Shows or hides the profile area.
    /// </summary>
    /// <param name="enabled">A value indicating whether the profile area should be shown.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// The profile also requires <see cref="IFlourishShellBuilder.UseProfile" /> and
    /// <see cref="IFlourishShellBuilder.UseTitleBar" /> to be enabled.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// titlebar.ShowProfile();
    /// ]]></code>
    /// </example>
    IFlourishTitlebarBuilder ShowProfile(bool enabled = true);

    /// <summary>
    /// Shows or hides the title bar theme toggle.
    /// </summary>
    /// <param name="enabled">A value indicating whether the theme toggle should be shown.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// titlebar.ShowThemeToggle();
    /// ]]></code>
    /// </example>
    IFlourishTitlebarBuilder ShowThemeToggle(bool enabled = true);

    /// <summary>
    /// Sets whether closing the title bar should exit through the tray flow.
    /// </summary>
    /// <param name="enabled">A value indicating whether tray exit behavior should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// titlebar.SetTrayExit(enabled: true);
    /// ]]></code>
    /// </example>
    IFlourishTitlebarBuilder SetTrayExit(bool enabled = false);

    /// <summary>
    /// Sets the title text.
    /// </summary>
    /// <param name="title">The title displayed in the title bar.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// titlebar.SetTitle("Foobar");
    /// ]]></code>
    /// </example>
    IFlourishTitlebarBuilder SetTitle(string title);

    /// <summary>
    /// Sets the subtitle text.
    /// </summary>
    /// <param name="subtitle">The subtitle displayed next to the title.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// titlebar.SetSubtitle("Workspace");
    /// ]]></code>
    /// </example>
    IFlourishTitlebarBuilder SetSubtitle(string subtitle);

    /// <summary>
    /// Sets the logo image by using a WPF pack URI.
    /// </summary>
    /// <param name="packUri">The pack URI of the logo image.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// titlebar.SetLogo("pack://application:,,,/Assets/logo.png");
    /// ]]></code>
    /// </example>
    IFlourishTitlebarBuilder SetLogo(string packUri);

    /// <summary>
    /// Sets the logo image directly.
    /// </summary>
    /// <param name="logoSource">The image source displayed in the logo area.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder SetLogo(ImageSource logoSource);

    /// <summary>
    /// Sets the fallback text used when no logo image is available.
    /// </summary>
    /// <param name="fallbackText">The fallback text. Flourish displays the first character.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder SetLogoFallbackText(string fallbackText);

    /// <summary>
    /// Sets the search box placeholder text.
    /// </summary>
    /// <param name="placeholder">The placeholder text displayed in the search box.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// titlebar.SetSearchPlaceholder("Search images");
    /// ]]></code>
    /// </example>
    IFlourishTitlebarBuilder SetSearchPlaceholder(string placeholder);

    /// <summary>
    /// Handles title bar search text changes.
    /// </summary>
    /// <param name="searchTextChanged">The callback invoked whenever the search text changes.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder SetSearchHandler(Action<string> searchTextChanged);

    /// <summary>
    /// Handles title bar search text changes with access to the application service provider.
    /// </summary>
    /// <param name="searchTextChanged">The callback invoked whenever the search text changes.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishTitlebarBuilder SetSearchHandler(
        Action<IServiceProvider, string> searchTextChanged
    );

    /// <summary>
    /// Sets when breadcrumb navigation should be displayed.
    /// </summary>
    /// <param name="behavior">The breadcrumb display behavior.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// titlebar.SetBreadcrumbBehavior(BreadcrumbShowOption.Auto);
    /// ]]></code>
    /// </example>
    IFlourishTitlebarBuilder SetBreadcrumbBehavior(
        BreadcrumbShowOption behavior = BreadcrumbShowOption.Auto
    );
}
