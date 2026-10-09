using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArkheideSystem.Flourish.WPF.Abstract;

namespace ArkheideSystem.Flourish.WPF.Controls;

public class ActionMenu : Button
{
    private ContextMenu? menu;
    private readonly System.Windows.Threading.DispatcherTimer hoverClose = new() { Interval = TimeSpan.FromMilliseconds(160) };
    private bool openedByHover;
    public ActionMenu()
    {
        Icon = "more_vert"; Variant = ButtonVariant.Quiet;
        MouseEnter += (_, _) => { hoverClose.Stop(); if (OpenOnHover && menu?.IsOpen != true) Open(true); };
        MouseLeave += (_, _) => { if (openedByHover) hoverClose.Start(); };
        hoverClose.Tick += (_, _) => { hoverClose.Stop(); if (openedByHover && !IsMouseOver && menu?.IsMouseOver != true && menu is not null) menu.IsOpen = false; };
        Unloaded += (_, _) => { hoverClose.Stop(); if (menu is not null) menu.IsOpen = false; };
        IsEnabledChanged += (_, _) => { if (!IsEnabled && menu is not null) menu.IsOpen = false; };
    }
    public static readonly DependencyProperty OpenOnHoverProperty = DependencyProperty.Register(nameof(OpenOnHover), typeof(bool), typeof(ActionMenu), new PropertyMetadata(false));
    public bool OpenOnHover { get => (bool)GetValue(OpenOnHoverProperty); set => SetValue(OpenOnHoverProperty, value); }
    public static readonly DependencyProperty ActionsProperty = DependencyProperty.Register(nameof(Actions), typeof(IEnumerable<MenuAction>), typeof(ActionMenu), new PropertyMetadata(null));
    public IEnumerable<MenuAction>? Actions { get => (IEnumerable<MenuAction>?)GetValue(ActionsProperty); set => SetValue(ActionsProperty, value); }
    public event EventHandler<Exception>? ActionFailed;
    protected override void OnClick() { base.OnClick(); if (IsEnabled) Open(); }
    public void Open() => Open(false);
    private void Open(bool hover)
    {
        if (!IsEnabled || Actions is null) return;
        if (menu?.IsOpen == true) { openedByHover = hover; return; }
        if (menu is not null) menu.IsOpen = false;
        var origin = Keyboard.FocusedElement;
        openedByHover = hover;
        menu = new ContextMenu { PlacementTarget = this };
        var opened = menu;
        opened.MouseEnter += (_, _) => hoverClose.Stop();
        opened.MouseLeave += (_, _) => { if (openedByHover) hoverClose.Start(); };
        opened.Closed += (_, _) =>
        {
            hoverClose.Stop();
            if (hover && origin is UIElement element && element.IsVisible && element.IsEnabled) Keyboard.Focus(origin);
            else if (IsVisible && IsEnabled) Focus();
        };
        FrameworkResources.ShareResources(this, menu);
        menu.SetResourceReference(StyleProperty, "Flourish.ContextMenu");
        foreach (var action in Actions)
        {
            var item = new MenuItem { Header = action.Text, IsEnabled = !action.Disabled };
            item.SetResourceReference(StyleProperty, "Flourish.MenuItem");
            if (action.Destructive) item.SetResourceReference(ForegroundProperty, "Flourish.Brush.Danger");
            item.Click += async (_, _) =>
            {
                opened.IsOpen = false;
                try { await action.OnClick(); }
                catch (Exception ex) { if (ActionFailed is null) throw; ActionFailed.Invoke(this, ex); }
            };
            menu.Items.Add(item);
        }
        menu.IsOpen = true;
    }
}

