using System.Threading;
using System.Threading.Tasks;

namespace ArkheideSystem.Flourish.Profile;

/// <summary>Persists and removes protected profile credentials.</summary>
/// <remarks>
/// Platform packages own encryption and storage. Core only defines the credential lifecycle.
/// </remarks>
internal interface IProfileCredentialStore
{
    /// <summary>Reads stored credentials, or null when none are available.</summary>
    Task<StoredProfileCredentials?> ReadAsync(
        CancellationToken cancellationToken = default
    );

    /// <summary>Protects and persists the supplied credentials.</summary>
    Task SaveAsync(
        StoredProfileCredentials credentials,
        CancellationToken cancellationToken = default
    );

    /// <summary>Removes any persisted credentials.</summary>
    Task ClearAsync(CancellationToken cancellationToken = default);
}
