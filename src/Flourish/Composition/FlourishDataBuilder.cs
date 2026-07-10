using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Composition;

internal sealed class FlourishDataBuilder(FlourishDataOptions options) : IFlourishDataBuilder
{
    public IFlourishDataBuilder SetLocale(string locale)
    {
        options.Locale = ValidateNotBlank(locale, nameof(locale)).Trim();
        return this;
    }

    public IFlourishDataBuilder AddLocale(string path)
    {
        options.LocalePaths.Add(ValidateNotBlank(path, nameof(path)).Trim());
        return this;
    }

    public IFlourishDataBuilder SetAppPreferenceDataPath(string path)
    {
        options.AppPreferenceDataPath = ValidateNotBlank(path, nameof(path));
        options.HasConfiguration = true;
        return this;
    }

    public IFlourishDataBuilder SetAppName(string appName)
    {
        options.AppName = ValidateNotBlank(appName, nameof(appName));
        options.HasConfiguration = true;
        return this;
    }

    public IFlourishDataBuilder SetAppCompany(string companyName)
    {
        options.CompanyName = ValidateNotBlank(companyName, nameof(companyName));
        options.HasConfiguration = true;
        return this;
    }

    private static string ValidateNotBlank(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value;
    }
}
