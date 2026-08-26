using System;

using ArkheideSystem.Flourish.Abstract;
using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Configuration;

internal static class BuilderValidation
{
    internal static string NotBlank(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value;
    }

    internal static void PositiveFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be greater than 0.");
        }
    }

    internal static void NonNegativeFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value must be finite and non-negative."
            );
        }
    }

    internal static void Enum<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!System.Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Unknown value.");
        }
    }

    internal static void PageType(Type pageType, string parameterName)
    {
        if (!typeof(Page).IsAssignableFrom(pageType) || pageType.IsAbstract || pageType.ContainsGenericParameters)
        {
            throw new ArgumentException(
                $"{pageType.FullName} must be a closed, concrete System.Windows.Controls.Page type.",
                parameterName
            );
        }
    }
}
