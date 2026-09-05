using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Projects;

internal interface IProjectCatalogStore
{
    ProjectCatalog Load();

    void Save(ProjectCatalog catalog);
}

internal sealed class ProjectCatalogStore : IProjectCatalogStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    private readonly string? filePath;
    private readonly Lock gate = new();
    private ProjectCatalog? lastPersistedCatalog;

    public ProjectCatalogStore(string? filePath)
    {
        this.filePath = string.IsNullOrWhiteSpace(filePath)
            ? null
            : Path.GetFullPath(filePath);
    }

    internal string? FilePath => filePath;

    private string ManagedFileName => Path.GetFileName(filePath!);

    public ProjectCatalog Load()
    {
        string? path = filePath;
        if (path is null)
        {
            return ProjectCatalog.Empty;
        }

        if (!File.Exists(path))
        {
            lock (gate)
            {
                lastPersistedCatalog = ProjectCatalog.Empty;
            }

            return ProjectCatalog.Empty;
        }

        try
        {
            using var stream = new FileStream(
                path,
                new FileStreamOptions
                {
                    Mode = FileMode.Open,
                    Access = FileAccess.Read,
                    Share = FileShare.ReadWrite | FileShare.Delete,
                    Options = FileOptions.SequentialScan,
                }
            );
            ProjectCatalogDocument? document =
                JsonSerializer.Deserialize<ProjectCatalogDocument>(stream, SerializerOptions);
            if (document is null)
            {
                throw new InvalidDataException($"{ManagedFileName} is empty.");
            }

            var catalog = new ProjectCatalog(document.Projects ?? [], document.ActiveProjectId);
            lock (gate)
            {
                lastPersistedCatalog = Snapshot(catalog);
            }

            return catalog;
        }
        catch (JsonException error)
        {
            throw new InvalidDataException(
                $"{ManagedFileName} contains invalid JSON and could not be loaded.",
                error
            );
        }
    }

    public void Save(ProjectCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        if (filePath is null)
        {
            return;
        }

        lock (gate)
        {
            if (CatalogEquals(lastPersistedCatalog, catalog))
            {
                return;
            }

            SaveCore(catalog);
            lastPersistedCatalog = Snapshot(catalog);
        }
    }

    private void SaveCore(ProjectCatalog catalog)
    {
        string path = filePath!;
        string directory =
            Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException($"{ManagedFileName} has no parent directory.");
        Directory.CreateDirectory(directory);
        string temporaryPath = Path.Combine(
            directory,
            $".{ManagedFileName}.{Guid.NewGuid():N}.tmp"
        );
        var document = new ProjectCatalogDocument([.. catalog.Projects], catalog.ActiveProjectId);

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
            }

            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static ProjectCatalog Snapshot(ProjectCatalog catalog)
    {
        return new ProjectCatalog([.. catalog.Projects], catalog.ActiveProjectId);
    }

    private static bool CatalogEquals(ProjectCatalog? left, ProjectCatalog right)
    {
        if (
            left is null
            || !StringComparer.Ordinal.Equals(left.ActiveProjectId, right.ActiveProjectId)
            || left.Projects.Count != right.Projects.Count
        )
        {
            return false;
        }

        for (int index = 0; index < left.Projects.Count; index++)
        {
            if (left.Projects[index] != right.Projects[index])
            {
                return false;
            }
        }

        return true;
    }

    private sealed record ProjectCatalogDocument(
        ProjectDescriptor[] Projects,
        string? ActiveProjectId
    );
}

internal sealed record ProjectCatalog(
    IReadOnlyList<ProjectDescriptor> Projects,
    string? ActiveProjectId
)
{
    internal static ProjectCatalog Empty { get; } = new([], null);
}
