using System.ComponentModel;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using ArkheideSystem.Flourish.WPF.Abstract;
using Xunit;
using F = ArkheideSystem.Flourish.WPF.Controls;

namespace ArkheideSystem.Tests.Flourish.WPF;

public class PopupTests
{
    [Theory]
    [InlineData(PopupStrength.Weak, false)]
    [InlineData(PopupStrength.Strong, true)]
    public void Owned_popups_apply_strength_to_the_real_native_popup(PopupStrength strength, bool staysOpen) => NativeTest.Run(() =>
    {
        var dropdown = new F.DropdownSurface { Header = "Details", Strength = strength, Content = new TextBlock { Text = "Content" } };
        NativeTest.Stage(dropdown);
        Assert.Equal(staysOpen, NativeTest.Part<Popup>(dropdown, "PART_Popup").StaysOpen);
        dropdown.Strength = strength == PopupStrength.Strong ? PopupStrength.Weak : PopupStrength.Strong;
        Assert.Equal(!staysOpen, NativeTest.Part<Popup>(dropdown, "PART_Popup").StaysOpen);
        var selector = new F.MultiSelectBox { Strength = strength, Options = new[] { new MultiSelectOption("a", "Alpha") } };
        NativeTest.Stage(selector);
        Assert.Equal(staysOpen, NativeTest.Part<Popup>(selector, "PART_Popup").StaysOpen);
        selector.Strength = strength == PopupStrength.Strong ? PopupStrength.Weak : PopupStrength.Strong;
        Assert.Equal(!staysOpen, NativeTest.Part<Popup>(selector, "PART_Popup").StaysOpen);
    });

    [Fact]
    public void Weak_defaults_preserve_compatibility_and_invalid_strength_is_rejected() => NativeTest.Run(() =>
    {
        using var dialog = new F.Dialog();
        var dropdown = new F.DropdownSurface(); var menu = new F.ActionMenu(); var split = new F.SplitButton(); var selector = new F.MultiSelectBox();
        Assert.Equal(PopupStrength.Weak, dialog.Strength); Assert.True(dialog.Dismissible);
        Assert.Equal(PopupStrength.Weak, dropdown.Strength); Assert.Equal(PopupStrength.Weak, menu.Strength);
        Assert.Equal(PopupStrength.Weak, split.Strength); Assert.Equal(PopupStrength.Weak, selector.Strength);
        Assert.Throws<ArgumentException>(() => dialog.Strength = (PopupStrength)42);
        Assert.Throws<ArgumentException>(() => dropdown.Strength = (PopupStrength)42);
        Assert.Throws<ArgumentException>(() => menu.Strength = (PopupStrength)42);
        Assert.Throws<ArgumentException>(() => split.Strength = (PopupStrength)42);
        Assert.Throws<ArgumentException>(() => selector.Strength = (PopupStrength)42);
    });

    [Theory]
    [InlineData(PopupStrength.Weak, false)]
    [InlineData(PopupStrength.Strong, true)]
    public void Dropdown_trigger_automation_obeys_strength_and_explicit_close_preserves_binding(PopupStrength strength, bool remainsOpen) => NativeTest.Run(() =>
    {
        var model = new OpenModel();
        var dropdown = new F.DropdownSurface { Strength = strength, Header = "Details" };
        dropdown.SetBinding(F.DropdownSurface.IsOpenProperty, OpenBinding(model));
        NativeTest.Stage(dropdown);
        var trigger = NativeTest.Part<ToggleButton>(dropdown, "Trigger");
        var automation = (IToggleProvider)new ToggleButtonAutomationPeer(trigger).GetPattern(PatternInterface.Toggle);
        try
        {
            automation.Toggle(); Assert.True(dropdown.IsOpen); Assert.True(model.IsOpen);
            automation.Toggle(); Assert.Equal(remainsOpen, dropdown.IsOpen); Assert.Equal(remainsOpen, model.IsOpen);
            dropdown.Close(); Assert.False(dropdown.IsOpen); Assert.False(model.IsOpen);
            Assert.NotNull(dropdown.GetBindingExpression(F.DropdownSurface.IsOpenProperty));
        }
        finally { dropdown.Close(); }
    });

