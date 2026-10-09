using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ArkheideSystem.Flourish.WPF.Abstract;
using Xunit;
using F = ArkheideSystem.Flourish.WPF.Controls;
using ThemeMode = ArkheideSystem.Flourish.WPF.Abstract.ThemeMode;

namespace ArkheideSystem.Tests.Flourish.WPF;

public class ControlTests
{
    [Theory]
    [InlineData(ButtonVariant.Primary, "Primary", "PrimaryInk")]
    [InlineData(ButtonVariant.Secondary, "Canvas", "Text")]
    [InlineData(ButtonVariant.Danger, "Danger", "Surface")]
    [InlineData(ButtonVariant.Quiet, null, "Accent")]
    [InlineData(ButtonVariant.Underline, null, "Accent")]
    [InlineData(ButtonVariant.Elevated, "Surface", "Text")]
    public void Native_button_variants_resolve_the_Blazor_semantic_paint_roles(ButtonVariant variant, string? background, string foreground) => NativeTest.Run(() =>
    {
        var button = new F.Button { Text = "Action", Variant = variant };
        var stage = NativeTest.Stage(button, 360, 100);
        var frame = NativeTest.Part<Border>(button, "Frame");
        if (background is null) Assert.Equal((byte)0, Assert.IsType<SolidColorBrush>(frame.Background).Color.A);
        else Assert.Equal(((SolidColorBrush)stage.FindResource("Flourish.Brush." + background)).Color, Assert.IsType<SolidColorBrush>(frame.Background).Color);
        Assert.Equal(((SolidColorBrush)stage.FindResource("Flourish.Brush." + foreground)).Color, Assert.IsType<SolidColorBrush>(button.Foreground).Color);
        Assert.Equal(48, button.MinHeight);
        if (variant == ButtonVariant.Elevated) Assert.NotNull(button.Effect);
    });

    [Theory]
    [InlineData(UniformGridVariant.Filled, "Primary", "PrimaryInk")]
    [InlineData(UniformGridVariant.Outlined, "Canvas", "Text")]
    [InlineData(UniformGridVariant.Danger, "Danger", "Surface")]
    [InlineData(UniformGridVariant.Elevated, "Surface", "Text")]
    public void Native_uniform_action_variants_keep_the_same_semantic_roles(UniformGridVariant variant, string background, string foreground) => NativeTest.Run(() =>
    {
        var button = new F.UniformGridButton { Text = "Destination", GridVariant = variant };
        var stage = NativeTest.Stage(button, 280, 280);
        var frame = NativeTest.Part<Border>(button, "Frame");
        Assert.Equal(((SolidColorBrush)stage.FindResource("Flourish.Brush." + background)).Color, Assert.IsType<SolidColorBrush>(frame.Background).Color);
        Assert.Equal(((SolidColorBrush)stage.FindResource("Flourish.Brush." + foreground)).Color, Assert.IsType<SolidColorBrush>(button.Foreground).Color);
    });

