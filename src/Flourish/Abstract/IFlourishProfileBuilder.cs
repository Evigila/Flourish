using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Configures the profile shown when the Shell profile feature is enabled.
/// </summary>
public interface IFlourishProfileBuilder
{
    /// <summary>
    /// Sets the profile displayed before a user signs in.
    /// </summary>
    /// <param name="imagePath">An optional local or pack URI image path.</param>
    /// <param name="userName">The non-empty default user name.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishProfileBuilder SetDefaultProfile(
        string? imagePath = null,
        string userName = "User"
    );

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
