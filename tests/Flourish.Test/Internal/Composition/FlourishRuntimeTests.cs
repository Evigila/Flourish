using ArkheideSystem.Flourish.Internal.Composition;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class FlourishRuntimeTests
{
    [Fact]
    public void Services_ReturnsHostServiceProvider()
    {
        var serviceProvider = Mock.Of<IServiceProvider>();
        var host = new Mock<IHost>();
        host.SetupGet(candidate => candidate.Services).Returns(serviceProvider);
        var sut = new FlourishRuntime(host.Object);

        var result = sut.Services;

        Assert.Same(serviceProvider, result);
    }

    [Fact]
    public void GetRequiredService_ReturnsServiceFromHostServiceProvider()
    {
        var expected = new TestService();
        using var services = new ServiceCollection().AddSingleton(expected).BuildServiceProvider();
        var host = new Mock<IHost>();
        host.SetupGet(candidate => candidate.Services).Returns(services);
        var sut = new FlourishRuntime(host.Object);

        var result = sut.GetRequiredService<TestService>();

        Assert.Same(expected, result);
    }

    [Fact]
    public void Start_WhenCalledMoreThanOnce_StartsHostOnce()
    {
        var host = CreateHost();
        var sut = new FlourishRuntime(host.Object);

        sut.Start();
        sut.Start();

        host.Verify(candidate => candidate.StartAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StopAsync_WhenRuntimeHasNotStarted_DoesNotStopHost()
    {
        var host = CreateHost();
        var sut = new FlourishRuntime(host.Object);

        await sut.StopAsync();

        host.Verify(
            candidate => candidate.StopAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task StartAfterStop_StartsHostAgainAndPropagatesCancellationToken()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        var host = CreateHost();
        var sut = new FlourishRuntime(host.Object);

        sut.Start();
        await sut.StopAsync(cancellationTokenSource.Token);
        sut.Start();

        host.Verify(
            candidate => candidate.StartAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
        host.Verify(
            candidate => candidate.StopAsync(cancellationTokenSource.Token),
            Times.Once
        );
    }

    [Fact]
    public async Task StopAsync_WhenHostThrows_AllowsRuntimeToStartAgain()
    {
        var expected = new InvalidOperationException("Host stop failed.");
        var host = CreateHost();
        host.Setup(candidate => candidate.StopAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromException(expected));
        var sut = new FlourishRuntime(host.Object);
        sut.Start();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.StopAsync());
        sut.Start();

        Assert.Same(expected, exception);
        host.Verify(
            candidate => candidate.StartAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
    }

    [Fact]
    public void Dispose_DisposesHost()
    {
        var host = CreateHost();
        var sut = new FlourishRuntime(host.Object);

        sut.Dispose();

        host.Verify(candidate => candidate.Dispose(), Times.Once);
    }

    private static Mock<IHost> CreateHost()
    {
        var host = new Mock<IHost>();
        host.Setup(candidate => candidate.StartAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        host.Setup(candidate => candidate.StopAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return host;
    }

    private sealed class TestService
    {
    }
}
