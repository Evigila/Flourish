using System;
using System.Threading;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Layout;

internal sealed class ContentLayoutService : IContentLayoutService
{
    private readonly Lock gate = new();
    private bool isCenterContentEnabled;
    private double contentWidth;
    private FlourishContentLayoutSettings current;
    private long version;

    public ContentLayoutService(FlourishLayoutOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        isCenterContentEnabled = options.IsCenterContentEnabled;
        contentWidth =
            double.IsFinite(options.CenterContentWidth) && options.CenterContentWidth > 0
                ? options.CenterContentWidth
                : 1200;
        current = CreateSnapshot();
    }

    public FlourishContentLayoutSettings Current => Volatile.Read(ref current);

    public event EventHandler<FlourishStateTransitionEventArgs<FlourishContentLayoutSettings>>? Changed;

    public void SetCenterContent(bool enabled, double contentWidth = 1200)
    {
        ValidateContentWidth(contentWidth);

        FlourishContentLayoutSettings previous;
        FlourishContentLayoutSettings current;
        lock (gate)
        {
            if (isCenterContentEnabled == enabled && this.contentWidth.Equals(contentWidth))
            {
                return;
            }

            previous = Volatile.Read(ref this.current);
            isCenterContentEnabled = enabled;
            this.contentWidth = contentWidth;
            version++;
            current = CreateSnapshot();
            Volatile.Write(ref this.current, current);
        }

        Changed?.Invoke(this, new FlourishStateTransitionEventArgs<FlourishContentLayoutSettings>(previous, current));
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
