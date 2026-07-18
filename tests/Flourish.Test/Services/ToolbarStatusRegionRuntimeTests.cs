using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class ToolbarStatusRegionRuntimeTests
{
    [Fact]
    public void Toolbar_RuntimeMutationsUseStableIdsAndPublishAffectedPage()
    {
        var options = new FlourishShellOptions { IsDynamicToolbarEnabled = true };
        var sut = new FlourishToolbarService(options);
        FlourishToolbarChangedEventArgs? change = null;
        sut.Changed += (_, args) => change = args;
        var save = new FlourishToolbarItem("Save", "S", "save") { Id = "save" };

        sut.Add(save, typeof(EditorPage));
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
        var options = new FlourishShellOptions { IsDynamicToolbarEnabled = true };
        var item = new FlourishToolbarItem("Save", "S", "save") { Id = "save" };
        var sut = new FlourishToolbarService(options);
        var changes = 0;
        sut.Changed += (_, _) => changes++;

        sut.Replace<EditorPage>([item]);
        sut.Replace<EditorPage>([item]);
        sut.Upsert(item, typeof(EditorPage));

        Assert.Equal(1, changes);
        Assert.Equal(1, sut.Current.Version);
    }

    [Fact]
    public void Status_HandleOnlyRemovesTheRegistrationItOwns()
    {
        var options = new FlourishShellOptions { IsStatusBarEnabled = true };
        var sut = new FlourishStatusService(options);
        var oldHandle = sut.Show("sync", "Syncing", "S");
        var newHandle = sut.Show("sync", "Complete", "C");

        oldHandle.UpdateText("Stale");
        oldHandle.UpdateIcon("X");
        oldHandle.Dispose();

        Assert.Equal("Complete", Assert.Single(sut.Current.Items).Text);
        Assert.Equal("C", Assert.Single(sut.Current.Items).IconGlyph);
        newHandle.UpdateText("Done");
        Assert.Equal("Done", Assert.Single(sut.Current.Items).Text);
        newHandle.Dispose();
        Assert.Empty(sut.Current.Items);
    }

    [Fact]
    public void Region_UpsertMovesRegistrationAndStaleHandleCannotRemoveReplacement()
    {
        var options = new FlourishShellOptions();
        var sut = new ShellRegionService(options);
        var oldHandle = sut.Add(
            "runtime",
            FlourishRegion.ToolbarStart,
            _ => new Border()
        );
        var replacement = sut.Upsert(
            "runtime",
            FlourishRegion.FooterEnd,
            _ => new TextBlock(),
            order: 20
        );

        oldHandle.Dispose();

        var entry = Assert.Single(sut.Current.Entries);
        Assert.Equal(FlourishRegion.FooterEnd, entry.Region);
        Assert.Equal(20, entry.Order);
        Assert.Empty(sut.GetContents(FlourishRegion.ToolbarStart));
        Assert.Single(sut.GetContents(FlourishRegion.FooterEnd));

        replacement.Dispose();
        Assert.Empty(sut.Current.Entries);
    }

    private sealed class EditorPage : Page { }
}
