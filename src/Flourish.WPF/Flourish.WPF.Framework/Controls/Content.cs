using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ArkheideSystem.Flourish.WPF.Abstract;

namespace ArkheideSystem.Flourish.WPF.Controls;

/// <summary>Inherited palette context for transparent controls within a primary presentation surface.</summary>
public static class PresentationContext
{
    public static readonly DependencyProperty IsPrimaryProperty = DependencyProperty.RegisterAttached("IsPrimary", typeof(bool), typeof(PresentationContext), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
    public static bool GetIsPrimary(DependencyObject target) => (bool)target.GetValue(IsPrimaryProperty);
    public static void SetIsPrimary(DependencyObject target, bool value) => target.SetValue(IsPrimaryProperty, value);
    public static readonly DependencyProperty IsAccessFormProperty = DependencyProperty.RegisterAttached("IsAccessForm", typeof(bool), typeof(PresentationContext), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
    public static bool GetIsAccessForm(DependencyObject target) => (bool)target.GetValue(IsAccessFormProperty);
    public static void SetIsAccessForm(DependencyObject target, bool value) => target.SetValue(IsAccessFormProperty, value);
    public static readonly DependencyProperty IsChromeProperty = DependencyProperty.RegisterAttached("IsChrome", typeof(bool), typeof(PresentationContext), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
    public static bool GetIsChrome(DependencyObject target) => (bool)target.GetValue(IsChromeProperty);
    public static void SetIsChrome(DependencyObject target, bool value) => target.SetValue(IsChromeProperty, value);
}

/// <summary>One native content/heading/actions composition core.</summary>
public class Card : ContentControl
{
    static Card() => DefaultStyleKeyProperty.OverrideMetadata(typeof(Card), new FrameworkPropertyMetadata(typeof(Card)));
    public Card() { SizeChanged += (_, _) => UpdateCardGeometry(); UpdateCardGeometry(); }
    private static readonly DependencyPropertyKey ProminentPaddingPropertyKey = DependencyProperty.RegisterReadOnly(nameof(ProminentPadding), typeof(Thickness), typeof(Card), new PropertyMetadata(new Thickness(24)));
    public static readonly DependencyProperty ProminentPaddingProperty = ProminentPaddingPropertyKey.DependencyProperty;
    private static readonly DependencyPropertyKey HeadingSizePropertyKey = DependencyProperty.RegisterReadOnly(nameof(HeadingSize), typeof(double), typeof(Card), new PropertyMetadata(28d));
    public static readonly DependencyProperty HeadingSizeProperty = HeadingSizePropertyKey.DependencyProperty;
    public Thickness ProminentPadding => (Thickness)GetValue(ProminentPaddingProperty);
    public double HeadingSize => (double)GetValue(HeadingSizeProperty);
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(Card), new PropertyMetadata(""));
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(nameof(Description), typeof(string), typeof(Card), new PropertyMetadata(""));
    public static readonly DependencyProperty ActionsProperty = DependencyProperty.Register(nameof(Actions), typeof(object), typeof(Card), new PropertyMetadata(null));
    public static readonly DependencyProperty SideContentProperty = DependencyProperty.Register(nameof(SideContent), typeof(object), typeof(Card), new PropertyMetadata(null));
    public static readonly DependencyProperty ToneProperty = DependencyProperty.Register(nameof(Tone), typeof(PresentationTone), typeof(Card), new PropertyMetadata(PresentationTone.Primary));
    public static readonly DependencyProperty ProminentProperty = DependencyProperty.Register(nameof(Prominent), typeof(bool), typeof(Card), new PropertyMetadata(false));
    public static readonly DependencyProperty StackedProperty = DependencyProperty.Register(nameof(Stacked), typeof(bool), typeof(Card), new PropertyMetadata(false));
    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string Description { get => (string)GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }
    public object? Actions { get => GetValue(ActionsProperty); set => SetValue(ActionsProperty, value); }
    public object? SideContent { get => GetValue(SideContentProperty); set => SetValue(SideContentProperty, value); }
    public PresentationTone Tone { get => (PresentationTone)GetValue(ToneProperty); set => SetValue(ToneProperty, value); }
    public bool Prominent { get => (bool)GetValue(ProminentProperty); set => SetValue(ProminentProperty, value); }
    public bool Stacked { get => (bool)GetValue(StackedProperty); set => SetValue(StackedProperty, value); }
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == PresentationContext.IsPrimaryProperty || e.Property == PresentationContext.IsAccessFormProperty) UpdateCardGeometry();
    }
    private void UpdateCardGeometry()
    {
        var width = ActualWidth > 0 ? ActualWidth : 960;
        SetValue(ProminentPaddingPropertyKey, new Thickness(Math.Clamp(width * .04, 24, 48)));
        SetValue(HeadingSizePropertyKey, PresentationContext.GetIsPrimary(this) && PresentationContext.GetIsAccessForm(this) ? Math.Clamp(width * .038, 30, 44) : 28d);
    }
}
public class IdentityCard : Card { static IdentityCard() => DefaultStyleKeyProperty.OverrideMetadata(typeof(IdentityCard), new FrameworkPropertyMetadata(typeof(IdentityCard))); }
public class Section : Card { static Section() => DefaultStyleKeyProperty.OverrideMetadata(typeof(Section), new FrameworkPropertyMetadata(typeof(Section))); }
public class PageHeading : Card
{
    static PageHeading() => DefaultStyleKeyProperty.OverrideMetadata(typeof(PageHeading), new FrameworkPropertyMetadata(typeof(PageHeading)));
    public PageHeading() { Loaded += (_, _) => Dispatcher.BeginInvoke(AttachSticky); Unloaded += (_, _) => DetachSticky(); }
    public static readonly DependencyProperty CompactProperty = DependencyProperty.Register(nameof(Compact), typeof(bool), typeof(PageHeading), new PropertyMetadata(false, (d, e) => ((PageHeading)d).UpdateCompact()));
    public static readonly DependencyProperty StickyProperty = DependencyProperty.Register(nameof(Sticky), typeof(bool), typeof(PageHeading), new PropertyMetadata(true, (d, e) => { var heading = (PageHeading)d; if (heading.IsLoaded) heading.AttachSticky(); }));
    private static readonly DependencyPropertyKey EffectiveCompactPropertyKey = DependencyProperty.RegisterReadOnly(nameof(EffectiveCompact), typeof(bool), typeof(PageHeading), new PropertyMetadata(false));
    public static readonly DependencyProperty EffectiveCompactProperty = EffectiveCompactPropertyKey.DependencyProperty;
    public bool Compact { get => (bool)GetValue(CompactProperty); set => SetValue(CompactProperty, value); }
    public bool Sticky { get => (bool)GetValue(StickyProperty); set => SetValue(StickyProperty, value); }
    public bool EffectiveCompact => (bool)GetValue(EffectiveCompactProperty);
    private ScrollViewer? scroll;
    private TranslateTransform? translation;
    private Transform? previousTransform;
    private int previousZIndex;
    private bool automaticCompact;
    private void UpdateCompact() => SetValue(EffectiveCompactPropertyKey, Compact || automaticCompact);
    private void AttachSticky()
    {
        DetachSticky(); if (!IsLoaded || !Sticky) return;
        // Only the page's leading heading participates. Example cards and nested surfaces keep ordinary layout.
        if (VisualTreeHelper.GetParent(this) is Panel immediate && immediate.Children.IndexOf(this) != 0) return;
        var pageBodyFound = false;
        for (var parent = VisualTreeHelper.GetParent(this); parent is not null; parent = VisualTreeHelper.GetParent(parent))
        {
            if (parent is Card) return;
            if (parent is PageBody) pageBodyFound = true;
            if (parent is not ScrollViewer candidate) continue;
            if (!pageBodyFound)
            {
                var direct = VisualTreeHelper.GetParent(this) as Panel;
                if (direct is null || !direct.Children.OfType<PageBody>().Any()) return;
            }
            if (!ReferenceEquals(candidate.Style, TryFindResource("Flourish.ScrollViewer"))) return;
            scroll = candidate; break;
        }
        if (scroll is null) return;
        previousTransform = RenderTransform; previousZIndex = Panel.GetZIndex(this);
        translation = new TranslateTransform(); var group = new TransformGroup();
        if (previousTransform is not null && !previousTransform.Value.IsIdentity) group.Children.Add(previousTransform);
        group.Children.Add(translation); SetCurrentValue(RenderTransformProperty, group); Panel.SetZIndex(this, 15);
        scroll.ScrollChanged += OnScrollChanged; UpdateSticky();
    }
    private void OnScrollChanged(object sender, ScrollChangedEventArgs args) { if (ReferenceEquals(args.OriginalSource, scroll)) UpdateSticky(); }
    private void UpdateSticky()
    {
        if (scroll is null || translation is null || !IsArrangeValid) return;
        var documentTop = TransformToAncestor(scroll).Transform(new Point()).Y - translation.Y + scroll.VerticalOffset;
        translation.Y = Math.Max(0, scroll.VerticalOffset - documentTop);
        // The same hysteresis as Blazor shell.js prevents chatter around the collapse point.
        if (automaticCompact && scroll.VerticalOffset < 24) automaticCompact = false;
        else if (!automaticCompact && scroll.VerticalOffset > 96) automaticCompact = true;
        UpdateCompact();
    }
    private void DetachSticky()
    {
        if (scroll is not null) scroll.ScrollChanged -= OnScrollChanged;
        if (translation is not null) { SetCurrentValue(RenderTransformProperty, previousTransform ?? Transform.Identity); Panel.SetZIndex(this, previousZIndex); }
        scroll = null; translation = null; previousTransform = null; automaticCompact = false; UpdateCompact();
    }
}
public class PageBody : ContentControl
{
    static PageBody() => DefaultStyleKeyProperty.OverrideMetadata(typeof(PageBody), new FrameworkPropertyMetadata(typeof(PageBody)));
    public static readonly DependencyProperty FillHeightProperty = DependencyProperty.Register(nameof(FillHeight), typeof(bool), typeof(PageBody), new PropertyMetadata(false));
    public static readonly DependencyProperty CompactSpacingProperty = DependencyProperty.Register(nameof(CompactSpacing), typeof(bool), typeof(PageBody), new PropertyMetadata(false));
    public static readonly DependencyProperty FullWidthProperty = DependencyProperty.Register(nameof(FullWidth), typeof(bool), typeof(PageBody), new PropertyMetadata(false));
    public bool FillHeight { get => (bool)GetValue(FillHeightProperty); set => SetValue(FillHeightProperty, value); }
    public bool CompactSpacing { get => (bool)GetValue(CompactSpacingProperty); set => SetValue(CompactSpacingProperty, value); }
    public bool FullWidth { get => (bool)GetValue(FullWidthProperty); set => SetValue(FullWidthProperty, value); }
}
public class ContentContainer : ContentControl { static ContentContainer() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentContainer), new FrameworkPropertyMetadata(typeof(ContentContainer))); }
public class ContentSurface : ContentControl { static ContentSurface() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentSurface), new FrameworkPropertyMetadata(typeof(ContentSurface))); }
public class NavigationSurface : ContentSurface { }
public class ShellHeader : Card { static ShellHeader() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ShellHeader), new FrameworkPropertyMetadata(typeof(ShellHeader))); }
public class UniformGridItem : Card
{
    static UniformGridItem() => DefaultStyleKeyProperty.OverrideMetadata(typeof(UniformGridItem), new FrameworkPropertyMetadata(typeof(UniformGridItem)));
    public static readonly DependencyProperty GridVariantProperty = DependencyProperty.Register(nameof(GridVariant), typeof(UniformGridVariant), typeof(UniformGridItem), new PropertyMetadata(UniformGridVariant.Elevated));
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(string), typeof(UniformGridItem), new PropertyMetadata(""));
    public static readonly DependencyProperty IconSupportProperty = DependencyProperty.Register(nameof(IconSupport), typeof(bool?), typeof(UniformGridItem), new PropertyMetadata(null));
    private static readonly DependencyPropertyKey SupportsIconPropertyKey = DependencyProperty.RegisterReadOnly(nameof(SupportsIcon), typeof(bool), typeof(UniformGridItem), new PropertyMetadata(false));
    public static readonly DependencyProperty SupportsIconProperty = SupportsIconPropertyKey.DependencyProperty;
    private static readonly DependencyPropertyKey InGridPropertyKey = DependencyProperty.RegisterReadOnly(nameof(InGrid), typeof(bool), typeof(UniformGridItem), new PropertyMetadata(false));
    public static readonly DependencyProperty InGridProperty = InGridPropertyKey.DependencyProperty;
    public UniformGridVariant GridVariant { get => (UniformGridVariant)GetValue(GridVariantProperty); set => SetValue(GridVariantProperty, value); }
    public string Icon { get => (string)GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public bool? IconSupport { get => (bool?)GetValue(IconSupportProperty); set => SetValue(IconSupportProperty, value); }
    public bool InGrid => (bool)GetValue(InGridProperty);
    public bool SupportsIcon => (bool)GetValue(SupportsIconProperty);
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == IconProperty || e.Property == IconSupportProperty) SetValue(SupportsIconPropertyKey, IconSupport ?? !string.IsNullOrWhiteSpace(Icon));
    }
    protected override void OnVisualParentChanged(DependencyObject oldParent)
    {
        base.OnVisualParentChanged(oldParent);
        var grid = VisualTreeHelper.GetParent(this) as UniformGrid;
        SetValue(InGridPropertyKey, grid is not null);
        // The owning grid supplies the same variant as for UniformGridButton. An explicit item variant wins.
        if (grid is not null && (ReadLocalValue(GridVariantProperty) == DependencyProperty.UnsetValue || BindingOperations.GetBinding(this, GridVariantProperty)?.Source is UniformGrid))
            SetBinding(GridVariantProperty, new Binding(nameof(UniformGrid.Variant)) { Source = grid });
        else if (grid is null && BindingOperations.GetBinding(this, GridVariantProperty)?.Source is UniformGrid)
            ClearValue(GridVariantProperty);
    }
}
public class PresentationBand : Card { static PresentationBand() => DefaultStyleKeyProperty.OverrideMetadata(typeof(PresentationBand), new FrameworkPropertyMetadata(typeof(PresentationBand))); }
public class PresentationHero : PresentationBand
{
    static PresentationHero() => DefaultStyleKeyProperty.OverrideMetadata(typeof(PresentationHero), new FrameworkPropertyMetadata(typeof(PresentationHero)));
    public PresentationHero() { SizeChanged += (_, _) => UpdateGeometry(); UpdateGeometry(); }
    public static readonly DependencyProperty StackTitleWordsProperty = DependencyProperty.Register(nameof(StackTitleWords), typeof(bool), typeof(PresentationHero), new PropertyMetadata(false));
    public static readonly DependencyProperty SubtitleProperty = DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(PresentationHero), new PropertyMetadata(""));
    private static readonly DependencyPropertyKey TitleSizePropertyKey = DependencyProperty.RegisterReadOnly(nameof(TitleSize), typeof(double), typeof(PresentationHero), new PropertyMetadata(72d));
    public static readonly DependencyProperty TitleSizeProperty = TitleSizePropertyKey.DependencyProperty;
    private static readonly DependencyPropertyKey SubtitleSizePropertyKey = DependencyProperty.RegisterReadOnly(nameof(SubtitleSize), typeof(double), typeof(PresentationHero), new PropertyMetadata(32d));
    public static readonly DependencyProperty SubtitleSizeProperty = SubtitleSizePropertyKey.DependencyProperty;
    private static readonly DependencyPropertyKey LayoutPaddingPropertyKey = DependencyProperty.RegisterReadOnly(nameof(LayoutPadding), typeof(Thickness), typeof(PresentationHero), new PropertyMetadata(new Thickness(24, 48, 24, 48)));
    public static readonly DependencyProperty LayoutPaddingProperty = LayoutPaddingPropertyKey.DependencyProperty;
    private static readonly DependencyPropertyKey CopyGapPropertyKey = DependencyProperty.RegisterReadOnly(nameof(CopyGap), typeof(Thickness), typeof(PresentationHero), new PropertyMetadata(new Thickness(0, 40, 0, 0)));
    public static readonly DependencyProperty CopyGapProperty = CopyGapPropertyKey.DependencyProperty;
    private static readonly DependencyPropertyKey CopyInsetPropertyKey = DependencyProperty.RegisterReadOnly(nameof(CopyInset), typeof(Thickness), typeof(PresentationHero), new PropertyMetadata(new Thickness()));
    public static readonly DependencyProperty CopyInsetProperty = CopyInsetPropertyKey.DependencyProperty;
    private static readonly DependencyPropertyKey IsSplitPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsSplit), typeof(bool), typeof(PresentationHero), new PropertyMetadata(false));
    public static readonly DependencyProperty IsSplitProperty = IsSplitPropertyKey.DependencyProperty;
    public bool StackTitleWords { get => (bool)GetValue(StackTitleWordsProperty); set => SetValue(StackTitleWordsProperty, value); }
    public string Subtitle { get => (string)GetValue(SubtitleProperty); set => SetValue(SubtitleProperty, value); }
    public double TitleSize => (double)GetValue(TitleSizeProperty);
    public double SubtitleSize => (double)GetValue(SubtitleSizeProperty);
    public Thickness LayoutPadding => (Thickness)GetValue(LayoutPaddingProperty);
    public Thickness CopyGap => (Thickness)GetValue(CopyGapProperty);
    public Thickness CopyInset => (Thickness)GetValue(CopyInsetProperty);
    public bool IsSplit => (bool)GetValue(IsSplitProperty);
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == SideContentProperty) UpdateGeometry();
    }
    private void UpdateGeometry()
    {
        var width = ActualWidth > 0 ? ActualWidth : 1180;
        var split = SideContent is not null;
        var title = split ? width <= 560 ? Math.Clamp(width * .16, 52, 72) : width <= 860 ? Math.Clamp(width * .12, 58, 88) : Math.Clamp(width * .095, 64, 112)
            : width <= 560 ? Math.Clamp(width * .16, 56, 82) : Math.Clamp(width * .11, 72, 128);
        SetValue(IsSplitPropertyKey, split); SetValue(TitleSizePropertyKey, title); SetValue(SubtitleSizePropertyKey, Math.Clamp(width * .035, 32, 44));
        var horizontal = width <= 560 ? 18 : Math.Clamp(width * .05, 24, 72);
        var vertical = split ? Math.Clamp(width * .09, 58, 116) : 48;
        SetValue(LayoutPaddingPropertyKey, new Thickness(horizontal, vertical, horizontal, vertical));
        SetValue(CopyGapPropertyKey, new Thickness(0, split ? 24 : 40, 0, 0));
        SetValue(CopyInsetPropertyKey, new Thickness(0, split && width > 860 ? Math.Clamp(width * .04, 28, 42) : 0, 0, 0));
    }
}
public class PresentationFooter : Card { static PresentationFooter() => DefaultStyleKeyProperty.OverrideMetadata(typeof(PresentationFooter), new FrameworkPropertyMetadata(typeof(PresentationFooter))); }
public class OfferCard : Card
{
    static OfferCard() => DefaultStyleKeyProperty.OverrideMetadata(typeof(OfferCard), new FrameworkPropertyMetadata(typeof(OfferCard)));
    public static readonly DependencyProperty IdProperty = DependencyProperty.Register(nameof(Id), typeof(string), typeof(OfferCard), new PropertyMetadata(""));
    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(OfferCard), new PropertyMetadata(false));
    private static readonly DependencyPropertyKey IsEnhancedPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsEnhanced), typeof(bool), typeof(OfferCard), new PropertyMetadata(false));
    public static readonly DependencyProperty IsEnhancedProperty = IsEnhancedPropertyKey.DependencyProperty;
    public bool IsEnhanced { get => (bool)GetValue(IsEnhancedProperty); internal set => SetValue(IsEnhancedPropertyKey, value); }
    public string Id { get => (string)GetValue(IdProperty); set => SetValue(IdProperty, value); }
    public bool IsActive { get => (bool)GetValue(IsActiveProperty); internal set => SetValue(IsActiveProperty, value); }
}
public class AccessBrand : Card { static AccessBrand() => DefaultStyleKeyProperty.OverrideMetadata(typeof(AccessBrand), new FrameworkPropertyMetadata(typeof(AccessBrand))); }
public class AccessPanel : Card
{
    static AccessPanel() => DefaultStyleKeyProperty.OverrideMetadata(typeof(AccessPanel), new FrameworkPropertyMetadata(typeof(AccessPanel)));
    public static readonly DependencyProperty BrandProperty = DependencyProperty.Register(nameof(Brand), typeof(object), typeof(AccessPanel), new PropertyMetadata(null));
    public static readonly DependencyProperty WideProperty = DependencyProperty.Register(nameof(Wide), typeof(bool), typeof(AccessPanel), new PropertyMetadata(false));
    public static readonly DependencyProperty EmphasizedProperty = DependencyProperty.Register(nameof(Emphasized), typeof(bool), typeof(AccessPanel), new PropertyMetadata(false));
    public object? Brand { get => GetValue(BrandProperty); set => SetValue(BrandProperty, value); }
    public bool Wide { get => (bool)GetValue(WideProperty); set => SetValue(WideProperty, value); }
    public bool Emphasized { get => (bool)GetValue(EmphasizedProperty); set => SetValue(EmphasizedProperty, value); }
}
public class AccessFormSurface : Card
{
    static AccessFormSurface() => DefaultStyleKeyProperty.OverrideMetadata(typeof(AccessFormSurface), new FrameworkPropertyMetadata(typeof(AccessFormSurface)));
    private Window? viewport;
    public AccessFormSurface()
    {
        SizeChanged += (_, _) => UpdateGeometry();
        Loaded += (_, _) => { viewport = Window.GetWindow(this); if (viewport is not null) viewport.SizeChanged += ViewportChanged; UpdateGeometry(); };
        Unloaded += (_, _) => { if (viewport is not null) viewport.SizeChanged -= ViewportChanged; viewport = null; };
        UpdateGeometry();
    }
    private void ViewportChanged(object sender, SizeChangedEventArgs e) => UpdateGeometry();
    private static readonly DependencyPropertyKey SurfacePaddingPropertyKey = DependencyProperty.RegisterReadOnly(nameof(SurfacePadding), typeof(Thickness), typeof(AccessFormSurface), new PropertyMetadata(new Thickness(40)));
    public static readonly DependencyProperty SurfacePaddingProperty = SurfacePaddingPropertyKey.DependencyProperty;
    private static readonly DependencyPropertyKey SurfaceRadiusPropertyKey = DependencyProperty.RegisterReadOnly(nameof(SurfaceRadius), typeof(CornerRadius), typeof(AccessFormSurface), new PropertyMetadata(new CornerRadius(28)));
    public static readonly DependencyProperty SurfaceRadiusProperty = SurfaceRadiusPropertyKey.DependencyProperty;
    public Thickness SurfacePadding => (Thickness)GetValue(SurfacePaddingProperty);
    public CornerRadius SurfaceRadius => (CornerRadius)GetValue(SurfaceRadiusProperty);
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) { base.OnPropertyChanged(e); if (e.Property == ToneProperty) UpdateGeometry(); }
    private void UpdateGeometry()
    {
        var width = viewport?.ActualWidth > 0 ? viewport.ActualWidth : ActualWidth > 0 ? ActualWidth : 960;
        var narrow = width <= (Tone == PresentationTone.Primary ? 560 : 760);
        SetValue(SurfaceRadiusPropertyKey, new CornerRadius(narrow ? 22 : 28));
        SetValue(SurfacePaddingPropertyKey, narrow ? new Thickness(18, 24, 18, 24) : new Thickness(Tone == PresentationTone.Primary ? Math.Clamp(width * .04, 28, 42) : Math.Clamp(width * .05, 26, 40)));
    }
}
public class AccessActions : InlineActions { }
public class AttributionFooter : Card { static AttributionFooter() => DefaultStyleKeyProperty.OverrideMetadata(typeof(AttributionFooter), new FrameworkPropertyMetadata(typeof(AttributionFooter))); }
public class CopyText : System.Windows.Controls.TextBox
{
    static CopyText() => DefaultStyleKeyProperty.OverrideMetadata(typeof(CopyText), new FrameworkPropertyMetadata(typeof(CopyText)));
    public CopyText() { IsReadOnly = true; TextWrapping = TextWrapping.Wrap; }
}
public class CodeBlock : CopyText { static CodeBlock() => DefaultStyleKeyProperty.OverrideMetadata(typeof(CodeBlock), new FrameworkPropertyMetadata(typeof(CodeBlock))); }
public class LogoDisplayer : Card
{
    static LogoDisplayer() => DefaultStyleKeyProperty.OverrideMetadata(typeof(LogoDisplayer), new FrameworkPropertyMetadata(typeof(LogoDisplayer)));
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(ImageSource), typeof(LogoDisplayer), new PropertyMetadata(null));
    public static readonly DependencyProperty ShowNameProperty = DependencyProperty.Register(nameof(ShowName), typeof(bool), typeof(LogoDisplayer), new PropertyMetadata(true));
    public static readonly DependencyProperty ArtisticProperty = DependencyProperty.Register(nameof(Artistic), typeof(bool), typeof(LogoDisplayer), new PropertyMetadata(false));
    public ImageSource? Source { get => (ImageSource?)GetValue(SourceProperty); set => SetValue(SourceProperty, value); }
    public bool ShowName { get => (bool)GetValue(ShowNameProperty); set => SetValue(ShowNameProperty, value); }
    public bool Artistic { get => (bool)GetValue(ArtisticProperty); set => SetValue(ArtisticProperty, value); }
}
public class ImagePreview : Card
{
    static ImagePreview() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ImagePreview), new FrameworkPropertyMetadata(typeof(ImagePreview)));
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(ImageSource), typeof(ImagePreview), new PropertyMetadata(null));
    public static readonly DependencyProperty AltProperty = DependencyProperty.Register(nameof(Alt), typeof(string), typeof(ImagePreview), new PropertyMetadata(""));
    public ImageSource? Source { get => (ImageSource?)GetValue(SourceProperty); set => SetValue(SourceProperty, value); }
    public string Alt { get => (string)GetValue(AltProperty); set => SetValue(AltProperty, value); }
}
public class DisplayBoard : Card
{
    static DisplayBoard() => DefaultStyleKeyProperty.OverrideMetadata(typeof(DisplayBoard), new FrameworkPropertyMetadata(typeof(DisplayBoard)));
    public static readonly DependencyProperty CopyValueProperty = DependencyProperty.Register(nameof(CopyValue), typeof(string), typeof(DisplayBoard), new PropertyMetadata(""));
    public string CopyValue { get => (string)GetValue(CopyValueProperty); set => SetValue(CopyValueProperty, value); }
    private Button? copy;
    public override void OnApplyTemplate()
    {
        if (copy is not null) copy.Click -= Copy;
        base.OnApplyTemplate(); copy = GetTemplateChild("PART_Copy") as Button;
        if (copy is not null) copy.Click += Copy;
    }
    private void Copy(object sender, RoutedEventArgs e) { Clipboard.SetText(CopyValue); if (copy is not null) copy.Text = "Copied"; }
}
public class EmptyState : Card
{
    static EmptyState() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EmptyState), new FrameworkPropertyMetadata(typeof(EmptyState)));
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(nameof(Variant), typeof(EmptyStateVariant), typeof(EmptyState), new PropertyMetadata(EmptyStateVariant.Standard));
    public EmptyStateVariant Variant { get => (EmptyStateVariant)GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
}
public class Notice : Card
{
    static Notice() => DefaultStyleKeyProperty.OverrideMetadata(typeof(Notice), new FrameworkPropertyMetadata(typeof(Notice)));
    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(nameof(Kind), typeof(NoticeKind), typeof(Notice), new PropertyMetadata(NoticeKind.Information));
    public NoticeKind Kind { get => (NoticeKind)GetValue(KindProperty); set => SetValue(KindProperty, value); }
}
public class LoadingState : Card
{
    static LoadingState() => DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingState), new FrameworkPropertyMetadata(typeof(LoadingState)));
}
public class InteractionBoundary : ContentControl
{
    public static readonly DependencyProperty BusyProperty = DependencyProperty.Register(nameof(Busy), typeof(bool), typeof(InteractionBoundary), new PropertyMetadata(false, (d, e) => d.CoerceValue(IsEnabledProperty)));
    public bool Busy { get => (bool)GetValue(BusyProperty); set => SetValue(BusyProperty, value); }
    protected override bool IsEnabledCore => base.IsEnabledCore && !Busy;
}
public class Disclosure : Expander { static Disclosure() => DefaultStyleKeyProperty.OverrideMetadata(typeof(Disclosure), new FrameworkPropertyMetadata(typeof(Disclosure))); }
public class ToggleSection : HeaderedContentControl
{
    static ToggleSection() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSection), new FrameworkPropertyMetadata(typeof(ToggleSection)));
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(bool), typeof(ToggleSection), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) => ((ToggleSection)d).ValueChanged?.Invoke(d, (bool)e.NewValue)));
    public bool Value { get => (bool)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public event EventHandler<bool>? ValueChanged;
}
public class ExpansionIndicator : FrameworkElement
{
    public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(typeof(ExpansionIndicator), new FrameworkPropertyMetadata(SystemColors.ControlTextBrush, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty ExpandedProperty = DependencyProperty.Register(nameof(Expanded), typeof(bool), typeof(ExpansionIndicator), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
    public Brush Foreground { get => (Brush)GetValue(ForegroundProperty); set => SetValue(ForegroundProperty, value); }
    public bool Expanded { get => (bool)GetValue(ExpandedProperty); set => SetValue(ExpandedProperty, value); }
    public ExpansionIndicator() { Width = 10; Height = 6; Focusable = false; }
    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        var geometry = Geometry.Parse(Expanded ? "M 0 6 L 10 6 L 5 0 Z" : "M 0 0 L 10 0 L 5 6 Z");
        drawingContext.DrawGeometry(Foreground, null, geometry);
    }
}
public class BackToTop : Button
{
    public BackToTop() { Text = "↑"; Variant = ButtonVariant.Quiet; ToolTip = "Back to top"; Click += (_, _) => Ancestor<ScrollViewer>(this)?.ScrollToTop(); }
    internal static T? Ancestor<T>(DependencyObject child) where T : DependencyObject
    {
        for (var parent = VisualTreeHelper.GetParent(child); parent is not null; parent = VisualTreeHelper.GetParent(parent)) if (parent is T found) return found;
        return null;
    }
}
public class SectionNavigator : StackPanel
{
    public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(nameof(Target), typeof(FrameworkElement), typeof(SectionNavigator), new PropertyMetadata(null, (d, e) => ((SectionNavigator)d).Refresh()));
    public FrameworkElement? Target { get => (FrameworkElement?)GetValue(TargetProperty); set => SetValue(TargetProperty, value); }
    public SectionNavigator() { Loaded += (_, _) => Refresh(); }
    public void Refresh()
    {
        Children.Clear(); if (Target is null) return;
        foreach (var section in Descendants(Target).OfType<Section>())
        {
            var link = new Button { Text = section.Title, Variant = ButtonVariant.Quiet, ToolTip = section.Title };
            link.Click += (_, _) => section.BringIntoView(); Children.Add(link);
        }
    }
    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++) { var child = VisualTreeHelper.GetChild(root, i); yield return child; foreach (var descendant in Descendants(child)) yield return descendant; }
    }
}
public class NavigationChoices : UniformGrid
{
    public IEnumerable<NavigationChoiceItem>? Items
    {
        set
        {
            Children.Clear(); foreach (var item in value ?? [])
            {
                var button = new UniformGridButton { Text = item.Label, Disabled = item.Disabled };
                button.Click += (_, _) => Navigate?.Invoke(this, item.Target); Children.Add(button);
            }
        }
    }
    public event EventHandler<string>? Navigate;
}
public class ServiceMenu : ActionMenu
{
    public IEnumerable<ServiceLink>? Links { set => Actions = value?.Select(link => new MenuAction(link.Label, () => { Navigate?.Invoke(this, link.Href); return Task.CompletedTask; })).ToArray(); }
    public event EventHandler<string>? Navigate;
}

