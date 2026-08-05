using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
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
    private const string DefaultLocale = "en-US";
    private const string CatalogMarker = ".Localization.Catalogs.";
    private static readonly Regex CompositeFormatItemPattern = new(
        @"(?<!\{)\{(?<index>\d+)(?:,[^{}]+)?(?::[^{}]+)?\}(?!\})",
        RegexOptions.CultureInvariant
    );

    private readonly IFlourishLocalization flourishLocalization;
    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> catalogs;
    private readonly IReadOnlySet<string> catalogKeys;
    private readonly ConditionalWeakTable<
        DependencyObject,
        Dictionary<DependencyProperty, string>
    > resourceKeys = new();
    private readonly Lock gate = new();
    private Dispatcher? dispatcher;
    private bool isStarted;
    private bool isDisposed;

    public GalleryLocalizationService(IFlourishLocalization flourishLocalization)
    {
        this.flourishLocalization =
            flourishLocalization ?? throw new ArgumentNullException(nameof(flourishLocalization));
        catalogs = LoadCatalogs();
        catalogKeys = GalleryLocaleKeys.All.ToHashSet(StringComparer.Ordinal);
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

    public string Get(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Locale key cannot be empty.", nameof(key));
        }

        return TryGet(CurrentLocale, key) ?? TryGet(DefaultLocale, key) ?? key;
    }

    public string Format(string key, params object?[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        return string.Format(CultureInfo.CurrentCulture, Get(key), arguments);
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

            var values = resourceKeys.GetOrCreateValue(target);
            if (!values.TryGetValue(entry.Property, out var key))
            {
                if (!IsCatalogKey(value))
                {
                    continue;
                }

                key = value;
                values.Add(entry.Property, key);
            }

            target.SetCurrentValue(entry.Property, Get(key));
        }

        if (target is DataGridColumn { Header: string header } column)
        {
            var values = resourceKeys.GetOrCreateValue(column);
            if (!values.TryGetValue(DataGridColumn.HeaderProperty, out var key))
            {
                if (!IsCatalogKey(header))
                {
                    return;
                }

                key = header;
                values.Add(DataGridColumn.HeaderProperty, key);
            }

            column.SetCurrentValue(DataGridColumn.HeaderProperty, Get(key));
        }
    }

    private bool IsCatalogKey(string value)
    {
        return catalogKeys.Contains(value);
    }

    private string? TryGet(string locale, string key)
    {
        return
            catalogs.TryGetValue(locale, out var catalog) && catalog.TryGetValue(key, out var value)
            ? value
            : null;
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

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> LoadCatalogs()
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
            var tail = resourceName[
                (
                    resourceName.IndexOf(CatalogMarker, StringComparison.Ordinal)
                    + CatalogMarker.Length
                )..
            ];
            var separator = tail.IndexOf('.', StringComparison.Ordinal);
            if (separator <= 0)
            {
                continue;
            }

            var locale = tail[..separator];
            using var stream =
                assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException(
                    $"Gallery locale resource '{resourceName}' could not be opened."
                );
            var values = ParseCatalog(stream, resourceName);
            if (!grouped.TryGetValue(locale, out var merged))
            {
                merged = new Dictionary<string, string>(StringComparer.Ordinal);
                grouped.Add(locale, merged);
            }

            foreach (var (key, localized) in values)
            {
                if (!merged.TryAdd(key, localized))
                {
                    throw new InvalidDataException(
                        $"Gallery locale key '{key}' is defined by more than one '{locale}' resource."
                    );
                }
            }
        }

        ValidateCatalogs(grouped);
        return grouped.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyDictionary<string, string>)pair.Value,
            StringComparer.OrdinalIgnoreCase
        );
    }

    private static IReadOnlyDictionary<string, string> ParseCatalog(
        Stream stream,
        string resourceName
    )
    {
        try
        {
            using var document = JsonDocument.Parse(stream);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException(
                    $"Gallery locale resource '{resourceName}' must contain a JSON object."
                );
            }

            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (string.IsNullOrWhiteSpace(property.Name))
                {
                    throw new InvalidDataException(
                        $"Gallery locale resource '{resourceName}' contains an empty key."
                    );
                }

                if (
                    property.Value.ValueKind != JsonValueKind.String
                    || string.IsNullOrWhiteSpace(property.Value.GetString())
                )
                {
                    throw new InvalidDataException(
                        $"Gallery locale resource '{resourceName}' contains an empty or non-string value for key '{property.Name}'."
                    );
                }

                if (!values.TryAdd(property.Name, property.Value.GetString()!))
                {
                    throw new InvalidDataException(
                        $"Gallery locale resource '{resourceName}' contains duplicate key '{property.Name}'."
                    );
                }
            }

            if (values.Count == 0)
            {
                throw new InvalidDataException(
                    $"Gallery locale resource '{resourceName}' does not contain any translations."
                );
            }

            return values;
        }
        catch (JsonException error)
        {
            throw new InvalidDataException(
                $"Gallery locale resource '{resourceName}' contains invalid JSON.",
                error
            );
        }
    }

    private static void ValidateCatalogs(
        IReadOnlyDictionary<string, Dictionary<string, string>> grouped
    )
    {
        if (!grouped.TryGetValue(DefaultLocale, out var english))
        {
            throw new InvalidDataException(
                $"Gallery localization requires a '{DefaultLocale}' fallback catalog."
            );
        }

        var declaredKeys = GalleryLocaleKeys.All.ToHashSet(StringComparer.Ordinal);
        if (declaredKeys.Count != GalleryLocaleKeys.All.Count)
        {
            throw new InvalidDataException("GalleryLocaleKeys.All contains duplicate keys.");
        }

        var missingDeclarations = english
            .Keys.Except(declaredKeys, StringComparer.Ordinal)
            .ToArray();
        var missingEnglish = declaredKeys.Except(english.Keys, StringComparer.Ordinal).ToArray();
        if (missingDeclarations.Length > 0 || missingEnglish.Length > 0)
        {
            throw new InvalidDataException(
                "The Gallery English catalog and GalleryLocaleKeys.All must contain the same keys."
            );
        }

        foreach (var (locale, catalog) in grouped)
        {
            if (!catalog.Keys.ToHashSet(StringComparer.Ordinal).SetEquals(english.Keys))
            {
                throw new InvalidDataException(
                    $"Gallery locale '{locale}' and the '{DefaultLocale}' catalog must contain the same keys."
                );
            }

            foreach (var (key, localized) in catalog)
            {
                var englishFormat = english[key];

                var englishIndexes = GetCompositeFormatIndexes(englishFormat);
                var localizedIndexes = GetCompositeFormatIndexes(localized);
                if (!englishIndexes.SequenceEqual(localizedIndexes, StringComparer.Ordinal))
                {
                    throw new InvalidDataException(
                        $"Gallery locale '{locale}' does not preserve the format placeholders for key '{key}'."
                    );
                }
            }
        }
    }

    private static string[] GetCompositeFormatIndexes(string value)
    {
        return CompositeFormatItemPattern
            .Matches(value)
            .Select(match => match.Groups["index"].Value)
            .OrderBy(index => index, StringComparer.Ordinal)
            .ToArray();
    }
}
