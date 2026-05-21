using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using WinRT;

namespace Vistara;

internal class Program
{
    private static IHost? host;

    public static T Fetch<T>()
        where T : class
    {
        return (host?.Services.GetRequiredService(typeof(T)) as T)
            ?? throw new Exception("Cannot find service of specified type");
    }

    [STAThread]
    static int Main(string[] args)
    {
        ComWrappersSupport.InitializeComWrappers();
        Microsoft.UI.Xaml.Application.Start(
            (p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(
                    DispatcherQueue.GetForCurrentThread()
                );
                SynchronizationContext.SetSynchronizationContext(context);

                ConfigureHost();

                var app = Fetch<App>();
            }
        );

        return 0;
    }

    private static void ConfigureHost()
    {
        host = Host.CreateDefaultBuilder()
            .ConfigureServices(
                (_, services) =>
                {
                    services.AddSingleton<App>();
                }
            )
            .Build();
    }
}
