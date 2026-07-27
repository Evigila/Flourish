using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class GalleryNavigationTreeTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string ProgramPath = Path.Combine(
        RepositoryRoot,
        "src",
        "Gallery",
        "Program.cs"
    );

    [Fact]
    public void About_IsARegisteredFixedPageInsteadOfScrollableGroupContent()
    {
        var source = File.ReadAllText(ProgramPath);

        Assert.Contains(
            "services.AddNavigable<AboutPage>(\"About\", \"\\uE946\")",
            source,
            StringComparison.Ordinal
        );
        Assert.Contains(
            ".InitFixedNavigableViewItem<AboutPage>()",
            source,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain(
            "group.InitNavigableViewItem<AboutPage>()",
            source,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void NavigationTree_UsesSeparateConfigurationAndShellApiPages()
    {
        var source = File.ReadAllText(ProgramPath);
        Assert.Contains("\"Configuration\",", source, StringComparison.Ordinal);
        Assert.Contains("\"Shell\",", source, StringComparison.Ordinal);
        Assert.Contains("\"Actions\",", source, StringComparison.Ordinal);
        Assert.DoesNotContain("\"Surfaces\"", source, StringComparison.Ordinal);
        Assert.False(
            Regex.IsMatch(source, @"\.InitGroup\(\s*""Commands"""),
            "The interactive command nodes belong to Actions, not a second Commands group."
        );

        string[] configurationPages =
        [
            "ConfigurationPage",
        ];
        string[] shellPages =
        [
            "AppearancePage",
            "TitleBarRuntimePage",
            "NavigationRuntimePage",
            "ProfileConfigurationPage",
            "WindowRuntimePage",
            "StatusBarConfigurationPage",
            "DynamicToolbarConfigurationPage",
            "ToolTipsConfigurationPage",
            "MotionConfigurationPage",
            "CustomHandlerConfigurationPage",
        ];

        foreach (var page in configurationPages.Concat(shellPages))
        {
            Assert.Contains(
                $"services.AddNavigable<{page}>",
                source,
                StringComparison.Ordinal
            );
            Assert.Contains(
                $"group.InitNavigableViewItem<{page}>()",
                source,
                StringComparison.Ordinal
            );
        }

        string[] removedPages = ["ShellConfigurationPage", "ServicesConfigurationPage"];
        foreach (var page in removedPages)
        {
            Assert.DoesNotContain(
                $"services.AddNavigable<{page}>",
                source,
                StringComparison.Ordinal
            );
            Assert.DoesNotContain(
                $"group.InitNavigableViewItem<{page}>()",
                source,
                StringComparison.Ordinal
            );
        }

        Assert.DoesNotContain(
            "group.InitNavigableViewItem<ToolbarStatusPage>()",
            source,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain(
            "services.AddNavigable<ToolbarStatusPage>",
            source,
            StringComparison.Ordinal
        );
    }

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
                && Directory.Exists(Path.Combine(directory.FullName, "src", "Gallery"))
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
