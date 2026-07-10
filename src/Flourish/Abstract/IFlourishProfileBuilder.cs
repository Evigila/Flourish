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
    /// <remarks>
    /// The combined <paramref name="userName" /> is split using the name order active when this
    /// method is called. Call <see cref="SetNameOrder" /> first when configuring both values.
    /// </remarks>
    IFlourishProfileBuilder SetDefaultProfile(
        string? imagePath = null,
        string userName = "User"
    );

    /// <summary>
    /// Sets the order used to display profile names and initials.
    /// </summary>
    /// <param name="nameOrder">The order applied to the first and last name.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishProfileBuilder SetNameOrder(NameOrder nameOrder);

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
