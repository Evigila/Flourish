using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Composition;

internal sealed class FlourishShellBuilder(FlourishShellOptions options) : IFlourishShellBuilder
{
    public IFlourishShellBuilder UseTitleBar(bool enabled = true)
    {
        options.IsTitlebarEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder UseNavigation(bool enabled = true)
    {
        options.IsNavigationPanelEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder UseDynamicToolbar(bool enabled = true)
    {
        options.IsDynamicToolbarEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder UseTips(int delay = 200)
    {
        if (delay < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(delay),
                delay,
                "Tooltip delay cannot be negative."
            );
        }

        options.Tips.InitialShowDelayMilliseconds = delay;
        options.Tips.SpawnableMargin = 5;
        options.IsTipsEnabled = true;
        return this;
    }

    public IFlourishShellBuilder UseMotion(bool enabled = true)
    {
        options.Motion.IsEnabled = enabled;
        return this;
    }

    public IFlourishShellBuilder UseMaterialEffect(
        MaterialEffect effect = MaterialEffect.Mica
    )
    {
        ValidateEnum(effect, nameof(effect));
        options.MaterialEffect = effect;
        options.IsMaterialEffectEnabled = effect != MaterialEffect.None;
        return this;
    }

    public IFlourishShellBuilder UseGlobalFont(string fontFamily, double fontSize = 14)
    {
        options.FontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
        ValidatePositiveFinite(fontSize, nameof(fontSize));
        options.FontSize = fontSize;
        return this;
    }

    public IFlourishShellBuilder UseStatusBar(bool enabled = true)
    {
        options.IsStatusBarEnabled = enabled;
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

    private static void ValidatePositiveFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value must be greater than 0."
            );
        }
    }

    private static void ValidateEnum<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Unknown value.");
        }
    }
}
