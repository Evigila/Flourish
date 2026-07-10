namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides data when the active profile or login state changes.
/// </summary>
public sealed class ProfileChangedEventArgs(ProfileUser profile, ProfileLoginState loginState)
    : EventArgs
{
    /// <summary>
    /// Gets the active profile display information.
    /// </summary>
    public ProfileUser Profile { get; } = profile;

    /// <summary>
    /// Gets the current login state.
    /// </summary>
    public ProfileLoginState LoginState { get; } = loginState;
}
