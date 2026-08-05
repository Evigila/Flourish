using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Gallery.Models;
using WpfDataGrid = System.Windows.Controls.DataGrid;

namespace ArkheideSystem.Gallery.Localization;

/// <summary>
/// Client-side localization for the Gallery application. The service deliberately owns
/// its catalogs and UI traversal rather than adding application copy to Flourish.
/// </summary>
public sealed class GalleryLocalizationService : IGalleryLocalization, IDisposable
{
    private const string CatalogMarker = ".Localization.Catalogs.";

    private readonly IFlourishLocalization flourishLocalization;
    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> catalogs;
    private readonly IReadOnlySet<string> catalogSources;
    private readonly ConditionalWeakTable<DependencyObject, Dictionary<DependencyProperty, string>>
        sourceValues = new();
    private readonly Lock gate = new();
    private Dispatcher? dispatcher;
    private bool isStarted;
    private bool isDisposed;

    public GalleryLocalizationService(IFlourishLocalization flourishLocalization)
    {
        this.flourishLocalization =
            flourishLocalization ?? throw new ArgumentNullException(nameof(flourishLocalization));
        catalogs = LoadCatalogs();
        catalogSources = catalogs.Values.SelectMany(catalog => catalog.Keys).ToHashSet(
            StringComparer.Ordinal
        );
    }

    public string CurrentLocale => flourishLocalization.CurrentLocale;

    public event EventHandler? Changed;

