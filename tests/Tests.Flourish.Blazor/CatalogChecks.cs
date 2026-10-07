using System.Reflection;
using ArkheideSystem.Gallery.Flourish.Blazor.Models;
using Microsoft.AspNetCore.Components;
using Controls = ArkheideSystem.Flourish.Blazor.Components;
using Primitives = ArkheideSystem.Flourish.Blazor.Components.Primitives;

namespace ArkheideSystem.Tests.Flourish.Blazor;

internal static class CatalogChecks
{
    public static void Run()
    {
        var entries = ComponentCatalog.Groups.SelectMany(group => group.Entries).ToArray();
        var exported = typeof(Controls.Button).Assembly.GetExportedTypes()
            .Where(type => type.IsClass && !type.IsAbstract && typeof(IComponent).IsAssignableFrom(type))
            .OrderBy(type => type.FullName, StringComparer.Ordinal).ToArray();
        var documentedTypes = entries.Select(entry => entry.ComponentType)
            .OrderBy(type => type.FullName, StringComparer.Ordinal).ToArray();
        Check(exported.SequenceEqual(documentedTypes), "Gallery must cover every exported component exactly once. Missing: "
            + string.Join(", ", exported.Except(documentedTypes).Select(type => type.FullName)));
        foreach (var entry in entries)
        {
            Check(entry.Usage.ComponentType == entry.ComponentType, $"Usage metadata differs for {entry.Name}.");
            Check(!string.IsNullOrWhiteSpace(entry.Usage.Scenario.Token) && !string.IsNullOrWhiteSpace(entry.Usage.Guidance.Token),
                $"Intended usage must be explicit for {entry.Name}.");
            if (entry.Usage.PreferredEntry is { } preferred)
                Check(entries.Any(candidate => candidate.ComponentType == preferred && candidate.IsProductionEntry),
                    $"Preferred production entry is undocumented for {entry.Name}.");
            var declared = entry.ComponentType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.IsDefined(typeof(ParameterAttribute), true)).Select(property => property.Name)
                .Order(StringComparer.Ordinal).ToArray();
            var documented = entry.ApiParameters.Where(parameter => !parameter.Name.Contains('.'))
                .Select(parameter => parameter.Name).Order(StringComparer.Ordinal).ToArray();
            Check(declared.SequenceEqual(documented), $"Own/inherited API parameter coverage differs for {entry.Name}.");
            Check(entry.ApiParameters.Select(parameter => parameter.Name).Distinct(StringComparer.Ordinal).Count()
                == entry.ApiParameters.Count, $"Duplicate API rows in {entry.Name}.");
        }

        ComponentEntry Entry(Type type) => entries.Single(entry => entry.ComponentType == type);
        string Default(Type type, string name) => Entry(type).ApiParameters.Single(parameter => parameter.Name == name).DefaultValue;
        void Equal(string expected, Type type, string name)
            => Check(Default(type, name) == expected, $"Unexpected {type.Name}.{name} default: {Default(type, name)}.");

