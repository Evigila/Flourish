using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract.Runtime;

/// <summary>
/// Controls the Flourish profile flyout independently from profile authentication.
/// </summary>
public interface IProfileFlyoutService
{
    /// <summary>
    /// Gets an immutable snapshot of the profile flyout state.
    /// </summary>
    FlourishProfileFlyoutState Current { get; }

    /// <summary>
    /// Occurs synchronously after the profile flyout state changes.
    /// </summary>
    event EventHandler<FlourishProfileFlyoutChangedEventArgs>? Changed;

    /// <summary>
    /// Enables or disables the profile feature, closing its flyout when disabled.
    /// </summary>
    /// <param name="enabled"><see langword="true"/> to enable the profile feature; otherwise, <see langword="false"/>.</param>
    void SetEnabled(bool enabled);

    /// <summary>
    /// Opens the profile flyout.
    /// </summary>
    /// <exception cref="InvalidOperationException">The profile feature is disabled.</exception>
    void Show();

    /// <summary>
    /// Closes the profile flyout.
    /// </summary>
    void Hide();

    /// <summary>
    /// Opens or closes the profile flyout according to its current state.
    /// </summary>
    /// <exception cref="InvalidOperationException">The profile feature is disabled.</exception>
    void Toggle();

    /// <summary>
    /// Changes the page displayed in the profile flyout.
    /// </summary>
    /// <param name="pageType">A closed, concrete type derived from <see cref="Page"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="pageType"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="pageType"/> is not a closed, concrete <see cref="Page"/> type.</exception>
    void SetContentPage(Type pageType);
}
