using Application = System.Windows.Application;

namespace AcksheedSys.Flourish.Abstract;

public interface IFlourish : IDisposable
{
    IServiceProvider Services { get; }

    T GetRequiredService<T>()
        where T : notnull;

    void Start();

    Task StopAsync(CancellationToken cancellationToken = default);

    void Show(Application application);
}
