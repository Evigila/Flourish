using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ArkheideSystem.Flourish.WPF.Abstract;
using ArkheideSystem.Gallery.Flourish.WPF;
using Xunit;
using F = ArkheideSystem.Flourish.WPF.Controls;

namespace ArkheideSystem.Tests.Flourish.WPF;

public class CatalogTests
{
    [Fact]
    public void Every_public_native_control_has_usage_metadata_and_executable_gallery_registration()
    {
        var assembly = typeof(F.Button).Assembly;
        var controls = assembly.GetExportedTypes().Where(type => type.Namespace == typeof(F.Button).Namespace && !type.IsAbstract && typeof(FrameworkElement).IsAssignableFrom(type)).OrderBy(type => type.Name).ToArray();
        Assert.True(controls.Length >= 60, "The native port inventory must cover the current semantic capabilities.");
        Assert.Equal(controls, GalleryCatalog.Entries.Select(item => item.ComponentType).OrderBy(type => type.Name));
        Assert.Equal(GalleryCatalog.Entries.Count, GalleryCatalog.Entries.Select(item => item.Route).Distinct(StringComparer.Ordinal).Count());
        var catalog = Assert.IsAssignableFrom<Type>(assembly.GetType("ArkheideSystem.Flourish.WPF.ComponentUsageCatalog"));
        var entries = Assert.IsAssignableFrom<IEnumerable>(catalog.GetProperty("Entries")!.GetValue(null));
        var keys = entries.Cast<object>().Select(pair => (Type)pair.GetType().GetProperty("Key")!.GetValue(pair)!).ToHashSet();
        Assert.All(controls, type => Assert.Contains(type, keys));
        Assert.DoesNotContain(controls, type => type.Name is "ConfirmationHost" or "ConfirmationService" or "StandaloneTextBox" or "MultiSelectDropdown");
    }

    public static IEnumerable<object[]> Samples() => GalleryCatalog.Entries.Select(entry => new object[] { entry.Route });

    [Theory]
    [MemberData(nameof(Samples))]
    public void Gallery_sample_constructs_and_native_templates_resolve_offscreen(string route) => NativeTest.Run(async () =>
    {
        var owner = new Window();
        try
        {
            var entry = GalleryCatalog.Entries.Single(item => item.Route == route);
            var sample = Assert.IsAssignableFrom<FrameworkElement>(entry.Create(owner, new LiteralProvider()));
            var stage = NativeTest.Stage(sample, 1000, 800);
            using var connection = NativeTest.Connect(stage);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            NativeTest.Layout(stage, 1000, 800);
            Assert.True(stage.DesiredSize.Width > 0, entry.Name);
            foreach (var control in NativeTest.Descendants<Control>(stage))
            {
                control.ApplyTemplate();
                Assert.True(control.Template is not null, entry.Name + ": " + control.GetType().Name + " requires a native template.");
            }
            NativeTest.Export(stage, "catalog-" + route.Trim('/').Replace('/', '-'), 1000, 800);
        }
        finally { owner.Close(); }
    });

    internal sealed class LiteralProvider : ITextProvider
    {
        public string Culture => "en-US";
        public CultureInfo FormatCulture => CultureInfo.GetCultureInfo("en-US");
        public event EventHandler? Changed { add { } remove { } }
        public string Get(TextReference text, params object?[] arguments) => arguments.Length == 0 ? text.FallbackText ?? text.Token : string.Format(FormatCulture, text.FallbackText ?? text.Token, arguments);
    }
}
