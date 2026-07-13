namespace ArkheideSystem.Flourish.Internal.Configuration;

internal static class NavigationPanelDimensions
{
    internal const double MinimumCollapsedWidth = 56;

    internal static void ValidateCollapsedWidth(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value < 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value must be non-negative and finite."
            );
        }

        if (value > 0 && value < MinimumCollapsedWidth)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                $"Collapsed navigation panel width must be 0 (fully hidden) or at least {MinimumCollapsedWidth}."
            );
        }
    }
}
