using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using FlourishTextBlock = ArkheideSystem.Flourish.Controls.FlourishTextBlock;
using WpfPanel = System.Windows.Controls.Panel;
using WpfStackPanel = System.Windows.Controls.StackPanel;

namespace ArkheideSystem.Flourish.Internal.Interaction;

/// <summary>
/// Reconciles status item snapshots with stable WPF views instead of rebuilding the panel.
/// </summary>
internal sealed class StatusItemViewCache(WpfPanel host)
{
    private static readonly Thickness FirstItemMargin = new();
    private static readonly Thickness FollowingItemMargin = new(14, 0, 0, 0);
    private readonly Dictionary<string, StatusItemView> viewsById = new(StringComparer.Ordinal);
    private long appliedVersion = -1;

    /// <summary>Builds or reconciles the complete authoritative snapshot.</summary>
    internal void Synchronize(FlourishStatusBarSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        SynchronizeCore(snapshot.Items);
        appliedVersion = snapshot.Version;
    }

    /// <summary>
    /// Applies a newer change, falling back to keyed reconciliation when an event was skipped.
    /// </summary>
    /// <returns><see langword="true" /> when the snapshot was applied.</returns>
    internal bool Apply(FlourishStatusBarChangedEventArgs change)
    {
        ArgumentNullException.ThrowIfNull(change);
        var snapshot = change.Current;
        if (snapshot.Version <= appliedVersion)
        {
            return false;
        }

        if (
            snapshot.Version != appliedVersion + 1
            || change.ChangeKind == FlourishRuntimeChangeKind.Reset
            || !TryApplyIncremental(change)
        )
        {
            SynchronizeCore(snapshot.Items);
        }

        appliedVersion = snapshot.Version;
        return true;
    }

    private bool TryApplyIncremental(FlourishStatusBarChangedEventArgs change)
    {
        if (change.ItemId is null)
        {
            return change.ChangeKind == FlourishRuntimeChangeKind.Updated;
        }

        var itemId = change.ItemId;
        switch (change.ChangeKind)
        {
            case FlourishRuntimeChangeKind.Added:
            case FlourishRuntimeChangeKind.Updated:
            case FlourishRuntimeChangeKind.Moved:
                if (!TryFindItem(change.Current.Items, itemId, out var item, out var visibleIndex))
                {
                    return false;
                }

                return Upsert(item, visibleIndex);

            case FlourishRuntimeChangeKind.Removed:
                if (ContainsItem(change.Current.Items, itemId))
                {
                    return false;
                }

                return Remove(itemId);

            default:
                return false;
        }
    }

    private void SynchronizeCore(IReadOnlyList<FlourishStatusItem> items)
    {
        var activeIds = new HashSet<string>(items.Count, StringComparer.Ordinal);
        var desiredViews = new List<UIElement>(items.Count);
        foreach (var item in items)
        {
            activeIds.Add(item.Id);
            var view = GetOrCreate(item);
            Update(view, item);
            if (item.IsVisible)
            {
                desiredViews.Add(view.Root);
            }
        }

        SynchronizeChildren(desiredViews);

        foreach (var staleId in viewsById.Keys.Where(id => !activeIds.Contains(id)).ToArray())
        {
            viewsById.Remove(staleId);
        }

        UpdateMargins();
    }

    private bool Upsert(FlourishStatusItem item, int visibleIndex)
    {
        var view = GetOrCreate(item);
        Update(view, item);

        var existingIndex = host.Children.IndexOf(view.Root);
        if (!item.IsVisible)
        {
            if (existingIndex >= 0)
            {
                host.Children.RemoveAt(existingIndex);
                UpdateMargins();
            }

            return true;
        }

        if (existingIndex == visibleIndex)
        {
            return true;
        }

        if (existingIndex >= 0)
        {
            host.Children.RemoveAt(existingIndex);
        }

        if (visibleIndex < 0 || visibleIndex > host.Children.Count)
        {
            return false;
        }

        host.Children.Insert(visibleIndex, view.Root);
        UpdateMargins();
        return true;
    }

