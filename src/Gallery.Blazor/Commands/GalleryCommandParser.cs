using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Blazor.Abstract;
using Microsoft.AspNetCore.Components;

namespace ArkheideSystem.Gallery.Blazor.Commands;

public sealed class GalleryCommandParser(NavigationManager navigation, IAppearanceService appearance) : ICommandParser
{
    public const string OpenRecords = "gallery.open-records";
    public const string OpenControls = "gallery.open-controls";
    public const string OpenAppearance = "gallery.open-appearance";
    public const string ToggleTheme = "gallery.toggle-theme";

    public void RegisterCommands(ICommandRegistrar commands)
    {
        commands.Register(OpenRecords, () => navigation.NavigateTo("/records"));
        commands.Register(OpenControls, () => navigation.NavigateTo("/controls"));
        commands.Register(OpenAppearance, () => navigation.NavigateTo("/appearance"));
        commands.Register(ToggleTheme, () => appearance.SetTheme(
            appearance.Current.Theme == ApplicationTheme.Dark ? ApplicationTheme.Light : ApplicationTheme.Dark));
    }
}
