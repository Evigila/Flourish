using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using ArkheideSystem.Flourish.WPF.Abstract;

namespace ArkheideSystem.Flourish.WPF.Controls;

public class TextBox : System.Windows.Controls.TextBox
{
    static TextBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBox), new FrameworkPropertyMetadata(typeof(TextBox)));
    public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(TextBox), new PropertyMetadata(""));
    public string Placeholder { get => (string)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }
}

public class NumberBox : TextBox
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double?), typeof(NumberBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ValueChanged));
    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(NumberBox), new PropertyMetadata(double.NegativeInfinity));
    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(NumberBox), new PropertyMetadata(double.PositiveInfinity));
    public double? Value { get => (double?)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public double Minimum { get => (double)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
    public double Maximum { get => (double)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
    public CultureInfo FormatCulture { get; set; } = CultureInfo.CurrentCulture;
    public bool Commit()
    {
        if (string.IsNullOrWhiteSpace(Text)) { SetCurrentValue(ValueProperty, null); return true; }
        if (!double.TryParse(Text, NumberStyles.Float | NumberStyles.AllowThousands, FormatCulture, out var value) || !double.IsFinite(value) || value < Minimum || value > Maximum) return false;
        SetCurrentValue(ValueProperty, value);
        return true;
    }
    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e) { Commit(); base.OnLostKeyboardFocus(e); }
    protected override void OnKeyDown(KeyEventArgs e) { if (e.Key == Key.Enter) { e.Handled = Commit(); } base.OnKeyDown(e); }
    private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var input = (NumberBox)d;
        input.SetCurrentValue(TextProperty, ((double?)e.NewValue)?.ToString(input.FormatCulture) ?? "");
    }
}
public class DateBox : DatePicker
{
    static DateBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(DateBox), new FrameworkPropertyMetadata(typeof(DateBox)));
}
public class CheckBox : System.Windows.Controls.CheckBox
{
    static CheckBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckBox), new FrameworkPropertyMetadata(typeof(CheckBox)));
}
public class ToggleSwitch : CheckBox
{
    static ToggleSwitch() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSwitch), new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
}
public class SelectBox : ComboBox
{
    static SelectBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectBox), new FrameworkPropertyMetadata(typeof(SelectBox)));
    public SelectBox() { DisplayMemberPath = nameof(SelectOption.Text); SelectedValuePath = nameof(SelectOption.Value); }
    public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(nameof(Options), typeof(IEnumerable<SelectOption>), typeof(SelectBox), new PropertyMetadata(null, OptionsChanged));
    public IEnumerable<SelectOption>? Options { get => (IEnumerable<SelectOption>?)GetValue(OptionsProperty); set => SetValue(OptionsProperty, value); }
    private static void OptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((SelectBox)d).SetCurrentValue(ItemsSourceProperty, e.NewValue);
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (GetTemplateChild("PART_Popup") is System.Windows.Controls.Primitives.Popup { Child: FrameworkElement child }) FrameworkResources.ShareResources(this, child);
    }
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is ComboBoxItem container && item is SelectOption option) container.IsEnabled = !option.Disabled;
    }
}
public class ReferenceDropdown : SelectBox
{
    public ReferenceDropdown() { DisplayMemberPath = nameof(ReferenceItem.Name); SelectedValuePath = nameof(ReferenceItem.Id); IsEditable = true; IsTextSearchEnabled = true; }
    public static readonly DependencyProperty ReferencesProperty = DependencyProperty.Register(nameof(References), typeof(IEnumerable<ReferenceItem>), typeof(ReferenceDropdown), new PropertyMetadata(null, (d,e) => ((ReferenceDropdown)d).SetCurrentValue(ItemsSourceProperty, e.NewValue)));
    public IEnumerable<ReferenceItem>? References { get => (IEnumerable<ReferenceItem>?)GetValue(ReferencesProperty); set => SetValue(ReferencesProperty, value); }
    public event EventHandler<string>? CreateRequested;
    public bool RequestCreate()
    {
        if (!IsEnabled || string.IsNullOrWhiteSpace(Text) || References?.Any(item => string.Equals(item.Name, Text, StringComparison.CurrentCultureIgnoreCase)) == true || CreateRequested is null) return false;
        CreateRequested.Invoke(this, Text.Trim()); return true;
    }
    protected override void OnKeyDown(KeyEventArgs e) { if (e.Key == Key.Enter && RequestCreate()) e.Handled = true; else base.OnKeyDown(e); }
}

/// <summary>Native text editing with cancellable delayed query notifications, including immediate clear.</summary>
public class SearchBox : TextBox
{
    static SearchBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchBox), new FrameworkPropertyMetadata(typeof(SearchBox)));
    private readonly DispatcherTimer timer = new();
    public SearchBox()
    {
        MaxLength = 200;
        timer.Tick += (_, _) => { timer.Stop(); SearchRequested?.Invoke(this, Text); };
        Unloaded += (_, _) => timer.Stop();
        IsEnabledChanged += (_, _) => { if (!IsEnabled) timer.Stop(); };
    }
    public static readonly DependencyProperty DebounceMillisecondsProperty = DependencyProperty.Register(nameof(DebounceMilliseconds), typeof(int), typeof(SearchBox), new PropertyMetadata(400), value => (int)value >= 0);
    public int DebounceMilliseconds { get => (int)GetValue(DebounceMillisecondsProperty); set => SetValue(DebounceMillisecondsProperty, value); }
    public event EventHandler<string>? SearchRequested;
    private Button? clear;
    public override void OnApplyTemplate() { if (clear is not null) clear.Click -= Clear; base.OnApplyTemplate(); clear = GetTemplateChild("PART_Clear") as Button; if (clear is not null) clear.Click += Clear; }
    private void Clear(object sender, RoutedEventArgs e) { SetCurrentValue(TextProperty, ""); Focus(); }
    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);
        timer.Stop();
        if (!IsEnabled) return;
        if (Text.Length == 0 || DebounceMilliseconds == 0) SearchRequested?.Invoke(this, Text);
        else { timer.Interval = TimeSpan.FromMilliseconds(DebounceMilliseconds); timer.Start(); }
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape) { SetCurrentValue(TextProperty, ""); e.Handled = true; }
        else if (e.Key == Key.Enter) { timer.Stop(); SearchRequested?.Invoke(this, Text); e.Handled = true; }
        base.OnKeyDown(e);
    }
}

