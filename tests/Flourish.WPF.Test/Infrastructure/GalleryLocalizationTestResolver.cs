using System;
using System.Collections.Generic;
using System.Linq;

using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ArkheideSystem.Flourish.WPF.Test.Infrastructure;

/// <summary>Resolves Gallery Localize markup to English for structure-only XAML assertions.</summary>
internal static partial class GalleryLocalizationTestResolver
{
    private static readonly Lazy<IReadOnlyDictionary<string, string>> English = new(
        LoadEnglishCatalog
    );

    internal static XDocument LoadXaml(string path, LoadOptions options = LoadOptions.None)
    {
        var document = XDocument.Load(path, options);
        foreach (var attribute in document.Root!.DescendantsAndSelf().Attributes().ToArray())
        {
            var match = LocalizePattern().Match(attribute.Value);
            if (match.Success && English.Value.TryGetValue(match.Groups["key"].Value, out var value))
            {
                attribute.Value = value;
            }
        }

        return document;
    }

    private static IReadOnlyDictionary<string, string> LoadEnglishCatalog()
    {
        var path = Path.Combine(
            TestPaths.RepositoryRoot,
            "src",
            "Gallery.WPF",
            "Localization",
            "Culture.json"
        );
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document
            .RootElement.EnumerateObject()
            .ToDictionary(
                entry => entry.Name,
                entry => entry.Value.GetProperty("en-US").GetString()!,
                StringComparer.Ordinal
            );
    }

    [GeneratedRegex(
        @"^\{culture:Localize\s+Key=(?<key>[A-Za-z_][A-Za-z0-9_]*)(?:,.*)?\}$",
        RegexOptions.CultureInvariant
    )]
    private static partial Regex LocalizePattern();
}
