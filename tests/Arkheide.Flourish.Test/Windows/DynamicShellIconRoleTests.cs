using System.Reflection;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Internal.Composition;
using ArkheideSystem.Flourish.Views.Windows;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class DynamicShellIconRoleTests
{
    [Fact]
    public void RegionElementFactory_UsesIconRoleOnlyForGlyphContent()
    {
        StaTest.Run(() =>
        {
            var method = typeof(FlourishRegionElementFactory).GetMethod(
                "CreateIconOrText",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            Assert.NotNull(method);

            var icon = Assert.IsType<FlourishTextBlock>(
                method.Invoke(null, ["\uE8A5", "Fallback", "FlourishFontSizeIcon"])
            );
            var fallback = Assert.IsType<FlourishTextBlock>(
                method.Invoke(null, [string.Empty, "Fallback", "FlourishFontSizeIcon"])
            );

            Assert.Equal(FlourishTextRole.Icon, icon.Role);
            Assert.Equal(FlourishTextRole.Body, fallback.Role);
        });
    }

    [Fact]
    public void DynamicShellIconBinders_AssignTheIconRole()
    {
        StaTest.Run(() =>
        {
            var shellIcon = new FlourishTextBlock();
            InvokeIconBinder(
                typeof(FlourishShellWindow),
                shellIcon,
                "FlourishFontSizeIcon"
            );

            var statusIcon = new FlourishTextBlock();
            InvokeIconBinder(
                typeof(ShellStatusSurfaceController),
                statusIcon,
                "FlourishIconFontSizeSystemStatusView"
            );

            Assert.Equal(FlourishTextRole.Icon, shellIcon.Role);
            Assert.Equal(FlourishTextRole.Icon, statusIcon.Role);
        });
    }

    private static void InvokeIconBinder(
        Type ownerType,
        FlourishTextBlock textBlock,
        string resourceKey
    )
    {
        var method = ownerType.GetMethod(
            "BindIconTypography",
            BindingFlags.NonPublic | BindingFlags.Static,
            null,
            [typeof(FlourishTextBlock), typeof(string)],
            null
        );
        Assert.NotNull(method);
        method.Invoke(null, [textBlock, resourceKey]);
    }
}
