using System.Windows.Controls;
using ArkheideSystem.Gallery.Localization;

namespace ArkheideSystem.Gallery.Views;

public partial class DynamicToolbarConfigurationPage : Page
{
    public DynamicToolbarConfigurationPage(IGalleryLocalization galleryLocalization)
    {
        InitializeComponent();
        galleryLocalization.Apply(this);
    }
}
