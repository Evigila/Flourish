using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Shell.Regions;
using ArkheideSystem.Flourish.Shell.StatusBar;
using ArkheideSystem.Flourish.Shell.Toolbar;

using System.Windows.Controls;

namespace ArkheideSystem.Tests.Flourish.WPF.Services;

public sealed class ToolbarStatusRegionRuntimeTests
{
    [Fact]
    public void Toolbar_RuntimeMutationsUseStableIdsAndPublishAffectedPage()
    {
        var options = new ToolbarOptions { IsDynamicToolbarEnabled = true };
        var sut = new ToolbarService(options);
        ToolbarChangedEventArgs? change = null;
        sut.Changed += (_, args) => change = args;
        var save = new ToolbarItem("Save", "S", "save") { Id = "save" };

        sut.AddItem(save, typeof(EditorPage));
        sut.SetItemEnabled("save", false, typeof(EditorPage));
        sut.SetItemVisible("save", false, typeof(EditorPage));

        var item = Assert.Single(sut.Current.Pages[typeof(EditorPage)].Items);
        Assert.False(item.IsEnabled);
        Assert.False(item.IsVisible);
        Assert.Equal(typeof(EditorPage), change?.PageType);
        Assert.Equal(3, sut.Current.Version);
    }

    [Fact]
    public void Toolbar_EquivalentReplaceAndUpsertDoNotPublish()
    {
        var options = new ToolbarOptions { IsDynamicToolbarEnabled = true };
        var item = new ToolbarItem("Save", "S", "save") { Id = "save" };
        var sut = new ToolbarService(options);
        var changes = 0;
        sut.Changed += (_, _) => changes++;

        sut.Set(typeof(EditorPage), [item]);
        sut.Set(typeof(EditorPage), [item]);
        sut.SetItem(item, typeof(EditorPage));

        Assert.Equal(1, changes);
        Assert.Equal(1, sut.Current.Version);
    }

    [Fact]
    public void Toolbar_WpfFacadeProjectsThePlatformNeutralState()
    {
        var sut = new ToolbarService(new ToolbarOptions { IsDynamicToolbarEnabled = true });
        var item = new ToolbarItem("Save", "S", "save") { Id = "save" };

        sut.Set(typeof(EditorPage), [item], iconOnly: false);

        ToolbarViewState view = Assert.Single(sut.State.Current.Views).Value;
        Assert.Equal([item], view.Items);
        Assert.False(view.IconOnly);
        Assert.Equal(sut.State.Current.Version, sut.Current.Version);
        Assert.Equal([item], sut.Current.Pages[typeof(EditorPage)].Items);
    }

    [Fact]
    public void Status_HandleOnlyRemovesTheRegistrationItOwns()
    {
        var options = new StatusBarOptions { IsStatusBarEnabled = true };
        var sut = new StatusBarService(options);
        var oldHandle = sut.Show("sync", "Syncing", "S");
        var newHandle = sut.Show("sync", "Complete", "C");

        oldHandle.SetText("Stale");
        oldHandle.SetIcon("X");
        oldHandle.Dispose();

        Assert.Equal("Complete", Assert.Single(sut.Current.Items).Text);
        Assert.Equal("C", Assert.Single(sut.Current.Items).IconGlyph);
        newHandle.SetText("Done");
        Assert.Equal("Done", Assert.Single(sut.Current.Items).Text);
        newHandle.Dispose();
        Assert.Empty(sut.Current.Items);
    }

    [Fact]
    public void Region_UpsertMovesRegistrationAndStaleHandleCannotRemoveReplacement()
    {
        var options = new ShellRegionOptions();
        var sut = new ShellRegionService(options);
        var oldHandle = sut.Add("runtime", ShellRegion.ToolbarStart, _ => new Border());
        var replacement = sut.Set(
            "runtime",
            ShellRegion.FooterEnd,
            _ => new TextBlock(),
            order: 20
        );

        oldHandle.Dispose();

        var entry = Assert.Single(sut.Current.Entries);
        Assert.Equal(ShellRegion.FooterEnd, entry.Region);
        Assert.Equal(20, entry.Order);
        Assert.Empty(sut.GetContents(ShellRegion.ToolbarStart));
        Assert.Single(sut.GetContents(ShellRegion.FooterEnd));

        replacement.Dispose();
        Assert.Empty(sut.Current.Entries);
    }

    private sealed class EditorPage : Page { }
}
