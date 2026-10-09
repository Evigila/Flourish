using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using ArkheideSystem.Flourish.WPF;
using Xunit;
using F = ArkheideSystem.Flourish.WPF.Controls;
using ThemeMode = ArkheideSystem.Flourish.WPF.Abstract.ThemeMode;

namespace ArkheideSystem.Tests.Flourish.WPF;

public class AppearanceTests
{
    [Theory]
    [InlineData(ThemeMode.Light, "#153A32", "#16745F", "#F3F5F5", "#FFFFFF", "#112924", "#4F645D", "#9FAEA9")]
    [InlineData(ThemeMode.Dark, "#BBD7C9", "#75CBB2", "#18231D", "#263A30", "#E7EFEA", "#B8C9BF", "#526F60")]
    public void Palette_matches_Blazor_foundation(ThemeMode mode, params string[] colors) => NativeTest.Run(() =>
    {
        var palette = DesignResources.Load(mode);
        if (SystemParameters.HighContrast)
        {
            Assert.Equal(SystemColors.HighlightColor, Assert.IsType<SolidColorBrush>(palette["Flourish.Brush.Primary"]).Color);
            Assert.Equal(SystemColors.HighlightTextColor, Assert.IsType<SolidColorBrush>(palette["Flourish.Brush.PrimaryInk"]).Color);
            Assert.Equal(SystemColors.WindowTextColor, Assert.IsType<SolidColorBrush>(palette["Flourish.Brush.Text"]).Color);
            Assert.Equal(SystemColors.WindowColor, Assert.IsType<SolidColorBrush>(palette["Flourish.Brush.Surface"]).Color);
            Assert.Equal(SystemColors.WindowTextColor, Assert.IsType<SolidColorBrush>(palette["Flourish.Brush.WarningInk"]).Color);
            Assert.All(new[] { "Chrome", "ChromeHover", "ChromePressed" }, role => Assert.Equal(SystemColors.HighlightColor, Assert.IsType<SolidColorBrush>(palette["Flourish.Brush." + role]).Color));
            Assert.Equal(SystemColors.HighlightTextColor, Assert.IsType<SolidColorBrush>(palette["Flourish.Brush.ChromeInk"]).Color);
            Assert.Equal(0, Assert.IsType<DropShadowEffect>(palette["Flourish.Shadow.Control"]).Opacity);
            Assert.Equal(true, palette["Flourish.Motion.Reduced"]);
            return;
        }
        var roles = new[] { "Primary", "Accent", "Canvas", "Surface", "Text", "Muted", "Border" };
        for (var index = 0; index < roles.Length; index++)
            Assert.Equal((Color)ColorConverter.ConvertFromString(colors[index]), Assert.IsType<SolidColorBrush>(palette["Flourish.Brush." + roles[index]]).Color);
        Assert.Equal(Colors.White, Assert.IsType<SolidColorBrush>(palette["Flourish.Brush.SurfaceLight"]).Color);
        Assert.Equal(((SolidColorBrush)palette["Flourish.Brush.Canvas"]).Color, ((SolidColorBrush)palette["Flourish.Brush.SurfaceAlternate"]).Color);
        Assert.Equal("Segoe UI", Assert.IsType<FontFamily>(palette["Flourish.FontFamily"]).Source);
        Assert.Equal(SystemParameters.HighContrast || !SystemParameters.ClientAreaAnimation, palette["Flourish.Motion.Reduced"]);
        Assert.Equal(.16, Assert.IsType<DropShadowEffect>(palette["Flourish.Shadow.Control"]).Opacity);
        var states = mode == ThemeMode.Light
            ? new[] { "#E5E8EB", "#C9DFDA", "#B5C9C4", "#2F5049", "#2A4842", "#9D322D", "#7E2824", "#712420", "#1565C0", "#F2A33A", "#2F5049", "#2A4842" }
            : new[] { "#31483B", "#395A48", "#335141", "#D5E5DE", "#C0CEC8", "#FFB4AB", "#8C3430", "#7E2F2B", "#8CB8FF", "#E5A047", "#395A48", "#335141" };
        var stateRoles = new[] { "DisplayBoard", "SurfaceHover", "SurfacePressed", "PrimaryHover", "PrimaryPressed", "Danger", "DangerHover", "DangerPressed", "Information", "Warning", "ChromeHover", "ChromePressed" };
        for (var index = 0; index < stateRoles.Length; index++)
            Assert.Equal((Color)ColorConverter.ConvertFromString(states[index]), Assert.IsType<SolidColorBrush>(palette["Flourish.Brush." + stateRoles[index]]).Color);
        Assert.Equal(((SolidColorBrush)palette["Flourish.Brush.Surface"]).Color, ((SolidColorBrush)palette["Flourish.Brush.PrimaryInk"]).Color);
        Assert.Equal(((SolidColorBrush)palette[mode == ThemeMode.Light ? "Flourish.Brush.Text" : "Flourish.Brush.Surface"]).Color, ((SolidColorBrush)palette["Flourish.Brush.WarningInk"]).Color);
        Assert.Equal((byte)102, ((SolidColorBrush)palette["Flourish.Brush.Overlay"]).Color.A);
        Assert.All(palette.Keys.Cast<object>().OfType<string>().Where(key => key.StartsWith("Flourish.Brush.", StringComparison.Ordinal)), key => Assert.True(((Brush)palette[key]).IsFrozen));
    });

