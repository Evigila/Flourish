using System.Windows;
using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Describes the semantic appearance of a <see cref="FlourishCard" />.
/// </summary>
public enum FlourishCardAppearance
{
    /// <summary>A standard filled content surface.</summary>
    Standard,

    /// <summary>A secondary content surface with reduced visual emphasis.</summary>
    Subtle,

    /// <summary>A surface emphasized with the application accent.</summary>
    Accent,

    /// <summary>A content surface with elevation.</summary>
    Elevated,

    /// <summary>An introductory content surface with a gradient background and elevation.</summary>
    Hero,
}

/// <summary>
/// A themed content surface for grouping related Flourish content.
/// </summary>
public class FlourishCard : ContentControl
{
    /// <summary>
    /// Identifies the <see cref="Appearance" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty AppearanceProperty = DependencyProperty.Register(
        nameof(Appearance),
        typeof(FlourishCardAppearance),
        typeof(FlourishCard),
        new FrameworkPropertyMetadata(FlourishCardAppearance.Standard),
        IsAppearanceValid
    );

    static FlourishCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlourishCard),
            new FrameworkPropertyMetadata(typeof(FlourishCard))
        );
    }

    /// <summary>
    /// Gets or sets the semantic visual appearance of the card.
    /// </summary>
    public FlourishCardAppearance Appearance
    {
        get => (FlourishCardAppearance)GetValue(AppearanceProperty);
        set => SetValue(AppearanceProperty, value);
    }

    private static bool IsAppearanceValid(object value)
    {
        return value is FlourishCardAppearance appearance && Enum.IsDefined(appearance);
    }
}
