using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CKey = ArkheideSystem.Essential.Culture.Key;
using Localizer = ArkheideSystem.Essential.Culture.Localizer;
using ArkheideSystem.Flourish.Abstract;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Configuration;

namespace ArkheideSystem.Gallery.WPF.Views;

public partial class ConfigurationPage : Page
{
    private static CultureRegistration? cultureFileRegistration;

    private readonly ObservableCollection<string> availableLocales = [];
    private readonly IConfiguration configuration;
    private readonly ISettingsStore settings;
    private readonly ILocalizationService localization;
    private bool isRefreshingLocale;

    public ConfigurationPage(
        IConfiguration configuration,
        ISettingsStore settings,
        ILocalizationService localization
    )
    {
        this.configuration = configuration;
        this.settings = settings;
        this.localization = localization;
        InitializeComponent();
        LocaleBox.ItemsSource = availableLocales;
        CultureFilePathBox.Text = Path.Combine(AppContext.BaseDirectory, "FlourishCulture.Json");

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
                ReadOutput.WriteLine(Localizer.Parse(CKey.Dynamic_EnterAConfigurationPath_7DFDBF45));
                return;
            }

            ReadOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Read01_611124DE, key, configuration[key] ?? "<null>")
            );
        }
        catch (Exception error)
        {
            ReadOutput.WriteLine(Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message));
        }
    }

    private void Reload_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (configuration is not IConfigurationRoot root)
            {
                throw new InvalidOperationException("Can't reload the configuration.");
            }

            root.Reload();
            var valueCount = configuration.AsEnumerable().Count();
            ReadOutput.WriteLine(
                Localizer.Parse(
                    CKey.Dynamic_ReloadedConfigurationProvidersSnapshotV0Contains1ValuesCaptured2_75761543,
                    0,
                    valueCount,
                    DateTime.Now
                )
            );
        }
        catch (Exception error)
        {
            ReadOutput.WriteLine(Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message));
        }
    }

    private async void SetValue_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteSettingUpdateAsync(
            CKey.Dynamic_Set_B6F6F3AD,
            () => settings.SetAsync(WriteKeyBox.Text, WriteValueBox.Text).AsTask()
        );
    }

    private async void AppendValue_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteSettingUpdateAsync(
            CKey.Dynamic_Append_FC15CC0A,
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
            CKey.Dynamic_Merge_8851AAA7,
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
            CKey.Controls_Remove_C3812FC4,
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
            Localizer.Current.SetCulture(locale);
            RefreshLocaleState();
            LocaleOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_LocaleChangedTo0_1C2A91ED, localization.Current.Locale)
            );
        }
        catch (Exception error)
        {
            LocaleOutput.WriteLine(Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message));
        }
    }

    private void RegisterCultureFile_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (cultureFileRegistration is not null)
            {
                cultureFileRegistration.Dispose();
            }

            cultureFileRegistration = localization.RegisterFile(CultureFilePathBox.Text);
            CultureFilePathBox.Text = cultureFileRegistration.FilePath;
            CultureFileOutput.WriteLine(
                Localizer.Parse(
                    CKey.Dynamic_Registered0From1_29302AFF,
                    string.Join(", ", cultureFileRegistration.Locales),
                    cultureFileRegistration.FilePath
                )
            );
            RefreshLocaleState();
        }
        catch (Exception error)
        {
            CultureFileOutput.WriteLine(Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message));
        }
    }

    private void ReloadCultureFile_Click(object sender, RoutedEventArgs e)
    {
        if (cultureFileRegistration is null)
        {
            CultureFileOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_RegisterALocaleFileFirst_5BC84B5D)
            );
            return;
        }

        try
        {
            cultureFileRegistration.Reload();
            CultureFileOutput.WriteLine(
                Localizer.Parse(
                    CKey.Dynamic_Reloaded0At1T_19E0356E,
                    string.Join(", ", cultureFileRegistration.Locales),
                    DateTime.Now
                )
            );
        }
        catch (Exception error)
        {
            CultureFileOutput.WriteLine(Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message));
        }
    }

    private void UnregisterCultureFile_Click(object sender, RoutedEventArgs e)
    {
        if (cultureFileRegistration is null)
        {
            CultureFileOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_NoLocaleFileIsRegisteredByThisPage_156204FE)
            );
            return;
        }

        try
        {
            var locales = string.Join(", ", cultureFileRegistration.Locales);
            var removed = cultureFileRegistration.IsRegistered;
            cultureFileRegistration.Dispose();
            cultureFileRegistration = null;
            CultureFileOutput.WriteLine(
                removed
                    ? Localizer.Parse(CKey.Dynamic_UnregisteredLocaleSource0_7FAC9B2D, locales)
                    : Localizer.Parse(CKey.Dynamic_ThatLocaleSourceWasAlreadyUnregistered_C7896D3D)
            );
            RefreshLocaleState();
        }
        catch (Exception error)
        {
            CultureFileOutput.WriteLine(Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message));
        }
    }

    private async Task ExecuteSettingUpdateAsync(
        string operationKey,
        Func<Task<SettingsUpdateResult>> update
    )
    {
        try
        {
            var result = await update();
            WriteOutput.WriteLine(
                result.Changed
                    ? Localizer.Parse(
                        CKey.Dynamic_Text0Saved1ConfigurationReloaded2_4F6CD457,
                        Localizer.Parse(operationKey),
                        result.FilePath,
                        result.ConfigurationReloaded
                    )
                    : Localizer.Parse(
                        CKey.Dynamic_Text0CompletedWithoutChangingTheDocument_4AAA1AD3,
                        Localizer.Parse(operationKey)
                    )
            );
        }
        catch (Exception error)
        {
            WriteOutput.WriteLine(Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message));
        }
    }

    private void Localization_Changed(object? sender, LocalizationChangedEventArgs e)
    {
        Dispatcher.BeginInvoke(RefreshLocaleState);
    }

    private void RefreshLocaleState()
    {
        isRefreshingLocale = true;
        try
        {
            var locales = localization.Current.AvailableLocales;
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
                    localization.Current.Locale,
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
