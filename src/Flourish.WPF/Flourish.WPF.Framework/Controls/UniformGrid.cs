using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ArkheideSystem.Flourish.WPF.Abstract;

namespace ArkheideSystem.Flourish.WPF.Controls;

/// <summary>Blazor's full-width rectangle tracks and bounded square cells in native measure/arrange.</summary>
public class UniformGrid : Panel
{
    public UniformGrid() => ClipToBounds = true;
    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(nameof(Columns), typeof(int), typeof(UniformGrid), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure), v => (int)v >= 0);
    public static readonly DependencyProperty RowsProperty = DependencyProperty.Register(nameof(Rows), typeof(int), typeof(UniformGrid), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure), v => (int)v >= 0);
    public static readonly DependencyProperty NarrowColumnsProperty = DependencyProperty.Register(nameof(NarrowColumns), typeof(int), typeof(UniformGrid), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsMeasure), v => (int)v > 0);
    public static readonly DependencyProperty ShapeProperty = DependencyProperty.Register(nameof(Shape), typeof(UniformGridShape), typeof(UniformGrid), new FrameworkPropertyMetadata(UniformGridShape.Rectangle, FrameworkPropertyMetadataOptions.AffectsMeasure));
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(nameof(Variant), typeof(UniformGridVariant), typeof(UniformGrid), new FrameworkPropertyMetadata(UniformGridVariant.Elevated, FrameworkPropertyMetadataOptions.AffectsMeasure));
    public static readonly DependencyProperty CellHeightProperty = DependencyProperty.Register(nameof(CellHeight), typeof(double), typeof(UniformGrid), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure), v => double.IsNaN((double)v) || double.IsFinite((double)v) && (double)v > 0);
    public static readonly DependencyProperty MaxCellSizeProperty = DependencyProperty.Register(nameof(MaxCellSize), typeof(double), typeof(UniformGrid), new FrameworkPropertyMetadata(280d, FrameworkPropertyMetadataOptions.AffectsMeasure), v => double.IsFinite((double)v) && (double)v > 0);
    public static readonly DependencyProperty MaxCellHeightProperty = DependencyProperty.Register(nameof(MaxCellHeight), typeof(double), typeof(UniformGrid), new FrameworkPropertyMetadata(260d, FrameworkPropertyMetadataOptions.AffectsMeasure), v => double.IsFinite((double)v) && (double)v > 0);
    public int Columns { get => (int)GetValue(ColumnsProperty); set => SetValue(ColumnsProperty, value); }
    public int Rows { get => (int)GetValue(RowsProperty); set => SetValue(RowsProperty, value); }
    public int NarrowColumns { get => (int)GetValue(NarrowColumnsProperty); set => SetValue(NarrowColumnsProperty, value); }
    public UniformGridShape Shape { get => (UniformGridShape)GetValue(ShapeProperty); set => SetValue(ShapeProperty, value); }
    public UniformGridVariant Variant { get => (UniformGridVariant)GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
    public double CellHeight { get => (double)GetValue(CellHeightProperty); set => SetValue(CellHeightProperty, value); }
    public double MaxCellSize { get => (double)GetValue(MaxCellSizeProperty); set => SetValue(MaxCellSizeProperty, value); }
    public double MaxCellHeight { get => (double)GetValue(MaxCellHeightProperty); set => SetValue(MaxCellHeightProperty, value); }
    private (int Columns, int Rows, double Width, double Height) Geometry(double width)
    {
        if (Shape == UniformGridShape.Square && !double.IsNaN(CellHeight)) throw new InvalidOperationException("CellHeight requires Rectangle shape.");
        if (!double.IsFinite(width)) width = Math.Max(1, Columns) * (Shape == UniformGridShape.Square ? MaxCellSize : 2 * MaxCellHeight);
        var columns = width <= 560 ? NarrowColumns : Columns > 0 ? Columns : Rows > 0 ? Math.Max(1, (InternalChildren.Count + Rows - 1) / Rows) : Math.Max(1, (int)Math.Floor((width + 1) / (Shape == UniformGridShape.Square ? MaxCellSize + 1 : 2 * (double.IsNaN(CellHeight) ? MaxCellHeight : CellHeight) + 1)));
        columns = Math.Max(1, Math.Min(columns, Math.Max(1, InternalChildren.Count)));
        var rows = Rows > 0 ? Math.Max(Rows, (InternalChildren.Count + columns - 1) / columns) : (InternalChildren.Count + columns - 1) / columns;
        var cellWidth = Math.Max(0, (width - columns + 1) / columns);
        if (Shape == UniformGridShape.Square) cellWidth = Math.Min(MaxCellSize, cellWidth);
        var height = Shape == UniformGridShape.Square ? cellWidth : double.IsNaN(CellHeight) ? Math.Min(MaxCellHeight, cellWidth / 2) : CellHeight;
        return (columns, rows, cellWidth, height);
    }
    protected override Size MeasureOverride(Size available)
    {
        var geometry = Geometry(available.Width);
        foreach (UIElement child in InternalChildren)
        {
            if (child is UniformGridButton button) button.SetCurrentValue(UniformGridButton.GridVariantProperty, Variant);
            child.Measure(new Size(geometry.Width, geometry.Height));
        }
        return new Size(geometry.Width * geometry.Columns + geometry.Columns - 1, Math.Max(0, geometry.Rows * geometry.Height + geometry.Rows - 1));
    }
    protected override Size ArrangeOverride(Size finalSize)
    {
        var g = Geometry(finalSize.Width);
        for (var i = 0; i < InternalChildren.Count; i++) { var column = Rows > 0 && Columns == 0 ? i / g.Rows : i % g.Columns; var row = Rows > 0 && Columns == 0 ? i % g.Rows : i / g.Columns; InternalChildren[i].Arrange(new Rect(column * (g.Width + 1), row * (g.Height + 1), g.Width, g.Height)); }
        return finalSize;
    }
    protected override Geometry GetLayoutClip(Size layoutSlotSize)
    {
        var radius = Variant is UniformGridVariant.Filled or UniformGridVariant.Danger ? 16 : 28;
        return new RectangleGeometry(new Rect(RenderSize), radius, radius);
    }
}
