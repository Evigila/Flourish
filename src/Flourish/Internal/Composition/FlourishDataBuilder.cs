using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal sealed class FlourishDataBuilder(FlourishDataOptions options)
    : FlourishBuilderMutationGuard,
        IFlourishDataBuilder
{
    public IFlourishDataBuilder InitLocale(string locale = "EN")
    {
        ThrowIfFrozen();
        options.Locale = ValidateNotBlank(locale, nameof(locale)).Trim();
        return this;
    }

    public IFlourishDataBuilder InitLocaleFile(string path)
    {
        ThrowIfFrozen();
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
