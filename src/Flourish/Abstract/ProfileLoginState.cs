namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes the current Flourish profile login state.
/// </summary>
public enum ProfileLoginState
{
    /// <summary>
    /// No user is currently signed in.
    /// </summary>
    SignedOut,

    /// <summary>
    /// A user is signed in for the current application session.
    /// </summary>
    SignedIn,

    /// <summary>
    /// A user is signed in and the login will be restored on the next startup.
    /// </summary>
    SignedInRemembered,
}
