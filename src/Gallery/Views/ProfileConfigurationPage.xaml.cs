using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Gallery.Localization;

namespace ArkheideSystem.Gallery.Views;

public partial class ProfileConfigurationPage : Page
{
    private readonly IProfileService profile;
    private readonly IGalleryLocalization localization;

    public ProfileConfigurationPage(
        IProfileService profile,
        IGalleryLocalization localization
    )
    {
        this.profile = profile;
        this.localization = localization;
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
                localization.Format(
                    "Name order updated: {0}; display name {1}.",
                    profile.NameOrder,
                    profile.CurrentProfile.DisplayName
                )
            );
        }
        catch (Exception error)
        {
            ProfileOutput.WriteLine(localization.Format("Error: {0}", error.Message));
        }
    }
}