public class SplitButton : ContentControl
{
    static SplitButton() => DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitButton), new FrameworkPropertyMetadata(typeof(SplitButton)));
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(SplitButton), new PropertyMetadata(""));
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(nameof(Variant), typeof(ButtonVariant), typeof(SplitButton), new PropertyMetadata(ButtonVariant.Primary));
    public static readonly DependencyProperty PrimaryDisabledProperty = DependencyProperty.Register(nameof(PrimaryDisabled), typeof(bool), typeof(SplitButton), new PropertyMetadata(false));
    public static readonly DependencyProperty BusyProperty = DependencyProperty.Register(nameof(Busy), typeof(bool), typeof(SplitButton), new PropertyMetadata(false, StateChanged));
    public static readonly DependencyProperty DisabledProperty = DependencyProperty.Register(nameof(Disabled), typeof(bool), typeof(SplitButton), new PropertyMetadata(false, StateChanged));
    public static readonly DependencyProperty ActionsProperty = DependencyProperty.Register(nameof(Actions), typeof(IEnumerable<MenuAction>), typeof(SplitButton), new PropertyMetadata(null));
    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public ButtonVariant Variant { get => (ButtonVariant)GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
    public bool PrimaryDisabled { get => (bool)GetValue(PrimaryDisabledProperty); set => SetValue(PrimaryDisabledProperty, value); }
    public bool Busy { get => (bool)GetValue(BusyProperty); set => SetValue(BusyProperty, value); }
    public bool Disabled { get => (bool)GetValue(DisabledProperty); set => SetValue(DisabledProperty, value); }
    public IEnumerable<MenuAction>? Actions { get => (IEnumerable<MenuAction>?)GetValue(ActionsProperty); set => SetValue(ActionsProperty, value); }
    public event RoutedEventHandler? Click;
    private Button? primary;
    public override void OnApplyTemplate()
    {
        if (primary is not null) primary.Click -= PrimaryClick;
        base.OnApplyTemplate();
        primary = GetTemplateChild("PART_Primary") as Button;
        if (primary is not null) primary.Click += PrimaryClick;
    }
    private void PrimaryClick(object sender, RoutedEventArgs e) { if (!PrimaryDisabled && IsEnabled) Click?.Invoke(this, e); }
    protected override bool IsEnabledCore => base.IsEnabledCore && !Busy && !Disabled;
    private static void StateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => d.CoerceValue(IsEnabledProperty);
}

