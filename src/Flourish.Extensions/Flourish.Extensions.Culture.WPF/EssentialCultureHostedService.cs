using System;
using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Essential.Culture;
using ArkheideSystem.Flourish.Abstract;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.Flourish.Extensions.Culture.WPF;

/// <summary>Owns the lifetime of the Flourish-to-Essential-Culture connection.</summary>
internal sealed class EssentialCultureHostedService(
    ILocalizationService localization,
    ShellCultureApplicator shellApplicator
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
        SynchronizeCulture(localization.Current.Locale);
        localization.Changed += Localization_Changed;
        try
        {
            var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
            shellApplicator.Start(dispatcher);
            isStarted = true;
        }
        catch
        {
            localization.Changed -= Localization_Changed;
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
        localization.Changed -= Localization_Changed;
        shellApplicator.Stop();
        return Task.CompletedTask;
    }

    private void Localization_Changed(
        object? sender,
        LocalizationChangedEventArgs e
    )
    {
        if (e.Kind == LocalizationChangeKind.LocaleChanged)
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
