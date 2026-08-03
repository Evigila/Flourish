using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract.Builder;

/// <summary>
/// Configures Flourish shell features and shared options.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigShell(shell =>
/// {
///     shell.UseTitleBar()
///          .UseMultiProject()
///          .UseNavigation()
///          .UseCenterContent(enabled: true, contentWidth: 1200)
///          .UseDynamicToolbar()
///          .UseTips(enabled: true, delay: 200)
///          .UseMotion()
///          .UseSmoothScroll()
///          .UseMaterialEffect(enabled: true, effect: MaterialEffect.Mica)
///          .InitGlobalFont("Segoe UI", 12, 14, 22, 16, 24, 32)
///          .UseStatusBar();
/// });
/// ]]></code>
/// </example>
public interface IFlourishShellBuilder
{
    /// <summary>
    /// Enables or disables the shell title bar.
    /// </summary>
    /// <param name="enabled">A value indicating whether the title bar should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseTitleBar(bool enabled = true);

    /// <summary>
    /// Enables or disables the project-aware title bar and its project selection surface.
    /// </summary>
    /// <param name="enabled">A value indicating whether the title selector should expose every registered project and the new-project action.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// Project metadata is restored from and written to the configured project catalog. The built-in
    /// <see cref="IProjectBehavior" /> supplies save prompts and placeholder <c>.txt</c> files;
    /// applications can replace that behavior for their own project lifecycle. Selecting a project
    /// changes the active shell identity but does not load application-owned project content. When
    /// disabled, Flourish does not route Ctrl+S or window closing through project behavior.
    /// </remarks>
    IFlourishShellBuilder UseMultiProject(bool enabled = true);

    /// <summary>
    /// Enables or disables the shell navigation panel.
    /// </summary>
    /// <param name="enabled">A value indicating whether navigation should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseNavigation(bool enabled = true);