    private bool Remove(string itemId)
    {
        if (!viewsById.Remove(itemId, out var view))
        {
            return false;
        }

        var existingIndex = host.Children.IndexOf(view.Root);
        if (existingIndex >= 0)
        {
            host.Children.RemoveAt(existingIndex);
            UpdateMargins();
        }

        return true;
    }

    private StatusItemView GetOrCreate(FlourishStatusItem item)
    {
        if (viewsById.TryGetValue(item.Id, out var view))
        {
            return view;
        }

        var iconText = new FlourishTextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
        };
        BindResource(iconText, FlourishTextBlock.FontFamilyProperty, "FlourishIconFontFamily");
        BindResource(iconText, FlourishTextBlock.FontSizeProperty, "FlourishIconFontSizeStatusBar");
        BindResource(
            iconText,
            FlourishTextBlock.LineHeightProperty,
            "FlourishIconFontSizeStatusBar"
        );
        BindResource(
            iconText,
            FlourishTextBlock.ForegroundProperty,
            "FlourishNeutralForeground2Brush"
        );

        var labelText = new FlourishTextBlock
        {
            Margin = new Thickness(5, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
        };
        BindResource(labelText, FlourishTextBlock.FontSizeProperty, "FlourishFontSizeSmall");
        BindResource(labelText, FlourishTextBlock.LineHeightProperty, "FlourishLineHeightSmall");
        BindResource(
            labelText,
            FlourishTextBlock.ForegroundProperty,
            "FlourishNeutralForeground2Brush"
        );

        var root = new WpfStackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
        };
        root.Children.Add(iconText);
        root.Children.Add(labelText);

        view = new StatusItemView(root, iconText, labelText);
        viewsById.Add(item.Id, view);
        return view;
    }

    private static void Update(StatusItemView view, FlourishStatusItem item)
    {
        if (!StringComparer.Ordinal.Equals(view.Icon.Text, item.IconGlyph))
        {
            view.Icon.Text = item.IconGlyph;
        }

        if (!StringComparer.Ordinal.Equals(view.Label.Text, item.Text))
        {
            view.Label.Text = item.Text;
        }
    }

    private static void BindResource(
        FrameworkElement element,
        DependencyProperty property,
        string resourceKey
    )
    {
        element.SetResourceReference(property, resourceKey);
    }

    private static bool TryFindItem(
        IReadOnlyList<FlourishStatusItem> items,
        string itemId,
        out FlourishStatusItem item,
        out int visibleIndex
    )
    {
        visibleIndex = 0;
        foreach (var candidate in items)
        {
            if (StringComparer.Ordinal.Equals(candidate.Id, itemId))
            {
                item = candidate;
                return true;
            }

            if (candidate.IsVisible)
            {
                visibleIndex++;
            }
        }

        item = null!;
        return false;
    }

    private static bool ContainsItem(IReadOnlyList<FlourishStatusItem> items, string itemId)
    {
        foreach (var item in items)
        {
            if (StringComparer.Ordinal.Equals(item.Id, itemId))
            {
                return true;
            }
        }

        return false;
    }

    private void SynchronizeChildren(IReadOnlyList<UIElement> desiredChildren)
    {
        for (var index = 0; index < desiredChildren.Count; index++)
        {
            var desired = desiredChildren[index];
            if (index < host.Children.Count && ReferenceEquals(host.Children[index], desired))
            {
                continue;
            }

            var existingIndex = host.Children.IndexOf(desired);
            if (existingIndex >= 0)
            {
                host.Children.RemoveAt(existingIndex);
            }

            host.Children.Insert(index, desired);
        }

        while (host.Children.Count > desiredChildren.Count)
        {
            host.Children.RemoveAt(host.Children.Count - 1);
        }
    }

    private void UpdateMargins()
    {
        for (var index = 0; index < host.Children.Count; index++)
        {
            var element = (FrameworkElement)host.Children[index];
            var margin = index == 0 ? FirstItemMargin : FollowingItemMargin;
            if (element.Margin != margin)
            {
                element.Margin = margin;
            }
        }
    }

    private sealed record StatusItemView(
        WpfStackPanel Root,
        FlourishTextBlock Icon,
        FlourishTextBlock Label
    );
}
