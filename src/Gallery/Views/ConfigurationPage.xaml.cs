using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Gallery.Localization;

namespace ArkheideSystem.Gallery.Views;

public partial class ConfigurationPage : Page
{
    private static FlourishLocaleRegistration? localeFileRegistration;

    private readonly ObservableCollection<string> availableLocales = [];
    private readonly IFlourishConfiguration configuration;
    private readonly IFlourishSettingsStore settings;
    private readonly IFlourishLocalization localization;
    private readonly IGalleryLocalization galleryLocalization;
    private bool isRefreshingLocale;
    public ConfigurationPage(
        IFlourishConfiguration configuration,
        IFlourishSettingsStore settings,
        IFlourishLocalization localization,
        IGalleryLocalization galleryLocalization
    )
    {
        this.configuration = configuration;
        this.settings = settings;
        this.localization = localization;
        this.galleryLocalization = galleryLocalization;
        InitializeComponent();
        LocaleBox.ItemsSource = availableLocales;
        LocaleFilePathBox.Text = Path.Combine(AppContext.BaseDirectory, "lang_es-ES.json");

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
                ReadOutput.WriteLine(
                    galleryLocalization.Get("Enter a configuration path.")
                );
                return;
            }

            ReadOutput.WriteLine(
                galleryLocalization.Format(
                    "Read {0}: {1}",
                    key,
                    configuration[key] ?? "<null>"
                )
            );
        }
        catch (Exception error)
        {
            ReadOutput.WriteLine(galleryLocalization.Format("Error: {0}", error.Message));
        }
    }

    private void Reload_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            configuration.Reload();
            var snapshot = configuration.Current;
            ReadOutput.WriteLine(
                galleryLocalization.Format(
                    "Reloaded configuration providers. Snapshot v{0} contains {1} values (captured {2:T}).",
                    snapshot.Version,
                    snapshot.Values.Count,
                    snapshot.CapturedAt.LocalDateTime
                )
            );
        }
        catch (Exception error)
        {
            ReadOutput.WriteLine(galleryLocalization.Format("Error: {0}", error.Message));
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
            LocaleOutput.WriteLine(
                galleryLocalization.Format(
                    "Locale changed to {0}.",
                    localization.CurrentLocale
                )
            );
        }
        catch (Exception error)
        {
            LocaleOutput.WriteLine(galleryLocalization.Format("Error: {0}", error.Message));
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
                galleryLocalization.Format(
                    "Registered {0} from {1}.",
                    localeFileRegistration.Locale,
                    localeFileRegistration.FilePath
                )
            );
            RefreshLocaleState();
        }
        catch (Exception error)
        {
            LocaleFileOutput.WriteLine(
                galleryLocalization.Format("Error: {0}", error.Message)
            );
        }
    }

    private void ReloadLocaleFile_Click(object sender, RoutedEventArgs e)
    {
        if (localeFileRegistration is null)
        {
            LocaleFileOutput.WriteLine(
                galleryLocalization.Get("Register a locale file first.")
            );
            return;
        }

        try
        {
            localization.ReloadFile(localeFileRegistration);
            LocaleFileOutput.WriteLine(
                galleryLocalization.Format(
                    "Reloaded {0} at {1:T}.",
                    localeFileRegistration.Locale,
                    DateTime.Now
                )
            );
        }
        catch (Exception error)
        {
            LocaleFileOutput.WriteLine(
                galleryLocalization.Format("Error: {0}", error.Message)
            );
        }
    }

    private void UnregisterLocaleFile_Click(object sender, RoutedEventArgs e)
    {
        if (localeFileRegistration is null)
        {
            LocaleFileOutput.WriteLine(
                galleryLocalization.Get("No locale file is registered by this page.")
            );
            return;
        }

        try
        {
            var locale = localeFileRegistration.Locale;
            var removed = localization.Unregister(localeFileRegistration);
            localeFileRegistration = null;
            LocaleFileOutput.WriteLine(
                removed
                    ? galleryLocalization.Format("Unregistered locale source {0}.", locale)
                    : galleryLocalization.Get(
                        "That locale source was already unregistered."
                    )
            );
            RefreshLocaleState();
        }
        catch (Exception error)
        {
            LocaleFileOutput.WriteLine(
                galleryLocalization.Format("Error: {0}", error.Message)
            );
        }
    }

    private async Task ExecuteSettingUpdateAsync(
        string operation,
        Func<Task<FlourishSettingsUpdateResult>> update
    )
    {
        try
        {
            var result = await update();
            WriteOutput.WriteLine(
                result.Changed
                    ? galleryLocalization.Format(
                        "{0} saved {1}. Configuration reloaded: {2}.",
                        galleryLocalization.Get(operation),
                        result.FilePath,
                        result.ConfigurationReloaded
                    )
                    : galleryLocalization.Format(
                        "{0} completed without changing the document.",
                        galleryLocalization.Get(operation)
                    )
            );
        }
        catch (Exception error)
        {
            WriteOutput.WriteLine(galleryLocalization.Format("Error: {0}", error.Message));
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
