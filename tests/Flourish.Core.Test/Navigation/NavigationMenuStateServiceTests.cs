using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Navigation;

using Xunit;

namespace ArkheideSystem.Flourish.Core.Test.Navigation;

public sealed class NavigationMenuStateServiceTests
{
    [Fact]
    public void Current_StartsWithAnImmutableEmptyVersionZeroSnapshot()
    {
        var sut = new NavigationMenuStateService();

        NavigationMenuSnapshot current = sut.Current;

        Assert.Empty(current.Groups);
        Assert.Empty(current.FixedItems);
        Assert.Equal(0, current.Version);
        Assert.Throws<NotSupportedException>(() =>
            Assert.IsAssignableFrom<ICollection<NavigationMenuGroup>>(current.Groups)
                .Add(new NavigationMenuGroup("late", null, []))
        );
    }

    [Fact]
    public void Set_BatchesEditsIntoOneVersionAndOneTransition()
    {
        var sut = new NavigationMenuStateService();
        StateTransitionEventArgs<NavigationMenuSnapshot>? transition = null;
        int changes = 0;
        sut.Changed += (_, args) =>
        {
            changes++;
            transition = args;
        };

        sut.Set(editor =>
        {
            editor.AddGroup("main", "Main");
            editor.AddItem("main", Page("home", "Home"));
            editor.AddFixedItem(Command("settings"));
            editor.SetItemExpanded("home", true);
        });

        Assert.Equal(1, changes);
        Assert.NotNull(transition);
        Assert.Equal(0, transition.Previous.Version);
        Assert.Empty(transition.Previous.Groups);
        Assert.Same(sut.Current, transition.Current);
        Assert.Equal(1, sut.Current.Version);
        Assert.True(sut.Current.Groups[0].Items[0].IsExpanded);
    }

    [Fact]
    public void Set_NoOpKeepsSnapshotIdentityVersionAndEvents()
    {
        var sut = SeededService();
        NavigationMenuSnapshot before = sut.Current;
        int changes = 0;
        sut.Changed += (_, _) => changes++;

        sut.Set(editor =>
        {
            editor.SetGroupTitle("main", "Main");
            editor.SetGroupOrder("main", 0);
            editor.SetItemVisible("home", true);
            Assert.False(editor.RemoveItem("missing"));
            Assert.False(editor.RemoveGroup("missing"));
        });

        Assert.Same(before, sut.Current);
        Assert.Equal(1, sut.Current.Version);
        Assert.Equal(0, changes);
    }

    [Fact]
    public void Set_CallbackFailureRollsBackEveryEdit()
    {
        var sut = SeededService();
        NavigationMenuSnapshot before = sut.Current;

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() =>
            sut.Set(editor =>
            {
                editor.SetGroupTitle("main", "Changed");
                editor.AddFixedItem(Command("temporary"));
                throw new InvalidOperationException("callback failed");
            })
        );

