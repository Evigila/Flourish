namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Represents the outcome of a profile authentication attempt.
/// </summary>
/// <param name="Succeeded">A value indicating whether authentication succeeded.</param>
/// <param name="ErrorMessage">An optional user-facing failure message.</param>
public sealed record ProfileAuthenticationResult(bool Succeeded, string? ErrorMessage = null)
{
    /// <summary>
    /// Creates a successful authentication result.
    /// </summary>
    public static ProfileAuthenticationResult Success() => new(true);

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
