using System.Windows;

namespace ArkheideSystem.Flourish.Controls;

internal sealed class FlourishTitlebarFeaturePresenter(FrameworkElement contentHost)
{
    public bool IsEnabled => contentHost.Visibility == Visibility.Visible;

    public void SetEnabled(bool enabled)
    {
        contentHost.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
    }
}
