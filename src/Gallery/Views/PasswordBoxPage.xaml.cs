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
            new("Password", "Gets or sets the current plaintext value without data binding."),
            new("SecurePassword", "Returns the current value as a read-only SecureString."),
            new("PasswordChar", "Selects the glyph used to mask each character."),
            new("MaxLength", "Limits the number of accepted characters."),
            new("PasswordChanged", "Reports that the password value changed."),
            new("Clear", "Removes the complete current password."),
            new("SelectAll", "Selects the complete value in the internal editor."),
            new("FocusEditor", "Moves keyboard focus to the internal password editor."),
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
