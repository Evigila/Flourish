using System.Windows;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Configures custom WPF content displayed in predefined Flourish regions.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureCustomHandler(custom =>
/// {
///     custom.Add(
///         FlourishRegion.TitlebarEnd,
///         services => new Button { Content = "Account" });
/// });
/// ]]></code>
/// </example>
public interface IFlourishCustomHandlerBuilder
{
    /// <summary>
    /// Adds custom content to a shell region.
    /// </summary>
    /// <param name="region">The shell region that receives the content.</param>
    /// <param name="contentFactory">A factory that creates the WPF element when the shell is created.</param>
    /// <param name="order">The display order inside the region. Lower values are displayed first.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishCustomHandlerBuilder Add(
        FlourishRegion region,
        Func<IServiceProvider, FrameworkElement> contentFactory,
        int order = 0
    );

    /// <summary>
    /// Adds custom content to a shell region.
    /// </summary>
    /// <param name="region">The shell region that receives the content.</param>
    /// <param name="contentFactory">A factory that creates the WPF element when the shell is created.</param>
    /// <param name="order">The display order inside the region. Lower values are displayed first.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishCustomHandlerBuilder Add(
        FlourishRegion region,
        Func<FrameworkElement> contentFactory,
        int order = 0
    );

    /// <summary>
    /// Adds an existing WPF element to a shell region.
    /// </summary>
    /// <param name="region">The shell region that receives the content.</param>
    /// <param name="content">The WPF element displayed in the region.</param>
    /// <param name="order">The display order inside the region. Lower values are displayed first.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// Prefer the factory overload when the element depends on application services or should be
    /// created together with the shell window.
    /// </remarks>
    IFlourishCustomHandlerBuilder Add(
        FlourishRegion region,
        FrameworkElement content,
        int order = 0
    );

    /// <summary>
    /// Replaces the built-in title bar profile placeholder with custom WPF content.
    /// </summary>
    /// <param name="contentFactory">A factory that creates the profile content when the shell is created.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// Call <see cref="IFlourishTitlebarBuilder.SetProfile(NameOrder)" /> to display the profile trigger.
    /// </remarks>
    IFlourishCustomHandlerBuilder SetProfileContent(
        Func<IServiceProvider, FrameworkElement> contentFactory
    );

    /// <summary>
    /// Replaces the built-in title bar profile placeholder with custom WPF content.
    /// </summary>
    /// <param name="contentFactory">A factory that creates the profile content when the shell is created.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// Call <see cref="IFlourishTitlebarBuilder.SetProfile(NameOrder)" /> to display the profile trigger.
    /// </remarks>
    IFlourishCustomHandlerBuilder SetProfileContent(Func<FrameworkElement> contentFactory);

    /// <summary>
    /// Replaces the built-in title bar profile placeholder with an existing WPF element.
    /// </summary>
    /// <param name="content">The profile content displayed in the title bar.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// Call <see cref="IFlourishTitlebarBuilder.SetProfile(NameOrder)" /> to display the profile trigger.
    /// </remarks>
    IFlourishCustomHandlerBuilder SetProfileContent(FrameworkElement content);

    /// <summary>
    /// Adds a compact command button to the end of the title bar.
    /// </summary>
    /// <param name="displayName">The action text used for the tooltip and fallback label.</param>
    /// <param name="iconGlyph">The icon glyph displayed for the action.</param>
    /// <param name="commandKey">The optional command key sent to <see cref="ICommandParser" /> when the action is clicked.</param>
    /// <param name="order">The display order among title bar end-region content. Lower values are displayed first.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishCustomHandlerBuilder AddTitlebarAction(
        string displayName,
        string iconGlyph,
        string? commandKey,
        int order = 0
    );

    /// <summary>
    /// Adds a compact callback button to the end of the title bar.
    /// </summary>
    /// <param name="displayName">The action text used for the tooltip and fallback label.</param>
    /// <param name="iconGlyph">The icon glyph displayed for the action.</param>
    /// <param name="action">The callback invoked when the action is clicked.</param>
    /// <param name="order">The display order among title bar end-region content. Lower values are displayed first.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishCustomHandlerBuilder AddTitlebarActionHandler(
        string displayName,
        string iconGlyph,
        Action<IServiceProvider> action,
        int order = 0
    );

    /// <summary>
    /// Adds custom content to the end of the shell footer.
    /// </summary>
    /// <param name="contentFactory">A factory that creates the footer content when the shell is created.</param>
    /// <param name="order">The display order in the footer region. Lower values are displayed first.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishCustomHandlerBuilder AddFooterContent(
        Func<IServiceProvider, FrameworkElement> contentFactory,
        int order = 0
    );

    /// <summary>
    /// Adds custom content to the selected shell footer region.
    /// </summary>
    /// <param name="region">The footer region. Must be <see cref="FlourishRegion.FooterStart" /> or <see cref="FlourishRegion.FooterEnd" />.</param>
    /// <param name="contentFactory">A factory that creates the footer content when the shell is created.</param>
    /// <param name="order">The display order in the footer region. Lower values are displayed first.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishCustomHandlerBuilder AddFooterContent(
        FlourishRegion region,
        Func<IServiceProvider, FrameworkElement> contentFactory,
        int order = 0
    );

    /// <summary>
    /// Adds custom content to the end of the shell footer.
    /// </summary>
    /// <param name="contentFactory">A factory that creates the footer content when the shell is created.</param>
    /// <param name="order">The display order in the footer region. Lower values are displayed first.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishCustomHandlerBuilder AddFooterContent(
        Func<FrameworkElement> contentFactory,
        int order = 0
    );

    /// <summary>
    /// Adds custom content to the selected shell footer region.
    /// </summary>
    /// <param name="region">The footer region. Must be <see cref="FlourishRegion.FooterStart" /> or <see cref="FlourishRegion.FooterEnd" />.</param>
    /// <param name="contentFactory">A factory that creates the footer content when the shell is created.</param>
    /// <param name="order">The display order in the footer region. Lower values are displayed first.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishCustomHandlerBuilder AddFooterContent(
        FlourishRegion region,
        Func<FrameworkElement> contentFactory,
        int order = 0
    );

    /// <summary>
    /// Adds an existing WPF element to the end of the shell footer.
    /// </summary>
    /// <param name="content">The footer content displayed in the shell footer.</param>
    /// <param name="order">The display order in the footer region. Lower values are displayed first.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishCustomHandlerBuilder AddFooterContent(FrameworkElement content, int order = 0);

    /// <summary>
    /// Adds an existing WPF element to the selected shell footer region.
    /// </summary>
    /// <param name="region">The footer region. Must be <see cref="FlourishRegion.FooterStart" /> or <see cref="FlourishRegion.FooterEnd" />.</param>
    /// <param name="content">The footer content displayed in the shell footer.</param>
    /// <param name="order">The display order in the footer region. Lower values are displayed first.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishCustomHandlerBuilder AddFooterContent(
        FlourishRegion region,
        FrameworkElement content,
        int order = 0
    );

    /// <summary>
    /// Adds a command button to the end of the shell footer.
    /// </summary>
    /// <param name="displayText">The command display text.</param>
    /// <param name="iconGlyph">The icon glyph displayed before the text.</param>
    /// <param name="commandKey">The optional command key sent to <see cref="ICommandParser" /> when clicked.</param>
    /// <param name="order">The display order in the footer region. Lower values are displayed first.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishCustomHandlerBuilder AddFooterCommand(
        string displayText,
        string iconGlyph,
        string? commandKey,
        int order = 0
    );

    /// <summary>
    /// Adds a command button to the selected shell footer region.
    /// </summary>
    /// <param name="region">The footer region. Must be <see cref="FlourishRegion.FooterStart" /> or <see cref="FlourishRegion.FooterEnd" />.</param>
    /// <param name="displayText">The command display text.</param>
    /// <param name="iconGlyph">The icon glyph displayed before the text.</param>
    /// <param name="commandKey">The optional command key sent to <see cref="ICommandParser" /> when clicked.</param>
    /// <param name="order">The display order in the footer region. Lower values are displayed first.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishCustomHandlerBuilder AddFooterCommand(
        FlourishRegion region,
        string displayText,
        string iconGlyph,
        string? commandKey,
        int order = 0
    );

    /// <summary>
    /// Adds a callback button to the end of the shell footer.
    /// </summary>
    /// <param name="displayText">The command display text.</param>
    /// <param name="iconGlyph">The icon glyph displayed before the text.</param>
    /// <param name="action">The callback invoked when the command is clicked.</param>
    /// <param name="order">The display order in the footer region. Lower values are displayed first.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishCustomHandlerBuilder AddFooterCommandHandler(
        string displayText,
        string iconGlyph,
        Action<IServiceProvider> action,
        int order = 0
    );

    /// <summary>
    /// Adds a callback button to the selected shell footer region.
    /// </summary>
    /// <param name="region">The footer region. Must be <see cref="FlourishRegion.FooterStart" /> or <see cref="FlourishRegion.FooterEnd" />.</param>
    /// <param name="displayText">The command display text.</param>
    /// <param name="iconGlyph">The icon glyph displayed before the text.</param>
    /// <param name="action">The callback invoked when the command is clicked.</param>
    /// <param name="order">The display order in the footer region. Lower values are displayed first.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishCustomHandlerBuilder AddFooterCommandHandler(
        FlourishRegion region,
        string displayText,
        string iconGlyph,
        Action<IServiceProvider> action,
        int order = 0
    );
}
