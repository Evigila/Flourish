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
                Key.Controls_ChoosesTheHorizontalOrVerticalFixedLayout_8EF172C1
            ),
            new(
                "Icon",
                Key.Controls_SuppliesOptionalIconContentRenderedOnlyByTheVerticalLayout_064A3C53
            ),
            new(
                "IsChecked",
                Key.Controls_GetsOrSetsTrueFalseOrNullWhenThreeStateBehaviorIsEnabled_1CE68B0F
            ),
            new(
                "IsThreeState",
                Key.Controls_AllowsTheControlToEnterTheIndeterminateState_31967F58
            ),
            new("Content", Key.Controls_SuppliesTheVisibleOptionLabel_2FF959F7),
            new("Checked", Key.Controls_ReportsATransitionToTheCheckedState_0D8A8B08),
            new(
                "Unchecked",
                Key.Controls_ReportsATransitionToTheUncheckedState_ABD97118
            ),
            new(
                "Indeterminate",
                Key.Controls_ReportsATransitionToTheNullState_C6DF7C32
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
