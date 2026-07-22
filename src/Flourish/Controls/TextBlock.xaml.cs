using System.Windows;
using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Describes the semantic typography role of a <see cref="FlourishTextBlock" />.
/// </summary>
public enum FlourishTextRole
{
    /// <summary>Regular body copy.</summary>
    Body,

    /// <summary>
    /// A single wrapped block of body copy. Use <see cref="Document" /> to arrange multiple
    /// <see cref="Paragraph" /> elements with standard indentation and spacing.
    /// </summary>
    Paragraph,

    /// <summary>Compact supporting copy.</summary>
    Caption,

    /// <summary>De-emphasized supporting copy.</summary>
    Muted,

    /// <summary>A label associated with an input field.</summary>
    FieldLabel,

    /// <summary>A subtitle below a larger heading.</summary>
    Subtitle,

    /// <summary>Compact supporting copy below a heading.</summary>
    Description,

    /// <summary>A heading used inside a card, presenter, or compact content surface.</summary>
    CardTitle,

    /// <summary>The large heading used by a <see cref="Chunk" /> content section.</summary>
    SectionTitle,

    /// <summary>The primary page heading reserved for <see cref="HeaderChunk" />.</summary>
    PageTitle,

    /// <summary>Status or feedback text.</summary>
    Status,

    /// <summary>An icon glyph rendered with the configured icon typeface.</summary>
    Icon,
}

/// <summary>
/// A Flourish-styled text element with a semantic typography role.
/// </summary>
public class FlourishTextBlock : TextBlock
{
    /// <summary>
    /// Identifies the <see cref="Role" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty RoleProperty = DependencyProperty.Register(
        nameof(Role),
        typeof(FlourishTextRole),
        typeof(FlourishTextBlock),
        new FrameworkPropertyMetadata(FlourishTextRole.Body),
        IsRoleValid
    );

    static FlourishTextBlock()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlourishTextBlock),
            new FrameworkPropertyMetadata(typeof(FlourishTextBlock))
        );
    }

    /// <summary>
    /// Gets or sets the semantic typography role of the text.
    /// </summary>
    public FlourishTextRole Role
    {
        get => (FlourishTextRole)GetValue(RoleProperty);
        set => SetValue(RoleProperty, value);
    }

    private static bool IsRoleValid(object value)
    {
        return value is FlourishTextRole role && Enum.IsDefined(role);
    }
}
