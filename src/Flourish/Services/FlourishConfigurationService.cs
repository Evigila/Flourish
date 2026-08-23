using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace ArkheideSystem.Flourish.Services;

internal sealed class FlourishConfigurationService : IFlourishConfiguration, IDisposable
{
    private readonly IConfiguration configuration;
    private readonly Lock gate = new();
    private readonly IDisposable reloadSubscription;
    private FlourishConfigurationSnapshot current;
    private long version;
    private bool isDisposed;

    public FlourishConfigurationService(IConfiguration configuration)
    {
        this.configuration =
            configuration ?? throw new ArgumentNullException(nameof(configuration));
        current = CaptureSnapshot();
        reloadSubscription = ChangeToken.OnChange(
            configuration.GetReloadToken,
            Configuration_Reloaded
        );
    }

    public FlourishConfigurationSnapshot Current
    {
        get
        {
            lock (gate)
            {
                return current;
            }
        }
    }

    public string? this[string key]
    {
        get
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            return configuration[key];
        }
    }

    public event EventHandler<FlourishConfigurationChangedEventArgs>? Changed;

    public T? Get<T>(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return configuration.GetValue<T>(key);
    }

    public T? GetSection<T>(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return configuration.GetSection(path).Get<T>();
    }

    public void Reload()
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);
        if (configuration is not IConfigurationRoot configurationRoot)
        {
            throw new NotSupportedException(
                "The host configuration does not expose reloadable providers."
            );
        }

        configurationRoot.Reload();
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        isDisposed = true;
        reloadSubscription.Dispose();
    }

    private void Configuration_Reloaded()
    {
        FlourishConfigurationSnapshot previous;
        FlourishConfigurationSnapshot next;
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            previous = current;
            next = CaptureSnapshot();
            current = next;
        }

        var changedKeys = previous
            .Values.Keys.Concat(next.Values.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(key => !string.Equals(previous[key], next[key], StringComparison.Ordinal))
            .OrderBy(key => key, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        Changed?.Invoke(
            this,
            new FlourishConfigurationChangedEventArgs(previous, next, changedKeys)
        );
    }

    private FlourishConfigurationSnapshot CaptureSnapshot()
    {
        var values = configuration
            .AsEnumerable()
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Key))
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase);
        return new FlourishConfigurationSnapshot(
            Interlocked.Increment(ref version),
            DateTimeOffset.UtcNow,
            values
        );
    }
}
