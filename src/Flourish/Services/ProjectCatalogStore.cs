using System.IO;
using System.Text.Json;
using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Services;

internal interface IProjectCatalogStore
{
    ProjectCatalog Load();

    void Save(ProjectCatalog catalog);
}

internal sealed class ProjectCatalogStore : IProjectCatalogStore
{
    private const string CatalogFileName = "projects.json";
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };
    private readonly string filePath;

    public ProjectCatalogStore(IAppSettingsStore appSettingsStore)
    {
        ArgumentNullException.ThrowIfNull(appSettingsStore);
        var appSettingsPath = Path.GetFullPath(appSettingsStore.FilePath);
        var directory = Path.GetDirectoryName(appSettingsPath)
            ?? throw new InvalidOperationException("appsettings.json has no parent directory.");
        filePath = Path.Combine(directory, CatalogFileName);
    }

    internal string FilePath => filePath;

    public ProjectCatalog Load()
    {
        if (!File.Exists(filePath))
        {
            return ProjectCatalog.Empty;
        }

        try
        {
            using var stream = new FileStream(
                filePath,
                new FileStreamOptions
                {
                    Mode = FileMode.Open,
                    Access = FileAccess.Read,
                    Share = FileShare.ReadWrite | FileShare.Delete,
                    Options = FileOptions.SequentialScan,
                }
            );
            var document = JsonSerializer.Deserialize<ProjectCatalogDocument>(
                stream,
                SerializerOptions
            );
            if (document is null)
            {
                throw new InvalidDataException($"{CatalogFileName} is empty.");
            }

            return new ProjectCatalog(document.Projects ?? [], document.ActiveProjectId);
        }
        catch (JsonException error)
        {
            throw new InvalidDataException(
                $"{CatalogFileName} contains invalid JSON and could not be loaded.",
                error
            );
        }
    }

    public void Save(ProjectCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var directory = Path.GetDirectoryName(filePath)
            ?? throw new InvalidOperationException($"{CatalogFileName} has no parent directory.");
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(
            directory,
            $".{CatalogFileName}.{Guid.NewGuid():N}.tmp"
        );
        var document = new ProjectCatalogDocument(
            [.. catalog.Projects],
            catalog.ActiveProjectId
        );

        try
        {
            using (
                var stream = new FileStream(
                    temporaryPath,
                    new FileStreamOptions
                    {
                        Mode = FileMode.CreateNew,
                        Access = FileAccess.Write,
                        Share = FileShare.None,
                        Options = FileOptions.SequentialScan,
                    }
                )
            )
            {
                JsonSerializer.Serialize(stream, document, SerializerOptions);
                stream.Flush(flushToDisk: true);
            }

            File.Move(temporaryPath, filePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private sealed record ProjectCatalogDocument(
        FlourishProject[] Projects,
        string? ActiveProjectId
    );
}

internal sealed record ProjectCatalog(
    IReadOnlyList<FlourishProject> Projects,
    string? ActiveProjectId
)
{
    internal static ProjectCatalog Empty { get; } = new([], null);
}
