using System.Windows;
using System.Windows.Automation;
using ArkheideSystem.Essential.Culture;
using ArkheideSystem.Flourish.Extensions.Culture.WPF;
using ArkheideSystem.Flourish.WPF;
using ArkheideSystem.Flourish.WPF.Abstract;
using F = ArkheideSystem.Flourish.WPF.Controls;
using ThemeSelection = ArkheideSystem.Flourish.WPF.Abstract.ThemeMode;

namespace ArkheideSystem.Gallery.Flourish.WPF;

public sealed class MainWindow : Window
{
    private readonly EssentialTextProvider texts = new("Gallery");
    private readonly ThemeSession theme;
    private readonly F.ApplicationShell shell;

    public MainWindow()
    {
        Width = 1280;
        Height = 880;
        MinWidth = 320;
        MinHeight = 540;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        FrameworkResources.Apply(this);
        theme = DesignResources.Apply(this);
        SetResourceReference(BackgroundProperty, "Flourish.Brush.Canvas");
        SetResourceReference(ForegroundProperty, "Flourish.Brush.Text");

        var builder = new FrameworkBuilder().UseEssentialCulture(texts)
            .ConfigureProject(project => project.SetProjectName(new TextReference("Gallery", "ProjectName", "Flourish Gallery")).SetLogo(Artwork.GalleryLogo))
            .ConfigureTopBar(top => top.AddMenu(new TextReference("Gallery", "Pages", "Pages"), () =>
            [
                new(T("Overview", "Overview"), () => NavigateFromMenuAsync("/")),
                new(T("Category_inputs", "Inputs"), () => NavigateFromMenuAsync("/controls/inputs")),
                new(T("Category_data", "Data"), () => NavigateFromMenuAsync("/controls/data")),
                new(T("Appearance", "Appearance"), () => NavigateFromMenuAsync("/appearance"))
            ]).InjectToRight(CreateSettings));
        builder.ConfigureNavigation(navigation =>
        {
            navigation.AddNav(new TextReference("Gallery", "Overview", "Overview"), "home", "/", Overview);
            foreach (var category in GalleryCatalog.Categories)
            {
                var group = category;
                navigation.AddNav(new TextReference("Gallery", "Category_" + group, group), CategoryIcon(group), "/controls/" + group,
                    () => CategoryPage(group), secondary =>
                    {
                        foreach (var entry in GalleryCatalog.Entries.Where(item => item.Category == group))
                        {
                            var sample = entry;
                            secondary.AddSubNav(sample.Name, "widgets", sample.Route, () => SamplePage(sample));
                        }
                    });
            }
            navigation.AddNav(new TextReference("Gallery", "Appearance", "Appearance"), "palette", "/appearance", Appearance);
        });
        shell = new F.ApplicationShell { Options = builder.Build() };
        Content = shell;
        texts.Changed += TextChanged;
        UpdateTitle();
        Loaded += async (_, _) => await shell.NavigateAsync("/");
        Closed += (_, _) =>
        {
            texts.Changed -= TextChanged;
            texts.Dispose();
            theme.Dispose();
        };
    }

    private string T(string token, string fallback) => texts.Get(new("Gallery", token, fallback));
    private async Task NavigateFromMenuAsync(string route) { await shell.NavigateAsync(route); }
    private void TextChanged(object? sender, EventArgs args) => Dispatcher.InvokeAsync(UpdateTitle);
    private void UpdateTitle() => Title = T("ProjectName", "Flourish Gallery") + " · WPF";

    private TElement Localize<TElement>(TElement element, DependencyProperty property, string token, string fallback)
        where TElement : FrameworkElement
    {
        var subscribed = false;
        void Update() => element.SetCurrentValue(property, T(token, fallback));
        void Changed(object? sender, EventArgs args)
        {
            if (element.Dispatcher.CheckAccess()) { if (subscribed) Update(); }
            else element.Dispatcher.InvokeAsync(() => { if (subscribed) Update(); });
        }
        element.Loaded += (_, _) => { if (subscribed) return; subscribed = true; texts.Changed += Changed; Update(); };
        element.Unloaded += (_, _) => { if (!subscribed) return; subscribed = false; texts.Changed -= Changed; };
        Update();
        return element;
    }

