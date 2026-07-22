using System.Windows;
using System.Windows.Markup;
using ArkheideSystem.Flourish.Internal.Interaction;
using WpfBorder = System.Windows.Controls.Border;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// A page-leading, full-width <see cref="Presenter" /> with an emphasized background and title.
/// </summary>
/// <remarks>
/// Authors should explicitly assign <see cref="Presenter.Title" />,
/// <see cref="Presenter.Content" />, <see cref="Presenter.PresenterMode" />, and
/// <see cref="Presenter.PresenterPosition" />. Unlike an ordinary <see cref="Presenter" />,
/// implicit XAML content is assigned to <see cref="Presenter.Body" />; presentation content must
/// use an explicit <see cref="Presenter.Presentation" /> property element.
/// </remarks>
[TemplatePart(Name = PartHeaderSurface, Type = typeof(WpfBorder))]
[TemplatePart(Name = PartClipHost, Type = typeof(FrameworkElement))]
[ContentProperty(nameof(Body))]
public class HeaderChunk : Presenter
{
    private const string PartHeaderSurface = "PART_HeaderSurface";
    private const string PartClipHost = "PART_ClipHost";

    private readonly RoundedClipCoordinator roundedClip = new();

    static HeaderChunk()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(HeaderChunk),
            new FrameworkPropertyMetadata(typeof(HeaderChunk))
        );
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        roundedClip.Detach();
        base.OnApplyTemplate();

        roundedClip.Attach(
            GetTemplateChild(PartClipHost) as FrameworkElement,
            GetTemplateChild(PartHeaderSurface) as WpfBorder
        );
    }
}
