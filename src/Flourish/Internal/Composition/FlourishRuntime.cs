using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Themes;
using ArkheideSystem.Flourish.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Application = System.Windows.Application;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal sealed class FlourishRuntime(IHost host) : IFlourish
{
    private bool isStarted;

    public IServiceProvider Services => host.Services;

    public T GetRequiredService<T>()
        where T : notnull
    {
        return host.Services.GetRequiredService<T>();
    }

    public void Start()
    {
        if (isStarted)
        {
            return;
        }

        host.Start();
        isStarted = true;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!isStarted)
        {
            return;
        }

        try
        {
            await host.StopAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            isStarted = false;
        }
    }

    internal int Run(Application application)
    {
        ArgumentNullException.ThrowIfNull(application);

        Start();
        try
        {
            var mainWindow = PrepareShell(application);
            return application.Run(mainWindow);
        }
        finally
        {
            StopAsync().GetAwaiter().GetResult();
        }
    }

    public void Show(Application application)
    {
        ArgumentNullException.ThrowIfNull(application);

        var mainWindow = PrepareShell(application);
        mainWindow.Show();
    }

    public void Dispose()
    {
        host.Dispose();
    }

    private FlourishShellWindow PrepareShell(Application application)
    {
        EnsureApplicationResources(application);

        var mainWindow = host.Services.GetRequiredService<FlourishShellWindow>();
        application.MainWindow = mainWindow;
        return mainWindow;
    }

    private static void EnsureApplicationResources(Application application)
    {
        FlourishThemeResources.EnsureMerged(application.Resources);
    }
}
