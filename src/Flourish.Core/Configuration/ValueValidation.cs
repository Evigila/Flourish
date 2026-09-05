using System;

namespace ArkheideSystem.Flourish.Configuration;

/// <summary>Validates platform-neutral configuration values.</summary>
internal static class ValueValidation
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
        if (!double.IsFinite(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value must be greater than 0."
            );
        }
    }

    internal static void NonNegativeFinite(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value < 0)
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
}
