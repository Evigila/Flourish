using System.Windows;

namespace ArkheideSystem.Flourish.Internal.Model.Shell;

/// <summary>
/// Describes application-provided WPF content displayed in a Flourish shell region.
/// </summary>
internal sealed class FlourishRegionContent
{
    private static long nextId;

    /// <summary>
    /// Creates a region content definition.
    /// </summary>
    /// <param name="region">The shell region that receives the content.</param>
    /// <param name="contentFactory">A factory that creates the WPF element when the shell is created.</param>
    /// <param name="order">The display order inside the region. Lower values are displayed first.</param>
    /// <param name="id">The optional stable runtime registration ID.</param>
    /// <param name="isEnabled">A value indicating whether the content is initially enabled.</param>
    public FlourishRegionContent(
        FlourishRegion region,
        Func<IServiceProvider, FrameworkElement> contentFactory,
        int order = 0,
        string? id = null,
        bool isEnabled = true
    )
    {
        if (!Enum.IsDefined(region))
        {
            throw new ArgumentOutOfRangeException(nameof(region), region, "Unknown shell region.");
        }

        Id = string.IsNullOrWhiteSpace(id)
            ? $"region:{region}:{Interlocked.Increment(ref nextId)}"
            : id;
        Region = region;
        ContentFactory = contentFactory ?? throw new ArgumentNullException(nameof(contentFactory));
        Order = order;
        IsEnabled = isEnabled;
    }

    /// <summary>Gets the stable registration ID.</summary>
    public string Id { get; }

    /// <summary>
    /// Gets the shell region that receives the content.
    /// </summary>
    public FlourishRegion Region { get; }

    /// <summary>
    /// Gets the factory used to create the WPF element when the shell is created.
    /// </summary>
    public Func<IServiceProvider, FrameworkElement> ContentFactory { get; }

    /// <summary>
    /// Gets the display order inside the region.
    /// </summary>
    public int Order { get; }

    /// <summary>Gets whether the content is currently enabled.</summary>
    public bool IsEnabled { get; }

    internal FrameworkElement CreateContent(IServiceProvider services)
    {
        return ContentFactory(services)
            ?? throw new InvalidOperationException(
                $"The content factory for region {Region} returned null."
            );
    }
}