    [Fact]
    public void Native_action_invocation_respects_disabled_busy_and_can_execute() => NativeTest.Run(() =>
    {
        var button = new F.Button { Text = "Save" };
        NativeTest.Stage(button, 300, 100);
        var invocations = 0;
        button.Click += (_, _) => invocations++;
        var peer = new ButtonAutomationPeer(button);
        var invoke = (System.Windows.Automation.Provider.IInvokeProvider)peer.GetPattern(PatternInterface.Invoke);
        button.Disabled = true;
        Assert.False(button.IsEnabled);
        Assert.Throws<ElementNotEnabledException>(() => invoke.Invoke());
        button.Disabled = false;
        button.Busy = true;
        Assert.False(button.IsEnabled);
        Assert.Throws<ElementNotEnabledException>(() => invoke.Invoke());
        button.Busy = false;
        Assert.True(button.IsEnabled);
        typeof(System.Windows.Controls.Button).GetMethod("OnClick", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(button, null);
        Assert.Equal(1, invocations);
    });

    [Theory]
    [InlineData(ThemeMode.Light)]
    [InlineData(ThemeMode.Dark)]
    public void Dropdown_expansion_indicators_follow_the_current_native_text_role(ThemeMode mode) => NativeTest.Run(() =>
    {
        var content = new StackPanel();
        content.Children.Add(new F.SelectBox { Options = new[] { new SelectOption("design", "Design") }, SelectedValue = "design" });
        content.Children.Add(new F.MultiSelectBox { Options = new[] { new MultiSelectOption("design", "Design") } });
        var stage = NativeTest.Stage(content, 400, 160, mode);
        var indicators = NativeTest.Descendants<F.ExpansionIndicator>(stage).ToArray();
        Assert.Equal(2, indicators.Length);
        var expected = ((SolidColorBrush)stage.FindResource("Flourish.Brush.Text")).Color;
        Assert.All(indicators, indicator => Assert.Equal(expected, Assert.IsType<SolidColorBrush>(indicator.Foreground).Color));
    });

    [Fact]
    public void Structured_action_has_one_accessible_label_and_passive_trailing_content() => NativeTest.Run(() =>
    {
        var button = new F.Button { Text = "Edit record", Description = "Change the contact details", TrailingText = "Ctrl+E" };
        NativeTest.Stage(button, 440, 100);
        var peer = UIElementAutomationPeer.CreatePeerForElement(button);
        Assert.Contains("Edit record", peer.GetName());
        Assert.Contains("Change the contact details", peer.GetName());
        Assert.Contains("Ctrl+E", peer.GetName());
        Assert.Single(NativeTest.Descendants<F.Button>(button));
        Assert.NotNull(button.Template);
    });

    [Fact]
    public void Selected_option_renders_its_readable_label_instead_of_record_debug_text() => NativeTest.Run(() =>
    {
        var select = new F.SelectBox { Options = new[] { new SelectOption("design", "Design"), new SelectOption("locked", "Locked", Disabled:true) }, SelectedValue = "design" };
        NativeTest.Stage(select, 360, 80);
        var selected = NativeTest.Part<ContentPresenter>(select, "Selection");
        Assert.Contains(NativeTest.Descendants<TextBlock>(selected), text => text.Text == "Design");
        Assert.DoesNotContain(NativeTest.Descendants<TextBlock>(selected), text => text.Text.Contains("SelectOption {", StringComparison.Ordinal));
    });

    [Fact]
    public void Native_single_line_input_is_48_dip_and_dense_pager_keeps_page_number_visible() => NativeTest.Run(() =>
    {
        var input = new F.TextBox { Text = "Readable text" };
        NativeTest.Stage(input, 360, 80);
        Assert.Equal(48, input.DesiredSize.Height);
        var pager = new F.DataPager { TotalCount = 20, PageSize = 10 };
        var stage = NativeTest.Stage(pager, 420, 80);
        var page = NativeTest.Descendants<F.TextBox>(stage).Single();
        Assert.Equal("1", page.Text);
        Assert.Equal(38, page.DesiredSize.Height);
        Assert.True(NativeTest.Part<ScrollViewer>(page, "PART_ContentHost").ActualWidth >= 40);
    });

    [Fact]
    public void Structured_action_rejects_missing_primary_label_and_a_second_content_contract() => NativeTest.Run(() =>
    {
        Assert.Throws<InvalidOperationException>(() => NativeTest.Stage(new F.Button { Description = "Description without label" }));
        Assert.Throws<InvalidOperationException>(() => NativeTest.Stage(new F.Button { Text = "Label", Description = "Description", Content = new TextBlock { Text = "Second label" } }));
    });

    [Fact]
    public void Native_text_and_numeric_binding_remains_two_way_after_a_commit() => NativeTest.Run(() =>
    {
        var model = new AmountModel { Amount = 2.5 };
        var number = new F.NumberBox { FormatCulture = CultureInfo.GetCultureInfo("pt-BR"), Minimum = 0, Maximum = 10 };
        BindingOperations.SetBinding(number, F.NumberBox.ValueProperty, new Binding(nameof(model.Amount)) { Source = model, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        NativeTest.Stage(number, 320, 100);
        number.Text = "3,75";
        Assert.True(number.Commit());
        Assert.Equal(3.75, model.Amount);
        Assert.NotNull(BindingOperations.GetBindingExpression(number, F.NumberBox.ValueProperty));
        number.Text = "20";
        Assert.False(number.Commit());
        Assert.Equal(3.75, model.Amount);
        number.Text = "";
        Assert.True(number.Commit());
        Assert.Null(model.Amount);
    });

    [Fact]
    public void Masked_input_normalizes_ascii_slots_and_preserves_two_way_raw_value_binding() => NativeTest.Run(() =>
    {
        var model = new TextModel();
        var mask = new F.MaskedInput { Mask = "AA-0000" };
        mask.SetBinding(F.MaskedInput.ValueProperty, new Binding(nameof(TextModel.Text)) { Source = model, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        NativeTest.Stage(mask, 360, 80);
        mask.Text = "ab-12é34";
        Assert.Equal("AB1234", mask.Value); Assert.Equal("AB-1234", mask.Text); Assert.Equal("AB1234", model.Text); Assert.True(mask.IsComplete);
        Assert.NotNull(mask.GetBindingExpression(F.MaskedInput.ValueProperty));
        mask.Value = "z9-4x56";
        Assert.Equal("Z9456", mask.Value); Assert.Equal("Z9-456", mask.Text); Assert.False(mask.IsComplete);
        mask.Value = ""; Assert.Equal("", mask.Text); Assert.False(mask.IsComplete);
    });

    [Fact]
    public void Autocomplete_ignores_stale_results_and_cancels_on_disable_without_showing_a_popup() => NativeTest.Run(async () =>
    {
        var older = new TaskCompletionSource<IEnumerable<SelectOption>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var canceled = new TaskCompletionSource<IEnumerable<SelectOption>>(TaskCreationOptions.RunContinuationsAsynchronously);
        CancellationToken olderToken = default, canceledToken = default;
        var autocomplete = new F.SearchAutocomplete { Search = (query, token) =>
        {
            if (query == "older") { olderToken = token; return older.Task; }
            if (query == "canceled") { canceledToken = token; return canceled.Task; }
            return Task.FromResult<IEnumerable<SelectOption>>(new[] { new SelectOption("current", "Current result") });
        } };
        var oldRequest = autocomplete.SearchAsync("older");
        await autocomplete.SearchAsync("current");
        Assert.True(olderToken.IsCancellationRequested);
        older.SetResult(new[] { new SelectOption("stale", "Stale result") }); await oldRequest;
        Assert.Equal("current", Assert.Single(autocomplete.Options!).Value);
        var canceledRequest = autocomplete.SearchAsync("canceled");
        autocomplete.IsEnabled = false; Assert.True(canceledToken.IsCancellationRequested);
        canceled.SetResult(new[] { new SelectOption("disabled", "Disabled result") }); await canceledRequest;
        Assert.Equal("current", Assert.Single(autocomplete.Options!).Value);
    });

    [Fact]
    public void Split_action_locks_only_primary_when_requested_and_both_during_busy() => NativeTest.Run(() =>
    {
        var split = new F.SplitButton { Text = "Publish", Actions = new[] { new MenuAction("Preview", () => Task.CompletedTask) } };
        NativeTest.Stage(split, 340, 100);
        var primary = NativeTest.Part<F.Button>(split, "PART_Primary");
        var secondary = NativeTest.Descendants<F.ActionMenu>(split).Single();
        split.PrimaryDisabled = true;
        Assert.False(primary.IsEnabled);
        Assert.True(secondary.IsEnabled);
        split.Busy = true;
        Assert.False(primary.IsEnabled);
        Assert.False(secondary.IsEnabled);
    });

    [Fact]
    public void Multi_selection_emits_complete_snapshots_and_preserves_fixed_and_disabled_members() => NativeTest.Run(() =>
    {
        var selector = new F.MultiSelectBox { ReorderEnabled = true, Options = new[]
        {
            new MultiSelectOption("fixed", "Fixed", true, CanDeselect:false, CanReorder:false),
            new MultiSelectOption("a", "Alpha", true), new MultiSelectOption("b", "Beta"),
            new MultiSelectOption("locked", "Locked", Disabled:true)
        }, MinimumSelected = 1, MaximumSelected = 3 };
        MultiSelectChange? observed = null;
        selector.Changed += (_, change) => observed = change;
        Assert.False(selector.SetSelected("fixed", false));
        Assert.False(selector.SetSelected("locked", true));
        Assert.False(selector.Move("a", -1));
        Assert.True(selector.Move("b", -1));
        Assert.Equal(new[] { "fixed", "b", "a", "locked" }, observed!.OrderedKeys);
        Assert.Equal(new[] { "a", "fixed" }, observed.SelectedKeys.Order());
        Assert.True(selector.SetSelected("b", true));
        Assert.Equal(new[] { "a", "b", "fixed" }, observed.SelectedKeys.Order());
        Assert.False(selector.SetSelected("unknown", true));
        selector.IsEnabled = false;
        Assert.False(selector.SetSelected("b", false));
    });

    [Fact]
    public void Multi_selection_rejects_duplicate_or_fixed_unselected_options() => NativeTest.Run(() =>
    {
        var duplicate = new F.MultiSelectBox { Options = new[] { new MultiSelectOption("same", "A"), new MultiSelectOption("same", "B") } };
        Assert.Throws<ArgumentException>(() => duplicate.SetSelected("same", true));
        var inconsistent = new F.MultiSelectBox { Options = new[] { new MultiSelectOption("fixed", "Fixed", CanDeselect:false) } };
        Assert.Throws<ArgumentException>(() => inconsistent.SetSelected("fixed", true));
    });

    [Fact]
    public void Multi_selection_async_creation_checks_duplicates_bounds_and_unload_lifetime() => NativeTest.Run(async () =>
    {
        var pending = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = new List<string>();
        var selector = new F.MultiSelectBox { Options = new[] { new MultiSelectOption("known", "Known", true) }, MaximumSelected = 2,
            CreateRequested = label => { calls.Add(label); return pending.Task; } };
        Assert.Equal("Known", selector.EffectiveText);
        Assert.False(await selector.RequestCreateAsync(" known "));
        var create = selector.RequestCreateAsync("  New choice  ");
        Assert.True(selector.Creating); Assert.False(selector.IsEnabled);
        Assert.False(await selector.RequestCreateAsync("Second choice"));
        Assert.Equal(new[] { "New choice" }, calls);
        selector.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
        pending.SetResult(true); Assert.False(await create);
        Assert.False(selector.Creating); Assert.True(selector.IsEnabled);
        Assert.Single(selector.Options!);
        selector.MaximumSelected = 1;
        Assert.False(await selector.RequestCreateAsync("Beyond maximum"));
        Assert.Single(calls);
    });

    [Fact]
    public void Search_debounce_clear_disable_and_unload_have_native_lifetimes() => NativeTest.Run(async () =>
    {
        var search = new F.SearchBox { DebounceMilliseconds = 40 };
        var calls = new List<string>();
        search.SearchRequested += (_, value) => calls.Add(value);
        search.Text = "obsolete";
        search.Text = "current";
        await Task.Delay(100);
        Assert.Equal(new[] { "current" }, calls);
        search.Text = "";
        Assert.Equal(new[] { "current", "" }, calls);
        search.Text = "canceled by disable";
        search.IsEnabled = false;
        await Task.Delay(100);
        Assert.Equal(2, calls.Count);
        search.IsEnabled = true;
        search.Text = "canceled by unload";
        search.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
        await Task.Delay(100);
        Assert.Equal(2, calls.Count);
    });

    [Fact]
    public void Uniform_rectangles_fill_equal_tracks_and_squares_remain_bounded() => NativeTest.Run(() =>
    {
        var rectangles = new F.UniformGrid { Columns = 3, CellHeight = 96 };
        for (var index = 0; index < 5; index++) rectangles.Children.Add(new Border());
        NativeTest.Layout(rectangles, 900, 193);
        Assert.All(rectangles.Children.Cast<FrameworkElement>(), cell => Assert.Equal(96, cell.ActualHeight));
        Assert.Equal((900d - 2) / 3, ((FrameworkElement)rectangles.Children[0]).ActualWidth, 4);
        var squares = new F.UniformGrid { Columns = 2, Shape = UniformGridShape.Square, MaxCellSize = 280 };
        squares.Children.Add(new Border()); squares.Children.Add(new Border());
        NativeTest.Layout(squares, 900, 280);
        Assert.All(squares.Children.Cast<FrameworkElement>(), cell => { Assert.Equal(280, cell.ActualWidth); Assert.Equal(cell.ActualWidth, cell.ActualHeight); });
        var invalid = new F.UniformGrid { Shape = UniformGridShape.Square, CellHeight = 100 };
        Assert.Throws<InvalidOperationException>(() => invalid.Measure(new Size(900, 500)));
    });

    [Fact]
    public void Validation_field_shares_one_renderer_and_clears_resolved_native_errors() => NativeTest.Run(() =>
    {
        var model = new TextModel { Text = "valid" };
        var input = new F.TextBox();
        var binding = new Binding(nameof(model.Text)) { Source = model, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.Explicit, NotifyOnValidationError = true };
        binding.ValidationRules.Add(new NonemptyRule());
        input.SetBinding(System.Windows.Controls.TextBox.TextProperty, binding);
        var field = new F.Field { Label = "Required name", Required = true, Content = input };
        NativeTest.Stage(field, 400, 200);
        input.Text = ""; input.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty)!.UpdateSource();
        Assert.True(Validation.GetHasError(input));
        var messages = NativeTest.Descendants<F.ValidationMessages>(field).Single();
        Assert.Contains(messages.Items.Cast<string>(), message => message == "A name is required.");
        Assert.NotNull(AutomationProperties.GetLabeledBy(input));
        input.Text = "repaired"; input.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty)!.UpdateSource();
        Assert.False(Validation.GetHasError(input));
        Assert.Empty(messages.Items);
        field.Errors = new[] { "Server validation" };
        Assert.Contains("Server validation", messages.Items.Cast<string>());
    });

    [Fact]
    public void Field_tracks_default_native_validation_notifications_and_replaces_its_input_without_stale_errors() => NativeTest.Run(() =>
    {
        F.TextBox Input()
        {
            var input = new F.TextBox();
            var binding = new Binding(nameof(TextModel.Text)) { Source = new TextModel { Text = "valid" }, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.Explicit };
            binding.ValidationRules.Add(new NonemptyRule());
            input.SetBinding(System.Windows.Controls.TextBox.TextProperty, binding);
            return input;
        }
        void Commit(F.TextBox input, string text) { input.Text = text; input.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty)!.UpdateSource(); }
        var first = Input(); var field = new F.Field { Label = "Name", Content = first };
        var stage = NativeTest.Stage(field, 400, 200);
        Commit(first, ""); Assert.True(Validation.GetHasError(first));
        Assert.Contains("A name is required.", field.Messages);
        var next = Input(); field.Content = next; NativeTest.Layout(stage, 400, 200);
        Assert.Empty(field.Messages); Assert.NotNull(AutomationProperties.GetLabeledBy(next));
        Commit(first, "valid again"); Commit(first, "");
        Assert.Empty(field.Messages);
        Commit(next, ""); Assert.Contains("A name is required.", field.Messages);
        Commit(next, "fixed"); Assert.Empty(field.Messages);
    });

    [Fact]
    public void Dialog_retains_keyed_content_and_pre_cancellation_never_starts_a_native_window() => NativeTest.Run(async () =>
    {
        var first = new F.TextBox { Text = "retained draft" };
        using var dialog = new F.Dialog { Views = new[] { new DialogView("first", first), new DialogView("second", new TextBlock { Text = "other view" }) } };
        dialog.View = "first";
        Assert.Same(first, dialog.Content);
        dialog.View = "second";
        dialog.View = "first";
        Assert.Same(first, dialog.Content);
        Assert.Equal("retained draft", first.Text);
        var owner = new Window();
        try
        {
            Assert.Null(await dialog.ShowAsync(owner, new CancellationToken(true)));
            Assert.False(dialog.IsOpen);
            Assert.False(await dialog.CloseAsync());
        }
        finally { owner.Close(); }
    });

    private sealed class AmountModel { public double? Amount { get; set; } }
    private sealed class TextModel { public string Text { get; set; } = ""; }
    private sealed class NonemptyRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) => string.IsNullOrWhiteSpace(value?.ToString()) ? new ValidationResult(false, "A name is required.") : ValidationResult.ValidResult;
    }
}
