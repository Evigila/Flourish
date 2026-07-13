using System.Windows;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Provides runtime registration of content in Flourish shell regions.</summary>
public interface IShellRegionService
{
    /// <summary>Occurs after region registrations change.</summary>
    event EventHandler<FlourishShellRegionChangedEventArgs>? Changed;

    /// <summary>Gets an immutable snapshot of region registrations.</summary>
    FlourishShellRegionSnapshot Current { get; }

    /// <summary>Adds a registration and returns a handle that removes it when disposed.</summary>
    IShellRegionRegistration Add(
        string id,
        FlourishRegion region,
        Func<IServiceProvider, FrameworkElement> contentFactory,
        int order = 0
    );

    /// <summary>Adds or replaces a registration by stable ID.</summary>
    IShellRegionRegistration Upsert(
        string id,
        FlourishRegion region,
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
    void Clear(FlourishRegion region);
}

/// <summary>Controls the lifetime of a runtime shell region registration.</summary>
public interface IShellRegionRegistration : IDisposable
{
    /// <summary>Gets the stable registration ID.</summary>
    string Id { get; }

    /// <summary>Gets the target region.</summary>
    FlourishRegion Region { get; }
}

/// <summary>Describes the state of one region registration.</summary>
public sealed record FlourishShellRegionEntry(
    string Id,
    FlourishRegion Region,
    int Order,
    bool IsEnabled
);

/// <summary>Represents all current region registrations.</summary>
public sealed record FlourishShellRegionSnapshot(
    IReadOnlyList<FlourishShellRegionEntry> Entries,
    long Version
);

/// <summary>Provides data for <see cref="IShellRegionService.Changed" />.</summary>
public sealed class FlourishShellRegionChangedEventArgs(
    FlourishShellRegionSnapshot current,
    FlourishRuntimeChangeKind changeKind,
    FlourishRegion region,
    string? registrationId
) : EventArgs
{
    /// <summary>Gets the new state.</summary>
    public FlourishShellRegionSnapshot Current { get; } = current;

    /// <summary>Gets the mutation kind.</summary>
    public FlourishRuntimeChangeKind ChangeKind { get; } = changeKind;

    /// <summary>Gets the affected region.</summary>
    public FlourishRegion Region { get; } = region;

    /// <summary>Gets the affected registration ID, if applicable.</summary>
    public string? RegistrationId { get; } = registrationId;
}