    /// <summary>
    /// Enables or disables a maximum width for navigated page content and aligned Shell content
    /// regions. Content stretches across narrower viewports and is centered after the viewport
    /// exceeds <paramref name="contentWidth" />. The limit remains active while the navigation
    /// panel transitions and while the window is maximized. The page scrolling surface remains
    /// full width, so its vertical scrollbar stays at the edge of the Shell content area.
    /// </summary>
    /// <param name="enabled">
    /// A value indicating whether the content width limit should be enabled.
    /// </param>
    /// <param name="contentWidth">
    /// The finite, positive maximum width of navigated page content in device-independent pixels.
    /// </param>
    /// <param name="usePersistedPreference">Whether the persisted user preference is restored and updated.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseCenterContent(
        bool enabled = true,
        double contentWidth = 1200,
        bool usePersistedPreference = true
    );

    /// <summary>
    /// Enables or disables the dynamic toolbar surface.
    /// </summary>
    /// <param name="enabled">A value indicating whether the dynamic toolbar should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseDynamicToolbar(bool enabled = true);

    /// <summary>
    /// Enables or disables Flourish tooltips and configures their initial display delay.
    /// </summary>
    /// <param name="enabled">A value indicating whether Flourish tooltips should be enabled.</param>
    /// <param name="delay">The initial tooltip delay in milliseconds.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseTips(bool enabled = true, int delay = 200);

    /// <summary>
    /// Enables or disables Flourish motion.
    /// </summary>
    /// <param name="enabled">A value indicating whether motion should be enabled.</param>
    /// <param name="usePersistedPreference">Whether the persisted user preference is restored and updated.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseMotion(
        bool enabled = true,
        bool usePersistedPreference = true
    );

    /// <summary>
    /// Enables or disables the shell material effect and selects the effect to use.
    /// </summary>
    /// <param name="enabled">A value indicating whether the material effect should be enabled.</param>
    /// <param name="effect">The material effect applied to the shell window.</param>
    /// <param name="usePersistedPreference">Whether the persisted user preference is restored and updated.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseMaterialEffect(
        bool enabled = true,
        MaterialEffect effect = MaterialEffect.Mica,
        bool usePersistedPreference = true
    );

    /// <summary>
    /// Enables or disables custom primary, secondary, and accent theme colors.
    /// </summary>
    /// <param name="enabled">A value indicating whether the custom colors should be enabled.</param>
    /// <param name="colors">The application theme colors.</param>
    /// <param name="usePersistedPreference">Whether the persisted user preference is restored and updated.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseThemeColors(
        bool enabled,
        FlourishThemeColors colors,
        bool usePersistedPreference = true
    );

    /// <summary>
    /// Enables or disables a custom shared corner radius for Flourish controls and surfaces.
    /// </summary>
    /// <param name="enabled">A value indicating whether the custom radius should be enabled.</param>
    /// <param name="radius">A finite, non-negative radius in device-independent pixels.</param>
    /// <param name="usePersistedPreference">Whether the persisted user preference is restored and updated.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseCornerRadius(
        bool enabled,
        double radius = 6,
        bool usePersistedPreference = true
    );

    /// <summary>
    /// Enables or disables smooth mouse-wheel scrolling for Flourish scroll viewers by default.
    /// </summary>
    /// <param name="enabled">A value indicating whether smooth scrolling should be enabled.</param>
    /// <param name="usePersistedPreference">Whether the persisted user preference is restored and updated.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// A locally assigned
    /// <see cref="ArkheideSystem.Flourish.Controls.ScrollViewer.IsSmoothScrollingEnabled" />
    /// value takes precedence over this shell default.
    /// </remarks>
    IFlourishShellBuilder UseSmoothScroll(
        bool enabled = true,
        bool usePersistedPreference = true
    );

    /// <summary>
    /// Configures the global font and its explicit text and icon size scale.
    /// </summary>
    /// <param name="fontFamily">The font family name.</param>
    /// <param name="smallFontSize">The small font size.</param>
    /// <param name="standardFontSize">The standard font size.</param>
    /// <param name="iconFontSize">The icon font size.</param>
    /// <param name="largeFontSize">The large font size.</param>
    /// <param name="extraLargeFontSize">The extra-large font size.</param>
    /// <param name="headerSizeFontSize">The header-size font size.</param>
    /// <param name="usePersistedPreference">Whether the persisted user preference is restored and updated.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder InitGlobalFont(
        string fontFamily = "Microsoft Yahei",
        double smallFontSize = 12,
        double standardFontSize = 14,
        double iconFontSize = 22,
        double largeFontSize = 16,
        double extraLargeFontSize = 24,
        double headerSizeFontSize = 32,
        bool usePersistedPreference = true
    );

    /// <summary>
    /// Overrides the font used by one page type while leaving all other pages on the global font.
    /// </summary>
    /// <typeparam name="TPage">The WPF page type that receives the override.</typeparam>
    /// <param name="fontFamily">The page-specific font family name.</param>
    /// <param name="smallFontSize">The page-specific small size, or <see langword="null"/> to follow the global size.</param>
    /// <param name="standardFontSize">The page-specific standard size, or <see langword="null"/> to follow the global size.</param>
    /// <param name="iconFontSize">The page-specific icon size, or <see langword="null"/> to follow the global size.</param>
    /// <param name="largeFontSize">The page-specific large size, or <see langword="null"/> to follow the global size.</param>
    /// <param name="extraLargeFontSize">The page-specific extra-large size, or <see langword="null"/> to follow the global size.</param>
    /// <param name="headerSizeFontSize">The page-specific header size, or <see langword="null"/> to follow the global size.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder InitOverrideFont<TPage>(
        string fontFamily,
        double? smallFontSize,
        double? standardFontSize,
        double? iconFontSize,
        double? largeFontSize,
        double? extraLargeFontSize,
        double? headerSizeFontSize
    )
        where TPage : Page;

    /// <summary>
    /// Enables or disables the shell status bar.
    /// </summary>
    /// <param name="enabled">A value indicating whether the status bar should be enabled.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishShellBuilder UseStatusBar(bool enabled = true);
}
