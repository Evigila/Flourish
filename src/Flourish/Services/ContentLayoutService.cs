using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class ContentLayoutService : IContentLayoutService
{
    private readonly Lock gate = new();
    private bool isCenterContentEnabled;
    private double contentWidth;
    private long version;

    public ContentLayoutService(FlourishShellOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        isCenterContentEnabled = options.IsCenterContentEnabled;
        contentWidth =
            double.IsFinite(options.CenterContentWidth) && options.CenterContentWidth > 0
                ? options.CenterContentWidth
                : 1200;
    }

    public FlourishContentLayoutSettings Current
    {
        get
        {
            lock (gate)
            {
                return CreateSnapshot();
            }
        }
    }

    public event EventHandler<FlourishContentLayoutChangedEventArgs>? Changed;

    public void SetCenterContent(bool enabled, double contentWidth = 1200)
    {
        ValidateContentWidth(contentWidth);

        FlourishContentLayoutSettings previous;
        FlourishContentLayoutSettings current;
        lock (gate)
        {
            if (
                isCenterContentEnabled == enabled
                && this.contentWidth.Equals(contentWidth)
            )
            {
                return;
            }

            previous = CreateSnapshot();
            isCenterContentEnabled = enabled;
            this.contentWidth = contentWidth;
            version++;
            current = CreateSnapshot();
        }

        Changed?.Invoke(this, new FlourishContentLayoutChangedEventArgs(previous, current));
    }

    private FlourishContentLayoutSettings CreateSnapshot() =>
        new(isCenterContentEnabled, contentWidth, version);

    private static void ValidateContentWidth(double contentWidth)
    {
        if (!double.IsFinite(contentWidth) || contentWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(contentWidth),
                contentWidth,
                "Content width must be positive and finite."
            );
        }
    }
}