        Assert.Equal("callback failed", error.Message);
        Assert.Same(before, sut.Current);
        Assert.Equal("Main", sut.Current.Groups[0].Title);
        Assert.DoesNotContain(sut.Current.FixedItems, item => item.Id == "temporary");
    }

    [Fact]
    public void Set_FinalValidationFailureRollsBackEveryEdit()
    {
        var sut = SeededService();
        NavigationMenuSnapshot before = sut.Current;
        int changes = 0;
        sut.Changed += (_, _) => changes++;

        Assert.Throws<InvalidOperationException>(() =>
            sut.Set(editor =>
            {
                editor.AddGroup("other");
                editor.AddItem("other", Page("duplicate-route", "Home"));
            })
        );

        Assert.Same(before, sut.Current);
        Assert.Single(sut.Current.Groups);
        Assert.Equal(0, changes);
    }

    [Fact]
    public void Editor_CannotBeUsedAfterTransactionCompletes()
    {
        var sut = new NavigationMenuStateService();
        INavigationMenuEditor? captured = null;
        sut.Set(editor =>
        {
            captured = editor;
            editor.AddGroup("main");
        });

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() =>
            captured!.AddGroup("late")
        );

        Assert.Contains("transaction completes", error.Message, StringComparison.Ordinal);
        Assert.Single(sut.Current.Groups);
    }

    [Fact]
    public void Set_ReentrantTransactionIsRejectedWithoutLosingState()
    {
        var sut = SeededService();
        NavigationMenuSnapshot before = sut.Current;

        Assert.Throws<InvalidOperationException>(() =>
            sut.Set(editor =>
            {
                editor.AddGroup("outer");
                sut.Set(inner => inner.AddGroup("inner"));
            })
        );

        Assert.Same(before, sut.Current);
        Assert.Equal("main", Assert.Single(sut.Current.Groups).Id);
    }

    [Fact]
    public void GroupOperations_ControlInsertionOrderingTitleAndRemoval()
    {
        var sut = new NavigationMenuStateService();
        sut.Set(editor =>
        {
            editor.AddGroup("third", "Old");
            editor.SetGroupIndex("first", 0, "First");
            editor.SetGroupIndex("second", 1, "Second");
            editor.AddItem("second", Command("removed-with-group"));
            editor.SetGroupTitle("third", "Third");
            editor.SetGroupOrder("third", 1);
            Assert.True(editor.RemoveGroup("second"));
        });

        Assert.Equal(["first", "third"], sut.Current.Groups.Select(group => group.Id));
        Assert.Equal(["First", "Third"], sut.Current.Groups.Select(group => group.Title));
        Assert.Empty(sut.Current.Groups.SelectMany(group => group.Items));
    }

    [Fact]
    public void ItemInsertionOperations_PreserveGroupAndFixedOrdering()
    {
        var sut = new NavigationMenuStateService();
        sut.Set(editor =>
        {
            editor.AddGroup("main");
            editor.AddItem("main", Command("third"));
            editor.SetItemIndex("main", Command("first"), 0);
            editor.SetItemIndex("main", Command("second"), 1);
            editor.AddFixedItem(Command("fixed-second"));
            editor.SetFixedItemIndex(Command("fixed-first"), 0);
        });

        Assert.Equal(
            ["first", "second", "third"],
            sut.Current.Groups[0].Items.Select(item => item.Id)
        );
        Assert.Equal(
            ["fixed-first", "fixed-second"],
            sut.Current.FixedItems.Select(item => item.Id)
        );
    }

    [Fact]
    public void SetItem_ReplacesInPlaceAndCanMoveBetweenSections()
    {
        var sut = new NavigationMenuStateService();
        sut.Set(editor =>
        {
            editor.AddGroup("main");
            editor.AddItem("main", Command("first"));
            editor.AddItem("main", Command("target"));
            editor.SetItem("main", Command("target") with { Label = "Updated" });
            editor.SetItem(null, Command("target") with { Label = "Fixed" }, isFixed: true);
        });

        Assert.Equal("first", Assert.Single(sut.Current.Groups[0].Items).Id);
        NavigationMenuItem fixedItem = Assert.Single(sut.Current.FixedItems);
        Assert.Equal("target", fixedItem.Id);
        Assert.Equal("Fixed", fixedItem.Label);
    }

    [Fact]
    public void SetItemPosition_MovesParentAndItsChildrenAsOneSubtree()
    {
        var sut = new NavigationMenuStateService();
        sut.Set(editor =>
        {
            editor.AddGroup("source");
            editor.AddGroup("target");
            editor.AddItem("source", Command("parent"));
            editor.AddItem("source", Command("unrelated"));
            editor.AddItem("source", Command("child") with { ParentId = "parent" });
            editor.SetItemPosition("parent", "target", 0);
        });

        Assert.Equal("unrelated", Assert.Single(sut.Current.Groups[0].Items).Id);
        Assert.Equal(
            ["parent", "child"],
            sut.Current.Groups[1].Items.Select(item => item.Id)
        );
    }

    [Fact]
    public void SetItemPosition_CanMoveAnIndependentItemToFixedSection()
    {
        var sut = SeededService();

        sut.Set(editor => editor.SetItemPosition("home", null, 0, isFixed: true));

        Assert.Empty(sut.Current.Groups[0].Items);
        Assert.Equal("home", Assert.Single(sut.Current.FixedItems).Id);
    }

    [Fact]
    public void RemoveItem_RemovesParentAndImmediateChildren()
    {
        var sut = new NavigationMenuStateService();
        sut.Set(editor =>
        {
            editor.AddGroup("main");
            editor.AddItem("main", Command("parent"));
            editor.AddItem("main", Command("child") with { ParentId = "parent" });
            editor.AddItem("main", Command("survivor"));
        });

        sut.Set(editor => Assert.True(editor.RemoveItem("parent")));

        Assert.Equal("survivor", Assert.Single(sut.Current.Groups[0].Items).Id);
    }

    [Fact]
    public void ItemUpdates_ApplyTransformationAndPresentationFlags()
    {
        var sut = SeededService();

        sut.Set(editor =>
        {
            editor.SetItem("home", item => item with { Label = "Start" });
            editor.SetItemVisible("home", false);
            editor.SetItemEnabled("home", false);
            editor.SetItemExpanded("home", true);
        });

        NavigationMenuItem item = Assert.Single(sut.Current.Groups[0].Items);
        Assert.Equal("Start", item.Label);
        Assert.False(item.IsVisible);
        Assert.False(item.IsEnabled);
        Assert.True(item.IsExpanded);
    }

    [Fact]
    public void StableIdsAndPageKeys_AreComparedOrdinally()
    {
        var sut = new NavigationMenuStateService();

        sut.Set(editor =>
        {
            editor.AddGroup("Main");
            editor.AddGroup("main");
            editor.AddItem("Main", Page("Home", "Home"));
            editor.AddItem("main", Page("home", "home"));
        });

        Assert.Equal(["Main", "main"], sut.Current.Groups.Select(group => group.Id));
        Assert.Equal(
            ["Home", "home"],
            sut.Current.Groups.SelectMany(group => group.Items).Select(item => item.Id)
        );
    }

    [Fact]
    public void DuplicateItemIdAcrossGroupAndFixedSection_IsRejectedAtomically()
    {
        var sut = new NavigationMenuStateService();

        Assert.Throws<InvalidOperationException>(() =>
            sut.Set(editor =>
            {
                editor.AddGroup("main");
                editor.AddItem("main", Command("same"));
                editor.AddFixedItem(Command("same"));
            })
        );

        Assert.Equal(0, sut.Current.Version);
        Assert.Empty(sut.Current.Groups);
    }

    [Fact]
    public void DuplicateGroupIdWithMatchingCase_IsRejected()
    {
        var sut = new NavigationMenuStateService();

        Assert.Throws<InvalidOperationException>(() =>
            sut.Set(editor =>
            {
                editor.AddGroup("main");
                editor.AddGroup("main");
            })
        );

        Assert.Equal(0, sut.Current.Version);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ParentMustExistInTheSameGroupOrFixedSection(bool fixedSection)
    {
        var sut = new NavigationMenuStateService();

        Assert.Throws<InvalidOperationException>(() =>
            sut.Set(editor =>
            {
                editor.AddGroup("main");
                editor.AddItem("main", Command("parent"));
                NavigationMenuItem child = Command("child") with { ParentId = "parent" };
                if (fixedSection)
                {
                    editor.AddFixedItem(child);
                }
                else
                {
                    editor.AddGroup("other");
                    editor.AddItem("other", child);
                }
            })
        );

        Assert.Equal(0, sut.Current.Version);
    }

    [Fact]
    public void NavigationTree_RejectsMoreThanOneParentChildLevel()
    {
        var sut = new NavigationMenuStateService();

        Assert.Throws<InvalidOperationException>(() =>
            sut.Set(editor =>
            {
                editor.AddGroup("main");
                editor.AddItem("main", Command("root"));
                editor.AddItem("main", Command("child") with { ParentId = "root" });
                editor.AddItem("main", Command("grandchild") with { ParentId = "child" });
            })
        );

        Assert.Equal(0, sut.Current.Version);
    }

    [Fact]
    public void NavigationTree_RejectsSelfParenting()
    {
        var sut = new NavigationMenuStateService();

        Assert.Throws<InvalidOperationException>(() =>
            sut.Set(editor =>
            {
                editor.AddGroup("main");
                editor.AddItem("main", Command("self") with { ParentId = "self" });
            })
        );

        Assert.Equal(0, sut.Current.Version);
    }

    [Fact]
    public void ItemTransformation_CannotChangeStableId()
    {
        var sut = SeededService();
        NavigationMenuSnapshot before = sut.Current;

        Assert.Throws<InvalidOperationException>(() =>
            sut.Set(editor =>
                editor.SetItem("home", item => item with { Id = "renamed" })
            )
        );

        Assert.Same(before, sut.Current);
    }

    [Theory]
    [InlineData(null, "Home")]
    [InlineData("", "Home")]
    [InlineData("Home", "")]
    public void PageItem_RequiresNavigationKeyAndLabel(string? navigationKey, string label)
    {
        var sut = new NavigationMenuStateService();

        Assert.Throws<InvalidOperationException>(() =>
            sut.Set(editor =>
            {
                editor.AddGroup("main");
                editor.AddItem(
                    "main",
                    new NavigationMenuItem(
                        "page",
                        label,
                        NavigationMenuItemKind.Page,
                        navigationKey: navigationKey
                    )
                );
            })
        );

        Assert.Equal(0, sut.Current.Version);
    }

    [Fact]
    public void InvalidPosition_RollsBackRemovalOfExistingItem()
    {
        var sut = SeededService();
        NavigationMenuSnapshot before = sut.Current;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.Set(editor => editor.SetItemPosition("home", "main", 5))
        );

        Assert.Same(before, sut.Current);
        Assert.Equal("home", Assert.Single(sut.Current.Groups[0].Items).Id);
    }

    [Fact]
    public void ChangedObserverFailureOccursAfterStateCommits()
    {
        var sut = new NavigationMenuStateService();
        sut.Changed += (_, _) => throw new InvalidOperationException("observer failed");

        Assert.Throws<InvalidOperationException>(() =>
            sut.Set(editor => editor.AddGroup("main"))
        );

        Assert.Equal(1, sut.Current.Version);
        Assert.Equal("main", Assert.Single(sut.Current.Groups).Id);
    }

    [Fact]
    public void ConcurrentTransactions_CommitConsistentUniqueState()
    {
        var sut = new NavigationMenuStateService();
        int changes = 0;
        sut.Changed += (_, _) => Interlocked.Increment(ref changes);

        Parallel.For(
            0,
            32,
            index => sut.Set(editor => editor.AddGroup($"group:{index}"))
        );

        Assert.Equal(32, sut.Current.Version);
        Assert.Equal(32, sut.Current.Groups.Count);
        Assert.Equal(32, sut.Current.Groups.Select(group => group.Id).Distinct().Count());
        Assert.Equal(32, Volatile.Read(ref changes));
    }

    private static NavigationMenuStateService SeededService()
    {
        var sut = new NavigationMenuStateService();
        sut.Set(editor =>
        {
            editor.AddGroup("main", "Main");
            editor.AddItem("main", Page("home", "Home"));
        });
        return sut;
    }

    private static NavigationMenuItem Page(string id, string navigationKey)
    {
        return NavigationMenuItem.Page(id, navigationKey, id);
    }

    private static NavigationMenuItem Command(string id)
    {
        return NavigationMenuItem.Command(id, id);
    }
}
