namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Represents the outcome of a profile authentication attempt.
/// </summary>
public sealed class ProfileAuthenticationResult
{
    private ProfileAuthenticationResult(bool succeeded, string? errorMessage)
    {
        Succeeded = succeeded;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Gets a value indicating whether authentication succeeded.
    /// </summary>
    public bool Succeeded { get; }

    /// <summary>
    /// Gets the optional user-facing failure message.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Creates a successful authentication result.
    /// </summary>
    public static ProfileAuthenticationResult Success() => new(true, null);

    /// <summary>
    /// Creates a failed authentication result.
    /// </summary>
    /// <param name="errorMessage">The user-facing failure message.</param>
    public static ProfileAuthenticationResult Failure(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException("Error message cannot be empty.", nameof(errorMessage));
        }

        return new(false, errorMessage);
    }
}
