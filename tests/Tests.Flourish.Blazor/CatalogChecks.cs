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
        Check(entries.Length == 77, "Every registered component must be covered by the API audit.");
        foreach (var entry in entries)
        {
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
        Equal("ButtonVariant.Filled", typeof(Controls.Button), "Variant");
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
        Equal(string.Empty, typeof(Primitives.ReferenceDropdown<>), "SelectedId");
        Equal("[]", typeof(Primitives.MultiSelectDropdown<,>), "SelectedValues");
        Equal("new GridInteractions()", typeof(Primitives.EditingGrid), "Interactions");

        foreach (var type in new[] { typeof(Controls.UniformGridButton), typeof(Controls.UniformGridItem) })
        {
            var entry = Entry(type);
            Check(entry.ApiParameters.All(parameter => parameter.Name != "Shape"), "Shape is not a child component parameter.");
            foreach (var name in new[] { "Shape", "Columns", "Rows", "NarrowColumns", "MaxCellSize", "MaxCellHeight", "IconSupport", "Variant", "Filled" })
            {
                var contextual = entry.ApiParameters.Single(parameter => parameter.Name == $"UniformGrid.{name}");
                var actual = Entry(typeof(Controls.UniformGrid)).ApiParameters.Single(parameter => parameter.Name == name);
                Check(contextual.Type == actual.Type && contextual.DefaultValue == actual.DefaultValue,
                    $"Parent {name} configuration must match the real UniformGrid API.");
                Check(contextual.Description.Contains("配置在外层 UniformGrid 上", StringComparison.Ordinal), "Parent rows require explicit ownership.");
            }
        }
        Equal("2", typeof(Controls.UniformGridButton), "FormActions.Columns");
        Check(Entry(typeof(Primitives.SearchAutocomplete<>)).ApiParameters.Single(parameter => parameter.Name == "FilterItems")
            .Description.Contains("启用", StringComparison.Ordinal), "Boolean filtering must not be described as a function.");
        Console.WriteLine($"Catalog audit: {entries.Length} components, {entries.Sum(entry => entry.ApiParameters.Count)} rows, "
            + $"{entries.Sum(entry => entry.ApiParameters.Count(parameter => parameter.DefaultValue.Length > 0))} documented defaults.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
