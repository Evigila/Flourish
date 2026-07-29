using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract.Runtime;

/// <summary>Provides typed conveniences for profile flyout configuration.</summary>
public static class ProfileFlyoutServiceExtensions
{
    /// <summary>Changes the page displayed in the profile flyout.</summary>
    public static void SetContentPage<TPage>(this IProfileFlyoutService service)
        where TPage : Page
    {
        ArgumentNullException.ThrowIfNull(service);
        service.SetContentPage(typeof(TPage));
    }
}