        Equal("false", typeof(Controls.Button), "Disabled");
        Equal("\"\"", typeof(Controls.Button), "Description");
        Equal("\"\"", typeof(Controls.Button), "TrailingText");
        Equal("HorizontalAlignment.Center", typeof(Controls.InlineActions), "Alignment");
        Equal("EmptyStateVariant.Standard", typeof(Controls.EmptyState), "Variant");
        Equal("PresentationTone.Canvas", typeof(Controls.PresentationBand), "Tone");
        Equal("2", typeof(Controls.PresentationBand), "HeadingLevel");
        Equal("true", typeof(Controls.OfferStage), "AutoRotate");
        Equal("2200", typeof(Controls.OfferStage), "RotationIntervalMilliseconds");
        Equal("false", typeof(Controls.AccessPanel), "Wide");
        Equal("false", typeof(Controls.AccessPanel), "Emphasized");
        Equal("false", typeof(ArkheideSystem.Flourish.Blazor.Components.Patterns.ContentSurface), "DocumentFlow");
        Equal("ButtonVariant.Primary", typeof(Controls.Button), "Variant");
        Equal("\"button\"", typeof(Controls.Button), "Type");
        Equal("\"\"", typeof(Controls.Button), "Icon");
        Equal(string.Empty, typeof(Controls.Button), "Href");
        Equal(string.Empty, typeof(Controls.Button), "OnClick");
        Equal(string.Empty, typeof(Controls.Button), "ChildContent");
        Equal("true", typeof(Controls.DisplayBoard), "Dotted");
        Equal("UniformGridShape.Rectangle", typeof(Controls.UniformGrid), "Shape");
        Equal("UniformGridVariant.Elevated", typeof(Controls.UniformGrid), "Variant");
        Equal("280", typeof(Controls.UniformGrid), "MaxCellSize");
        Equal("260", typeof(Controls.UniformGrid), "MaxCellHeight");
        Equal(string.Empty, typeof(Controls.UniformGrid), "CellHeight");
        Equal("false", typeof(Controls.PageBody), "FillHeight");
        Equal("false", typeof(Controls.PageBody), "CompactSpacing");
        Equal("false", typeof(Controls.UniformGrid), "Centered");
        foreach (var type in new[] { typeof(Controls.UniformGrid), typeof(Controls.UniformGridItem), typeof(Controls.UniformGridButton) })
            Equal(string.Empty, type, "IconSupport");
        Equal(string.Empty, typeof(Controls.UniformGrid), "Columns");
        Equal("false", typeof(Controls.CheckBox), "Value");
        Equal("false", typeof(Controls.ExpansionIndicator), "Expanded");
        Equal(string.Empty, typeof(Controls.CheckBox), "ValueChanged");
        Equal(string.Empty, typeof(Controls.CheckBox), "ValueExpression");
        Equal("\"any\"", typeof(Controls.NumberBox<>), "Step");
        Equal(string.Empty, typeof(Controls.NumberBox<>), "Value");
        Equal(string.Empty, typeof(Controls.NumberBox<>), "ValueChanged");
        Equal("[]", typeof(Controls.SelectBox<>), "Options");
        Equal("10", typeof(Controls.DataTable<>), "PageSize");
        Equal("true", typeof(Controls.DataTable<>), "Searchable");
        Equal("TableView.Table", typeof(Controls.DataTable<>), "View");
        Equal("CultureInfo.CurrentCulture", typeof(Controls.DataTable<>), "Culture");
        Equal("new TableText()", typeof(Controls.DataTable<>), "Text");
        Equal("[]", typeof(Controls.DataTable<>), "Items");
        Equal("[]", typeof(Controls.ListView<>), "Items");
        Equal("[]", typeof(Controls.ListView<>), "Columns");
        Equal("CultureInfo.CurrentCulture", typeof(Controls.ListView<>), "Culture");
        Equal("\"Records\"", typeof(Controls.ListView<>), "Label");
        Equal("\"No items.\"", typeof(Controls.ListView<>), "EmptyMessage");
        foreach (var name in new[] { "ItemKey", "RowHeaderKey", "CellTemplate", "Caption", "Class", "AdditionalAttributes" })
            Equal(string.Empty, typeof(Controls.ListView<>), name);
        Equal(string.Empty, typeof(Primitives.ReferenceDropdown<>), "Value");
        Equal("[]", typeof(Controls.MultiSelectBox), "Items");
        Equal("0", typeof(Controls.MultiSelectBox), "MinimumSelected");
        Equal("2147483647", typeof(Controls.MultiSelectBox), "MaximumSelections");
        Equal("false", typeof(Controls.MultiSelectBox), "CanReorder");
        Equal("false", typeof(Controls.MultiSelectBox), "Searchable");
        Equal("new GridInteractions()", typeof(Primitives.EditingGrid), "Interactions");

