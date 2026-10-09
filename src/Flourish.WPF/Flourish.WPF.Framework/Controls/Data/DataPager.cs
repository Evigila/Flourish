using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using ArkheideSystem.Flourish.WPF.Abstract;

namespace ArkheideSystem.Flourish.WPF.Controls;

public sealed class DataPager : UserControl
{
    private readonly Button previous = new() { Content = "‹", Variant = ButtonVariant.Quiet, Width = 40, MinWidth = 40, MinHeight = 40 };
    private readonly Button next = new() { Content = "›", Variant = ButtonVariant.Quiet, Width = 40, MinWidth = 40, MinHeight = 40 };
    private readonly TextBox input = new() { Width = 64, MinWidth = 64, Height = 38, MinHeight = 38, Padding = new Thickness(8, 0, 8, 0), FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center };
    private readonly TextBlock total = new() { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 12, 0) };
    private readonly TextBlock range = new() { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 16, 0) };
    private readonly DataText text;

    public DataPager()
    {
        text = new DataText(this, Refresh);
        var controls = new StackPanel { Orientation = Orientation.Horizontal };
        controls.Children.Add(previous); controls.Children.Add(next); controls.Children.Add(input); controls.Children.Add(total);
        var border = new Border { Child = controls, CornerRadius = new CornerRadius(12), Padding = new Thickness(3), BorderThickness = new Thickness(1) };
        border.SetResourceReference(Border.BackgroundProperty, "Flourish.Brush.Surface");
        border.SetResourceReference(Border.BorderBrushProperty, "Flourish.Brush.Border");
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        panel.Children.Add(range); panel.Children.Add(border); Content = panel;
        AutomationProperties.SetName(this, "Record pagination");
        AutomationProperties.SetName(previous, "Previous page"); AutomationProperties.SetName(next, "Next page");
        AutomationProperties.SetName(input, "Current page");
        previous.Click += (_, _) => GoToPage(CurrentPage - 1); next.Click += (_, _) => GoToPage(CurrentPage + 1);
        input.LostKeyboardFocus += (_, _) => Commit();
        input.KeyDown += (_, args) =>
        {
            if (args.Key == Key.Enter) { Commit(); args.Handled = true; }
            else if (args.Key == Key.Escape) { Refresh(); args.Handled = true; }
        };
        Refresh();
    }

    public static readonly DependencyProperty TotalCountProperty = DependencyProperty.Register(nameof(TotalCount), typeof(int), typeof(DataPager), new PropertyMetadata(0, Refresh), value => (int)value >= 0);
    public int TotalCount { get => (int)GetValue(TotalCountProperty); set => SetValue(TotalCountProperty, value); }
    public static readonly DependencyProperty CurrentPageProperty = DependencyProperty.Register(nameof(CurrentPage), typeof(int), typeof(DataPager), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, Refresh));
    public int CurrentPage { get => (int)GetValue(CurrentPageProperty); set => SetValue(CurrentPageProperty, value); }
    public static readonly DependencyProperty PageSizeProperty = DependencyProperty.Register(nameof(PageSize), typeof(int), typeof(DataPager), new PropertyMetadata(10, Refresh), value => (int)value > 0);
    public int PageSize { get => (int)GetValue(PageSizeProperty); set => SetValue(PageSizeProperty, value); }
    public static readonly DependencyProperty ShowRangeProperty = DependencyProperty.Register(nameof(ShowRange), typeof(bool), typeof(DataPager), new PropertyMetadata(true, Refresh));
    public bool ShowRange { get => (bool)GetValue(ShowRangeProperty); set => SetValue(ShowRangeProperty, value); }
    public static readonly DependencyProperty IsLimitedProperty = DependencyProperty.Register(nameof(IsLimited), typeof(bool), typeof(DataPager), new PropertyMetadata(false, Refresh));
    public bool IsLimited { get => (bool)GetValue(IsLimitedProperty); set => SetValue(IsLimitedProperty, value); }
    public int PageCount => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
    public event EventHandler<int>? PageChanged;
    public static readonly DependencyProperty TextProviderProperty = DependencyProperty.Register(nameof(TextProvider), typeof(ITextProvider), typeof(DataPager), new PropertyMetadata(null, (sender, args) => ((DataPager)sender).text.Provider = (ITextProvider?)args.NewValue));
    public ITextProvider? TextProvider { get => (ITextProvider?)GetValue(TextProviderProperty); set => SetValue(TextProviderProperty, value); }

    public void GoToPage(int page)
    {
        if (!IsEnabled) return;
        var effective = TableData.ClampPage(page, TotalCount, PageSize);
        if (CurrentPage == effective) { Refresh(); return; }
        SetCurrentValue(CurrentPageProperty, effective);
        PageChanged?.Invoke(this, effective);
    }
    private void Commit()
    {
        if (int.TryParse(input.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var page)) GoToPage(page);
        Refresh();
    }
    private static void Refresh(DependencyObject sender, DependencyPropertyChangedEventArgs args) => ((DataPager)sender).Refresh();
    private void Refresh()
    {
        var page = TableData.ClampPage(CurrentPage, TotalCount, PageSize);
        if (page != CurrentPage) SetCurrentValue(CurrentPageProperty, page);
        input.Text = page.ToString(CultureInfo.InvariantCulture);
        total.Text = $"/ {PageCount}";
        previous.IsEnabled = page > 1; next.IsEnabled = page < PageCount;
        var first = TotalCount == 0 ? 0 : (page - 1) * PageSize + 1;
        range.Text = $"{text.Get("Pager_Range", "Items {0}–{1} of", first, Math.Min(page * (long)PageSize, TotalCount))} {TotalCount}{(IsLimited ? " " + text.Get("Pager_Limited", "loaded") : string.Empty)}";
        range.Visibility = ShowRange ? Visibility.Visible : Visibility.Collapsed;
        AutomationProperties.SetName(this, text.Get("Pager_Label", "Record pagination"));
        AutomationProperties.SetName(previous, text.Get("Table_PreviousPage", "Previous page"));
        AutomationProperties.SetName(next, text.Get("Table_NextPage", "Next page"));
        AutomationProperties.SetName(input, text.Get("Pager_CurrentPage", "Current page"));
    }
}
