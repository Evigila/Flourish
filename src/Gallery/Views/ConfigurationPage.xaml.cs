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
    private bool isRefreshingLocale;
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
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        localization.Changed -= Localization_Changed;
        localization.Changed += Localization_Changed;
        RefreshLocaleState();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        localization.Changed -= Localization_Changed;
    }

    private void ReadValue_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var key = ReadKeyBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                ReadOutput.WriteLine("Enter a configuration path.");
                return;
            }

            ReadOutput.WriteLine($"Read {key}: {configuration[key] ?? "<null>"}");
        }
        catch (Exception error)
        {
            ReadOutput.WriteLine($"Error: {error.Message}");
        }
    }

    private void Reload_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            configuration.Reload();
            var snapshot = configuration.Current;
            ReadOutput.WriteLine(
                $"Reloaded configuration providers. Snapshot v{snapshot.Version} contains {snapshot.Values.Count} values (captured {snapshot.CapturedAt.LocalDateTime:T})."
            );
        }
        catch (Exception error)
        {
            ReadOutput.WriteLine($"Error: {error.Message}");
        }
    }

    private async void SetValue_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteSettingUpdateAsync("Set", () =>
            settings.SetAsync(WriteKeyBox.Text, WriteValueBox.Text).AsTask()
        );
    }

    private async void AppendValue_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteSettingUpdateAsync("Append", () =>
            settings.AppendAsync(WriteKeyBox.Text, WriteValueBox.Text).AsTask()
        );
    }

    private async void MergeValue_Click(object sender, RoutedEventArgs e)
    {
        var path = WriteKeyBox.Text.Trim();
        var separator = path.LastIndexOf(':');
        var parentPath = separator > 0 ? path[..separator] : path;
        var propertyName = separator > 0 ? path[(separator + 1)..] : "Value";

        await ExecuteSettingUpdateAsync("Merge", () =>
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
        await ExecuteSettingUpdateAsync(
            "Remove",
            () => settings.RemoveAsync(WriteKeyBox.Text).AsTask()
        );
    }

    private void LocaleBox_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        ApplySelectedLocale();

    private void ApplySelectedLocale()
    {
        if (!IsLoaded || isRefreshingLocale || LocaleBox.SelectedItem is not string locale)
        {
            return;
        }

        try
        {
            localization.SetLocale(locale);
            RefreshLocaleState();
            LocaleOutput.WriteLine($"Locale changed to {localization.CurrentLocale}.");
        }
        catch (Exception error)
        {
            LocaleOutput.WriteLine($"Error: {error.Message}");
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
            LocaleFileOutput.WriteLine(
                $"Registered {localeFileRegistration.Locale} from {localeFileRegistration.FilePath}."
            );
            RefreshLocaleState();
        }
        catch (Exception error)
        {
            LocaleFileOutput.WriteLine($"Error: {error.Message}");
        }
    }

    private void ReloadLocaleFile_Click(object sender, RoutedEventArgs e)
    {
        if (localeFileRegistration is null)
        {
            LocaleFileOutput.WriteLine("Register a locale file first.");
            return;
        }

        try
        {
            localization.ReloadFile(localeFileRegistration);
            LocaleFileOutput.WriteLine(
                $"Reloaded {localeFileRegistration.Locale} at {DateTime.Now:T}."
            );
        }
        catch (Exception error)
        {
            LocaleFileOutput.WriteLine($"Error: {error.Message}");
        }
    }

    private void UnregisterLocaleFile_Click(object sender, RoutedEventArgs e)
    {
        if (localeFileRegistration is null)
        {
            LocaleFileOutput.WriteLine("No locale file is registered by this page.");
            return;
        }

        try
        {
            var locale = localeFileRegistration.Locale;
            var removed = localization.Unregister(localeFileRegistration);
            localeFileRegistration = null;
            LocaleFileOutput.WriteLine(
                removed
                    ? $"Unregistered locale source {locale}."
                    : "That locale source was already unregistered."
            );
            RefreshLocaleState();
        }
        catch (Exception error)
        {
            LocaleFileOutput.WriteLine($"Error: {error.Message}");
        }
    }

    private async Task ExecuteSettingUpdateAsync(
        string operation,
        Func<Task<AppSettingsUpdateResult>> update
    )
    {
        try
        {
            var result = await update();
            WriteOutput.WriteLine(
                result.Changed
                    ? $"{operation} saved {result.FilePath}. Configuration reloaded: {result.ConfigurationReloaded}."
                    : $"{operation} completed without changing the document."
            );
        }
        catch (Exception error)
        {
            WriteOutput.WriteLine($"Error: {error.Message}");
        }
    }

    private void Localization_Changed(object? sender, FlourishLocalizationChangedEventArgs e)
    {
        Dispatcher.BeginInvoke(RefreshLocaleState);
    }

    private void RefreshLocaleState()
    {
        isRefreshingLocale = true;
        try
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

            LocaleBox.SelectedItem = availableLocales.FirstOrDefault(locale =>
                string.Equals(
                    locale,
                    localization.CurrentLocale,
                    StringComparison.OrdinalIgnoreCase
                )
            );
        }
        finally
        {
            isRefreshingLocale = false;
        }
    }

}