    private object CreateSettings()
    {
        var menu = new F.ServiceMenu { Text = string.Empty, Icon = "settings", Width = 48, Margin = new Thickness(0) };
        menu.SetResourceReference(System.Windows.Controls.Control.ForegroundProperty, "Flourish.Brush.ChromeInk");
        menu.Navigate += (_, target) =>
        {
            if (target.StartsWith("theme:", StringComparison.Ordinal) && Enum.TryParse<ThemeSelection>(target[6..], out var mode)) theme.SetTheme(mode);
            else if (target.StartsWith("culture:", StringComparison.Ordinal)) Localizer.Current.SetCulture(target[8..]);
        };
        var labelsSubscribed = false;
        void RefreshLabels()
        {
            menu.ToolTip = T("Appearance", "Appearance") + " · " + T("Language", "Language");
            AutomationProperties.SetName(menu, (string)menu.ToolTip);
            menu.Links =
            [
                new(T("Theme", "Theme") + ": " + T("System", "System"), "theme:System"),
                new(T("Theme", "Theme") + ": " + T("Light", "Light"), "theme:Light"),
                new(T("Theme", "Theme") + ": " + T("Dark", "Dark"), "theme:Dark"),
                new(T("Language", "Language") + ": English", "culture:en-US"),
                new(T("Language", "Language") + ": 简体中文", "culture:zh-CN")
            ];
        }
        void TextsChanged(object? sender, EventArgs args)
        {
            if (menu.Dispatcher.CheckAccess()) { if (labelsSubscribed) RefreshLabels(); }
            else menu.Dispatcher.InvokeAsync(() => { if (labelsSubscribed) RefreshLabels(); });
        }
        menu.Loaded += (_, _) => { if (!labelsSubscribed) { labelsSubscribed = true; texts.Changed += TextsChanged; RefreshLabels(); } };
        menu.Unloaded += (_, _) => { if (labelsSubscribed) { labelsSubscribed = false; texts.Changed -= TextsChanged; } };
        RefreshLabels();
        return menu;
    }

    private object Overview()
    {
        var stack = new F.FormLayout { Columns = 1 };
        var body = new F.PageBody { Content = stack };
        stack.Children.Add(Localize(new F.PageHeading(), F.Card.TitleProperty, "Overview", "Overview"));
        var description = Localize(new F.Notice(), F.Card.TitleProperty, "NativeTitle", "Native WPF, Blazor standards");
        Localize(description, F.Card.ContentProperty, "NativeDescription", "Framework owns native controls and behavior. Design supplies the optional palette. Abstract owns contracts. The convenience package composes all three.");
        stack.Children.Add(description);
        var choices = new F.UniformGrid { Shape = UniformGridShape.Square };
        foreach (var category in GalleryCatalog.Categories)
        {
            var group = category;
            var button = Localize(new F.UniformGridButton { Icon = CategoryIcon(group) }, F.Button.TextProperty, "Category_" + group, group);
            button.Click += async (_, _) => await shell.NavigateAsync("/controls/" + group);
            choices.Children.Add(button);
        }
        stack.Children.Add(choices);
        return body;
    }

    private object CategoryPage(string category)
    {
        var stack = new F.FormLayout { Columns = 1 };
        var body = new F.PageBody { Content = stack };
        stack.Children.Add(Localize(new F.PageHeading(), F.Card.TitleProperty, "Category_" + category, category));
        var grid = new F.UniformGrid { Shape = UniformGridShape.Square };
        foreach (var entry in GalleryCatalog.Entries.Where(item => item.Category == category))
        {
            var sample = entry;
            var button = new F.UniformGridButton { Text = sample.Name, Icon = CategoryIcon(category) };
            button.Click += async (_, _) => await shell.NavigateAsync(sample.Route);
            grid.Children.Add(button);
        }
        stack.Children.Add(grid);
        return body;
    }

