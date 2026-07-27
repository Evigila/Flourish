using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract.Builder;

/// <summary>
/// Configures the page hosted by the profile flyout enabled through
/// <see cref="IFlourishTitlebarBuilder.UseProfile(bool, NameOrder)" />.
/// </summary>
public interface IFlourishProfileBuilder
{
    /// <summary>
    /// Sets the page hosted inside the profile flyout.
    /// </summary>
    /// <typeparam name="TPage">The WPF page type resolved through dependency injection.</typeparam>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishProfileBuilder InitProfilePage<TPage>()
        where TPage : Page;
}
