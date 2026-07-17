using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WpfControl = System.Windows.Controls.Control;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Displays an append-only history of compact output messages inside a scrolling card surface.
/// </summary>
[TemplatePart(Name = OutputScrollViewerPartName, Type = typeof(ScrollViewer))]
public class OutputCard : WpfControl
{
    private const string OutputScrollViewerPartName = "PART_OutputScrollViewer";
    private static readonly DependencyPropertyKey OutputPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(Output),
            typeof(string),
            typeof(OutputCard),
            new FrameworkPropertyMetadata(string.Empty)
        );

    /// <summary>Identifies the read-only <see cref="Output" /> dependency property.</summary>
    public static readonly DependencyProperty OutputProperty =
        OutputPropertyKey.DependencyProperty;

    private ScrollViewer? _outputScrollViewer;
    private int _lineCount;
    private bool _scrollToEndPending;

    static OutputCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(OutputCard),
            new FrameworkPropertyMetadata(typeof(OutputCard))
        );
    }

    /// <summary>Gets the complete output history.</summary>
    public string Output => (string)GetValue(OutputProperty);

    /// <summary>Appends an empty line to the output history.</summary>
    public void WriteLine()
    {
        WriteLine(string.Empty);
    }

    /// <summary>Appends a message and a line boundary to the output history.</summary>
    /// <param name="message">The message to append. A <see langword="null" /> value writes an empty line.</param>
    public void WriteLine(string? message)
    {
        var separator = _lineCount == 0 ? string.Empty : Environment.NewLine;
        SetValue(OutputPropertyKey, string.Concat(Output, separator, message));
        _lineCount++;
        ScheduleScrollToEnd();
    }

    /// <summary>Removes every message from the output history.</summary>
    public void Clear()
    {
        SetValue(OutputPropertyKey, string.Empty);
        _lineCount = 0;
        _scrollToEndPending = false;
        _outputScrollViewer?.ScrollToHome();
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _outputScrollViewer = GetTemplateChild(OutputScrollViewerPartName) as ScrollViewer;

        if (_lineCount > 0)
        {
            ScheduleScrollToEnd();
        }
    }

    /// <inheritdoc />
    protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
    {
        // Output history must not determine an auto-sized row's height. The minimum
        // height constrains the template during measurement so the inner ScrollViewer
        // establishes a real extent instead of arranging a full-height child and merely
        // clipping it. A stretching parent can still arrange the card to match a taller
        // adjacent content column.
        if (!double.IsNaN(Height))
        {
            return base.MeasureOverride(constraint);
        }

        var constrainedHeight = double.IsPositiveInfinity(constraint.Height)
            ? MinHeight
            : Math.Min(constraint.Height, MinHeight);
        return base.MeasureOverride(
            new System.Windows.Size(constraint.Width, constrainedHeight)
        );
    }

    private void ScheduleScrollToEnd()
    {
        if (_outputScrollViewer is null || _scrollToEndPending)
        {
            return;
        }

        _scrollToEndPending = true;
        Dispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            () =>
            {
                _scrollToEndPending = false;
                _outputScrollViewer?.ScrollToEnd();
            }
        );
    }
}
