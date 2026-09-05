using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Identifies one runtime culture-file registration.</summary>
public sealed class CultureRegistration : IRegistration
{
    private IReadOnlyList<string> locales;
    private Action<CultureRegistration>? reload;
    private Func<CultureRegistration, bool>? unregister;

    internal CultureRegistration(
        Guid id,
        IReadOnlyList<string> locales,
        string filePath,
        Action<CultureRegistration> reload,
        Func<CultureRegistration, bool> unregister
    )
    {
        Id = id;
        this.locales = Array.AsReadOnly(locales.ToArray());
        FilePath = filePath;
        this.reload = reload;
        this.unregister = unregister;
    }

    /// <summary>Gets the stable registration identifier.</summary>
    public Guid Id { get; }

    /// <summary>Gets the registered locales in canonical form.</summary>
    public IReadOnlyList<string> Locales => locales;

    /// <summary>Gets the absolute culture-file path.</summary>
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

    internal void UpdateLocales(IReadOnlyList<string> values)
    {
        locales = Array.AsReadOnly(values.ToArray());
    }
}

/// <summary>Describes why localized values changed.</summary>
public enum LocalizationChangeKind
{
    /// <summary>The selected locale changed.</summary>
    LocaleChanged,

    /// <summary>A culture file was registered.</summary>
    FileRegistered,

    /// <summary>A registered culture file was reloaded.</summary>
    FileReloaded,

    /// <summary>A culture-file registration was removed.</summary>
    FileUnregistered,
}

/// <summary>Represents the current localization state.</summary>
public sealed record LocalizationState(
    string Locale,
    IReadOnlyList<string> AvailableLocales
);

/// <summary>Describes a runtime localization change.</summary>
public sealed class LocalizationChangedEventArgs(
    LocalizationChangeKind kind,
    string previousLocale,
    string currentLocale,
    IReadOnlyList<string> affectedLocales,
    CultureRegistration? registration
) : EventArgs
{
    /// <summary>Gets the reason for the change.</summary>
    public LocalizationChangeKind Kind { get; } = kind;

    /// <summary>Gets the selected locale before the change.</summary>
    public string PreviousLocale { get; } = previousLocale;

    /// <summary>Gets the selected locale after the change.</summary>
    public string CurrentLocale { get; } = currentLocale;

    /// <summary>Gets the locales whose values were affected.</summary>
    public IReadOnlyList<string> AffectedLocales { get; } =
        Array.AsReadOnly(affectedLocales.ToArray());

    /// <summary>Gets the affected file registration, when applicable.</summary>
    public CultureRegistration? Registration { get; } = registration;
}

/// <summary>Resolves localized text and manages runtime locale sources.</summary>
public interface ILocalizationService
{
    /// <summary>Gets the current locale and available locale catalog.</summary>
    LocalizationState Current { get; }

    /// <summary>Occurs when the locale or a registered source changes.</summary>
    event EventHandler<LocalizationChangedEventArgs>? Changed;

    /// <summary>Resolves a value, falling back to English and then the key.</summary>
    string Get(string key);

    /// <summary>Resolves and formats a value using the current culture.</summary>
    string Format(string key, params object?[] arguments);

    /// <summary>Changes the selected locale.</summary>
    void SetLocale(string locale);

    /// <summary>Loads and registers a FlourishCulture.Json catalog.</summary>
    CultureRegistration RegisterFile(string path);
}