    [Theory]
    [InlineData(PopupStrength.Weak, false)]
    [InlineData(PopupStrength.Strong, true)]
    public void Dropdown_escape_uses_strength_and_disable_releases_lifetime(PopupStrength strength, bool remainsOpen) => NativeTest.Run(() =>
    {
        var dropdown = new F.DropdownSurface { Strength = strength, Header = "Details" };
        var stage = NativeTest.Stage(dropdown);
        using var connection = NativeTest.Connect(stage);
        try
        {
            dropdown.IsOpen = true;
            var args = Escape(stage);
            NativeTest.Part<Popup>(dropdown, "PART_Popup").Child.RaiseEvent(args);
            Assert.True(args.Handled); Assert.Equal(remainsOpen, dropdown.IsOpen);
            dropdown.IsEnabled = false; Assert.False(dropdown.IsOpen);
        }
        finally { dropdown.Close(); }
    });

    [Fact]
    public void Strong_multi_selection_retains_snapshots_until_done_and_keeps_bindings() => NativeTest.Run(() =>
    {
        var model = new OpenModel();
        var selector = new F.MultiSelectBox { Strength = PopupStrength.Strong, CloseLabel = "Complete selection", Options = new[] { new MultiSelectOption("a", "Alpha"), new MultiSelectOption("b", "Beta", Disabled: true) } };
        selector.SetBinding(F.MultiSelectBox.IsOpenProperty, OpenBinding(model));
        NativeTest.Stage(selector);
        var trigger = NativeTest.Part<ToggleButton>(selector, "Trigger");
        var automation = (IToggleProvider)new ToggleButtonAutomationPeer(trigger).GetPattern(PatternInterface.Toggle);
        MultiSelectChange? changed = null;
        selector.Changed += (_, change) => changed = change;
        try
        {
            automation.Toggle(); automation.Toggle(); Assert.True(selector.IsOpen); Assert.True(model.IsOpen);
            Assert.True(selector.SetSelected("a", true)); Assert.True(selector.IsOpen);
            Assert.Equal(new[] { "a", "b" }, changed!.OrderedKeys); Assert.Equal(new[] { "a" }, changed.SelectedKeys);
            Assert.False(selector.SetSelected("b", true));
            var done = NativeTest.Part<StackPanel>(selector, "PART_Items").Children.OfType<F.Button>().Single(button => button.Text == "Complete selection");
            Click(done); Assert.False(selector.IsOpen); Assert.False(model.IsOpen);
            Assert.True(selector.Options!.Single(option => option.Key == "a").Selected);
            Assert.NotNull(selector.GetBindingExpression(F.MultiSelectBox.IsOpenProperty));
        }
        finally { selector.Close(); }
    });

