using System.Windows;

namespace AckSS.Flourish.Abstract;

/// <summary>
/// Configures custom WPF content displayed in Flourish shell regions.
/// </summary>
/// <example>
/// <code><![CDATA[
/// shell.UseRegions((_, regions) =>
/// {
///     regions.Add(
///         FlourishRegion.TitlebarEnd,
///         services => new Button { Content = "Account" });
/// });
/// ]]></code>
/// </example>
public interface IFlourishRegionBuilder
{
    /// <summary>
    /// Adds custom content to a shell region.
    /// </summary>
    /// <param name="region">The shell region that receives the content.</param>
    /// <param name="contentFactory">A factory that creates the WPF element when the shell is created.</param>
    /// <param name="order">The display order inside the region. Lower values are displayed first.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishRegionBuilder Add(
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
    IFlourishRegionBuilder Add(
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
    IFlourishRegionBuilder Add(FlourishRegion region, FrameworkElement content, int order = 0);
}