public class SearchAutocomplete : SelectBox
{
    public SearchAutocomplete()
    {
        IsEditable = true; IsTextSearchEnabled = false; StaysOpenOnEdit = true;
        Unloaded += (_, _) => Cancel();
        IsEnabledChanged += (_, _) => { if (!IsEnabled) { Cancel(); SetCurrentValue(IsDropDownOpenProperty, false); } };
    }
    public Func<string, CancellationToken, Task<IEnumerable<SelectOption>>>? Search { get; set; }
    public IEnumerable<SelectOption> Suggestions { get; set; } = [];
    public int MaximumLength { get; set; } = 200;
    public int MaximumResults { get; set; } = 8;
    public event EventHandler<Exception>? SearchFailed;
    private CancellationTokenSource? pending;
    private System.Windows.Controls.TextBox? editor;
    private long revision;
    private bool updating;
    public override void OnApplyTemplate()
    {
        if (editor is not null) editor.TextChanged -= InputChanged;
        base.OnApplyTemplate();
        editor = GetTemplateChild("PART_EditableTextBox") as System.Windows.Controls.TextBox;
        if (editor is not null) { editor.MaxLength = MaximumLength; editor.TextChanged += InputChanged; }
    }
    private async void InputChanged(object sender, TextChangedEventArgs e)
    {
        if (updating || !IsKeyboardFocusWithin) return;
        try { await SearchAsync(editor?.Text ?? Text); }
        catch (Exception ex) { if (SearchFailed is null) throw; SearchFailed.Invoke(this, ex); }
    }
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape) { Cancel(); SetCurrentValue(IsDropDownOpenProperty, false); e.Handled = true; }
        base.OnPreviewKeyDown(e);
    }
    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnLostKeyboardFocus(e);
        // Focus may move into the same native suggestion popup; evaluate after WPF finishes that transfer.
        Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
        {
            if (!IsKeyboardFocusWithin) { Cancel(); SetCurrentValue(IsDropDownOpenProperty, false); }
        });
    }
    public async Task SearchAsync(string value)
    {
        Dispatcher.VerifyAccess();
        if (MaximumLength < 1 || MaximumResults < 1) throw new InvalidOperationException("Search limits must be positive.");
        Cancel();
        if (!IsEnabled) return;
        var request = revision;
        var current = pending = new CancellationTokenSource();
        var token = current.Token;
        var query = value.Length > MaximumLength ? value[..MaximumLength] : value;
        try
        {
            var result = Search is { } search ? await search(query, token) : Suggestions.Where(o => o.Text.Contains(query, StringComparison.OrdinalIgnoreCase));
            if (token.IsCancellationRequested || request != revision) return;
            updating = true;
            try { Options = result.Take(MaximumResults).ToArray(); if (IsLoaded) SetCurrentValue(IsDropDownOpenProperty, true); }
            finally { updating = false; }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        finally { if (ReferenceEquals(current, pending)) pending = null; current.Dispose(); }
    }
    protected override void OnVisualParentChanged(DependencyObject oldParent)
    {
        base.OnVisualParentChanged(oldParent);
        if (VisualParent is null) Cancel();
    }
    private void Cancel() { revision++; pending?.Cancel(); pending = null; }
}
public class MaskedInput : TextBox
{
    public static readonly DependencyProperty MaskProperty = DependencyProperty.Register(nameof(Mask), typeof(string), typeof(MaskedInput), new PropertyMetadata("", MaskChanged));
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(string), typeof(MaskedInput), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ValueChanged));
    public string Mask { get => (string)GetValue(MaskProperty); set => SetValue(MaskProperty, value); }
    /// <summary>Unformatted value; native Text displays the same mask formatter as Blazor.</summary>
    public string Value { get => (string)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public bool IsComplete => Mask.Length == 0 || Value.Length == Mask.Count(InputMaskFormatter.IsSlot);
    private bool formatting;
    private static void MaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var input = (MaskedInput)d;
        if (input.Placeholder.Length == 0 || input.Placeholder == (string)e.OldValue) input.SetCurrentValue(PlaceholderProperty, e.NewValue);
        input.FormatText(input.Value);
    }
    private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var input = (MaskedInput)d;
        if (!input.formatting) input.FormatText((string?)e.NewValue ?? "");
    }
    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);
        if (!formatting) FormatText(Text);
    }
    private void FormatText(string value)
    {
        formatting = true;
        try
        {
            var caret = SelectionStart;
            var normalized = Mask.Length == 0 ? value : InputMaskFormatter.Normalize(Mask, value);
            var display = Mask.Length == 0 ? value : InputMaskFormatter.Format(Mask, normalized);
            SetCurrentValue(ValueProperty, normalized);
            if (Text != display) { SetCurrentValue(TextProperty, display); Select(Math.Min(caret, display.Length), 0); }
        }
        finally { formatting = false; }
    }
}
