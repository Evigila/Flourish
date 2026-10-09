using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using ArkheideSystem.Flourish.WPF.Abstract;

namespace ArkheideSystem.Flourish.WPF.Controls;

public class Button : System.Windows.Controls.Button
{
    static Button() => DefaultStyleKeyProperty.OverrideMetadata(typeof(Button), new FrameworkPropertyMetadata(typeof(Button)));
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(nameof(Variant), typeof(ButtonVariant), typeof(Button), new PropertyMetadata(ButtonVariant.Primary));
    public static readonly DependencyProperty BusyProperty = DependencyProperty.Register(nameof(Busy), typeof(bool), typeof(Button), new PropertyMetadata(false, StateChanged));
    public static readonly DependencyProperty DisabledProperty = DependencyProperty.Register(nameof(Disabled), typeof(bool), typeof(Button), new PropertyMetadata(false, StateChanged));
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(Button), new PropertyMetadata(""));
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(nameof(Description), typeof(string), typeof(Button), new PropertyMetadata(""));
    public static readonly DependencyProperty TrailingTextProperty = DependencyProperty.Register(nameof(TrailingText), typeof(string), typeof(Button), new PropertyMetadata(""));
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(string), typeof(Button), new PropertyMetadata(""));
    public static readonly DependencyProperty BusyLabelProperty = DependencyProperty.Register(nameof(BusyLabel), typeof(string), typeof(Button), new PropertyMetadata("Working…"));
    public ButtonVariant Variant { get => (ButtonVariant)GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
    public bool Busy { get => (bool)GetValue(BusyProperty); set => SetValue(BusyProperty, value); }
    public bool Disabled { get => (bool)GetValue(DisabledProperty); set => SetValue(DisabledProperty, value); }
    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public string Description { get => (string)GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }
    public string TrailingText { get => (string)GetValue(TrailingTextProperty); set => SetValue(TrailingTextProperty, value); }
    public string Icon { get => (string)GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public string BusyLabel { get => (string)GetValue(BusyLabelProperty); set => SetValue(BusyLabelProperty, value); }
    protected override bool IsEnabledCore => base.IsEnabledCore && !Busy && !Disabled;
    private static void StateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => d.CoerceValue(IsEnabledProperty);
    public override void OnApplyTemplate()
    {
        ValidateContent();
        base.OnApplyTemplate();
    }
    private void ValidateContent()
    {
        if ((!string.IsNullOrEmpty(Description) || !string.IsNullOrEmpty(TrailingText)) && (string.IsNullOrWhiteSpace(Text) || Content is not null))
            throw new InvalidOperationException("Structured Button requires Text and cannot combine Content with Description/TrailingText.");
    }
    protected override void OnClick()
    {
        if (Busy || Disabled || !IsEnabled) return;
        ValidateContent();
        base.OnClick();
    }
    protected override AutomationPeer OnCreateAutomationPeer() => new ActionPeer(this);
    private sealed class ActionPeer(Button owner) : ButtonAutomationPeer(owner)
    {
        protected override string GetNameCore() => string.Join(" ", new[] { base.GetNameCore(), owner.Text, owner.Description, owner.TrailingText }.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct());
        protected override string GetItemStatusCore() => owner.Busy ? owner.BusyLabel : base.GetItemStatusCore();
    }
}

public class UniformGridButton : Button
{
    static UniformGridButton() => DefaultStyleKeyProperty.OverrideMetadata(typeof(UniformGridButton), new FrameworkPropertyMetadata(typeof(UniformGridButton)));
    public static readonly DependencyProperty GridVariantProperty = DependencyProperty.Register(nameof(GridVariant), typeof(UniformGridVariant), typeof(UniformGridButton), new PropertyMetadata(UniformGridVariant.Elevated));
    public UniformGridVariant GridVariant { get => (UniformGridVariant)GetValue(GridVariantProperty); set => SetValue(GridVariantProperty, value); }
}

public class PrimaryNavigationItem : Button
{
    static PrimaryNavigationItem() => DefaultStyleKeyProperty.OverrideMetadata(typeof(PrimaryNavigationItem), new FrameworkPropertyMetadata(typeof(PrimaryNavigationItem)));
    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(PrimaryNavigationItem), new PropertyMetadata(false));
    public bool IsSelected { get => (bool)GetValue(IsSelectedProperty); set => SetValue(IsSelectedProperty, value); }
}
public class SecondaryNavigationItem : Button
{
    static SecondaryNavigationItem() => DefaultStyleKeyProperty.OverrideMetadata(typeof(SecondaryNavigationItem), new FrameworkPropertyMetadata(typeof(SecondaryNavigationItem)));
    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(SecondaryNavigationItem), new PropertyMetadata(false));
    public bool IsSelected { get => (bool)GetValue(IsSelectedProperty); set => SetValue(IsSelectedProperty, value); }
}
