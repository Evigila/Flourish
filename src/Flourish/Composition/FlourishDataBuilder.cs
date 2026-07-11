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

    private static string ValidateNotBlank(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value;
    }
}
