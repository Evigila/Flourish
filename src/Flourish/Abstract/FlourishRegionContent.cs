using System.Windows;

namespace AckSS.Flourish.Abstract;

/// <summary>
/// Describes application-provided WPF content displayed in a Flourish shell region.
/// </summary>
public sealed class FlourishRegionContent
{
    /// <summary>
    /// Creates a region content definition.
    /// </summary>
    /// <param name="region">The shell region that receives the content.</param>
    /// <param name="contentFactory">A factory that creates the WPF element when the shell is created.</param>
    /// <param name="order">The display order inside the region. Lower values are displayed first.</param>
    public FlourishRegionContent(
        FlourishRegion region,
        Func<IServiceProvider, FrameworkElement> contentFactory,
        int order = 0
    )
    {
        if (!Enum.IsDefined(region))
        {
            throw new ArgumentOutOfRangeException(nameof(region), region, "Unknown shell region.");
        }

        Region = region;
        ContentFactory = contentFactory ?? throw new ArgumentNullException(nameof(contentFactory));
        Order = order;
    }

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

    internal FrameworkElement CreateContent(IServiceProvider services)
    {
        return ContentFactory(services)
            ?? throw new InvalidOperationException(
                $"The content factory for region {Region} returned null."
            );
    }
}
