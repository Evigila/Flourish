using System.Threading.Tasks;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Commands;
using ArkheideSystem.Flourish.Shell.Toolbar;
using ArkheideSystem.Flourish.Test.Infrastructure;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ArkheideSystem.Flourish.Views.Windows;
using FlourishButton = ArkheideSystem.Flourish.Controls.Button;

namespace ArkheideSystem.Flourish.Test.Internal.Interaction;

public sealed class ShellToolbarControllerTests
{
    [Fact]
    public void SetPage_UsesPageToolbarAndKeepsTheCachedButtonsStable()
    {
        StaTest.Run(() =>
        {
            var fixture = CreateFixture();
            fixture.Service.Set(
                typeof(TestPage),
                [Item("page", "Page", "\uE8A7")],
                iconOnly: true
            );
            using var sut = fixture.CreateController();
            sut.Start();

            var defaultButton = Assert.IsType<FlourishButton>(
                Assert.Single(fixture.View.Items.Children)
            );
            Assert.Equal("Default", defaultButton.Content);

            sut.SetPage(typeof(TestPage));
            var pageButton = Assert.IsType<FlourishButton>(
                Assert.Single(fixture.View.Items.Children)
            );
            Assert.Null(pageButton.Content);
            Assert.Equal(30, pageButton.Width);

            sut.SetPage(typeof(TestPage));
            Assert.Same(pageButton, Assert.Single(fixture.View.Items.Children));
        });
    }

    [Fact]
    public void ServiceChanges_InvalidateOnlyTheToolbarThatCanAffectTheActivePage()
    {
        StaTest.Run(() =>
        {
            var fixture = CreateFixture();
            fixture.Service.Set(typeof(TestPage), [Item("page", "Page")]);
            fixture.Service.Set(typeof(OtherPage), [Item("other", "Other")]);
            using var sut = fixture.CreateController();
            sut.Start(typeof(TestPage));
            var original = Assert.Single(fixture.View.Items.Children);

            fixture.Service.Set(typeof(OtherPage), [Item("other-2", "Other 2")]);
            Assert.Same(original, Assert.Single(fixture.View.Items.Children));

            fixture.Service.SetDefault([Item("default-2", "Default 2")]);
            Assert.Same(original, Assert.Single(fixture.View.Items.Children));

            fixture.Service.Set(typeof(TestPage), [Item("page-2", "Page 2")]);
            Assert.NotSame(original, Assert.Single(fixture.View.Items.Children));

            sut.SetPage(typeof(FallbackPage));
            var fallback = Assert.Single(fixture.View.Items.Children);
            fixture.Service.SetDefault([Item("default-3", "Default 3")]);
            Assert.NotSame(fallback, Assert.Single(fixture.View.Items.Children));
        });
    }

    [Fact]
    public void EnablementChanges_PreserveTheActivePageAndRebuildItsToolbar()
    {
        StaTest.Run(() =>
        {
            var fixture = CreateFixture();
            fixture.Service.Set(typeof(TestPage), [Item("page", "Page")]);
            using var sut = fixture.CreateController();
            sut.Start(typeof(TestPage));

            fixture.Service.SetEnabled(false);
            Assert.Empty(fixture.View.Items.Children);
            Assert.Equal(
                Visibility.Collapsed,
                Assert.IsType<Border>(fixture.View.Content).Visibility
            );

            fixture.Service.SetEnabled(true);
            var button = Assert.IsType<FlourishButton>(
                Assert.Single(fixture.View.Items.Children)
            );
            Assert.Equal("Page", button.Content);
        });
    }

    [Fact]
    public void RefreshVisibility_IncludesToolbarRegionContent()
    {
        StaTest.Run(() =>
        {
            var fixture = CreateFixture();
            fixture.Service.SetDefault([]);
            using var sut = fixture.CreateController();
            sut.Start();
            Assert.Equal(
                Visibility.Collapsed,
                Assert.IsType<Border>(fixture.View.Content).Visibility
            );

            fixture.View.StartRegion.Children.Add(new Border());
            sut.RefreshVisibility();
            Assert.Equal(
                Visibility.Visible,
                Assert.IsType<Border>(fixture.View.Content).Visibility
            );

            fixture.Service.SetEnabled(false);
            Assert.Equal(
                Visibility.Collapsed,
                Assert.IsType<Border>(fixture.View.Content).Visibility
            );
        });
    }

    [Fact]
    public void CommandChangesAndClicks_UpdateInPlaceUntilDispose()
    {
        StaTest.Run(() =>
        {
            var canExecute = true;
            var executions = 0;
            var fixture = CreateFixture(Item("run", "Run", commandKey: "cmd_test_run"));
            using var registration = fixture.Commands.Register(
                "cmd_test_run",
                (context, _) =>
                {
                    Assert.Equal(CommandSource.Toolbar, context.Source);
                    executions++;
                    return ValueTask.FromResult(CommandResult.Handled);
                },
                _ => canExecute
            );
            var sut = fixture.CreateController();
            sut.Start();
            var button = Assert.IsType<FlourishButton>(
                Assert.Single(fixture.View.Items.Children)
            );
            Assert.True(button.IsEnabled);

            canExecute = false;
            fixture.Commands.NotifyCanExecuteChanged("cmd_test_run");
            Assert.Same(button, Assert.Single(fixture.View.Items.Children));
            Assert.False(button.IsEnabled);

            canExecute = true;
            fixture.Commands.NotifyCanExecuteChanged("cmd_test_run");
            button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.Equal(1, executions);

            sut.Dispose();
            sut.Dispose();
            button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            fixture.Service.SetDefault([Item("replacement", "Replacement")]);
            Assert.Equal(1, executions);
            Assert.Empty(fixture.View.Items.Children);
        });
    }

    private static Fixture CreateFixture(params FlourishToolbarItem[] defaultItems)
    {
        var options = new FlourishToolbarOptions { IsDynamicToolbarEnabled = true };
        options.ToolbarItems.AddRange(
            defaultItems.Length == 0 ? [Item("default", "Default")] : defaultItems
        );
        var service = new FlourishToolbarService(options);
        var commands = new CommandDispatcher();
        return new Fixture(options, service, commands, new FlourishToolbar());
    }

    private static FlourishToolbarItem Item(
        string id,
        string displayName,
        string? icon = null,
        string? commandKey = null
    ) =>
        new(displayName, icon ?? string.Empty, commandKey) { Id = id };

    private sealed record Fixture(
        FlourishToolbarOptions Options,
        FlourishToolbarService Service,
        CommandDispatcher Commands,
        FlourishToolbar View
    )
    {
        internal ShellToolbarController CreateController() =>
            new(View, Service, Commands, Commands);
    }

    private sealed class TestPage : Page { }

    private sealed class OtherPage : Page { }

    private sealed class FallbackPage : Page { }
}
