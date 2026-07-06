using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Configuration;

namespace AcksheedSys.Flourish.Composition;

internal sealed class FlourishDataBuilder(FlourishDataOptions options) : IFlourishDataBuilder
{
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