        foreach (var type in new[] { typeof(Controls.UniformGridButton), typeof(Controls.UniformGridItem) })
        {
            var entry = Entry(type);
            Check(entry.ApiParameters.All(parameter => parameter.Name != "Shape"), "Shape is not a child component parameter.");
            foreach (var name in new[] { "Shape", "Columns", "Rows", "NarrowColumns", "MaxCellSize", "MaxCellHeight", "CellHeight", "IconSupport", "Variant", "Centered" })
            {
                var contextual = entry.ApiParameters.Single(parameter => parameter.Name == $"UniformGrid.{name}");
                var actual = Entry(typeof(Controls.UniformGrid)).ApiParameters.Single(parameter => parameter.Name == name);
                Check(contextual.Type == actual.Type && contextual.DefaultValue == actual.DefaultValue,
                    $"Parent {name} configuration must match the real UniformGrid API.");
                Check(contextual.DescriptionPrefixKey == "Key.Catalog_Parameter_UniformGridOwner" && contextual.DescriptionOwner == type.Name && contextual.DescriptionKey == actual.DescriptionKey, "Parent rows require explicit ownership.");
            }
        }
        Equal("2", typeof(Controls.UniformGridButton), "FormActions.Columns");
        Check(Entry(typeof(Controls.UniformGrid)).ApiParameters.All(parameter => parameter.Name != "Filled")
            && Entry(typeof(Controls.PresentationFooter)).ApiParameters.All(parameter => parameter.Name is not ("BrandName" or "Watermark")),
            "Gallery still documents retired compatibility parameters.");
        Check(Entry(typeof(Primitives.SearchAutocomplete<>)).ApiParameters.Single(parameter => parameter.Name == "FilterItems")
            .DescriptionKey == "Key.Parameter_FilterItems_bool", "Boolean filtering must not be described as a function.");
        var multipleSelection = Entry(typeof(Controls.MultiSelectBox)).ApiParameters.Single(parameter => parameter.Name == "Changed");
        Check(multipleSelection.Type.Contains("MultiSelectChange", StringComparison.Ordinal),
            "Generic multi-selection must publish the shared ordered selection snapshot.");
        Check(Entry(typeof(Controls.MultiSelectBox)).ApiParameters.All(parameter => parameter.Name is not ("SelectedValues" or "SelectionChanged" or "MinimumVisible")),
            "Retired selection or display-specific contracts remain documented.");
        var referenceSelection = Entry(typeof(Primitives.ReferenceDropdown<>)).ApiParameters.Single(parameter => parameter.Name == "ValueChanged");
        Check(referenceSelection.DescriptionKey == "Key.Parameter_ValueChanged_Primitives_ReferenceDropdown",
            "Reference selection must state its actual nullable Value/ValueChanged binding contract.");
        Check(Entry(typeof(Primitives.ReferenceDropdown<>)).ApiParameters.All(parameter => parameter.Name is not ("SelectedId" or "SelectionChanged")),
            "Reference dropdown still exposes the retired selection parameter family.");
        foreach (var type in new[] { typeof(ArkheideSystem.Flourish.Blazor.Components.Patterns.ContentSurface), typeof(ArkheideSystem.Flourish.Blazor.Components.Patterns.NavigationSurface), typeof(Primitives.ShellHeader) })
            Check(Entry(type).ApiParameters.Any(parameter => parameter.Name == "Class")
                && Entry(type).ApiParameters.All(parameter => parameter.Name != "CssClass"),
                $"{type.Name} must use the single Class parameter contract.");
        Console.WriteLine($"Catalog audit: {entries.Length} components, {entries.Sum(entry => entry.ApiParameters.Count)} rows, "
            + $"{entries.Sum(entry => entry.ApiParameters.Count(parameter => parameter.DefaultValue.Length > 0))} documented defaults.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
