using System.Linq;
using System.Threading;

using System;
using System.Collections.Generic;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Identifies one runtime culture-file registration.
/// </summary>
public sealed class FlourishCultureRegistration : IRegistration
{
    private IReadOnlyList<string> locales;
    private Action<FlourishCultureRegistration>? reload;
    private Func<FlourishCultureRegistration, bool>? unregister;

    internal FlourishCultureRegistration(
        Guid id,
        IReadOnlyList<string> locales,
        string filePath,
        Action<FlourishCultureRegistration> reload,
        Func<FlourishCultureRegistration, bool> unregister
    )
    {
        Id = id;
        this.locales = Array.AsReadOnly(locales.ToArray());
        FilePath = filePath;
        this.reload = reload;
        this.unregister = unregister;
    }

    /// <summary>
    /// Gets the stable registration identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the registered locales in canonical form.
    /// </summary>
    public IReadOnlyList<string> Locales => locales;

    /// <summary>
    /// Gets the absolute culture-file path.
    /// </summary>
    public string FilePath { get; }

    /// <summary>Gets whether this culture file remains registered.</summary>
    public bool IsRegistered => Volatile.Read(ref unregister) is not null;

    /// <summary>Reloads the registered culture file from disk.</summary>
    public void Reload()
    {
        var callback = Volatile.Read(ref reload);
        if (callback is null)
        {
            throw new InvalidOperationException("The culture-file registration is not active.");
        }

        callback(this);
    }

    /// <summary>Unregisters this culture file. Repeated calls have no effect.</summary>
    public void Dispose()
    {
        var callback = Interlocked.Exchange(ref unregister, null);
        Interlocked.Exchange(ref reload, null);
        callback?.Invoke(this);
    }

    internal void MarkUnregistered()
    {
        Interlocked.Exchange(ref unregister, null);
        Interlocked.Exchange(ref reload, null);
    }

    internal void UpdateLocales(IReadOnlyList<string> values) =>
        locales = Array.AsReadOnly(values.ToArray());
}
