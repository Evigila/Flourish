using System;
using Xunit;
using ArkheideSystem.Flourish.Test.Infrastructure;

using System.Reflection;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Views.Windows;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class DynamicShellIconRoleTests
{
    [Fact]
    public void RegionElementFactory_UsesIconRoleOnlyForGlyphContent()
    {
        StaTest.Run(() =>
        {
            var method = typeof(ShellRegionElementFactory).GetMethod(
                "CreateIconOrText",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            Assert.NotNull(method);

            var icon = Assert.IsType<TextBlock>(
                method.Invoke(null, ["\uE8A5", "Fallback", "FlourishFontSizeStandardIcon"])
            );
            var fallback = Assert.IsType<TextBlock>(
                method.Invoke(null, [string.Empty, "Fallback", "FlourishFontSizeStandardIcon"])
            );

            Assert.Equal(TextRole.Icon, icon.Role);
            Assert.Equal(TextRole.Body, fallback.Role);
        });
    }

    [Fact]
    public void DynamicShellIconBinders_AssignTheIconRole()
    {
        StaTest.Run(() =>
        {
            var shellIcon = new TextBlock();
            InvokeIconBinder(
                typeof(ShellWindow),
                shellIcon,
                "FlourishFontSizeLargeIcon"
            );

            var statusIcon = new TextBlock();
            InvokeIconBinder(
                typeof(ShellStatusSurfaceController),
                statusIcon,
                "FlourishIconFontSizeSystemStatusView"
            );

            Assert.Equal(TextRole.Icon, shellIcon.Role);
            Assert.Equal(TextRole.Icon, statusIcon.Role);
        });
    }

    private static void InvokeIconBinder(
        Type ownerType,
        TextBlock textBlock,
        string resourceKey
    )
    {
        var method = ownerType.GetMethod(
            "BindIconTypography",
            BindingFlags.NonPublic | BindingFlags.Static,
            null,
            [typeof(TextBlock), typeof(string)],
            null
        );
        Assert.NotNull(method);
        method.Invoke(null, [textBlock, resourceKey]);
    }
}
