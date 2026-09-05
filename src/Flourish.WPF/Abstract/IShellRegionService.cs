using System;
using System.Collections.Generic;

using System.Windows;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Provides runtime registration of content in Flourish shell regions.</summary>
public interface IShellRegionService
{
    /// <summary>Occurs after region registrations change.</summary>
    event EventHandler<ShellRegionChangedEventArgs>? Changed;

    /// <summary>Gets an immutable snapshot of region registrations.</summary>
    ShellRegionSnapshot Current { get; }

    /// <summary>Adds a registration and returns a handle that removes it when disposed.</summary>
    IRegistration Add(
        string id,
        ShellRegion region,
        Func<IServiceProvider, FrameworkElement> contentFactory,
        int order = 0
    );

    /// <summary>Adds or replaces a registration by stable ID.</summary>
    IRegistration Set(
        string id,
        ShellRegion region,
        Func<IServiceProvider, FrameworkElement> contentFactory,
        int order = 0
    );

    /// <summary>Enables or disables a registration.</summary>
    void SetEnabled(string id, bool enabled);

    /// <summary>Changes the display order of a registration.</summary>
    void SetOrder(string id, int order);

    /// <summary>Removes a registration.</summary>
    bool Remove(string id);

    /// <summary>Removes all registrations in a region.</summary>
    void RemoveAll(ShellRegion region);
}

/// <summary>Describes the state of one region registration.</summary>
public sealed record ShellRegionEntry(
    string Id,
    ShellRegion Region,
    int Order,
    bool IsEnabled
);

/// <summary>Represents all current region registrations.</summary>
public sealed record ShellRegionSnapshot(
    IReadOnlyList<ShellRegionEntry> Entries,
    long Version
);

/// <summary>Provides data for <see cref="IShellRegionService.Changed" />.</summary>
public sealed class ShellRegionChangedEventArgs(
    ShellRegionSnapshot current,
    CollectionChangeKind changeKind,
    ShellRegion region,
    string? registrationId
) : EventArgs
{
    /// <summary>Gets the new state.</summary>
    public ShellRegionSnapshot Current { get; } = current;

    /// <summary>Gets the mutation kind.</summary>
    public CollectionChangeKind ChangeKind { get; } = changeKind;

    /// <summary>Gets the affected region.</summary>
    public ShellRegion Region { get; } = region;

    /// <summary>Gets the affected registration ID, if applicable.</summary>
    public string? RegistrationId { get; } = registrationId;
}