    internal void Start(Dispatcher applicationDispatcher)
    {
        ArgumentNullException.ThrowIfNull(applicationDispatcher);
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            if (isStarted)
            {
                return;
            }

            dispatcher = applicationDispatcher;
            isStarted = true;
            flourishLocalization.Changed += FlourishLocalization_Changed;
            EventManager.RegisterClassHandler(
                typeof(FrameworkElement),
                FrameworkElement.LoadedEvent,
                new RoutedEventHandler(Element_Loaded),
                handledEventsToo: true
            );
        }
    }

    public string Get(string sourceText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceText);
        return GetCatalog(CurrentLocale) is { } catalog
            && catalog.TryGetValue(sourceText, out var localized)
                ? localized
                : sourceText;
    }

    public string Format(string sourceFormat, params object?[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        return string.Format(CultureInfo.CurrentCulture, Get(sourceFormat), arguments);
    }

    public void Apply(DependencyObject root)
    {
        ArgumentNullException.ThrowIfNull(root);
        if (root.Dispatcher.CheckAccess())
        {
            ApplyCore(root);
            return;
        }

        root.Dispatcher.Invoke(() => ApplyCore(root));
    }

    public void Dispose()
    {
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            flourishLocalization.Changed -= FlourishLocalization_Changed;
        }
    }

    private void Element_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is DependencyObject dependencyObject)
        {
            ApplyObject(dependencyObject);
        }
    }

    private void FlourishLocalization_Changed(
        object? sender,
        FlourishLocalizationChangedEventArgs e
    )
    {
        var targetDispatcher = dispatcher;
        if (targetDispatcher is null)
        {
            return;
        }

        void Refresh()
        {
            if (Application.Current is not { } application)
            {
                return;
            }

            foreach (Window window in application.Windows)
            {
                ApplyCore(window);
            }

            Changed?.Invoke(this, EventArgs.Empty);
        }

        if (targetDispatcher.CheckAccess())
        {
            Refresh();
        }
        else
        {
            _ = targetDispatcher.BeginInvoke(DispatcherPriority.DataBind, Refresh);
        }
    }

    private void ApplyCore(DependencyObject root)
    {
        var visited = new HashSet<DependencyObject>(ReferenceEqualityComparer.Instance);
        var pending = new Stack<DependencyObject>();
        pending.Push(root);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!visited.Add(current))
            {
                continue;
            }

            ApplyObject(current);

            foreach (var child in EnumerateChildren(current))
            {
                pending.Push(child);
            }
        }
    }

    private void ApplyObject(DependencyObject target)
    {
        ApplyLocalValues(target);
        if (target is WpfDataGrid dataGrid)
        {
            foreach (var column in dataGrid.Columns)
            {
                ApplyLocalValues(column);
            }
        }

        if (target is ItemsControl { ItemsSource: IEnumerable items })
        {
            foreach (var item in items.OfType<ControlMemberRow>())
            {
                item.Apply(this);
            }
        }
    }

    private void ApplyLocalValues(DependencyObject target)
    {
        var entries = target.GetLocalValueEnumerator();
        while (entries.MoveNext())
        {
            var entry = entries.Current;
            if (entry.Value is not string value || !ShouldLocalize(target, entry.Property))
            {
                continue;
            }

            var values = sourceValues.GetOrCreateValue(target);
            if (!values.TryGetValue(entry.Property, out var source))
            {
                if (!IsCatalogSource(value))
                {
                    continue;
                }

                source = value;
                values.Add(entry.Property, source);
            }

            target.SetCurrentValue(entry.Property, Get(source));
        }

        if (target is DataGridColumn { Header: string header } column)
        {
            var values = sourceValues.GetOrCreateValue(column);
            if (!values.TryGetValue(DataGridColumn.HeaderProperty, out var source))
            {
                if (!IsCatalogSource(header))
                {
                    return;
                }

                source = header;
                values.Add(DataGridColumn.HeaderProperty, source);
            }

            column.SetCurrentValue(DataGridColumn.HeaderProperty, Get(source));
        }
    }

    private bool IsCatalogSource(string value)
    {
        return catalogSources.Contains(value);
    }

    private IReadOnlyDictionary<string, string>? GetCatalog(string locale)
    {
        return catalogs.TryGetValue(locale, out var catalog) ? catalog : null;
    }

    private static bool ShouldLocalize(DependencyObject target, DependencyProperty property)
    {
        if (property == AutomationProperties.NameProperty)
        {
            return true;
        }

        if (property.Name is "Title" or "Content" or "Header" or "Placeholder" or "ToolTip")
        {
            return true;
        }

        return property.Name == "Text"
            && target is not System.Windows.Controls.Primitives.TextBoxBase
            && target is not FlourishTextBox
            && target is not FlourishSearchBox
            && target is not CodeSpace;
    }

    private static IEnumerable<DependencyObject> EnumerateChildren(DependencyObject parent)
    {
        if (parent is FrameworkElement or FrameworkContentElement)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is DependencyObject dependencyObject)
                {
                    yield return dependencyObject;
                }
            }
        }

        if (parent is not Visual and not System.Windows.Media.Media3D.Visual3D)
        {
            yield break;
        }

        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (var index = 0; index < count; index++)
        {
            yield return VisualTreeHelper.GetChild(parent, index);
        }
    }

    private static IReadOnlyDictionary<
        string,
        IReadOnlyDictionary<string, string>
    > LoadCatalogs()
    {
        var assembly = typeof(GalleryLocalizationService).Assembly;
        var grouped = new Dictionary<string, Dictionary<string, string>>(
            StringComparer.OrdinalIgnoreCase
        );

        foreach (
            var resourceName in assembly
                .GetManifestResourceNames()
                .Where(name => name.Contains(CatalogMarker, StringComparison.Ordinal))
                .OrderBy(name => name, StringComparer.Ordinal)
        )
        {
            var tail = resourceName[(resourceName.IndexOf(CatalogMarker, StringComparison.Ordinal)
                + CatalogMarker.Length)..];
            var separator = tail.IndexOf('.', StringComparison.Ordinal);
            if (separator <= 0)
            {
                continue;
            }

            var locale = tail[..separator];
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException(
                    $"Gallery locale resource '{resourceName}' could not be opened."
                );
            var values = JsonSerializer.Deserialize<Dictionary<string, string>>(stream)
                ?? throw new InvalidDataException(
                    $"Gallery locale resource '{resourceName}' must contain an object."
                );
            if (!grouped.TryGetValue(locale, out var merged))
            {
                merged = new Dictionary<string, string>(StringComparer.Ordinal);
                grouped.Add(locale, merged);
            }

            foreach (var (source, localized) in values)
            {
                if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(localized))
                {
                    throw new InvalidDataException(
                        $"Gallery locale resource '{resourceName}' contains an empty entry."
                    );
                }

                if (merged.TryGetValue(source, out var existing) && existing != localized)
                {
                    throw new InvalidDataException(
                        $"Gallery locale source '{source}' has conflicting translations."
                    );
                }

                merged[source] = localized;
            }
        }

        return grouped.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyDictionary<string, string>)pair.Value,
            StringComparer.OrdinalIgnoreCase
        );
    }
}
