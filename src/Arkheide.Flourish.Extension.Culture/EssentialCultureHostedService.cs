using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.Flourish.Extension.Culture;

/// <summary>Owns the lifetime of the Flourish-to-Essential-Culture connection.</summary>
internal sealed class EssentialCultureHostedService(
    IFlourishLocalization flourishLocalization,
    FlourishShellCultureApplicator shellApplicator
) : IHostedService
{
    private bool isStarted;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (isStarted)
        {
            return Task.CompletedTask;
        }

        // Flourish owns the public culture selection. Initialize Essential Culture before the
        // shell is created so the first rendered frame never exposes stable tokens.
        SynchronizeCulture(flourishLocalization.CurrentLocale);
        flourishLocalization.Changed += FlourishLocalization_Changed;
        try
        {
            var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
            shellApplicator.Start(dispatcher);
            isStarted = true;
        }
        catch
        {
            flourishLocalization.Changed -= FlourishLocalization_Changed;
            shellApplicator.Stop();
            throw;
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (!isStarted)
        {
            return Task.CompletedTask;
        }

        isStarted = false;
        flourishLocalization.Changed -= FlourishLocalization_Changed;
        shellApplicator.Stop();
        return Task.CompletedTask;
    }

    private void FlourishLocalization_Changed(
        object? sender,
        FlourishLocalizationChangedEventArgs e
    )
    {
        if (e.Kind == FlourishLocalizationChangeKind.LocaleChanged)
        {
            SynchronizeCulture(e.CurrentLocale);
        }
    }

    private static void SynchronizeCulture(string culture)
    {
        if (!string.Equals(Localizer.Current.Culture, culture, StringComparison.OrdinalIgnoreCase))
        {
            Localizer.Current.SetCulture(culture);
        }
    }
}