/// <summary>Template layout for Card's ordinary, prominent and stacked variants.</summary>
internal sealed class CardLayout : Panel
{
    public CardLayout() => KeyboardNavigation.SetTabNavigation(this, KeyboardNavigationMode.Local);
    public static readonly DependencyProperty ProminentProperty = DependencyProperty.Register(nameof(Prominent), typeof(bool), typeof(CardLayout), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));
    public static readonly DependencyProperty StackedProperty = DependencyProperty.Register(nameof(Stacked), typeof(bool), typeof(CardLayout), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));
    public static readonly DependencyProperty ViewportWidthProperty = DependencyProperty.Register(nameof(ViewportWidth), typeof(double), typeof(CardLayout), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure));
    public bool Prominent { get => (bool)GetValue(ProminentProperty); set => SetValue(ProminentProperty, value); }
    public bool Stacked { get => (bool)GetValue(StackedProperty); set => SetValue(StackedProperty, value); }
    public double ViewportWidth { get => (double)GetValue(ViewportWidthProperty); set => SetValue(ViewportWidthProperty, value); }
    private bool Horizontal(double width) => Prominent && !Stacked && (ViewportWidth > 0 ? ViewportWidth : width) > 760;
    protected override Size MeasureOverride(Size available)
    {
        if (InternalChildren.Count != 2) return new Size();
        var copy = InternalChildren[0]; var actions = InternalChildren[1];
        KeyboardNavigation.SetTabNavigation(copy, KeyboardNavigationMode.Local); KeyboardNavigation.SetTabNavigation(actions, KeyboardNavigationMode.Local);
        KeyboardNavigation.SetTabIndex(copy, Prominent && !Stacked ? 1 : 0); KeyboardNavigation.SetTabIndex(actions, Prominent && !Stacked ? 0 : 1);
        var gap = actions.Visibility == Visibility.Collapsed ? 0 : Prominent ? 32 : 16;
        var width = double.IsFinite(available.Width) ? available.Width : 960;
        if (Horizontal(width))
        {
            var actionWidth = actions.Visibility == Visibility.Collapsed ? 0 : Math.Min(240, width);
            actions.Measure(new Size(actionWidth, double.PositiveInfinity));
            copy.Measure(new Size(Math.Max(0, width - actionWidth - gap), double.PositiveInfinity));
            return new Size(width, Math.Max(copy.DesiredSize.Height, actions.DesiredSize.Height));
        }
        copy.Measure(new Size(width, double.PositiveInfinity)); actions.Measure(new Size(width, double.PositiveInfinity));
        return new Size(Math.Max(copy.DesiredSize.Width, actions.DesiredSize.Width), copy.DesiredSize.Height + actions.DesiredSize.Height + gap);
    }
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (InternalChildren.Count != 2) return finalSize;
        var copy = InternalChildren[0]; var actions = InternalChildren[1];
        var gap = actions.Visibility == Visibility.Collapsed ? 0 : Prominent ? 32 : 16;
        if (Horizontal(finalSize.Width))
        {
            var actionWidth = actions.Visibility == Visibility.Collapsed ? 0 : Math.Min(240, finalSize.Width);
            actions.Arrange(new Rect(0, Math.Max(0, (finalSize.Height - actions.DesiredSize.Height) / 2), actionWidth, actions.DesiredSize.Height));
            copy.Arrange(new Rect(actionWidth + gap, Math.Max(0, (finalSize.Height - copy.DesiredSize.Height) / 2), Math.Max(0, finalSize.Width - actionWidth - gap), copy.DesiredSize.Height));
        }
        else
        {
            var actionsFirst = Prominent && !Stacked;
            var first = actionsFirst ? actions : copy; var last = actionsFirst ? copy : actions;
            first.Arrange(new Rect(0, 0, finalSize.Width, first.DesiredSize.Height));
            last.Arrange(new Rect(0, first.DesiredSize.Height + gap, finalSize.Width, last.DesiredSize.Height));
        }
        return finalSize;
    }
}

