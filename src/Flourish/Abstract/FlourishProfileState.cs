namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Represents the current profile, login state, and name presentation settings.
/// </summary>
/// <param name="Profile">The profile currently displayed by the shell.</param>
/// <param name="LoginState">The current authentication state.</param>
public sealed record FlourishProfileState(
    ProfileUser Profile,
    ProfileLoginState LoginState
)
{
    /// <summary>Gets the application-wide profile name order.</summary>
    public NameOrder NameOrder => Profile.NameOrder;
}
