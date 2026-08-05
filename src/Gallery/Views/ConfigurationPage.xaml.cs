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
                    galleryLocalization.Get(
                        GalleryLocaleKeys.DynamicEnterAConfigurationPath_7DFDBF45
                    )
                );
                return;
            }

            ReadOutput.WriteLine(
                galleryLocalization.Format(
                    GalleryLocaleKeys.DynamicRead01_611124DE,
                    key,
                    configuration[key] ?? "<null>"
                )
            );
        }
        catch (Exception error)
        {
            ReadOutput.WriteLine(
                galleryLocalization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
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
                    GalleryLocaleKeys.DynamicReloadedConfigurationProvidersSnapshotV0Contains1ValuesCaptured2_75761543,
                    snapshot.Version,
                    snapshot.Values.Count,
                    snapshot.CapturedAt.LocalDateTime
                )
            );
        }
        catch (Exception error)
        {
            ReadOutput.WriteLine(
                galleryLocalization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
        }
    }

    private async void SetValue_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteSettingUpdateAsync(
            GalleryLocaleKeys.DynamicSet_B6F6F3AD,
            () => settings.SetAsync(WriteKeyBox.Text, WriteValueBox.Text).AsTask()
        );
    }

    private async void AppendValue_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteSettingUpdateAsync(
            GalleryLocaleKeys.DynamicAppend_FC15CC0A,
            () => settings.AppendAsync(WriteKeyBox.Text, WriteValueBox.Text).AsTask()
        );
    }

    private async void MergeValue_Click(object sender, RoutedEventArgs e)
    {
        var path = WriteKeyBox.Text.Trim();
        var separator = path.LastIndexOf(':');
        var parentPath = separator > 0 ? path[..separator] : path;
        var propertyName = separator > 0 ? path[(separator + 1)..] : "Value";

        await ExecuteSettingUpdateAsync(
            GalleryLocaleKeys.DynamicMerge_8851AAA7,
            () =>
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
            GalleryLocaleKeys.ControlsRemove_C3812FC4,
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
                    GalleryLocaleKeys.DynamicLocaleChangedTo0_1C2A91ED,
                    localization.CurrentLocale
                )
            );
        }
        catch (Exception error)
        {
            LocaleOutput.WriteLine(
                galleryLocalization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
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
                    GalleryLocaleKeys.DynamicRegistered0From1_29302AFF,
                    localeFileRegistration.Locale,
                    localeFileRegistration.FilePath
                )
            );
            RefreshLocaleState();
        }
        catch (Exception error)
        {
            LocaleFileOutput.WriteLine(
                galleryLocalization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
        }
    }

    private void ReloadLocaleFile_Click(object sender, RoutedEventArgs e)
    {
        if (localeFileRegistration is null)
        {
            LocaleFileOutput.WriteLine(
                galleryLocalization.Get(GalleryLocaleKeys.DynamicRegisterALocaleFileFirst_5BC84B5D)
            );
            return;
        }

        try
        {
            localization.ReloadFile(localeFileRegistration);
            LocaleFileOutput.WriteLine(
                galleryLocalization.Format(
                    GalleryLocaleKeys.DynamicReloaded0At1T_19E0356E,
                    localeFileRegistration.Locale,
                    DateTime.Now
                )
            );
        }
        catch (Exception error)
        {
            LocaleFileOutput.WriteLine(
                galleryLocalization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
        }
    }

    private void UnregisterLocaleFile_Click(object sender, RoutedEventArgs e)
    {
        if (localeFileRegistration is null)
        {
            LocaleFileOutput.WriteLine(
                galleryLocalization.Get(
                    GalleryLocaleKeys.DynamicNoLocaleFileIsRegisteredByThisPage_156204FE
                )
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
                    ? galleryLocalization.Format(
                        GalleryLocaleKeys.DynamicUnregisteredLocaleSource0_7FAC9B2D,
                        locale
                    )
                    : galleryLocalization.Get(
                        GalleryLocaleKeys.DynamicThatLocaleSourceWasAlreadyUnregistered_C7896D3D
                    )
            );
            RefreshLocaleState();
        }
        catch (Exception error)
        {
            LocaleFileOutput.WriteLine(
                galleryLocalization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
        }
    }

    private async Task ExecuteSettingUpdateAsync(
        string operationKey,
        Func<Task<FlourishSettingsUpdateResult>> update
    )
    {
        try
        {
            var result = await update();
            WriteOutput.WriteLine(
                result.Changed
                    ? galleryLocalization.Format(
                        GalleryLocaleKeys.DynamicText0Saved1ConfigurationReloaded2_4F6CD457,
                        galleryLocalization.Get(operationKey),
                        result.FilePath,
                        result.ConfigurationReloaded
                    )
                    : galleryLocalization.Format(
                        GalleryLocaleKeys.DynamicText0CompletedWithoutChangingTheDocument_4AAA1AD3,
                        galleryLocalization.Get(operationKey)
                    )
            );
        }
        catch (Exception error)
        {
            WriteOutput.WriteLine(
                galleryLocalization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
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
