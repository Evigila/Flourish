using System.Text.Json;

namespace ArkheideSystem.Gallery.Flourish.Blazor.Models;

public static class ChangeLogCatalog
{
    public static IReadOnlyList<ReleaseNote> Releases { get; } = Load();

    private static IReadOnlyList<ReleaseNote> Load()
    {
        using var stream = typeof(ChangeLogCatalog).Assembly.GetManifestResourceStream("Gallery.ChangeLog.json")
            ?? throw new InvalidOperationException("The embedded ChangeLog catalog is missing.");
        return Array.AsReadOnly(JsonSerializer.Deserialize<ReleaseNote[]>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException("The ChangeLog catalog is empty."));
    }
}

public sealed record ReleaseNote(string Version, IReadOnlyList<string> ChangeKeys);