    [Fact]
    public void Strong_multi_selection_creation_blocks_completion_and_unload_releases_the_popup() => NativeTest.Run(async () =>
    {
        var pending = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var selector = new F.MultiSelectBox { Strength = PopupStrength.Strong, CreateRequested = _ => pending.Task, Options = Array.Empty<MultiSelectOption>() };
        NativeTest.Stage(selector);
        selector.IsOpen = true;
        var create = selector.RequestCreateAsync("New choice");
        Assert.True(selector.Creating); Assert.False(selector.Close()); Assert.True(selector.IsOpen);
        selector.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent)); Assert.False(selector.IsOpen);
        pending.SetResult(true); Assert.False(await create); Assert.False(selector.Creating);
    });

    [Theory]
    [InlineData(PopupStrength.Weak, false)]
    [InlineData(PopupStrength.Strong, true)]
    public void Action_menu_owns_its_popup_and_only_enabled_actions_can_complete_it(PopupStrength strength, bool remainsOpen) => NativeTest.Run(() =>
    {
        var calls = 0;
        var menu = new F.ActionMenu { Strength = strength, Actions = new[] { new MenuAction("Locked", () => { calls += 100; return Task.CompletedTask; }, Disabled: true), new MenuAction("Choose", () => { calls++; return Task.CompletedTask; }) } };
        var stage = NativeTest.Stage(menu);
        using var connection = NativeTest.Connect(stage);
        try
        {
            menu.Open(); var popup = Private<Popup>(menu, "popup");
            Assert.Equal(strength == PopupStrength.Strong, popup.StaysOpen);
            Assert.IsType<Border>(popup.Child); menu.Open(); Assert.Same(popup, Private<Popup>(menu, "popup"));
            var items = NativeTest.Descendants<MenuItem>(popup.Child).ToArray(); Assert.Equal(2, items.Length);
            items[0].RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent)); Assert.Equal(0, calls); Assert.True(menu.IsOpen);
            var args = Escape(stage); popup.Child.RaiseEvent(args); Assert.True(args.Handled); Assert.Equal(remainsOpen, menu.IsOpen);
            if (!menu.IsOpen) { menu.Open(); popup = Private<Popup>(menu, "popup"); items = NativeTest.Descendants<MenuItem>(popup.Child).ToArray(); }
            items[1].RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent)); Assert.Equal(1, calls); Assert.False(menu.IsOpen);
        }
        finally { menu.Close(); }
    });

    [Fact]
    public void Split_and_service_menus_forward_the_same_strength_contract() => NativeTest.Run(() =>
    {
        var split = new F.SplitButton { Strength = PopupStrength.Strong, Actions = new[] { new MenuAction("Choose", () => Task.CompletedTask) } };
        NativeTest.Stage(split);
        var secondary = NativeTest.Descendants<F.ActionMenu>(split).Single(); Assert.Equal(PopupStrength.Strong, secondary.Strength);
        split.Strength = PopupStrength.Weak; Assert.Equal(PopupStrength.Weak, secondary.Strength);
        var services = new F.ServiceMenu { Strength = PopupStrength.Strong, Links = new[] { new ServiceLink("Service", "/service") } };
        Assert.Equal(PopupStrength.Strong, services.Strength); Assert.Single(services.Actions!);
    });

    [Theory]
    [InlineData(PopupStrength.Weak, true, false, true)]
    [InlineData(PopupStrength.Strong, true, false, false)]
    [InlineData(PopupStrength.Weak, false, false, false)]
    [InlineData(PopupStrength.Weak, true, true, false)]
    public void Dialog_backdrop_obeys_strength_dismissible_and_busy_before_explicit_completion(PopupStrength strength, bool dismissible, bool busy, bool cancels) => NativeTest.Run(async () =>
    {
        using var dialog = new F.Dialog { Strength = strength, Dismissible = dismissible, Busy = busy };
        var closeChecks = 0; dialog.CanClose = () => { closeChecks++; return Task.FromResult(true); };
        var owner = Owner(); Exception? failure = null;
        dialog.Opened += (_, _) =>
        {
            try
            {
                var overlay = Assert.IsType<Grid>(Private<Window>(dialog, "window").Content);
                overlay.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Mouse.MouseDownEvent });
                Assert.Equal(!cancels, dialog.IsOpen); Assert.Equal(cancels ? 1 : 0, closeChecks);
                if (dialog.IsOpen) { dialog.Busy = false; Assert.True(dialog.CloseAsync("completed").GetAwaiter().GetResult()); }
            }
            catch (Exception error) { failure = error; }
            finally { if (dialog.IsOpen) dialog.Dispose(); }
        };
        try { var result = await dialog.ShowAsync(owner); if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw(); Assert.Equal(cancels ? null : "completed", result); }
        finally { owner.Close(); }
    });

    [Fact]
    public void Strong_dialog_blocks_escape_and_native_close_then_can_reopen_the_same_content() => NativeTest.Run(async () =>
    {
        var retained = new F.TextBox { Text = "Retained draft" };
        using var dialog = new F.Dialog { Strength = PopupStrength.Strong, Content = retained };
        var owner = Owner(); Exception? failure = null; var openings = 0;
        var keyRoot = new Grid(); using var connection = NativeTest.Connect(keyRoot);
        dialog.Opened += (_, _) =>
        {
            try
            {
                openings++;
                var window = Private<Window>(dialog, "window");
                var args = Escape(keyRoot); window.RaiseEvent(args); Assert.True(args.Handled); Assert.True(dialog.IsOpen);
                window.Close(); Assert.True(dialog.IsOpen);
                Assert.True(dialog.CloseAsync(openings).GetAwaiter().GetResult());
            }
            catch (Exception error) { failure = error; }
            finally { if (dialog.IsOpen) dialog.Dispose(); }
        };
        try
        {
            Assert.Equal(1, await dialog.ShowAsync(owner)); if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
            Assert.Null(LogicalTreeHelper.GetParent(dialog)); Assert.Same(retained, dialog.Content);
            Assert.Equal(2, await dialog.ShowAsync(owner)); if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
            Assert.Equal("Retained draft", retained.Text);
        }
        finally { owner.Close(); }
    });

    [Theory]
    [InlineData(PopupStrength.Weak, false)]
    [InlineData(PopupStrength.Strong, true)]
    public void Notice_actions_use_an_owned_popup_and_only_explicit_completion_closes_strong(PopupStrength strength, bool remainsOpen) => NativeTest.Run(() =>
    {
        var complete = new F.Button { Text = "Complete notice" };
        var payload = new F.Notice { Title = "Review", Content = "Supporting information", Actions = complete };
        var notice = new F.NoticeTrigger { Notice = payload, Strength = strength };
        complete.Click += (_, _) => notice.Close();
        var stage = NativeTest.Stage(notice); using var connection = NativeTest.Connect(stage);
        try
        {
            notice.Open(); var popup = Private<Popup>(notice, "noticePopup");
            Assert.Equal(strength == PopupStrength.Strong, popup.StaysOpen);
            Assert.Same(payload, Assert.IsType<ContentControl>(popup.Child).Content);
            notice.Open(); Assert.Same(popup, Private<Popup>(notice, "noticePopup")); Assert.True(notice.IsOpen);
            var args = Escape(stage); popup.Child.RaiseEvent(args); Assert.True(args.Handled); Assert.Equal(remainsOpen, notice.IsOpen);
            notice.Open(); Click(complete); Assert.False(notice.IsOpen); Assert.Same(payload, notice.Notice);
            Assert.Throws<ArgumentException>(() => notice.Strength = (PopupStrength)42);
        }
        finally { notice.Close(); }
    });

    private static Binding OpenBinding(OpenModel model) => new(nameof(OpenModel.IsOpen)) { Source = model, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
    private static KeyEventArgs Escape(System.Windows.Media.Visual root) => new(Keyboard.PrimaryDevice, PresentationSource.FromVisual(root)!, Environment.TickCount, Key.Escape) { RoutedEvent = Keyboard.PreviewKeyDownEvent };
    private static T Private<T>(object owner, string name) => Assert.IsAssignableFrom<T>(owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(owner));
    private static void Click(F.Button button) => typeof(System.Windows.Controls.Button).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(button, null);
    private static Window Owner() { var owner = new Window { Width = 800, Height = 600, Left = 0, Top = 0 }; new WindowInteropHelper(owner).EnsureHandle(); return owner; }
    private sealed class OpenModel : INotifyPropertyChanged
    {
        private bool isOpen;
        public bool IsOpen { get => isOpen; set { isOpen = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsOpen))); } }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