    private object SamplePage(GalleryEntry entry)
    {
        var stack = new F.FormLayout { Columns = 1 };
        var body = new F.PageBody { Content = stack };
        stack.Children.Add(new F.PageHeading { Title = entry.Name });
        var usage = entry.Usage;
        stack.Children.Add(new F.Notice { Kind = NoticeKind.Subtle, Title = usage.Kind + " · " + usage.Scenario, Content = usage.Guidance });
        var preview = Localize(new F.Section { Content = entry.Create(this, texts) }, F.Card.TitleProperty, "ExecutableExample", "Executable example");
        stack.Children.Add(preview);
        var nativeValueProperties = new HashSet<string>(StringComparer.Ordinal)
        {
            "Content", "Header", "Text", "SelectedValue", "SelectedItem", "ItemsSource", "IsChecked", "SelectedDate",
            "Value", "IsIndeterminate", "Command", "CommandParameter", "IsEnabled", "IsReadOnly", "MaxLength"
        };
        var api = string.Join(Environment.NewLine, entry.ComponentType.GetProperties()
            .Where(property => property.DeclaringType?.Assembly == entry.ComponentType.Assembly || nativeValueProperties.Contains(property.Name))
            .Select(property => property.PropertyType.Name + " " + property.Name)
            .Concat(entry.ComponentType.GetEvents()
                .Where(member => member.DeclaringType?.Assembly == entry.ComponentType.Assembly)
                .Select(member => "event " + member.EventHandlerType?.Name + " " + member.Name))
            .Concat(entry.ComponentType.GetMethods()
                .Where(method => method.IsPublic && !method.IsStatic && !method.IsSpecialName && method.DeclaringType?.Assembly == entry.ComponentType.Assembly
                    && method.Name is not ("OnApplyTemplate" or "MeasureOverride" or "ArrangeOverride"))
                .Select(method => method.ReturnType.Name + " " + method.Name + "(" + string.Join(", ", method.GetParameters().Select(parameter => parameter.ParameterType.Name + " " + parameter.Name)) + ")")));
        stack.Children.Add(Localize(new F.Section { Content = new F.CodeBlock { Text = api.Length == 0 ? entry.ComponentType.BaseType?.Name ?? entry.Name : api } }, F.Card.TitleProperty, "PublicApi", "Public API"));
        return body;
    }

    private object Appearance()
    {
        var stack = new F.FormLayout { Columns = 1 };
        var body = new F.PageBody { Content = stack };
        stack.Children.Add(Localize(new F.PageHeading(), F.Card.TitleProperty, "Appearance", "Appearance"));
        var primary = new F.TextBox { Text = theme.Palette.Primary, Placeholder = "#385E56" };
        var accent = new F.TextBox { Text = theme.Palette.Accent, Placeholder = "#DCCAB1" };
        stack.Children.Add(Localize(new F.Field { Content = primary }, F.Field.LabelProperty, "PrimaryColor", "Primary color"));
        stack.Children.Add(Localize(new F.Field { Content = accent }, F.Field.LabelProperty, "AccentColor", "Accent color"));
        var notice = new F.Notice { Title = T("ThemeDescription", "Theme and colors update existing native controls") };
        var apply = Localize(new F.Button(), F.Button.ContentProperty, "Apply", "Apply");
        apply.Click += (_, _) =>
        {
            try { theme.SetColors(primary.Text, accent.Text); notice.Title = T("Applied", "Applied"); }
            catch (ArgumentException error) { notice.Title = error.Message; }
        };
        stack.Children.Add(apply);
        stack.Children.Add(notice);
        return body;
    }

    private static string CategoryIcon(string category) => category switch
    {
        "actions" => "smart_button", "inputs" => "input", "data" => "table_chart", "overlays" => "picture_in_picture",
        "feedback" => "notifications", "progress" => "progress_activity", "content" => "view_module",
        "layout" => "view_quilt", "presentation" => "web", _ => "widgets"
    };
}

