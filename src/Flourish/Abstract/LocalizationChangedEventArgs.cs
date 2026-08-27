using System.Linq;

using System;
using System.Collections.Generic;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes a runtime localization change.
/// </summary>
public sealed class LocalizationChangedEventArgs(
    LocalizationChangeKind kind,
    string previousLocale,
    string currentLocale,
    IReadOnlyList<string> affectedLocales,
    CultureRegistration? registration
) : EventArgs
{
    /// <summary>
    /// Gets the reason for the change.
    /// </summary>
    public LocalizationChangeKind Kind { get; } = kind;

    /// <summary>
    /// Gets the selected locale before the change.
    /// </summary>
    public string PreviousLocale { get; } = previousLocale;

    /// <summary>
    /// Gets the selected locale after the change.
    /// </summary>
    public string CurrentLocale { get; } = currentLocale;

    /// <summary>
    /// Gets the locales whose values were affected.
    /// </summary>
    public IReadOnlyList<string> AffectedLocales { get; } =
        Array.AsReadOnly(affectedLocales.ToArray());

    /// <summary>
    /// Gets the affected file registration, when the change originated from a file.
    /// </summary>
    public CultureRegistration? Registration { get; } = registration;
}