/// <summary>Template layout for the asymmetric hero. The complete surface width owns breakpoints.</summary>
internal sealed class HeroLayout : Panel
{
    public static readonly DependencyProperty IsSplitProperty = DependencyProperty.Register(nameof(IsSplit), typeof(bool), typeof(HeroLayout), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));
    public static readonly DependencyProperty ViewportWidthProperty = DependencyProperty.Register(nameof(ViewportWidth), typeof(double), typeof(HeroLayout), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure));
    public bool IsSplit { get => (bool)GetValue(IsSplitProperty); set => SetValue(IsSplitProperty, value); }
    public double ViewportWidth { get => (double)GetValue(ViewportWidthProperty); set => SetValue(ViewportWidthProperty, value); }
    private bool Horizontal => IsSplit && ViewportWidth > 860;
    private double Gap => Horizontal ? Math.Clamp(ViewportWidth * .09, 54, 120) : 48;
    protected override Size MeasureOverride(Size available)
    {
        if (InternalChildren.Count != 2) return new Size();
        var width = double.IsFinite(available.Width) ? available.Width : 1180;
        var main = InternalChildren[0]; var side = InternalChildren[1];
        if (!IsSplit) { main.Measure(new Size(Math.Min(920, width), double.PositiveInfinity)); side.Measure(new Size(0, 0)); return new Size(width, main.DesiredSize.Height); }
        var sideWidth = Horizontal ? Math.Min(470, Math.Max(0, width - Gap)) : width;
        side.Measure(new Size(sideWidth, double.PositiveInfinity));
        main.Measure(new Size(Horizontal ? Math.Max(0, width - sideWidth - Gap) : width, double.PositiveInfinity));
        return new Size(width, Horizontal ? Math.Max(main.DesiredSize.Height, side.DesiredSize.Height) : main.DesiredSize.Height + Gap + side.DesiredSize.Height);
    }
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (InternalChildren.Count != 2) return finalSize;
        var main = InternalChildren[0]; var side = InternalChildren[1];
        if (!IsSplit) { main.Arrange(new Rect(0, 0, Math.Min(920, finalSize.Width), main.DesiredSize.Height)); side.Arrange(new Rect()); }
        else if (Horizontal)
        {
            var sideWidth = Math.Min(470, Math.Max(0, finalSize.Width - Gap)); var mainWidth = Math.Max(0, finalSize.Width - sideWidth - Gap);
            main.Arrange(new Rect(0, 0, mainWidth, main.DesiredSize.Height)); side.Arrange(new Rect(mainWidth + Gap, 0, sideWidth, side.DesiredSize.Height));
        }
        else { main.Arrange(new Rect(0, 0, finalSize.Width, main.DesiredSize.Height)); side.Arrange(new Rect(0, main.DesiredSize.Height + Gap, finalSize.Width, side.DesiredSize.Height)); }
        return finalSize;
    }
}

