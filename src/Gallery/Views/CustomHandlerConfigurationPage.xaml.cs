using System.Windows.Controls;
using ArkheideSystem.Gallery.Localization;

namespace ArkheideSystem.Gallery.Views;

public partial class CustomHandlerConfigurationPage : Page
{
    public CustomHandlerConfigurationPage(IGalleryLocalization galleryLocalization)
    {
        InitializeComponent();
        galleryLocalization.Apply(this);
    }
}
