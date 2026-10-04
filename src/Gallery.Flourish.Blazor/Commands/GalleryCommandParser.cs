using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Blazor.Abstract;
using Microsoft.AspNetCore.Components;

namespace ArkheideSystem.Gallery.Flourish.Blazor.Commands;

public sealed class GalleryCommandParser(NavigationManager navigation, IAppearanceService appearance) : ICommandParser
{
    public const string OpenFramework = "gallery.open-framework";
    public const string OpenControls = "gallery.open-controls";
    public const string OpenFoundations = "gallery.open-foundations";
    public const string OpenExamples = "gallery.open-examples";
    public const string ToggleTheme = "gallery.toggle-theme";

    public void RegisterCommands(ICommandRegistrar commands)
    {
        commands.Register(OpenFramework, () => navigation.NavigateTo("/framework"));
        commands.Register(OpenControls, () => navigation.NavigateTo("/controls"));
        commands.Register(OpenFoundations, () => navigation.NavigateTo("/foundations"));
        commands.Register(OpenExamples, () => navigation.NavigateTo("/examples"));
        commands.Register(ToggleTheme, () => appearance.SetTheme(
            appearance.Current.Theme == ApplicationTheme.Dark ? ApplicationTheme.Light : ApplicationTheme.Dark));
    }
}