/// <summary>Template-owned pulse lifetime. Removing the clock restores the current role/style opacity immediately.</summary>
internal static class ProgressPulse
{
    public static readonly DependencyProperty RunningProperty = DependencyProperty.RegisterAttached("Running", typeof(bool), typeof(ProgressPulse), new PropertyMetadata(false, Changed));
    public static bool GetRunning(DependencyObject element) => (bool)element.GetValue(RunningProperty);
    public static void SetRunning(DependencyObject element, bool value) => element.SetValue(RunningProperty, value);
    private static void Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        if (sender is not FrameworkElement element) return;
        element.Loaded -= Loaded; element.Unloaded -= Unloaded;
        element.Loaded += Loaded; element.Unloaded += Unloaded;
        Update(element);
    }
    private static void Loaded(object sender, RoutedEventArgs args) { if (sender is FrameworkElement element) Update(element); }
    private static void Unloaded(object sender, RoutedEventArgs args) { if (sender is FrameworkElement element) element.BeginAnimation(UIElement.OpacityProperty, null); }
    private static void Update(FrameworkElement element)
    {
        element.BeginAnimation(UIElement.OpacityProperty, GetRunning(element)
            ? new DoubleAnimation(.35, 1, TimeSpan.FromSeconds(.9)) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever }
            : null);
    }
}