public class MultiSelectBox : Control
{
    static MultiSelectBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiSelectBox), new FrameworkPropertyMetadata(typeof(MultiSelectBox)));
    public MultiSelectBox()
    {
        Unloaded += (_, _) => { lifetime++; if (popup is not null) popup.IsOpen = false; };
        IsEnabledChanged += (_, _) => { if (!IsEnabled && !Creating && popup is not null) popup.IsOpen = false; };
    }
    public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(nameof(Options), typeof(IEnumerable<MultiSelectOption>), typeof(MultiSelectBox), new PropertyMetadata(null, Refresh));
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(MultiSelectBox), new PropertyMetadata("", Refresh));
    public static readonly DependencyProperty SearchableProperty = DependencyProperty.Register(nameof(Searchable), typeof(bool), typeof(MultiSelectBox), new PropertyMetadata(false, Refresh));
    public static readonly DependencyProperty NewItemMaximumLengthProperty = DependencyProperty.Register(nameof(NewItemMaximumLength), typeof(int), typeof(MultiSelectBox), new PropertyMetadata(100, Refresh), v => (int)v > 0);
    private static readonly DependencyPropertyKey EffectiveTextPropertyKey = DependencyProperty.RegisterReadOnly(nameof(EffectiveText), typeof(string), typeof(MultiSelectBox), new PropertyMetadata("None selected"));
    public static readonly DependencyProperty EffectiveTextProperty = EffectiveTextPropertyKey.DependencyProperty;
    private static readonly DependencyPropertyKey CreatingPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Creating), typeof(bool), typeof(MultiSelectBox), new PropertyMetadata(false, (d,e) => d.CoerceValue(IsEnabledProperty)));
    public static readonly DependencyProperty CreatingProperty = CreatingPropertyKey.DependencyProperty;
    public static readonly DependencyProperty ReorderEnabledProperty = DependencyProperty.Register(nameof(ReorderEnabled), typeof(bool), typeof(MultiSelectBox), new PropertyMetadata(false, Refresh));
    public static readonly DependencyProperty MinimumSelectedProperty = DependencyProperty.Register(nameof(MinimumSelected), typeof(int), typeof(MultiSelectBox), new PropertyMetadata(0, Refresh), v => (int)v >= 0);
    public static readonly DependencyProperty MaximumSelectedProperty = DependencyProperty.Register(nameof(MaximumSelected), typeof(int), typeof(MultiSelectBox), new PropertyMetadata(int.MaxValue, Refresh), v => (int)v >= 0);
    public IEnumerable<MultiSelectOption>? Options { get => (IEnumerable<MultiSelectOption>?)GetValue(OptionsProperty); set => SetValue(OptionsProperty, value); }
    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public string EffectiveText => (string)GetValue(EffectiveTextProperty);
    public bool Searchable { get => (bool)GetValue(SearchableProperty); set => SetValue(SearchableProperty, value); }
    public int NewItemMaximumLength { get => (int)GetValue(NewItemMaximumLengthProperty); set => SetValue(NewItemMaximumLengthProperty, value); }
    public bool Creating => (bool)GetValue(CreatingProperty);
    private Func<string, Task<bool>>? createRequested;
    public Func<string, Task<bool>>? CreateRequested { get => createRequested; set { createRequested = value; Render(); } }
    protected override bool IsEnabledCore => base.IsEnabledCore && !Creating;
    public bool ReorderEnabled { get => (bool)GetValue(ReorderEnabledProperty); set => SetValue(ReorderEnabledProperty, value); }
    public int MinimumSelected { get => (int)GetValue(MinimumSelectedProperty); set => SetValue(MinimumSelectedProperty, value); }
    public int MaximumSelected { get => (int)GetValue(MaximumSelectedProperty); set => SetValue(MaximumSelectedProperty, value); }
    public event EventHandler<MultiSelectChange>? Changed;
    private StackPanel? items;
    private SearchBox? search;
    private System.Windows.Controls.Primitives.Popup? popup;
    private long lifetime;
    public override void OnApplyTemplate()
    {
        if (search is not null) search.TextChanged -= SearchChanged;
        if (search is not null) search.PreviewKeyDown -= SearchKeyDown;
        base.OnApplyTemplate();
        items = GetTemplateChild("PART_Items") as StackPanel;
        search = GetTemplateChild("PART_Search") as SearchBox;
        popup = GetTemplateChild("PART_Popup") as System.Windows.Controls.Primitives.Popup;
        if (search is not null) search.TextChanged += SearchChanged;
        if (search is not null) search.PreviewKeyDown += SearchKeyDown;
        if (popup?.Child is FrameworkElement child) FrameworkResources.ShareResources(this, child);
        Render();
    }
    private static void Refresh(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((MultiSelectBox)d).Render();
    private void SearchChanged(object sender, TextChangedEventArgs e) => Render();
    private async void SearchKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter && CreateRequested is not null) { e.Handled = true; await RequestCreateAsync(); } }
    public async Task<bool> RequestCreateAsync(string? query = null)
    {
        Dispatcher.VerifyAccess();
        var label = (query ?? search?.Text ?? "").Trim();
        if (!IsEnabled || CreateRequested is not { } create || label.Length == 0 || label.Length > NewItemMaximumLength) return false;
        var values = Snapshot();
        if (values.Count(o => o.Selected) >= MaximumSelected || values.Any(o => string.Equals(o.Label.Trim(), label, StringComparison.OrdinalIgnoreCase))) return false;
        var current = lifetime;
        SetValue(CreatingPropertyKey, true);
        try
        {
            var accepted = await create(label);
            if (current != lifetime) return false;
            if (accepted && search is not null) search.SetCurrentValue(TextBox.TextProperty, "");
            return accepted;
        }
        finally { SetValue(CreatingPropertyKey, false); Render(); }
    }
    private MultiSelectOption[] Snapshot()
    {
        var values = Options?.ToArray() ?? [];
        if (values.Any(o => string.IsNullOrWhiteSpace(o.Key) || string.IsNullOrWhiteSpace(o.Label) || !o.CanDeselect && !o.Selected) || values.Select(o => o.Key).Distinct(StringComparer.Ordinal).Count() != values.Length)
            throw new ArgumentException("MultiSelectBox requires unique stable keys, readable labels and selected required members.");
        if (MinimumSelected > MaximumSelected || values.Count(o => o.Selected) < MinimumSelected || values.Count(o => o.Selected) > MaximumSelected)
            throw new ArgumentException("The current selection must satisfy its configured bounds.");
        return values;
    }
    public bool SetSelected(string key, bool selected)
    {
        var values = Snapshot(); var index = Array.FindIndex(values, o => o.Key == key);
        if (!IsEnabled || index < 0 || values[index].Disabled || !selected && !values[index].CanDeselect) return false;
        if (values[index].Selected == selected) return true;
        var count = values.Count(o => o.Selected) + (selected ? 1 : -1);
        if (count < MinimumSelected || count > MaximumSelected) return false;
        values[index] = values[index] with { Selected = selected }; Publish(values); return true;
    }
    public bool Move(string key, int offset)
    {
        var values = Snapshot(); var index = Array.FindIndex(values, o => o.Key == key); var target = index + offset;
        if (!IsEnabled || !ReorderEnabled || !string.IsNullOrEmpty(search?.Text) || index < 0 || target < 0 || target >= values.Length || offset == 0) return false;
        if (values.Skip(Math.Min(index, target)).Take(Math.Abs(offset) + 1).Any(o => o.Disabled || !o.CanReorder)) return false;
        var list = values.ToList(); var item = list[index]; list.RemoveAt(index); list.Insert(target, item); Publish(list.ToArray()); return true;
    }
    private void Publish(MultiSelectOption[] values)
    {
        SetCurrentValue(OptionsProperty, values);
        Changed?.Invoke(this, new MultiSelectChange(Array.AsReadOnly(values.Select(o => o.Key).ToArray()), values.Where(o => o.Selected).Select(o => o.Key).ToHashSet(StringComparer.Ordinal)));
    }
    private void Render()
    {
        var values = Options?.ToArray() ?? [];
        var selected = values.Where(o => o.Selected).ToArray();
        SetValue(EffectiveTextPropertyKey, !string.IsNullOrWhiteSpace(Text) ? Text : selected.Length switch { 0 => "None selected", 1 => selected[0].Label, _ => $"{selected.Length} items selected" });
        if (items is null) return;
        if (search is not null) { search.Visibility = Searchable || CreateRequested is not null ? Visibility.Visible : Visibility.Collapsed; search.MaxLength = NewItemMaximumLength; }
        items.Children.Clear();
        foreach (var option in Snapshot().Where(o => string.IsNullOrWhiteSpace(search?.Text) || o.Label.Contains(search.Text, StringComparison.CurrentCultureIgnoreCase)))
        {
            var row = new DockPanel { LastChildFill = true };
            if (ReorderEnabled)
            {
                foreach (var direction in new[] { -1, 1 })
                {
                    var move = new Button { Text = direction == -1 ? "↑" : "↓", Variant = ButtonVariant.Quiet, MinHeight = 36, Height = 36,
                        IsEnabled = option.CanReorder && !option.Disabled && string.IsNullOrEmpty(search?.Text), ToolTip = direction == -1 ? "Move up" : "Move down" };
                    move.Click += (_, _) => Move(option.Key, direction); DockPanel.SetDock(move, Dock.Right); row.Children.Add(move);
                }
            }
            var check = new CheckBox { Content = option.Label, IsChecked = option.Selected, IsEnabled = !option.Disabled && option.CanDeselect && (option.Selected ? selected.Length > MinimumSelected : selected.Length < MaximumSelected), Margin = new Thickness(4) };
            check.Click += (_, _) => { if (!SetSelected(option.Key, check.IsChecked == true)) check.IsChecked = option.Selected; };
            row.Children.Add(check); items.Children.Add(row);
        }
        var label = search?.Text.Trim() ?? "";
        if (CreateRequested is not null && label.Length > 0 && label.Length <= NewItemMaximumLength && selected.Length < MaximumSelected && !values.Any(o => string.Equals(o.Label.Trim(), label, StringComparison.OrdinalIgnoreCase)))
        {
            var add = new Button { Text = $"Add \"{label}\"", Variant = ButtonVariant.Quiet, Busy = Creating };
            add.Click += async (_, _) => await RequestCreateAsync(); items.Children.Add(add);
        }
    }
}
