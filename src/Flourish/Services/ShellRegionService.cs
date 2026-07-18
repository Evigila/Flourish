using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class ShellRegionService : IShellRegionService
{
    private readonly Lock gate = new();
    private readonly FlourishShellOptions options;
    private readonly Dictionary<string, Guid> leases = new(StringComparer.Ordinal);
    private long version;

    public ShellRegionService(FlourishShellOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        ValidateUniqueIds(options.RegionContents);
    }

    public event EventHandler<FlourishShellRegionChangedEventArgs>? Changed;

    public FlourishShellRegionSnapshot Current
    {
        get
        {
            lock (gate)
            {
                return CreateSnapshot();
            }
        }
    }

    public IShellRegionRegistration Add(
        string id,
        FlourishRegion region,
        Func<IServiceProvider, FrameworkElement> contentFactory,
        int order = 0
    )
    {
        ValidateRegistration(id, region, contentFactory);
        var lease = Guid.NewGuid();
        Mutate(
            () =>
            {
                if (FindIndex(id) >= 0)
                {
                    throw new InvalidOperationException(
                        $"Shell region registration ID '{id}' is already registered."
                    );
                }

                options.RegionContents.Add(
                    new FlourishRegionContent(region, contentFactory, order, id)
                );
                leases[id] = lease;
                return true;
            },
            FlourishRuntimeChangeKind.Added,
            region,
            id
        );

        return new Registration(this, id, region, lease);
    }

    public IShellRegionRegistration Upsert(
        string id,
        FlourishRegion region,
        Func<IServiceProvider, FrameworkElement> contentFactory,
        int order = 0
    )
    {
        ValidateRegistration(id, region, contentFactory);
        var lease = Guid.NewGuid();
        FlourishRegion? previousRegion = null;
        FlourishShellRegionSnapshot snapshot;
        lock (gate)
        {
            var index = FindIndex(id);
            if (index >= 0)
            {
                var current = options.RegionContents[index];
                if (
                    current.Region == region
                    && current.Order == order
                    && current.IsEnabled
                    && current.ContentFactory.Equals(contentFactory)
                )
                {
                    leases[id] = lease;
                    return new Registration(this, id, region, lease);
                }

                previousRegion = current.Region;
                options.RegionContents[index] = new FlourishRegionContent(
                    region,
                    contentFactory,
                    order,
                    id
                );
            }
            else
            {
                options.RegionContents.Add(
                    new FlourishRegionContent(region, contentFactory, order, id)
                );
            }

            leases[id] = lease;
            version++;
            snapshot = CreateSnapshot();
        }

        if (previousRegion is { } oldRegion && oldRegion != region)
        {
            Changed?.Invoke(
                this,
                new FlourishShellRegionChangedEventArgs(
                    snapshot,
                    FlourishRuntimeChangeKind.Removed,
                    oldRegion,
                    id
                )
            );
        }

        Changed?.Invoke(
            this,
            new FlourishShellRegionChangedEventArgs(
                snapshot,
                previousRegion is null
                    ? FlourishRuntimeChangeKind.Added
                    : FlourishRuntimeChangeKind.Updated,
                region,
                id
            )
        );
        return new Registration(this, id, region, lease);
    }

    public void SetEnabled(string id, bool enabled)
    {
        id = ValidateId(id, nameof(id));
        FlourishRegion region = default;
        Mutate(
            () =>
            {
                var index = RequireIndex(id);
                var current = options.RegionContents[index];
                region = current.Region;
                if (current.IsEnabled == enabled)
                {
                    return false;
                }

                options.RegionContents[index] = new FlourishRegionContent(
                    current.Region,
                    current.ContentFactory,
                    current.Order,
                    current.Id,
                    enabled
                );
                return true;
            },
            FlourishRuntimeChangeKind.Updated,
            () => region,
            id
        );
    }

    public void SetOrder(string id, int order)
    {
        id = ValidateId(id, nameof(id));
        FlourishRegion region = default;
        Mutate(
            () =>
            {
                var index = RequireIndex(id);
                var current = options.RegionContents[index];
                region = current.Region;
                if (current.Order == order)
                {
                    return false;
                }

                options.RegionContents[index] = new FlourishRegionContent(
                    current.Region,
                    current.ContentFactory,
                    order,
                    current.Id,
                    current.IsEnabled
                );
                return true;
            },
            FlourishRuntimeChangeKind.Moved,
            () => region,
            id
        );
    }

    public bool Remove(string id)
    {
        id = ValidateId(id, nameof(id));
        return RemoveCore(id, lease: null);
    }

    public void Clear(FlourishRegion region)
    {
        ValidateRegion(region);
        Mutate(
            () =>
            {
                var ids = options
                    .RegionContents.Where(content => content.Region == region)
                    .Select(content => content.Id)
                    .ToArray();
                if (ids.Length == 0)
                {
                    return false;
                }

                options.RegionContents.RemoveAll(content => content.Region == region);
                foreach (var id in ids)
                {
                    leases.Remove(id);
                }

                return true;
            },
            FlourishRuntimeChangeKind.Reset,
            region,
            registrationId: null
        );
    }

    internal IReadOnlyList<FlourishRegionContent> GetContents(FlourishRegion region)
    {
        lock (gate)
        {
            return options
                .RegionContents.Where(content => content.Region == region && content.IsEnabled)
                .OrderBy(content => content.Order)
                .ToArray();
        }
    }

    private bool RemoveCore(string id, Guid? lease)
    {
        var removed = false;
        FlourishRegion region = default;
        Mutate(
            () =>
            {
                if (
                    lease is not null
                    && (!leases.TryGetValue(id, out var currentLease) || currentLease != lease)
                )
                {
                    return false;
                }

                var index = FindIndex(id);
                if (index < 0)
                {
                    return false;
                }

                region = options.RegionContents[index].Region;
                options.RegionContents.RemoveAt(index);
                leases.Remove(id);
                removed = true;
                return true;
            },
            FlourishRuntimeChangeKind.Removed,
            () => region,
            id
        );
        return removed;
    }

    private void Mutate(
        Func<bool> mutation,
        FlourishRuntimeChangeKind changeKind,
        FlourishRegion region,
        string? registrationId
    )
    {
        Mutate(mutation, changeKind, () => region, registrationId);
    }

    private void Mutate(
        Func<bool> mutation,
        FlourishRuntimeChangeKind changeKind,
        Func<FlourishRegion> region,
        string? registrationId
    )
    {
        FlourishShellRegionSnapshot snapshot;
        FlourishRegion affectedRegion;
        lock (gate)
        {
            if (!mutation())
            {
                return;
            }

            version++;
            affectedRegion = region();
            snapshot = CreateSnapshot();
        }

        Changed?.Invoke(
            this,
            new FlourishShellRegionChangedEventArgs(
                snapshot,
                changeKind,
                affectedRegion,
                registrationId
            )
        );
    }

    private FlourishShellRegionSnapshot CreateSnapshot()
    {
        var entries = options
            .RegionContents.OrderBy(content => content.Region)
            .ThenBy(content => content.Order)
            .Select(content => new FlourishShellRegionEntry(
                content.Id,
                content.Region,
                content.Order,
                content.IsEnabled
            ))
            .ToArray();
        return new FlourishShellRegionSnapshot(entries, version);
    }

    private int FindIndex(string id)
    {
        return options.RegionContents.FindIndex(content =>
            StringComparer.Ordinal.Equals(content.Id, id)
        );
    }

    private int RequireIndex(string id)
    {
        var index = FindIndex(id);
        return index >= 0
            ? index
            : throw new KeyNotFoundException($"Shell region registration ID '{id}' was not found.");
    }

    private static void ValidateRegistration(
        string id,
        FlourishRegion region,
        Func<IServiceProvider, FrameworkElement> contentFactory
    )
    {
        ValidateId(id, nameof(id));
        ValidateRegion(region);
        ArgumentNullException.ThrowIfNull(contentFactory);
    }

    private static string ValidateId(string id, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("A stable ID is required.", parameterName);
        }

        return id;
    }

    private static void ValidateRegion(FlourishRegion region)
    {
        if (!Enum.IsDefined(region))
        {
            throw new ArgumentOutOfRangeException(nameof(region), region, "Unknown shell region.");
        }
    }

    private static void ValidateUniqueIds(IEnumerable<FlourishRegionContent> contents)
    {
        var duplicate = contents
            .GroupBy(content => content.Id, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException(
                $"Shell region registration ID '{duplicate.Key}' is duplicated."
            );
        }
    }

    private sealed class Registration(
        ShellRegionService owner,
        string id,
        FlourishRegion region,
        Guid lease
    ) : IShellRegionRegistration
    {
        private ShellRegionService? owner = owner;

        public string Id { get; } = id;

        public FlourishRegion Region { get; } = region;

        public void Dispose()
        {
            Interlocked.Exchange(ref owner, null)?.RemoveCore(Id, lease);
        }
    }
}
