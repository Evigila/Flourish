using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ArkheideSystem.Flourish.WPF.Abstract;
using ArkheideSystem.Gallery.Flourish.WPF;
using Xunit;
using F = ArkheideSystem.Flourish.WPF.Controls;
using ThemeMode = ArkheideSystem.Flourish.WPF.Abstract.ThemeMode;

namespace ArkheideSystem.Tests.Flourish.WPF;

public class RenderTests
{
    [Fact]
    public void Export_loaded_split_hero_with_enough_viewport_for_complete_native_copy() => NativeTest.Run(async () =>
    {
        var owner = new Window();
        try
        {
            var entry = GalleryCatalog.Entries.Single(item => item.ComponentType == typeof(F.PresentationHero));
            var hero = Assert.IsAssignableFrom<FrameworkElement>(entry.Create(owner, new CatalogTests.LiteralProvider()));
            var stage = NativeTest.Stage(hero, 1440, 1200);
            using var connection = NativeTest.Connect(stage);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            NativeTest.Export(stage, "presentation-hero-desktop-complete", 1440, 1200);
        }
        finally { owner.Close(); }
    });

    [Theory]
    [InlineData(ThemeMode.Light)]
    [InlineData(ThemeMode.Dark)]
    public void Export_native_control_contact_sheet_without_starting_a_window(ThemeMode mode) => NativeTest.Run(() =>
    {
        var content = new StackPanel { Margin = new Thickness(28) };
        var heading = new TextBlock { Text = "Native Flourish WPF · " + mode, FontSize = 34, Margin = new Thickness(0, 0, 0, 24) };
        heading.SetResourceReference(TextBlock.ForegroundProperty, "Flourish.Brush.Text"); content.Children.Add(heading);
        var actions = new WrapPanel();
        foreach (var variant in Enum.GetValues<ButtonVariant>()) actions.Children.Add(new F.Button { Text = variant.ToString(), Variant = variant, Margin = new Thickness(0, 0, 12, 12), Icon = "check" });
        content.Children.Add(actions);
        content.Children.Add(new F.Button { Text = "Edit record", Description = "Change the contact details", TrailingText = "Ctrl+E", Margin = new Thickness(0, 0, 0, 24) });
        var inputs = new F.FormLayout();
        inputs.Children.Add(new F.Field { Label = "Name", Required = true, Content = new F.TextBox { Text = "Ada Lovelace" } });
        inputs.Children.Add(new F.Field { Label = "Department", Content = new F.SelectBox { Options = new[] { new SelectOption("design", "Design") }, SelectedValue = "design" } });
        inputs.Children.Add(new F.CheckBox { Content = "Send a copy", IsChecked = true });
        inputs.Children.Add(new F.ToggleSwitch { Content = "Enable reminders", IsChecked = true });
        content.Children.Add(inputs);
        content.Children.Add(new F.Notice { Title = "Saved successfully", Description = "Your changes are ready.", Kind = NoticeKind.Success, Margin = new Thickness(0, 24, 0, 24) });
        var uniform = new F.UniformGrid { Columns = 3, CellHeight = 110, Variant = UniformGridVariant.Filled };
        foreach (var title in new[] { "Design", "Engineering", "Operations" }) uniform.Children.Add(new F.UniformGridButton { Text = title });
        content.Children.Add(uniform);
        content.Children.Add(new F.DataTable { Columns = new[] { new TableColumn("name", "Name", nameof(DataTests.Record.Name)), new TableColumn("amount", "Amount", nameof(DataTests.Record.Amount)) },
            ItemsSource = new[] { new DataTests.Record("Ada Lovelace", 12.5m), new DataTests.Record("Grace Hopper", 20m) }, Margin = new Thickness(0, 24, 0, 0) });
        var stage = NativeTest.Stage(content, 1100, 1100, mode);
        NativeTest.Export(stage, "controls-" + mode.ToString().ToLowerInvariant(), 1100, 1100);
    });
}
