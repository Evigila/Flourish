using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.WPF;
using ArkheideSystem.Flourish.WPF.Abstract;
using Xunit;
using F = ArkheideSystem.Flourish.WPF.Controls;

namespace ArkheideSystem.Tests.Flourish.WPF;

public class ShellTests
{
    [Fact]
    public void Native_navigation_click_obeys_async_guard_and_discards_a_stale_route() => NativeTest.Run(async () =>
    {
        var home = new F.TextBox { Text = "Home draft" };
        var current = new F.TextBox { Text = "Current destination" };
        var pending = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var staleConstructed = 0;
        var builder = new FrameworkBuilder().ConfigureNavigation(nav =>
        {
            nav.AddNav("Home", "home", "/home", () => home);
            nav.AddNav("Pending", "hourglass_empty", "/pending", () => { staleConstructed++; return new TextBlock(); });
            nav.AddNav("Current", "settings", "/current", () => current);
            nav.AddNav("Disabled", "block", "/disabled", () => throw new InvalidOperationException(), disabled:true);
        });
        var shell = new F.ApplicationShell { Options = builder.Build(), Guard = new F.NavigationGuard { CanNavigate = route => route == "/pending" ? pending.Task : Task.FromResult(true) } };
        var stage = NativeTest.Stage(shell);
        var primary = NativeTest.Descendants<F.PrimaryNavigationItem>(stage).First();
        typeof(System.Windows.Controls.Button).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(primary, null);
        Assert.Equal("/home", shell.SelectedRoute); Assert.Same(home, shell.Content);
        NativeTest.Layout(stage, 1000, 700);
        var selected = NativeTest.Descendants<F.PrimaryNavigationItem>(stage).Single(item => item.IsSelected);
        Assert.True(NativeTest.Descendants<F.Icon>(selected).Single().Filled);
        var oldRequest = shell.NavigateAsync("/pending");
        Assert.False(oldRequest.IsCompleted);
        Assert.True(await shell.NavigateAsync("/current"));
        pending.SetResult(true);
        Assert.False(await oldRequest);
        Assert.Equal(0, staleConstructed); Assert.Equal("/current", shell.SelectedRoute); Assert.Same(current, shell.Content);
        Assert.False(await shell.NavigateAsync("/disabled"));
        Assert.False(await shell.NavigateAsync("/missing"));
        Assert.False(await shell.NavigateAsync("/current"));
    });

    [Fact]
    public void Shell_live_localization_detaches_on_unload_and_refreshes_after_reloading() => NativeTest.Run(() =>
    {
        var provider = new MutableProvider();
        var builder = new FrameworkBuilder { TextProvider = provider };
        builder.ConfigureProject(project => project.SetProjectName(new TextReference("Tests", "Project", "Original project")))
            .ConfigureNavigation(nav => nav.AddNav(new TextReference("Tests", "Home", "Home"), "home", "/home", () => new TextBlock()));
        var shell = new F.ApplicationShell { Options = builder.Build() };
        NativeTest.Stage(shell);
        shell.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
        Assert.Equal(1, provider.Subscriptions);
        var title = NativeTest.Part<TextBlock>(shell, "PART_Title");
        provider.Value = "Translated project"; provider.Notify();
        Assert.Equal("Translated project", title.Text);
        shell.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
        Assert.Equal(0, provider.Subscriptions);
        provider.Value = "Reloaded project"; provider.Notify();
        Assert.Equal("Translated project", title.Text);
        shell.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
        Assert.Equal(1, provider.Subscriptions); Assert.Equal("Reloaded project", title.Text);
        shell.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
        Assert.Equal(0, provider.Subscriptions);
    });

    [Fact]
    public void Shell_responsive_flags_and_whole_secondary_host_follow_native_available_width() => NativeTest.Run(async () =>
    {
        var shell = new F.ApplicationShell { Options = new FrameworkBuilder().ConfigureNavigation(nav =>
            nav.AddNav("Home", "home", "/home", () => new TextBlock(), secondary: sub => sub.AddSubNav("Detail", "description", "/home/detail", () => new TextBlock()))).Build() };
        var stage = NativeTest.Stage(shell, 1200, 700);
        Assert.True(shell.HasNavigation); Assert.False(shell.HasSecondary);
        Assert.Equal(Visibility.Collapsed, NativeTest.Part<Border>(shell, "PART_SecondaryHost").Visibility);
        Assert.True(await shell.NavigateAsync("/home/detail"));
        NativeTest.Layout(stage, 1200, 700);
        Assert.True(shell.HasSecondary); Assert.False(shell.IsNarrow); Assert.False(shell.IsCompact);
        Assert.Equal(232, NativeTest.Part<Border>(shell, "PART_SecondaryHost").ActualWidth);
        Assert.Equal(76, NativeTest.Part<Border>(shell, "PART_PrimaryHost").ActualWidth);
        NativeTest.Layout(stage, 700, 700);
        Assert.True(shell.IsNarrow); Assert.False(shell.IsCompact);
        Assert.Equal(58, NativeTest.Part<Border>(shell, "PART_PrimaryHost").ActualWidth);
        NativeTest.Layout(stage, 500, 700);
        Assert.True(shell.IsCompact);
    });

    [Fact]
    public void Shell_reapplying_the_native_template_keeps_one_toggle_handler_and_backdrop_closes_tree() => NativeTest.Run(async () =>
    {
        var shell = new F.ApplicationShell { Options = new FrameworkBuilder().ConfigureNavigation(nav =>
            nav.AddNav("Home", "home", "/home", () => new TextBlock(), secondary: sub =>
                sub.AddSubNav("Section", "folder", "/home/section", () => new TextBlock(), children: nested =>
                    nested.AddSubNav("Detail", "description", "/home/section/detail", () => new TextBlock())))).Build() };
        var stage = NativeTest.Stage(shell, 700, 700);
        Assert.True(await shell.NavigateAsync("/home/section/detail"));
        Assert.True(shell.HasNavigationTree); Assert.False(shell.NavigationOpen);
        shell.OnApplyTemplate();
        var toggle = NativeTest.Part<F.Button>(shell, "PART_NavigationToggle");
        var click = typeof(System.Windows.Controls.Button).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!;
        click.Invoke(toggle, null); Assert.True(shell.NavigationOpen);
        NativeTest.Layout(stage, 700, 700);
        var backdrop = NativeTest.Part<F.Button>(shell, "PART_NavigationBackdrop");
        Assert.Equal(Visibility.Visible, backdrop.Visibility);
        var scroll = NativeTest.Part<ScrollViewer>(shell, "PART_Scroll");
        Assert.False(scroll.IsHitTestVisible);
        Assert.Equal(System.Windows.Input.KeyboardNavigationMode.None, System.Windows.Input.KeyboardNavigation.GetTabNavigation(scroll));
        click.Invoke(backdrop, null); Assert.False(shell.NavigationOpen);
        Assert.True(scroll.IsHitTestVisible);
    });

    private sealed class MutableProvider : ITextProvider
    {
        private EventHandler? changed;
        public int Subscriptions { get; private set; }
        public string Value { get; set; } = "Original project";
        public string Culture => "en-US";
        public CultureInfo FormatCulture => CultureInfo.GetCultureInfo("en-US");
        public event EventHandler? Changed { add { changed += value; Subscriptions++; } remove { changed -= value; Subscriptions--; } }
        public string Get(TextReference reference, params object?[] arguments) => reference.Token == "Project" ? Value : reference.FallbackText ?? reference.Token;
        public void Notify() => changed?.Invoke(this, EventArgs.Empty);
    }
}
