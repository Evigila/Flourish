using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using WpfBinding = System.Windows.Data.Binding;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Presents multiple text blocks at the Large reading size within a rounded, lightly outlined surface.
/// </summary>
[ContentProperty(nameof(Items))]
public class Paragraph : ItemsControl
{
    private const string FirstLineIndent = "    ";

    static Paragraph()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Paragraph),
            new FrameworkPropertyMetadata(typeof(Paragraph))
        );
    }

    /// <inheritdoc />
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new ContentPresenter();
    }

    /// <inheritdoc />
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is ContentPresenter;
    }

    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(
        DependencyObject element,
        object item
    )
    {
        base.PrepareContainerForItemOverride(element, item);

        if (
            item is TextBlock textBlock
            && textBlock.ReadLocalValue(TextBlock.TextWrappingProperty)
                == DependencyProperty.UnsetValue
        )
        {
            textBlock.SetCurrentValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
        }

        if (element is ContentPresenter container && item is TextBlock source)
        {
            container.SetValue(
                ContentPresenter.ContentProperty,
                CreateTextProxy(source)
            );
        }
    }

    /// <inheritdoc />
    protected override void ClearContainerForItemOverride(
        DependencyObject element,
        object item
    )
    {
        if (element is ContentPresenter container && item is TextBlock)
        {
            container.ClearValue(ContentPresenter.ContentProperty);
        }

        base.ClearContainerForItemOverride(element, item);
    }

    private TextBlock CreateTextProxy(TextBlock source)
    {
        var proxy = new TextBlock { Name = "ParagraphTextProxy" };

        Bind(
            proxy,
            TextBlock.TextProperty,
            source,
            TextBlock.TextProperty,
            PrefixFirstLineConverter.Instance
        );
        Bind(proxy, TextBlock.FontFamilyProperty, source, TextBlock.FontFamilyProperty);
        Bind(proxy, TextBlock.FontSizeProperty, this, FontSizeProperty);
        Bind(proxy, TextBlock.FontStretchProperty, source, TextBlock.FontStretchProperty);
        Bind(proxy, TextBlock.FontStyleProperty, source, TextBlock.FontStyleProperty);
        Bind(proxy, TextBlock.FontWeightProperty, source, TextBlock.FontWeightProperty);
        Bind(proxy, TextBlock.ForegroundProperty, source, TextBlock.ForegroundProperty);
        Bind(proxy, TextBlock.BackgroundProperty, source, TextBlock.BackgroundProperty);
        Bind(proxy, TextBlock.PaddingProperty, source, TextBlock.PaddingProperty);
        Bind(proxy, TextBlock.BaselineOffsetProperty, source, TextBlock.BaselineOffsetProperty);
        Bind(proxy, TextBlock.LineHeightProperty, source, TextBlock.LineHeightProperty);
        Bind(
            proxy,
            TextBlock.LineStackingStrategyProperty,
            source,
            TextBlock.LineStackingStrategyProperty
        );
        Bind(proxy, TextBlock.TextAlignmentProperty, source, TextBlock.TextAlignmentProperty);
        Bind(proxy, TextBlock.TextDecorationsProperty, source, TextBlock.TextDecorationsProperty);
        Bind(proxy, TextBlock.TextTrimmingProperty, source, TextBlock.TextTrimmingProperty);
        Bind(proxy, TextBlock.TextWrappingProperty, source, TextBlock.TextWrappingProperty);
        Bind(proxy, FlowDirectionProperty, source, FlowDirectionProperty);
        Bind(proxy, LanguageProperty, source, LanguageProperty);
        Bind(
            proxy,
            AutomationProperties.AutomationIdProperty,
            source,
            AutomationProperties.AutomationIdProperty
        );
        Bind(
            proxy,
            AutomationProperties.HelpTextProperty,
            source,
            AutomationProperties.HelpTextProperty
        );
        Bind(
            proxy,
            AutomationProperties.NameProperty,
            source,
            AutomationProperties.NameProperty
        );

        return proxy;
    }

    private static void Bind(
        DependencyObject target,
        DependencyProperty targetProperty,
        DependencyObject source,
        DependencyProperty sourceProperty,
        IValueConverter? converter = null
    )
    {
        BindingOperations.SetBinding(
            target,
            targetProperty,
            new WpfBinding
            {
                Converter = converter,
                Mode = BindingMode.OneWay,
                Path = new PropertyPath(sourceProperty),
                Source = source,
            }
        );
    }

    private sealed class PrefixFirstLineConverter : IValueConverter
    {
        public static PrefixFirstLineConverter Instance { get; } = new();

        public object Convert(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture
        )
        {
            return value is string { Length: > 0 } text
                ? FirstLineIndent + text
                : string.Empty;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture
        )
        {
            return WpfBinding.DoNothing;
        }
    }
}
