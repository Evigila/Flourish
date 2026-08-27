using CKey = ArkheideSystem.Essential.Culture.Key;
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
                CKey.Controls_ChoosesTheHorizontalOrVerticalFixedLayout_8EF172C1
            ),
            new(
                "Icon",
                CKey.Controls_SuppliesOptionalIconContentRenderedOnlyByTheVerticalLayout_064A3C53
            ),
            new(
                "IsChecked",
                CKey.Controls_GetsOrSetsTrueFalseOrNullWhenThreeStateBehaviorIsEnabled_1CE68B0F
            ),
            new(
                "IsThreeState",
                CKey.Controls_AllowsTheControlToEnterTheIndeterminateState_31967F58
            ),
            new("Content", CKey.Controls_SuppliesTheVisibleOptionLabel_2FF959F7),
            new("Checked", CKey.Controls_ReportsATransitionToTheCheckedState_0D8A8B08),
            new(
                "Unchecked",
                CKey.Controls_ReportsATransitionToTheUncheckedState_ABD97118
            ),
            new(
                "Indeterminate",
                CKey.Controls_ReportsATransitionToTheNullState_C6DF7C32
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
