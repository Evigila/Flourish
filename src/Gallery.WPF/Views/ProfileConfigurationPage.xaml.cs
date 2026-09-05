using System;
using System.Threading.Tasks;

using CKey = ArkheideSystem.Essential.Culture.Key;
using Localizer = ArkheideSystem.Essential.Culture.Localizer;
using ArkheideSystem.Flourish.Abstract;
using System.Windows;
using System.Windows.Controls;

namespace ArkheideSystem.Gallery.WPF.Views;

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
            var state = profile.Current;
            ProfileOutput.WriteLine(
                Localizer.Parse(
                    CKey.Dynamic_NameOrderUpdated0DisplayName1_E5C6CE57,
                    state.NameOrder,
                    state.Profile.DisplayName
                )
            );
        }
        catch (Exception error)
        {
            ProfileOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
        }
    }
}