    [Fact]
    public void Custom_seeds_are_validated_and_keep_explicit_dark_values()
    {
        var custom = AppearancePalette.Create("#abcdef", "#010203");
        Assert.Equal("#ABCDEF", custom.Primary);
        Assert.Equal(custom.Primary, custom.DarkPrimary);
        Assert.Equal(custom.Accent, custom.DarkAccent);
        Assert.Throws<ArgumentException>(() => AppearancePalette.Create("red", "#010203"));
        Assert.Throws<ArgumentException>(() => AppearancePalette.Create("#123", "#010203"));
    }

    [Fact]
    public void Scoped_theme_hot_swaps_dynamic_resources_and_disposes_its_own_dictionary() => NativeTest.Run(() =>
    {
        var scope = new Grid();
        var outside = new ResourceDictionary { ["Host.Marker"] = "retained" };
        scope.Resources.MergedDictionaries.Add(outside);
        var target = new Border();
        target.SetResourceReference(Border.BackgroundProperty, "Flourish.Brush.Primary");
        scope.Children.Add(target);
        using var session = DesignResources.Apply(scope, ThemeMode.Light);
        var before = target.Background;
        session.SetTheme(ThemeMode.Dark);
        NativeTest.Layout(scope, 100, 100);
        if (!SystemParameters.HighContrast) Assert.NotEqual(((SolidColorBrush)before).Color, ((SolidColorBrush)target.Background).Color);
        session.SetColors("#AABBCC", "#223344");
        if (!SystemParameters.HighContrast) Assert.Equal(Color.FromRgb(0xAA, 0xBB, 0xCC), ((SolidColorBrush)target.Background).Color);
        session.Dispose();
        Assert.Single(scope.Resources.MergedDictionaries);
        Assert.Same(outside, scope.Resources.MergedDictionaries[0]);
        Assert.Throws<ObjectDisposedException>(() => session.SetTheme(ThemeMode.Light));
    });

    [Fact]
    public void Native_window_close_disposes_the_theme_owner_without_showing_the_window() => NativeTest.Run(() =>
    {
        var scope = new Window();
        FrameworkResources.Apply(scope);
        var session = DesignResources.Apply(scope);
        Assert.Equal(2, scope.Resources.MergedDictionaries.Count);
        scope.Close();
        Assert.Single(scope.Resources.MergedDictionaries);
        Assert.Throws<ObjectDisposedException>(() => session.SetColors("#ABCDEF", "#123456"));
        session.Dispose();
    });

    [Fact]
    public void Detached_popup_tree_shares_the_live_scope_dictionary_instead_of_copied_colors() => NativeTest.Run(() =>
    {
        var origin = new F.Button { Text = "Open" };
        var root = NativeTest.Stage(origin, 300, 100);
        using var session = DesignResources.Apply(root, ThemeMode.Light);
        var popupSurface = new Border();
        popupSurface.SetResourceReference(Border.BackgroundProperty, "Flourish.Brush.Primary");
        FrameworkResources.ShareResources(origin, popupSurface);
        NativeTest.Layout(popupSurface, 100, 100);
        var before = popupSurface.Background;
        session.SetColors("#AABBCC", "#DDEEFF");
        NativeTest.Layout(popupSurface, 100, 100);
        if (!SystemParameters.HighContrast)
        {
            Assert.NotEqual(((SolidColorBrush)before).Color, ((SolidColorBrush)popupSurface.Background).Color);
            Assert.Equal(Color.FromRgb(0xAA, 0xBB, 0xCC), ((SolidColorBrush)popupSurface.Background).Color);
        }
    });

