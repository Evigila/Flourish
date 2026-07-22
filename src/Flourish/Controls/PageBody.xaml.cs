using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Hosts the vertically stacked body of a navigated Flourish page in a scrolling viewport.
/// </summary>
/// <remarks>
/// Add page elements through <see cref="Children" /> or as direct XAML content. Do not replace
/// the inherited <see cref="ContentControl.Content" /> value; the Flourish Shell owns that value
/// while it applies the configured page-width constraint.
/// </remarks>
[ContentProperty(nameof(Children))]
public sealed class PageBody : ScrollViewer
{
    private readonly StackPanel _panel = new();
    private readonly PageElementCollection _children;

    static PageBody()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(PageBody),
            new FrameworkPropertyMetadata(typeof(PageBody))
        );
    }

    /// <summary>Initializes a new instance of the <see cref="PageBody" /> class.</summary>
    public PageBody()
    {
        _children = new PageElementCollection(_panel);
        _panel.SetResourceReference(
            FrameworkElement.MarginProperty,
            "FlourishPageContentMargin"
        );
        Content = _panel;
    }

    /// <summary>Gets the page elements arranged vertically inside the scrolling viewport.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public Collection<UIElement> Children => _children;

    private sealed class PageElementCollection : Collection<UIElement>
    {
        private readonly StackPanel panel;

        internal PageElementCollection(StackPanel panel)
        {
            this.panel = panel;
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, UIElement item)
        {
            ArgumentNullException.ThrowIfNull(item);
            ValidateItem(index, item, replacingIndex: null);
            base.InsertItem(index, item);
            panel.Children.Insert(index, item);
        }

        /// <inheritdoc />
        protected override void SetItem(int index, UIElement item)
        {
            ArgumentNullException.ThrowIfNull(item);
            ValidateItem(index, item, replacingIndex: index);
            panel.Children[index] = item;
            base.SetItem(index, item);
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            panel.Children.RemoveAt(index);
            base.RemoveItem(index);
        }

        /// <inheritdoc />
        protected override void ClearItems()
        {
            panel.Children.Clear();
            base.ClearItems();
        }

        private void ValidateItem(int index, UIElement item, int? replacingIndex)
        {
            if (item is not Chunk && item is not HeaderChunk)
            {
                throw new ArgumentException(
                    $"{nameof(PageBody)} accepts only {nameof(HeaderChunk)} and {nameof(Chunk)} children.",
                    nameof(item)
                );
            }

            if (item is HeaderChunk)
            {
                if (index != 0)
                {
                    throw new InvalidOperationException(
                        $"{nameof(HeaderChunk)} must be the first child of {nameof(PageBody)}."
                    );
                }

                if (
                    this.Where((_, existingIndex) => existingIndex != replacingIndex)
                        .Any(element => element is HeaderChunk)
                )
                {
                    throw new InvalidOperationException(
                        $"{nameof(PageBody)} can contain only one {nameof(HeaderChunk)}."
                    );
                }

                return;
            }

            if (
                index == 0
                && this.Where((_, existingIndex) => existingIndex != replacingIndex)
                    .Any(element => element is HeaderChunk)
            )
            {
                throw new InvalidOperationException(
                    $"{nameof(HeaderChunk)} must remain the first child of {nameof(PageBody)}."
                );
            }
        }
    }
}
