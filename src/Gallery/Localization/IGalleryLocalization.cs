using System.Windows;

namespace ArkheideSystem.Gallery.Localization;

/// <summary>
/// Resolves and applies Gallery-owned interface text. Flourish localization remains
/// responsible only for strings rendered by the framework itself.
/// </summary>
public interface IGalleryLocalization
{
    string CurrentLocale { get; }

    event EventHandler? Changed;

    string Get(string sourceText);

    string Format(string sourceFormat, params object?[] arguments);

    void Apply(DependencyObject root);
}
