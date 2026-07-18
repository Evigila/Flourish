using System.Text;
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

    private StringBuilder _outputBuilder = new();
    private ScrollViewer? _outputScrollViewer;
    private string? _materializedOutput = string.Empty;
    private int _lineCount;
    private bool _refreshPending;
    private long _refreshGeneration;

    static OutputCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(OutputCard),
            new FrameworkPropertyMetadata(typeof(OutputCard))
        );
    }

    /// <summary>Gets the complete output history.</summary>
    public string Output
    {
        get
        {
            VerifyAccess();
            return MaterializeOutput();
        }
    }

    /// <summary>Appends an empty line to the output history.</summary>
    public void WriteLine()
    {
        WriteLine(string.Empty);
    }

    /// <summary>Appends a message and a line boundary to the output history.</summary>
    /// <param name="message">The message to append. A <see langword="null" /> value writes an empty line.</param>
    public void WriteLine(string? message)
    {
        VerifyAccess();

        if (_lineCount > 0)
        {
            _outputBuilder.Append(Environment.NewLine);
        }

        _outputBuilder.Append(message);
        _materializedOutput = null;
        _lineCount++;
        ScheduleRefresh();
    }

    /// <summary>Removes every message from the output history.</summary>
    public void Clear()
    {
        VerifyAccess();

        // Replacing the builder releases large retained buffers after a long-running
        // output session instead of keeping their capacity alive for the next session.
        _outputBuilder = new StringBuilder();
        _materializedOutput = string.Empty;
        SetValue(OutputPropertyKey, string.Empty);
        _lineCount = 0;
        _refreshGeneration++;
        _refreshPending = false;
        _outputScrollViewer?.ScrollToHome();
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _outputScrollViewer = GetTemplateChild(OutputScrollViewerPartName) as ScrollViewer;

        if (_lineCount > 0)
        {
            ScheduleRefresh();
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

    private string MaterializeOutput()
    {
        return _materializedOutput ??= _outputBuilder.ToString();
    }

    private void ScheduleRefresh()
    {
        if (_refreshPending)
        {
            return;
        }

        _refreshPending = true;
        var generation = _refreshGeneration;
        Dispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            () =>
            {
                if (generation != _refreshGeneration)
                {
                    return;
                }

                _refreshPending = false;
                var output = MaterializeOutput();
                if (!ReferenceEquals(GetValue(OutputProperty), output))
                {
                    SetValue(OutputPropertyKey, output);
                }

                _outputScrollViewer?.ScrollToEnd();
            }
        );
    }
}