    [Fact]
    public void Framework_loads_without_Design_and_package_reference_graph_preserves_optional_paint() => NativeTest.Run(() =>
    {
        var scope = new Grid();
        FrameworkResources.Apply(scope);
        var button = new F.Button { Text = "Framework only" };
        scope.Children.Add(button);
        NativeTest.Layout(scope, 300, 100);
        Assert.NotNull(button.Template);
        var references = typeof(F.Button).Assembly.GetReferencedAssemblies();
        Assert.DoesNotContain(references, item => item.Name == "Flourish.WPF.Design");
        Assert.Contains(references, item => item.Name == "Flourish.WPF.Abstract");
        var aggregate = System.Xml.Linq.XDocument.Load(Path.Combine(NativeTest.Repository(), "src", "Flourish.WPF", "Flourish.WPF", "Flourish.WPF.csproj"));
        var names = aggregate.Descendants("ProjectReference").Select(item => (string?)item.Attribute("Include")).ToArray();
        Assert.Contains(names, name => name?.Contains("Flourish.WPF.Abstract") == true);
        Assert.Contains(names, name => name?.Contains("Flourish.WPF.Framework") == true);
        Assert.Contains(names, name => name?.Contains("Flourish.WPF.Design") == true);
    });

    [Fact]
    public void Native_icon_uses_official_artwork_and_preserves_numeric_names_and_native_namescope() => NativeTest.Run(() =>
    {
        Assert.Equal(4299, IconCatalog.Names.Count);
        Assert.True(IconCatalog.Contains("home"));
        Assert.True(IconCatalog.Contains("123"));
        Assert.False(IconCatalog.Contains("not_an_official_icon"));
        var icon = new F.Icon { Name = "123", ElementName = "NumericIcon" };
        var stage = NativeTest.Stage(icon, 26, 26);
        var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap(26, 26, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(stage);
        var geometry = VisualTreeHelper.GetDrawing(icon);
        Assert.NotNull(geometry);
        Assert.False(geometry.Bounds.IsEmpty);
        Assert.Equal("123", icon.Name);
        Assert.Equal("NumericIcon", ((FrameworkElement)icon).Name);
        var parsed = Assert.IsType<F.Icon>(System.Windows.Markup.XamlReader.Parse("<f:Icon xmlns:f='clr-namespace:ArkheideSystem.Flourish.WPF.Controls;assembly=Flourish.WPF.Framework' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' x:Name='NamedIcon' Name='123'/>"));
        Assert.Equal("123", parsed.Name);
        Assert.Equal("NamedIcon", parsed.ElementName);
        Assert.False(icon.Focusable);
        var peer = System.Windows.Automation.Peers.UIElementAutomationPeer.CreatePeerForElement(icon);
        Assert.False(peer.IsControlElement());
        Assert.False(peer.IsContentElement());
        icon.Name = "unknown";
        NativeTest.Layout(icon, 26, 26);
        bitmap.Render(stage);
        Assert.False(VisualTreeHelper.GetDrawing(icon)!.Bounds.IsEmpty);
    });

    [Fact]
    public void Every_official_icon_has_a_native_nonempty_outline_in_the_packaged_font() => NativeTest.Run(() =>
    {
        _ = NativeTest.Stage(new F.Icon(), 26, 26);
        var outline = typeof(IconCatalog).GetMethod("Outline", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
        foreach (var name in IconCatalog.Names)
        {
            var geometry = Assert.IsAssignableFrom<Geometry>(outline.Invoke(null, [name, false]));
            Assert.False(geometry.IsEmpty(), name);
            Assert.True(geometry.IsFrozen, name);
            var filled = Assert.IsAssignableFrom<Geometry>(outline.Invoke(null, [name, true]));
            Assert.False(filled.IsEmpty(), name);
            Assert.True(filled.IsFrozen, name);
        }
    });

    [Fact]
    public void Filled_icon_uses_the_official_variable_axis_and_hot_swaps_native_geometry() => NativeTest.Run(() =>
    {
        var icon = new F.Icon { Name = "home", Size = 48 };
        var stage = NativeTest.Stage(icon, 48, 48);
        var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap(48, 48, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(icon);
        var outlinedPixels = new byte[48 * 48 * 4]; bitmap.CopyPixels(outlinedPixels, 48 * 4, 0);
        icon.Filled = true;
        NativeTest.Layout(stage, 48, 48);
        bitmap.Clear(); bitmap.Render(icon);
        var filledPixels = new byte[48 * 48 * 4]; bitmap.CopyPixels(filledPixels, 48 * 4, 0);
        Assert.True(filledPixels.Where((_, index) => index % 4 == 3).Sum(alpha => alpha) > outlinedPixels.Where((_, index) => index % 4 == 3).Sum(alpha => alpha) * 1.5);
        var method = typeof(IconCatalog).GetMethod("Outline", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
        Assert.Same(method.Invoke(null, ["help_outline", true]), method.Invoke(null, ["unknown", true]));
    });
}
