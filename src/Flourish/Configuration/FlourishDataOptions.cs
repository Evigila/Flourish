using System.IO;

namespace AcksheedSys.Flourish.Configuration;

internal sealed class FlourishDataOptions
{
    public string? AppPreferenceDataPath { get; set; }

    public string? AppName { get; set; }

    public string? CompanyName { get; set; }

    public bool HasConfiguration { get; set; }

    public string GetRequiredAppPreferenceDataPath(FlourishShellOptions shellOptions)
    {
        if (!string.IsNullOrWhiteSpace(AppPreferenceDataPath))
        {
            return AppPreferenceDataPath;
        }

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            GetRequiredCompanyName(),
            GetRequiredAppName(shellOptions)
        );
    }

    public string GetRequiredAppName(FlourishShellOptions shellOptions)
    {
        var appName = string.IsNullOrWhiteSpace(AppName) ? shellOptions.Title : AppName;
        if (string.IsNullOrWhiteSpace(appName))
        {
            throw new InvalidOperationException(
                "Flourish preference data requires a non-empty application name. "
                    + "Set a title with titlebar.SetTitle(...) or configure data with SetAppName(...)."
            );
        }

        return appName;
    }

    public string GetRequiredCompanyName()
    {
        if (string.IsNullOrWhiteSpace(CompanyName))
        {
            throw new InvalidOperationException(
                "Flourish preference data requires a non-empty company name. "
                    + "Configure data with SetAppCompany(...)."
            );
        }

        return CompanyName;
    }
}
