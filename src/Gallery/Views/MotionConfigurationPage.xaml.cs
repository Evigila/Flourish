using System.Windows.Controls;
using ArkheideSystem.Gallery.Localization;

namespace ArkheideSystem.Gallery.Views;

public partial class MotionConfigurationPage : Page
{
    public MotionConfigurationPage(IGalleryLocalization galleryLocalization)
    {
        InitializeComponent();
        galleryLocalization.Apply(this);
    }
}
