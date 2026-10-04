using System;
using System.Linq;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Shell.Regions;
using ArkheideSystem.Flourish.Views.Windows;

using System.Windows;

namespace ArkheideSystem.Tests.Flourish.WPF.Internal.Composition;

public sealed class CustomContentBuilderTests
{
    [Fact]
    public void PublicContract_ExposesOnlyCanonicalCustomHandlerMethods()
    {
        var methods = typeof(ICustomContentBuilder).GetMethods();

        Assert.Equal(6, methods.Length);
        Assert.Equal(
            [
                "AddFooterCommand",
                "AddFooterCommandHandler",
                "AddRegionContent",
                "AddTitleBarAction",
                "AddTitleBarActionHandler",
                "SetProfileContent",
            ],
            methods.Select(method => method.Name).Order()
        );

        var add = Assert.Single(methods, method => method.Name == "AddRegionContent");
        Assert.Equal(
            [typeof(ShellRegion), typeof(Func<IServiceProvider, FrameworkElement>), typeof(int)],
            add.GetParameters().Select(parameter => parameter.ParameterType)
        );

        var setProfileContent = Assert.Single(
            methods,
            method => method.Name == "SetProfileContent"
        );
        Assert.Equal(
            typeof(Func<IServiceProvider, FrameworkElement>),
            Assert.Single(setProfileContent.GetParameters()).ParameterType
        );

        Assert.All(
            methods.Where(method =>
                method.Name.StartsWith("AddFooterCommand", StringComparison.Ordinal)
            ),
            method => Assert.Equal(typeof(ShellRegion), method.GetParameters()[0].ParameterType)
        );
    }

    [Fact]
    public void CanonicalMethods_RegisterContentInExplicitRegions()
    {
        var options = new ShellRegionOptions();
        ICustomContentBuilder builder = new CustomContentBuilder(options);

        builder
            .AddRegionContent(ShellRegion.FooterStart, _ => null!, order: 3)
            .SetProfileContent(_ => null!)
            .SetProfileContent(_ => null!)
            .AddFooterCommand(ShellRegion.FooterEnd, "Help", "H", "cmd_app_help", order: 5)
            .AddFooterCommandHandler(
                ShellRegion.FooterStart,
                "Refresh",
                "R",
                _ => { },
                order: 7
            );

        Assert.Collection(
            options.RegionContents,
            content =>
            {
                Assert.Equal(ShellRegion.FooterStart, content.Region);
                Assert.Equal(3, content.Order);
            },
            content =>
            {
                Assert.Equal(ShellRegion.TitleBarProfile, content.Region);
                Assert.Equal(0, content.Order);
            },
            content =>
            {
                Assert.Equal(ShellRegion.FooterEnd, content.Region);
                Assert.Equal(5, content.Order);
            },
            content =>
            {
                Assert.Equal(ShellRegion.FooterStart, content.Region);
                Assert.Equal(7, content.Order);
            }
        );
    }

    [Theory]
    [InlineData(ShellRegion.TitleBarEnd)]
    [InlineData(ShellRegion.ContentFooter)]
    public void FooterHelpers_WithNonFooterRegion_ThrowArgumentOutOfRangeException(
        ShellRegion region
    )
    {
        ICustomContentBuilder builder = new CustomContentBuilder(
            new ShellRegionOptions()
        );

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            builder.AddFooterCommand(region, "Help", "H", "cmd_app_help")
        );
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            builder.AddFooterCommandHandler(region, "Help", "H", _ => { })
        );
    }
}
