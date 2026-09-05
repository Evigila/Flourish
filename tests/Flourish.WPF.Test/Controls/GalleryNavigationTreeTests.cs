using System;
using System.Linq;
using Xunit;
using ArkheideSystem.Flourish.WPF.Test.Infrastructure;

using System.IO;
using System.Text.RegularExpressions;

namespace ArkheideSystem.Flourish.WPF.Test.Controls;

public sealed class GalleryNavigationTreeTests
{
    private static readonly string RepositoryRoot = TestPaths.RepositoryRoot;
    private static readonly string ProgramPath = Path.Combine(
        RepositoryRoot,
        "src",
        "Gallery.WPF",
        "Program.cs"
    );

    [Fact]
    public void About_IsARegisteredFixedPageInsteadOfScrollableGroupContent()
    {
        var source = File.ReadAllText(ProgramPath);

        Assert.Contains("services.AddNavigable<AboutPage>(", source, StringComparison.Ordinal);
        Assert.Contains("Key.Application_About_4EFCA0D1", source, StringComparison.Ordinal);
        Assert.Contains(
            ".AddFixedNavigableViewItem<AboutPage>()",
            source,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain(
            "group.AddNavigableViewItem<AboutPage>()",
            source,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void NavigationTree_UsesSeparateConfigurationAndShellApiPages()
    {
        var source = File.ReadAllText(ProgramPath);
        Assert.Contains(
            "Key.Application_Configuration_B332C349",
            source,
            StringComparison.Ordinal
        );
        Assert.Contains("Key.Shell_Shell_A7332854", source, StringComparison.Ordinal);
        Assert.Contains("Key.Application_Actions_FF8059DC", source, StringComparison.Ordinal);
        Assert.DoesNotContain("\"Surfaces\"", source, StringComparison.Ordinal);
        Assert.False(
            Regex.IsMatch(source, @"\.AddGroup\(\s*LangKey\.Application_Commands"),
            "The interactive command nodes belong to Actions, not a second Commands group."
        );

        string[] configurationPages = ["ConfigurationPage"];
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
            Assert.Contains($"services.AddNavigable<{page}>", source, StringComparison.Ordinal);
            Assert.Contains(
                $"group.AddNavigableViewItem<{page}>()",
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
                $"group.AddNavigableViewItem<{page}>()",
                source,
                StringComparison.Ordinal
            );
        }

        Assert.DoesNotContain(
            "group.AddNavigableViewItem<ToolbarStatusPage>()",
            source,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain(
            "services.AddNavigable<ToolbarStatusPage>",
            source,
            StringComparison.Ordinal
        );
    }
}
