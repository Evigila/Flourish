using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Configures the page hosted by the profile flyout enabled through
/// <see cref="IFlourishTitlebarBuilder.SetProfile(NameOrder)" />.
/// </summary>
public interface IFlourishProfileBuilder
{
    /// <summary>
    /// Sets the page hosted inside the profile flyout.
    /// </summary>
    /// <typeparam name="TPage">The WPF page type resolved through dependency injection.</typeparam>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishProfileBuilder SetProfilePage<TPage>()
        where TPage : Page;

    /// <summary>
    /// Sets the page hosted inside the profile flyout.
    /// </summary>
    /// <param name="pageType">A type derived from <see cref="Page" />.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishProfileBuilder SetProfilePage(Type pageType);
}
