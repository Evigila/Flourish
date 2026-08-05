using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class PasswordBoxPage : Page
{
    public PasswordBoxPage()
    {
        InitializeComponent();
        MemberGrid.ItemsSource = new ControlMemberRow[]
        {
            new(
                "Password",
                GalleryLocaleKeys.ControlsGetsOrSetsTheCurrentPlaintextValueWithoutDataBinding_28BC0947
            ),
            new(
                "SecurePassword",
                GalleryLocaleKeys.ControlsReturnsTheCurrentValueAsAReadOnlySecureString_BCBBC3F6
            ),
            new(
                "PasswordChar",
                GalleryLocaleKeys.ControlsSelectsTheGlyphUsedToMaskEachCharacter_F3405291
            ),
            new(
                "MaxLength",
                GalleryLocaleKeys.ControlsLimitsTheNumberOfAcceptedCharacters_F99BE746
            ),
            new(
                "PasswordChanged",
                GalleryLocaleKeys.ControlsReportsThatThePasswordValueChanged_3E6EEEE0
            ),
            new("Clear", GalleryLocaleKeys.ControlsRemovesTheCompleteCurrentPassword_9BF93B59),
            new(
                "SelectAll",
                GalleryLocaleKeys.ControlsSelectsTheCompleteValueInTheInternalEditor_C8301B0D
            ),
            new(
                "FocusEditor",
                GalleryLocaleKeys.ControlsMovesKeyboardFocusToTheInternalPasswordEditor_6DDD9733
            ),
        };
    }

    public string UsageCode { get; } =
        """
            <flourish:FlourishPasswordBox
              x:Name="PasswordInput"
              MaxLength="64"
              PasswordChanged="PasswordInput_PasswordChanged" />

            private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
            {
                using var password = PasswordInput.SecurePassword;
                SignInCommand.Execute(password);
                PasswordInput.Clear();
            }
            """;
}
