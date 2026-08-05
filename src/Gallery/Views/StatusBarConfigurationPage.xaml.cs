using System.Windows.Controls;
using ArkheideSystem.Gallery.Localization;

namespace ArkheideSystem.Gallery.Views;

public partial class StatusBarConfigurationPage : Page
{
    public StatusBarConfigurationPage(IGalleryLocalization galleryLocalization)
    {
        InitializeComponent();
        galleryLocalization.Apply(this);
    }
}
