namespace ArkheideSystem.Flourish.Abstract.Essential;

/// <summary>
/// Resolves localized text and manages runtime locale sources.
/// </summary>
public interface IFlourishLocalization
{
    /// <summary>
    /// Gets the currently selected locale in canonical form.
    /// </summary>
    string CurrentLocale { get; }

    /// <summary>
    /// Gets the built-in and currently registered locales.
    /// </summary>
    IReadOnlyList<string> AvailableLocales { get; }

    /// <summary>
    /// Raised when the locale or one of its registered sources changes.
    /// </summary>
    /// <remarks>The event is raised synchronously on the thread performing the change.</remarks>
    event EventHandler<FlourishLocalizationChangedEventArgs>? Changed;

    /// <summary>
    /// Resolves a localized value, falling back to English and then the key itself.
    /// </summary>
    string Get(string key);

    /// <summary>
    /// Resolves and formats a localized value using the current culture.
    /// </summary>
    string Format(string key, params object?[] arguments);

    /// <summary>
    /// Changes the currently selected locale.
    /// </summary>
    void SetLocale(string locale);

    /// <summary>
    /// Loads and registers a <c>FlourishCulture.Json</c> catalog.
    /// </summary>
    FlourishCultureRegistration RegisterFile(string path);

    /// <summary>
    /// Reloads a previously registered culture file from disk.
    /// </summary>
    void ReloadFile(FlourishCultureRegistration registration);

    /// <summary>
    /// Removes a previously registered culture file.
    /// </summary>
    bool Unregister(FlourishCultureRegistration registration);
}
