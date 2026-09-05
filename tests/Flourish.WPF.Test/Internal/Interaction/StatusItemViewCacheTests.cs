using System.Linq;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Views.Windows;
using ArkheideSystem.Flourish.WPF.Test.Infrastructure;

using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Controls;
using TextBlock = ArkheideSystem.Flourish.Controls.TextBlock;

namespace ArkheideSystem.Flourish.WPF.Test.Internal.Interaction;

public sealed class StatusItemViewCacheTests
{
    [Fact]
    public void RepeatedUpdatesReuseViewsAndResourceBindings()
    {
        StaTest.Run(() =>
        {
            var host = new StackPanel();
            var sut = new StatusItemViewCache(host);
            var first = Item("first", "Initial", "I");
            var second = Item("second", "Stable", "S");
            sut.Synchronize(Snapshot(0, first, second));

            var firstRoot = Assert.IsType<StackPanel>(host.Children[0]);
            var firstIcon = Assert.IsType<TextBlock>(firstRoot.Children[0]);
            var firstLabel = Assert.IsType<TextBlock>(firstRoot.Children[1]);
            var secondRoot = host.Children[1];

            Assert.Equal(TextRole.Icon, firstIcon.Role);
            Assert.Equal(TextRole.Body, firstLabel.Role);

            for (var version = 1; version <= 100; version++)
            {
                first = first with { Text = $"Update {version}", IconGlyph = version.ToString() };
                Assert.True(
                    sut.Apply(
                        Change(version, CollectionChangeKind.Updated, first.Id, first, second)
                    )
                );
            }

            Assert.Same(firstRoot, host.Children[0]);
            Assert.Same(secondRoot, host.Children[1]);
            Assert.Same(firstIcon, firstRoot.Children[0]);
            Assert.Same(firstLabel, firstRoot.Children[1]);
            Assert.Equal("100", firstIcon.Text);
            Assert.Equal("Update 100", firstLabel.Text);
            Assert.True(
                DependencyPropertyHelper
                    .GetValueSource(firstIcon, TextBlock.FontFamilyProperty)
                    .IsExpression
            );
            Assert.True(
                DependencyPropertyHelper
                    .GetValueSource(firstLabel, TextBlock.FontSizeProperty)
                    .IsExpression
            );
        });
    }

    [Fact]
    public void VisibilityMoveRemoveAndResetPreserveOnlyCurrentViews()
    {
        StaTest.Run(() =>
        {
            var host = new StackPanel();
            var sut = new StatusItemViewCache(host);
            var first = Item("first", "First", "1");
            var second = Item("second", "Second", "2");
            var third = Item("third", "Third", "3");
            sut.Synchronize(Snapshot(0, first, second, third));

            var firstRoot = Assert.IsType<StackPanel>(host.Children[0]);
            var secondRoot = Assert.IsType<StackPanel>(host.Children[1]);
            var thirdRoot = Assert.IsType<StackPanel>(host.Children[2]);

            first = first with { IsVisible = false };
            Assert.True(
                sut.Apply(
                    Change(1, CollectionChangeKind.Updated, first.Id, first, second, third)
                )
            );
            Assert.Equal([secondRoot, thirdRoot], host.Children.Cast<UIElement>());
            Assert.Equal(new Thickness(), secondRoot.Margin);
            Assert.Equal(new Thickness(14, 0, 0, 0), thirdRoot.Margin);

            Assert.True(
                sut.Apply(
                    Change(2, CollectionChangeKind.Moved, third.Id, third, second, first)
                )
            );
            Assert.Equal([thirdRoot, secondRoot], host.Children.Cast<UIElement>());
            Assert.Equal(new Thickness(), thirdRoot.Margin);
            Assert.Equal(new Thickness(14, 0, 0, 0), secondRoot.Margin);

            first = first with { IsVisible = true };
            Assert.True(
                sut.Apply(
                    Change(3, CollectionChangeKind.Updated, first.Id, third, second, first)
                )
            );
            Assert.Equal([thirdRoot, secondRoot, firstRoot], host.Children.Cast<UIElement>());

            Assert.True(
                sut.Apply(Change(4, CollectionChangeKind.Removed, second.Id, third, first))
            );
            Assert.Equal([thirdRoot, firstRoot], host.Children.Cast<UIElement>());

            var replacement = Item("replacement", "Replacement", "R");
            Assert.True(
                sut.Apply(Change(5, CollectionChangeKind.Reset, itemId: null, replacement))
            );
            var replacementRoot = Assert.IsType<StackPanel>(Assert.Single(host.Children));
            Assert.NotSame(firstRoot, replacementRoot);

            Assert.True(
                sut.Apply(
                    Change(6, CollectionChangeKind.Updated, first.Id, replacement, first)
                )
            );
            Assert.NotSame(firstRoot, host.Children[1]);
        });
    }

    [Fact]
    public void StaleEventsAreIgnoredAndVersionGapsReconcileTheSnapshot()
    {
        StaTest.Run(() =>
        {
            var host = new StackPanel();
            var sut = new StatusItemViewCache(host);
            var first = Item("first", "First", "1");
            sut.Synchronize(Snapshot(2, first));
            var firstRoot = host.Children[0];

            var stale = first with { Text = "Stale" };
            Assert.False(sut.Apply(Change(1, CollectionChangeKind.Updated, first.Id, stale)));
            Assert.Same(firstRoot, host.Children[0]);
            Assert.Equal("First", GetLabel(host.Children[0]).Text);

            var replacement = Item("replacement", "Replacement", "R");
            Assert.True(
                sut.Apply(Change(4, CollectionChangeKind.Added, replacement.Id, replacement))
            );
            var replacementRoot = Assert.Single(host.Children.Cast<UIElement>());
            Assert.NotSame(firstRoot, replacementRoot);

            Assert.True(
                sut.Apply(Change(5, CollectionChangeKind.Updated, itemId: null, replacement))
            );
            Assert.Same(replacementRoot, host.Children[0]);
            Assert.False(
                sut.Apply(Change(3, CollectionChangeKind.Reset, itemId: null, first))
            );
            Assert.Same(replacementRoot, host.Children[0]);
        });
    }

    private static StatusBarItem Item(string id, string text, string iconGlyph)
    {
        return new StatusBarItem(id, text, iconGlyph);
    }

    private static StatusBarSnapshot Snapshot(
        long version,
        params StatusBarItem[] items
    )
    {
        return new StatusBarSnapshot(true, true, true, items, version);
    }

    private static StatusBarChangedEventArgs Change(
        long version,
        CollectionChangeKind kind,
        string? itemId,
        params StatusBarItem[] items
    )
    {
        return new StatusBarChangedEventArgs(Snapshot(version, items), kind, itemId);
    }

    private static TextBlock GetLabel(UIElement root)
    {
        return Assert.IsType<TextBlock>(Assert.IsType<StackPanel>(root).Children[1]);
    }
}
