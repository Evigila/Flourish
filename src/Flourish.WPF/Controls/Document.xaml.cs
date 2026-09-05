using System;

using ArkheideSystem.Flourish.Abstract;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using WpfBinding = System.Windows.Data.Binding;
using WpfTextBlock = System.Windows.Controls.TextBlock;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Presents multiple <see cref="Paragraph" /> elements at the Large reading size within a
/// rounded, lightly outlined surface.
/// </summary>
[ContentProperty(nameof(Items))]
public class Document : ItemsControl
{
    private const string FirstLineIndent = "    ";

    static Document()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Document),
            new FrameworkPropertyMetadata(typeof(Document))
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
        if (item is not Paragraph)
        {
            throw new InvalidOperationException(
                $"{nameof(Document)} accepts only {nameof(Paragraph)} items."
            );
        }

        base.PrepareContainerForItemOverride(element, item);

        if (
            item is Paragraph paragraph
            && paragraph.ReadLocalValue(WpfTextBlock.TextWrappingProperty)
                == DependencyProperty.UnsetValue
        )
        {
            paragraph.SetCurrentValue(WpfTextBlock.TextWrappingProperty, TextWrapping.Wrap);
        }

        if (element is ContentPresenter container && item is Paragraph source)
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
        if (element is ContentPresenter container && item is Paragraph)
        {
            container.ClearValue(ContentPresenter.ContentProperty);
        }

        base.ClearContainerForItemOverride(element, item);
    }

    private WpfTextBlock CreateTextProxy(Paragraph source)
    {
        var proxy = new WpfTextBlock { Name = "DocumentParagraphTextProxy" };

        Bind(
            proxy,
            WpfTextBlock.TextProperty,
            source,
            WpfTextBlock.TextProperty,
            PrefixFirstLineConverter.Instance
        );
        Bind(proxy, WpfTextBlock.FontFamilyProperty, source, WpfTextBlock.FontFamilyProperty);
        Bind(proxy, WpfTextBlock.FontSizeProperty, this, FontSizeProperty);
        Bind(proxy, WpfTextBlock.FontStretchProperty, source, WpfTextBlock.FontStretchProperty);
        Bind(proxy, WpfTextBlock.FontStyleProperty, source, WpfTextBlock.FontStyleProperty);
        Bind(proxy, WpfTextBlock.FontWeightProperty, source, WpfTextBlock.FontWeightProperty);
        Bind(proxy, WpfTextBlock.ForegroundProperty, source, WpfTextBlock.ForegroundProperty);
        Bind(proxy, WpfTextBlock.BackgroundProperty, source, WpfTextBlock.BackgroundProperty);
        Bind(proxy, WpfTextBlock.PaddingProperty, source, WpfTextBlock.PaddingProperty);
        Bind(proxy, WpfTextBlock.BaselineOffsetProperty, source, WpfTextBlock.BaselineOffsetProperty);
        Bind(proxy, WpfTextBlock.LineHeightProperty, source, WpfTextBlock.LineHeightProperty);
        Bind(
            proxy,
            WpfTextBlock.LineStackingStrategyProperty,
            source,
            WpfTextBlock.LineStackingStrategyProperty
        );
        Bind(proxy, WpfTextBlock.TextAlignmentProperty, source, WpfTextBlock.TextAlignmentProperty);
        Bind(proxy, WpfTextBlock.TextDecorationsProperty, source, WpfTextBlock.TextDecorationsProperty);
        Bind(proxy, WpfTextBlock.TextTrimmingProperty, source, WpfTextBlock.TextTrimmingProperty);
        Bind(proxy, WpfTextBlock.TextWrappingProperty, source, WpfTextBlock.TextWrappingProperty);
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
