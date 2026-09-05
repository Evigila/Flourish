using System;

using ArkheideSystem.Flourish.Abstract;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Navigation;

internal enum NavigationItemKind
{
    GroupHeader,
    Page,
    Command,
}

internal sealed class NavigationItemDefinition(
    string key,
    string label,
    string? iconGlyph,
    int groupId,
    NavigationItemKind kind,
    Type? pageType = null,
    string? commandKey = null,
    bool isInitial = false,
    bool isFixed = false,
    int parentId = 0,
    int childId = 0,
    string? id = null
) : INotifyPropertyChanged
{
    private bool isActiveChildParent;
    private bool isEnabled = true;
    private bool isExpanded;
    private bool isExplicitlyVisible = true;
    private bool isTreeVisible = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Id { get; } = string.IsNullOrWhiteSpace(id) ? key : id;

    public string Key { get; internal set; } = key;

    public string Label { get; internal set; } = label;

    public string IconGlyph { get; internal set; } = iconGlyph ?? string.Empty;

    public int GroupId { get; } = groupId;

    public NavigationItemKind Kind { get; } = kind;

    public Type? PageType { get; internal set; } = pageType;

    public string? CommandKey { get; } = commandKey;

    public bool IsInitial { get; internal set; } = isInitial;

    public bool IsFixed { get; } = isFixed;

    public int ParentId { get; } = parentId;

    public int ChildId { get; } = childId;

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

    public bool IsGroupHeader => Kind == NavigationItemKind.GroupHeader;

    public bool IsNavigationItem => !IsGroupHeader;

    public bool IsPageItem => Kind == NavigationItemKind.Page;

    public bool IsCommandItem => Kind == NavigationItemKind.Command;

    public bool IsParent => ParentId != 0;

    public bool IsChild => ChildId != 0;

    public Thickness IndentMargin => IsChild ? new Thickness(16, 0, 0, 0) : new Thickness();

    public string ExpandGlyph =>
        !HasChildren ? string.Empty
        : IsExpanded ? "\uE70D"
        : "\uE76C";

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

    public bool IsEnabled
    {
        get => isEnabled;
        set
        {
            if (isEnabled == value)
            {
                return;
            }

            isEnabled = value;
            OnPropertyChanged();
        }
    }

    public bool IsExplicitlyVisible
    {
        get => isExplicitlyVisible;
        set
        {
            if (isExplicitlyVisible == value)
            {
                return;
            }

            isExplicitlyVisible = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsVisible));
        }
    }

    public bool IsTreeVisible
    {
        get => isTreeVisible;
        set
        {
            if (isTreeVisible == value)
            {
                return;
            }

            isTreeVisible = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsVisible));
        }
    }

    public bool IsVisible
    {
        get => IsExplicitlyVisible && IsTreeVisible;
        set
        {
            if (isTreeVisible == value)
            {
                return;
            }

            isTreeVisible = value;
            OnPropertyChanged();
        }
    }

    public void Validate()
    {
        if (IsPageItem && PageType is null)
        {
            throw new InvalidOperationException(
                "Navigation page items require a registered Page type."
            );
        }

        if (IsPageItem && !typeof(Page).IsAssignableFrom(PageType))
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
