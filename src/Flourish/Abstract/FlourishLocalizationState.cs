using System.Collections.Generic;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Represents the current Flourish localization state.</summary>
public sealed record FlourishLocalizationState(
    string Locale,
    IReadOnlyList<string> AvailableLocales
);
