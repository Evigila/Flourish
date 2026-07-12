using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class FlourishStatusServiceTests
{
    [Fact]
    public void Properties_ReturnCurrentStatusOptions()
    {
        var options = new FlourishShellOptions
        {
            IsLANConnectionStatusEnabled = true,
            IsPowerStatusEnabled = false,
        };
        options.StatusItems.Add(new FlourishStatusItem("Offline", "O"));
        var sut = new FlourishStatusService(options);

        Assert.Same(options.StatusItems, sut.StatusItems);
        Assert.True(sut.IsLANConnectionStatusEnabled);
        Assert.False(sut.IsPowerStatusEnabled);

        options.IsPowerStatusEnabled = true;
        options.StatusItems.Add(new FlourishStatusItem("Online", "N"));

        Assert.True(sut.IsPowerStatusEnabled);
        Assert.Equal(2, sut.StatusItems.Count);
        Assert.Equal("Online", sut.StatusItems[1].Text);
    }
}
