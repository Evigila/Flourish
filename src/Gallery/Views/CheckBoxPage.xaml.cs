using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class CheckBoxPage : Page
{
    public CheckBoxPage()
    {
        InitializeComponent();
        MemberGrid.ItemsSource = new ControlMemberRow[]
        {
            new(
                "Variant",
                GalleryLocaleKeys.ControlsChoosesTheHorizontalOrVerticalFixedLayout_8EF172C1
            ),
            new(
                "Icon",
                GalleryLocaleKeys.ControlsSuppliesOptionalIconContentRenderedOnlyByTheVerticalLayout_064A3C53
            ),
            new(
                "IsChecked",
                GalleryLocaleKeys.ControlsGetsOrSetsTrueFalseOrNullWhenThreeStateBehaviorIsEnabled_1CE68B0F
            ),
            new(
                "IsThreeState",
                GalleryLocaleKeys.ControlsAllowsTheControlToEnterTheIndeterminateState_31967F58
            ),
            new("Content", GalleryLocaleKeys.ControlsSuppliesTheVisibleOptionLabel_2FF959F7),
            new("Checked", GalleryLocaleKeys.ControlsReportsATransitionToTheCheckedState_0D8A8B08),
            new(
                "Unchecked",
                GalleryLocaleKeys.ControlsReportsATransitionToTheUncheckedState_ABD97118
            ),
            new(
                "Indeterminate",
                GalleryLocaleKeys.ControlsReportsATransitionToTheNullState_C6DF7C32
            ),
        };
    }

    public string UsageCode { get; } =
        """
            <flourish:CheckBox
              Content="Enable notifications"
              IsChecked="{Binding NotificationsEnabled, Mode=TwoWay}"
              Checked="Notifications_Changed"
              Unchecked="Notifications_Changed" />

            private void Notifications_Changed(object sender, RoutedEventArgs e)
            {
                SavePreferences();
            }
            """;
}
