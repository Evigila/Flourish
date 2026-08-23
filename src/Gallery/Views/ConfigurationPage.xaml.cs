using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ArkheideSystem.Gallery.Views;

public partial class ConfigurationPage : Page
{
    private static FlourishLocaleRegistration? localeFileRegistration;

    private readonly ObservableCollection<string> availableLocales = [];
    private readonly IFlourishConfiguration configuration;
    private readonly IFlourishSettingsStore settings;
    private readonly IFlourishLocalization localization;
    private bool isRefreshingLocale;

    public ConfigurationPage(
        IFlourishConfiguration configuration,
        IFlourishSettingsStore settings,
        IFlourishLocalization localization
    )
    {
        this.configuration = configuration;
        this.settings = settings;
        this.localization = localization;
        InitializeComponent();
        LocaleBox.ItemsSource = availableLocales;
        LocaleFilePathBox.Text = Path.Combine(
            AppContext.BaseDirectory,
            "Flourish.LangKey_es-ES.Json"
        );

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
                    Localizer.Parse(Key.Dynamic_EnterAConfigurationPath_7DFDBF45)
                );
                return;
            }

            ReadOutput.WriteLine(
                Localizer.Parse(
                    Key.Dynamic_Read01_611124DE,
                    key,
                    configuration[key] ?? "<null>"
                )
            );
        }
        catch (Exception error)
        {
            ReadOutput.WriteLine(
                Localizer.Parse(Key.Dynamic_Error0_43F78154, error.Message)
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
                Localizer.Parse(
                    Key.Dynamic_ReloadedConfigurationProvidersSnapshotV0Contains1ValuesCaptured2_75761543,
                    snapshot.Version,
                    snapshot.Values.Count,
                    snapshot.CapturedAt.LocalDateTime
                )
            );
        }
        catch (Exception error)
        {
            ReadOutput.WriteLine(
                Localizer.Parse(Key.Dynamic_Error0_43F78154, error.Message)
            );
        }
    }

    private async void SetValue_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteSettingUpdateAsync(
            Key.Dynamic_Set_B6F6F3AD,
            () => settings.SetAsync(WriteKeyBox.Text, WriteValueBox.Text).AsTask()
        );
    }

    private async void AppendValue_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteSettingUpdateAsync(
            Key.Dynamic_Append_FC15CC0A,
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
            Key.Dynamic_Merge_8851AAA7,
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
            Key.Controls_Remove_C3812FC4,
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
                Localizer.Parse(
                    Key.Dynamic_LocaleChangedTo0_1C2A91ED,
                    localization.CurrentLocale
                )
            );
        }
        catch (Exception error)
        {
            LocaleOutput.WriteLine(
                Localizer.Parse(Key.Dynamic_Error0_43F78154, error.Message)
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
                Localizer.Parse(
                    Key.Dynamic_Registered0From1_29302AFF,
                    localeFileRegistration.Locale,
                    localeFileRegistration.FilePath
                )
            );
            RefreshLocaleState();
        }
        catch (Exception error)
        {
            LocaleFileOutput.WriteLine(
                Localizer.Parse(Key.Dynamic_Error0_43F78154, error.Message)
            );
        }
    }

    private void ReloadLocaleFile_Click(object sender, RoutedEventArgs e)
    {
        if (localeFileRegistration is null)
        {
            LocaleFileOutput.WriteLine(
                Localizer.Parse(Key.Dynamic_RegisterALocaleFileFirst_5BC84B5D)
            );
            return;
        }

        try
        {
            localization.ReloadFile(localeFileRegistration);
            LocaleFileOutput.WriteLine(
                Localizer.Parse(
                    Key.Dynamic_Reloaded0At1T_19E0356E,
                    localeFileRegistration.Locale,
                    DateTime.Now
                )
            );
        }
        catch (Exception error)
        {
            LocaleFileOutput.WriteLine(
                Localizer.Parse(Key.Dynamic_Error0_43F78154, error.Message)
            );
        }
    }

    private void UnregisterLocaleFile_Click(object sender, RoutedEventArgs e)
    {
        if (localeFileRegistration is null)
        {
            LocaleFileOutput.WriteLine(
                Localizer.Parse(Key.Dynamic_NoLocaleFileIsRegisteredByThisPage_156204FE)
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
                    ? Localizer.Parse(
                        Key.Dynamic_UnregisteredLocaleSource0_7FAC9B2D,
                        locale
                    )
                    : Localizer.Parse(
                        Key.Dynamic_ThatLocaleSourceWasAlreadyUnregistered_C7896D3D
                    )
            );
            RefreshLocaleState();
        }
        catch (Exception error)
        {
            LocaleFileOutput.WriteLine(
                Localizer.Parse(Key.Dynamic_Error0_43F78154, error.Message)
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
                    ? Localizer.Parse(
                        Key.Dynamic_Text0Saved1ConfigurationReloaded2_4F6CD457,
                        Localizer.Parse(operationKey),
                        result.FilePath,
                        result.ConfigurationReloaded
                    )
                    : Localizer.Parse(
                        Key.Dynamic_Text0CompletedWithoutChangingTheDocument_4AAA1AD3,
                        Localizer.Parse(operationKey)
                    )
            );
        }
        catch (Exception error)
        {
            WriteOutput.WriteLine(
                Localizer.Parse(Key.Dynamic_Error0_43F78154, error.Message)
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
