using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace ArkheideSystem.Gallery.Flourish.Blazor.Models;

/// <summary>Keeps executable examples and their displayed source together.</summary>
public static class SampleCatalog
{
    private static readonly Lazy<IReadOnlyDictionary<string, ComponentSample>> Samples = new(Load);

    public static ComponentSample For(ComponentEntry entry) => Samples.Value.TryGetValue(entry.Name, out var sample)
        ? sample : throw new InvalidOperationException($"No executable example is registered for {entry.Name}.");

    public static ComponentEntry? Find(string key) => ComponentCatalog.Groups.SelectMany(group => group.Entries)
        .FirstOrDefault(entry => string.Equals(For(entry).Key, key, StringComparison.OrdinalIgnoreCase));

    private static IReadOnlyDictionary<string, ComponentSample> Load()
    {
        var assembly = typeof(SampleCatalog).Assembly;
        var result = new Dictionary<string, ComponentSample>(StringComparer.Ordinal);
        foreach (var type in assembly.GetTypes())
        {
            if (type.GetCustomAttribute<SampleFor>() is not { } registration) continue;
            if (!typeof(IComponent).IsAssignableFrom(type))
                throw new InvalidOperationException($"{type.Name} is not a Razor component.");
            using var stream = assembly.GetManifestResourceStream(type.FullName + ".razor")
                ?? throw new InvalidOperationException($"The source for {type.Name} was not embedded.");
            using var reader = new StreamReader(stream);
            // These two lines register a Gallery example; the remaining component is standalone.
            var source = Regex.Replace(reader.ReadToEnd(), @"^@(namespace|attribute)[^\r\n]*\r?\n", "", RegexOptions.Multiline).Trim();
            if (!result.TryAdd(registration.ComponentName, new(type, type.Name.ToLowerInvariant(), source)))
                throw new InvalidOperationException($"More than one sample is registered for {registration.ComponentName}.");
        }
        return result;
    }
}

public sealed record ComponentSample(Type ComponentType, string Key, string Source)
{
    public IDictionary<string, object> PreviewParameters { get; } = new Dictionary<string, object> { ["Preview"] = true };
}
