using System.Windows.Controls;
using ArkheideSystem.Gallery.Localization;

namespace ArkheideSystem.Gallery.Views;

public partial class ToolTipsConfigurationPage : Page
{
    public ToolTipsConfigurationPage(IGalleryLocalization galleryLocalization)
    {
        InitializeComponent();
        galleryLocalization.Apply(this);
    }
}
