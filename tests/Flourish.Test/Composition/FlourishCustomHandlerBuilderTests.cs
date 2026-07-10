using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Composition;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Test.Composition;

public sealed class FlourishCustomHandlerBuilderTests
{
    [Fact]
    public void PublicContract_ExposesOnlyCanonicalCustomHandlerMethods()
    {
        var methods = typeof(IFlourishCustomHandlerBuilder).GetMethods();

        Assert.Equal(6, methods.Length);
        Assert.Equal(
            [
                "Add",
                "AddFooterCommand",
                "AddFooterCommandHandler",
                "AddTitlebarAction",
                "AddTitlebarActionHandler",
                "SetProfileContent",
            ],
            methods.Select(method => method.Name).Order()
        );

        var add = Assert.Single(methods, method => method.Name == "Add");
        Assert.Equal(
            [
                typeof(FlourishRegion),
                typeof(Func<IServiceProvider, FrameworkElement>),
                typeof(int),
            ],
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
            methods.Where(method => method.Name.StartsWith("AddFooterCommand", StringComparison.Ordinal)),
            method =>
                Assert.Equal(
                    typeof(FlourishRegion),
                    method.GetParameters()[0].ParameterType
                )
        );
    }

    [Fact]
    public void CanonicalMethods_RegisterContentInExplicitRegions()
    {
        var options = new FlourishShellOptions();
        IFlourishCustomHandlerBuilder builder = new FlourishCustomHandlerBuilder(options);

        builder
            .Add(FlourishRegion.FooterStart, _ => null!, order: 3)
            .SetProfileContent(_ => null!)
            .SetProfileContent(_ => null!)
            .AddFooterCommand(
                FlourishRegion.FooterEnd,
                "Help",
                "H",
                "app.help",
                order: 5
            )
            .AddFooterCommandHandler(
                FlourishRegion.FooterStart,
                "Refresh",
                "R",
                _ => { },
                order: 7
            );

        Assert.Collection(
            options.RegionContents,
            content =>
            {
                Assert.Equal(FlourishRegion.FooterStart, content.Region);
                Assert.Equal(3, content.Order);
            },
            content =>
            {
                Assert.Equal(FlourishRegion.TitlebarProfile, content.Region);
                Assert.Equal(0, content.Order);
            },
            content =>
            {
                Assert.Equal(FlourishRegion.FooterEnd, content.Region);
                Assert.Equal(5, content.Order);
            },
            content =>
            {
                Assert.Equal(FlourishRegion.FooterStart, content.Region);
                Assert.Equal(7, content.Order);
            }
        );
    }

    [Theory]
    [InlineData(FlourishRegion.TitlebarEnd)]
    [InlineData(FlourishRegion.ContentFooter)]
    public void FooterHelpers_WithNonFooterRegion_ThrowArgumentOutOfRangeException(
        FlourishRegion region
    )
    {
        IFlourishCustomHandlerBuilder builder = new FlourishCustomHandlerBuilder(
            new FlourishShellOptions()
        );

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            builder.AddFooterCommand(region, "Help", "H", "app.help")
        );
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            builder.AddFooterCommandHandler(region, "Help", "H", _ => { })
        );
    }
}
