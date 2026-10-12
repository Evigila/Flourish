using System.Windows;
using System.Windows.Controls.Primitives;
using ArkheideSystem.Flourish.WPF.Abstract;

namespace ArkheideSystem.Flourish.WPF.Controls;

/// <summary>Guards the real toggle operation, including keyboard and automation invocation.</summary>
internal sealed class PopupTrigger : ToggleButton
{
    public PopupTrigger() { }
    public static readonly DependencyProperty StrengthProperty = DependencyProperty.Register(nameof(Strength), typeof(PopupStrength), typeof(PopupTrigger), new PropertyMetadata(PopupStrength.Weak), value => Enum.IsDefined((PopupStrength)value));
    public PopupStrength Strength { get => (PopupStrength)GetValue(StrengthProperty); set => SetValue(StrengthProperty, value); }
    protected override void OnToggle()
    {
        if (Strength != PopupStrength.Strong || IsChecked != true) base.OnToggle();
    }
}
