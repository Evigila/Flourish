using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace ArkheideSystem.Flourish.Blazor.Components.Internal;

internal static class SelectValueConversion
{
    internal static string Format<TValue>(TValue? value, CultureInfo culture)
    {
        // Select values are strings, not checkbox/conditional Boolean attributes.
        if (typeof(TValue) == typeof(bool) || typeof(TValue) == typeof(bool?))
            return value is null ? "" : (bool)(object)value ? "true" : "false";
        return BindConverter.FormatValue(value, culture)?.ToString() ?? "";
    }

    internal static bool TryParse<TValue>(string? value, CultureInfo culture, out TValue result)
    {
        if (typeof(TValue) == typeof(bool) || typeof(TValue) == typeof(bool?))
        {
            if (typeof(TValue) == typeof(bool?) && string.IsNullOrEmpty(value))
            {
                result = default!;
                return true;
            }
            if (bool.TryParse(value, out var boolean))
            {
                result = (TValue)(object)boolean;
                return true;
            }
            result = default!;
            return false;
        }
        return BindConverter.TryConvertTo(value, culture, out result!);
    }
}
