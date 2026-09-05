using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Windowing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ArkheideSystem.Flourish.WPF.Test.Services;

public sealed class WindowCloseOptionSynchronizerTests
{
    [Fact]
    public async Task Lifecycle_SynchronizesCoreBehaviorWithWpfTrayOption()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var options = new WindowOptions();
        var closeService = new WindowCloseService(services);
        using var sut = new WindowCloseOptionSynchronizer(options, closeService);

        await sut.StartAsync(CancellationToken.None);
        closeService.SetBehavior(WindowCloseBehavior.MinimizeToTray);

        Assert.True(options.IsTrayExitEnabled);

        await sut.StopAsync(CancellationToken.None);
        closeService.SetBehavior(WindowCloseBehavior.Prompt);

        Assert.True(options.IsTrayExitEnabled);
    }
}
