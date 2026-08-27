using System;
using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Appearance;
using ArkheideSystem.Flourish.Layout;
using ArkheideSystem.Flourish.Motion;
using ArkheideSystem.Flourish.ToolTips;
using ArkheideSystem.Flourish.Themes;
using ArkheideSystem.Flourish.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Application = System.Windows.Application;

namespace ArkheideSystem.Flourish.Hosting;

internal sealed class HostedApplicationRuntime(IHost host) : IApplicationRuntime
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

    private ShellWindow PrepareShell(Application application)
    {
        EnsureApplicationResources(application);
        host.Services.GetRequiredService<FontService>().Attach(application);
        host.Services.GetRequiredService<MotionService>().Attach(application);
        host.Services.GetRequiredService<ToolTipService>().Attach(application);

        var mainWindow = host.Services.GetRequiredService<ShellWindow>();
        application.MainWindow = mainWindow;
        return mainWindow;
    }

    private void EnsureApplicationResources(Application application)
    {
        ThemeResources.EnsureMerged(application.Resources);
        host.Services.GetRequiredService<ScrollService>().Attach(application);
    }
}
