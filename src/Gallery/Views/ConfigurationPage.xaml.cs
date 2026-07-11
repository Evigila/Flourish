using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Gallery.Views;

public partial class ConfigurationPage : Page
{
    private static FlourishLocaleRegistration? localeFileRegistration;

    private readonly ObservableCollection<string> availableLocales = [];
    private readonly IFlourishConfiguration configuration;
    private readonly IAppSettingsStore settings;
    private readonly IFlourishLocalization localization;
    public ConfigurationPage(
        IFlourishConfiguration configuration,
        IAppSettingsStore settings,
        IFlourishLocalization localization
    )
    {
        this.configuration = configuration;
        this.settings = settings;
        this.localization = localization;
        InitializeComponent();
        LocaleBox.ItemsSource = availableLocales;
        LocaleFilePathBox.Text = Path.Combine(AppContext.BaseDirectory, "lang_ES.json");

        Loaded += Page_Loaded;
        Unloaded += Page_Unloaded;
        RefreshLocaleState();
        RefreshSnapshotState();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        configuration.Changed -= Configuration_Changed;
        localization.Changed -= Localization_Changed;
        configuration.Changed += Configuration_Changed;
        localization.Changed += Localization_Changed;
        RefreshLocaleState();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        configuration.Changed -= Configuration_Changed;
        localization.Changed -= Localization_Changed;
    }

    private void ReadValue_Click(object sender, RoutedEventArgs e)
    {
        var key = ReadKeyBox.Text.Trim();
        ReadValueText.Text = string.IsNullOrWhiteSpace(key)
            ? "Enter a configuration path."
            : configuration[key] ?? "<null>";
    }

    private void Reload_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            configuration.Reload();
            RefreshSnapshotState();
        }
        catch (Exception error)
        {
            ReadValueText.Text = error.Message;
        }
    }

    private async void SetValue_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteSettingUpdateAsync(() =>
            settings.SetAsync(WriteKeyBox.Text, WriteValueBox.Text).AsTask()
        );
    }

    private async void AppendValue_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteSettingUpdateAsync(() =>
            settings.AppendAsync(WriteKeyBox.Text, WriteValueBox.Text).AsTask()
        );
    }

    private async void MergeValue_Click(object sender, RoutedEventArgs e)
    {
        var path = WriteKeyBox.Text.Trim();
        var separator = path.LastIndexOf(':');
        var parentPath = separator > 0 ? path[..separator] : path;
        var propertyName = separator > 0 ? path[(separator + 1)..] : "Value";

        await ExecuteSettingUpdateAsync(() =>
            settings
                .MergeAsync(
                    parentPath,
                    new Dictionary<string, object?>
                    {
                        [propertyName] = WriteValueBox.Text,
                        ["LastMergedAt"] = DateTimeOffset.Now,
                    }
                )
                .AsTask()
        );
    }

    private async void RemoveValue_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteSettingUpdateAsync(() => settings.RemoveAsync(WriteKeyBox.Text).AsTask());
    }

    private void ApplyLocale_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            localization.SetLocale(LocaleBox.Text);
            RefreshLocaleState();
        }
        catch (Exception error)
        {
            LocaleStatusText.Text = error.Message;
        }
    }

    private void RegisterLocaleFile_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (localeFileRegistration is not null)
            {
                localization.Unregister(localeFileRegistration);
            }

            localeFileRegistration = localization.RegisterFile(LocaleFilePathBox.Text);
            LocaleFilePathBox.Text = localeFileRegistration.FilePath;
            LocaleFileStatusText.Text =
                $"Registered {localeFileRegistration.Locale} from {localeFileRegistration.FilePath}.";
            RefreshLocaleState();
        }
        catch (Exception error)
        {
            LocaleFileStatusText.Text = error.Message;
        }
    }

    private void ReloadLocaleFile_Click(object sender, RoutedEventArgs e)
    {
        if (localeFileRegistration is null)
        {
            LocaleFileStatusText.Text = "Register a locale file first.";
            return;
        }

        try
        {
            localization.ReloadFile(localeFileRegistration);
            LocaleFileStatusText.Text =
                $"Reloaded {localeFileRegistration.Locale} at {DateTime.Now:T}.";
        }
        catch (Exception error)
        {
            LocaleFileStatusText.Text = error.Message;
        }
    }

    private void UnregisterLocaleFile_Click(object sender, RoutedEventArgs e)
    {
        if (localeFileRegistration is null)
        {
            LocaleFileStatusText.Text = "No locale file is registered by this page.";
            return;
        }

        var locale = localeFileRegistration.Locale;
        var removed = localization.Unregister(localeFileRegistration);
        localeFileRegistration = null;
        LocaleFileStatusText.Text = removed
            ? $"Unregistered locale source {locale}."
            : "That locale source was already unregistered.";
        RefreshLocaleState();
    }

    private async Task ExecuteSettingUpdateAsync(Func<Task<AppSettingsUpdateResult>> update)
    {
        try
        {
            var result = await update();
            WriteResultText.Text = result.Changed
                ? $"Saved {result.FilePath}. Configuration reloaded: {result.ConfigurationReloaded}."
                : "The transaction completed without changing the document.";
        }
        catch (Exception error)
        {
            WriteResultText.Text = error.Message;
        }
    }

    private void Configuration_Changed(object? sender, FlourishConfigurationChangedEventArgs e)
    {
        Dispatcher.BeginInvoke(() =>
        {
            if (!string.IsNullOrWhiteSpace(ReadKeyBox.Text))
            {
                ReadValueText.Text = configuration[ReadKeyBox.Text.Trim()] ?? "<null>";
            }

            RefreshSnapshotState();
        });
    }

    private void Localization_Changed(object? sender, FlourishLocalizationChangedEventArgs e)
    {
        Dispatcher.BeginInvoke(RefreshLocaleState);
    }

    private void RefreshLocaleState()
    {
        var locales = localization.AvailableLocales;
        if (!availableLocales.SequenceEqual(locales, StringComparer.OrdinalIgnoreCase))
        {
            availableLocales.Clear();
            foreach (var locale in locales)
            {
                availableLocales.Add(locale);
            }
        }

        LocaleBox.Text = localization.CurrentLocale;
        LocaleStatusText.Text = $"Current locale: {localization.CurrentLocale}";
    }

    private void RefreshSnapshotState()
    {
        var snapshot = configuration.Current;
        SnapshotText.Text =
            $"Snapshot v{snapshot.Version} contains {snapshot.Values.Count} values (captured {snapshot.CapturedAt.LocalDateTime:T}).";
    }
}
