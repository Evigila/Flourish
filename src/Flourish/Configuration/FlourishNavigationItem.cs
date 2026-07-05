using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using AcksheedSys.Flourish.Abstract;

namespace AcksheedSys.Flourish.Configuration;

internal enum FlourishNavigationItemKind
{
    GroupHeader,
    Page,
    Command,
}

internal sealed class FlourishNavigationItem : INotifyPropertyChanged
{
    private bool isActiveChildParent;
    private bool isExpanded;
    private bool isVisible = true;

    public FlourishNavigationItem(
        string key,
        string label,
        string? iconGlyph,
        int groupId,
        FlourishNavigationItemKind kind,
        Type? pageType = null,
        FlourishPageCacheMode cacheMode = FlourishPageCacheMode.Enabled,
        string? commandKey = null,
        bool isInitial = false,
        bool isFixed = false,
        int parentId = 0,
        int childId = 0
    )
    {
        Key = key;
        Label = label;
        IconGlyph = iconGlyph ?? string.Empty;
        GroupId = groupId;
        Kind = kind;
        PageType = pageType;
        CacheMode = cacheMode;
        CommandKey = commandKey;
        IsInitial = isInitial;
        IsFixed = isFixed;
        ParentId = parentId;
        ChildId = childId;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Key { get; }

    public string Label { get; internal set; }

    public string IconGlyph { get; internal set; }

    public int GroupId { get; }

    public FlourishNavigationItemKind Kind { get; }

    public Type? PageType { get; }

    public FlourishPageCacheMode CacheMode { get; internal set; }

    public string? CommandKey { get; }

    public bool IsInitial { get; internal set; }

    public bool IsFixed { get; }

    public int ParentId { get; }

    public int ChildId { get; }

    public bool HasChildren { get; internal set; }

    public bool IsActiveChildParent
    {
        get => isActiveChildParent;
        set
        {
            if (isActiveChildParent == value)
            {
                return;
            }

            isActiveChildParent = value;
            OnPropertyChanged();
        }
    }

    public bool IsGroupHeader => Kind == FlourishNavigationItemKind.GroupHeader;

    public bool IsNavigationItem => !IsGroupHeader;

    public bool IsPageItem => Kind == FlourishNavigationItemKind.Page;

    public bool IsCommandItem => Kind == FlourishNavigationItemKind.Command;

    public bool IsParent => ParentId != 0;

    public bool IsChild => ChildId != 0;

    public Thickness IndentMargin => IsChild ? new Thickness(16, 0, 0, 0) : new Thickness();

    public string ExpandGlyph => !HasChildren ? string.Empty : IsExpanded ? "\uE70D" : "\uE76C";

    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            if (isExpanded == value)
            {
                return;
            }

            isExpanded = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ExpandGlyph));
        }
    }

    public bool IsVisible
    {
        get => isVisible;
        set
        {
            if (isVisible == value)
            {
                return;
            }

            isVisible = value;
            OnPropertyChanged();
        }
    }

    public void Validate()
    {
        if (IsPageItem && (PageType is null || !typeof(Page).IsAssignableFrom(PageType)))
        {
            throw new ArgumentException(
                $"{PageType?.FullName ?? "Navigation item"} must derive from System.Windows.Controls.Page.",
                nameof(PageType)
            );
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
