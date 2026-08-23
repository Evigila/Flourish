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
                Key.Controls_GetsOrSetsTheCurrentPlaintextValueWithoutDataBinding_28BC0947
            ),
            new(
                "SecurePassword",
                Key.Controls_ReturnsTheCurrentValueAsAReadOnlySecureString_BCBBC3F6
            ),
            new("PasswordChar", Key.Controls_SelectsTheGlyphUsedToMaskEachCharacter_F3405291),
            new("MaxLength", Key.Controls_LimitsTheNumberOfAcceptedCharacters_F99BE746),
            new("PasswordChanged", Key.Controls_ReportsThatThePasswordValueChanged_3E6EEEE0),
            new("Clear", Key.Controls_RemovesTheCompleteCurrentPassword_9BF93B59),
            new("SelectAll", Key.Controls_SelectsTheCompleteValueInTheInternalEditor_C8301B0D),
            new(
                "FocusEditor",
                Key.Controls_MovesKeyboardFocusToTheInternalPasswordEditor_6DDD9733
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
