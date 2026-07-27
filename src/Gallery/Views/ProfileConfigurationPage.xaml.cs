using System.Windows;
using System.Windows.Controls;

namespace ArkheideSystem.Gallery.Views;

public partial class ProfileConfigurationPage : Page
{
    private readonly IProfileService profile;

    public ProfileConfigurationPage(IProfileService profile)
    {
        this.profile = profile;
        InitializeComponent();
    }

    private async void FirstLast_Click(object sender, RoutedEventArgs e) =>
        await SetNameOrderAsync(NameOrder.FirstLast);

    private async void LastFirst_Click(object sender, RoutedEventArgs e) =>
        await SetNameOrderAsync(NameOrder.LastFirst);

    private async Task SetNameOrderAsync(NameOrder order)
    {
        try
        {
            await profile.SetNameOrderAsync(order);
            ProfileOutput.WriteLine(
                $"Name order updated: {profile.NameOrder}; display name {profile.CurrentProfile.DisplayName}."
            );
        }
        catch (Exception error)
        {
            ProfileOutput.WriteLine($"Error: {error.Message}");
        }
    }
}
