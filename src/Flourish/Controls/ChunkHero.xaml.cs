using System.Windows;
using System.Windows.Markup;
using ArkheideSystem.Flourish.Internal.Interaction;
using WpfBorder = System.Windows.Controls.Border;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// A page-leading, full-width <see cref="Presenter" /> with an emphasized background and title.
/// </summary>
[TemplatePart(Name = PartHeroSurface, Type = typeof(WpfBorder))]
[TemplatePart(Name = PartClipHost, Type = typeof(FrameworkElement))]
[ContentProperty(nameof(Body))]
public class ChunkHero : Presenter
{
    private const string PartHeroSurface = "PART_HeroSurface";
    private const string PartClipHost = "PART_ClipHost";

    private readonly RoundedClipCoordinator roundedClip = new();

    static ChunkHero()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ChunkHero),
            new FrameworkPropertyMetadata(typeof(ChunkHero))
        );
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        roundedClip.Detach();
        base.OnApplyTemplate();

        roundedClip.Attach(
            GetTemplateChild(PartClipHost) as FrameworkElement,
            GetTemplateChild(PartHeroSurface) as WpfBorder
        );
    }
}
