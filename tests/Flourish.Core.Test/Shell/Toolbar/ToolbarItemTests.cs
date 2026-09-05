using ArkheideSystem.Flourish.Abstract;

using Xunit;

namespace ArkheideSystem.Flourish.Core.Test.Shell.Toolbar;

public sealed class ToolbarItemTests
{
    [Theory]
    [InlineData("Save", null, "toolbar:Save")]
    [InlineData("Save", "cmd_save", "toolbar:cmd_save")]
    [InlineData(" ", null, "toolbar:item")]
    [InlineData(" Save ", " ", "toolbar:Save")]
    public void Constructor_DerivesStableIdFromCommandThenDisplayName(
        string displayName,
        string? commandKey,
        string expectedId
    )
    {
        var item = new ToolbarItem(displayName, "S", commandKey);

        Assert.Equal(expectedId, item.Id);
        Assert.Equal(displayName, item.DisplayName);
        Assert.Equal("S", item.IconGlyph);
        Assert.Equal(commandKey, item.CommandKey);
        Assert.True(item.IsVisible);
        Assert.True(item.IsEnabled);
    }
}
