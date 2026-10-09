using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace ArkheideSystem.Gallery.Flourish.WPF;

/// <summary>Reads the existing Blazor Gallery symbol into native WPF drawing primitives.</summary>
internal static class Artwork
{
    public static ImageSource GalleryLogo { get; } = LoadGalleryLogo();

    private static ImageSource LoadGalleryLogo()
    {
        var resource = Application.GetResourceStream(new Uri("/Gallery.Flourish.WPF;component/Artwork/gallery.svg", UriKind.Relative))
            ?? throw new InvalidOperationException("The shared Gallery artwork is missing.");
        using var stream = resource.Stream;
        var root = XDocument.Load(stream).Root ?? throw new InvalidOperationException("The shared Gallery artwork has no root.");
        var path = root.Elements().Single(element => element.Name.LocalName == "path");
        var geometry = Geometry.Parse(path.Attribute("d")?.Value ?? throw new InvalidOperationException("The shared Gallery artwork has no path."));
        var drawing = new DrawingGroup();
        drawing.Children.Add(new GeometryDrawing(Brushes.Transparent, null, new RectangleGeometry(new Rect(0, -960, 960, 960))));
        var symbol = new DrawingGroup { Transform = new MatrixTransform(.8, 0, 0, .8, 96, -96) };
        symbol.Children.Add(new GeometryDrawing((Brush)new BrushConverter().ConvertFromString(path.Attribute("fill")?.Value ?? "#FFFFFF")!, null, geometry));
        drawing.Children.Add(symbol);
        var image = new DrawingImage(drawing);
        image.Freeze();
        return image;
    }
}
