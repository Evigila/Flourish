using System;
using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Configuration;

internal static class PageTypeValidation
{
    internal static void Validate(Type pageType, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(pageType);
        if (
            !typeof(Page).IsAssignableFrom(pageType)
            || pageType.IsAbstract
            || pageType.ContainsGenericParameters
        )
        {
            throw new ArgumentException(
                $"{pageType.FullName} must be a closed, concrete System.Windows.Controls.Page type.",
                parameterName
            );
        }
    }
}
