using System.IO;

namespace ArkheideSystem.Flourish.Test.Infrastructure;

internal static class TestPaths
{
    private static readonly Lazy<string> RepositoryRootPath = new(FindRepositoryRoot);

    public static string RepositoryRoot => RepositoryRootPath.Value;

    private static string FindRepositoryRoot()
    {
        for (
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            directory is not null;
            directory = directory.Parent
        )
        {
            if (
                File.Exists(Path.Combine(directory.FullName, "Flourish.slnx"))
                && Directory.Exists(Path.Combine(directory.FullName, "src", "Flourish"))
            )
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not locate the Flourish repository above {AppContext.BaseDirectory}."
        );
    }
}
