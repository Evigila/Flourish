using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ArkheideSystem.Flourish.WPF.Abstract;
using F = ArkheideSystem.Flourish.WPF.Controls;

namespace ArkheideSystem.Gallery.Flourish.WPF;

internal static class SimpleSamples
{
    internal static object Create(Type type, Window owner, ITextProvider texts) => type.Name switch
    {
        "Card" => Cards(),
        "Section" => new F.Section { Title = "Contact", Content = Form(), Actions = Button("Save") },
        "PageHeading" => Stack(new F.PageHeading { Title = "Record details", Description = "Page headings keep actions next to the current record.", Actions = Button("Edit") }, new F.PageHeading { Title = "Compact record details", Compact = true, Actions = Button("Back", ButtonVariant.Quiet) }),
        "PageBody" => new F.PageBody { Content = Stack(new F.PageHeading { Title = "Business page" }, new F.Section { Title = "Fields", Content = Form() }) },
        "ContentContainer" => new F.ContentContainer { Content = new F.Card { Title = "Bounded content", Content = "This native content remains centered as the window width changes." } },
        "ContentSurface" => new F.ContentSurface { Content = new F.PageBody { Content = new F.Section { Title = "Content host", Content = Form() } } },
        "NavigationSurface" => new F.NavigationSurface { Content = new F.PageBody { Content = new F.Section { Title = "Business content", Content = Form() } } },
        "ShellHeader" => new F.ShellHeader { Title = "Flourish", Description = "Component gallery", Actions = Button("Profile", ButtonVariant.Quiet) },
        "UniformGrid" => Grid(),
        "UniformGridItem" => new F.UniformGrid { Children = { new F.UniformGridItem { Title = "Read-only item", Content = "Facts", SideContent = "24" }, new F.UniformGridItem { Title = "Another item", Content = "Details", SideContent = "48" } } },
        "FormLayout" => Form(),
        "FormGroup" => new F.FormGroup { Title = "Contact details", Content = Form() },
        "FormActions" => new F.FormActions { Children = { Button("Save"), Button("Cancel", ButtonVariant.Secondary) } },
        "InlineActions" => new F.InlineActions { Children = { Button("Continue"), Button("Cancel", ButtonVariant.Secondary) } },
        "Field" => FieldValidation(),
        "ValidationMessages" => new F.ValidationMessages { Messages = ["A cross-field business rule failed.", "The hidden identifier is required."] },
        "Disclosure" => new F.Disclosure { Header = "Supporting details", Content = "These details retain their content when collapsed.", IsExpanded = true },
        "ToggleSection" => ToggleSettings(),
        "Notice" => Notices(),
        "EmptyState" => Stack(new F.EmptyState { Title = "No results", Description = "Change the query to find more records.", Actions = Button("Clear query", ButtonVariant.Secondary) }, new F.EmptyState { Title = "Choose a record", Variant = EmptyStateVariant.Watermark, MinHeight = 220 }),
        "LoadingState" => new F.LoadingState { Title = "Loading records", Content = "The host supplies actual task state." },
        "ProgressBar" => Progress(),
        "ProgressRing" => Rings(),
        "CopyText" => new F.CopyText { Text = "record-24 · Ctrl+C copies selected text" },
        "CodeBlock" => new F.CodeBlock { Text = "new FrameworkBuilder()\n    .ConfigureProject(project => project.SetProjectName(\"Application\"))\n    .Build();" },
        "DisplayBoard" => new F.DisplayBoard { Title = "Copyable preview", Content = Button("Production control"), CopyValue = "new Button { Content = \"Production control\" }" },
        "ImagePreview" => new F.ImagePreview { Title = "Native drawing", Alt = "Flourish sample logo", Source = Logo() },
        "LogoDisplayer" => new F.LogoDisplayer { Source = Logo(), Title = "Flourish", Artistic = true },
        "PresentationBand" => new F.PresentationBand { Title = "Full-width presentation band", Content = new F.ContentContainer { Content = "Presentation content uses library-owned geometry." }, Tone = PresentationTone.Primary },
        "PresentationHero" => new F.PresentationHero { Title = "Beautiful native experiences", Description = "The same design language, native desktop interactions.", StackTitleWords = false, Actions = Button("Explore controls"), Tone = PresentationTone.Primary, SideContent = new F.Card { Title = "24 controls", Content = "Native preview content" } },
        "PresentationFooter" => new F.PresentationFooter { Title = "Flourish", Description = "Native component gallery", Content = "© 2026 ArkheideSystem" },
        "OfferStage" => new F.OfferStage { Offers = [new F.OfferCard { Id = "personal", Title = "Personal", Description = "For an individual project", Content = "Sample offer", Actions = Button("Choose") }, new F.OfferCard { Id = "team", Title = "Team", Description = "For collaboration", Content = "Sample offer", Actions = Button("Choose") }] },
        "OfferCard" => new F.OfferCard { Title = "Team", Description = "Business capabilities are supplied by the host", Content = "Sample price: 24", Actions = Button("Choose") },
        "AccessBrand" => new F.AccessBrand { Title = "Flourish", Description = "Access your project" },
        "AccessPanel" => new F.AccessPanel { Title = "Welcome back", Content = AccessForm(), Emphasized = true, Brand = new F.AccessBrand { Title = "Flourish", Description = "Access your project" } },
        "AccessFormSurface" => new F.AccessFormSurface { Content = AccessForm() },
        "AccessActions" => new F.AccessActions { Children = { Button("Continue"), Button("Recover account", ButtonVariant.Quiet) } },
        "AttributionFooter" => new F.AttributionFooter { Title = "Flourish", Content = "Built with native Flourish controls" },
        "InteractionBoundary" => InteractionLock(),
        "PrimaryNavigationItem" => NavigationButton(new F.PrimaryNavigationItem { Text = "Home", Icon = "home", IsSelected = true }),
        "SecondaryNavigationItem" => NavigationButton(new F.SecondaryNavigationItem { Text = "Details", IsSelected = true }),
        "NavigationChoices" => NavigationChoices(),
        "ServiceMenu" => Services(),
        "SectionNavigator" => SectionNavigation(),
        "BackToTop" => ScrollToTop(),
        "ExpansionIndicator" => new F.ExpansionIndicator(),
        "FilePicker" => Files(),
        "Icon" => new F.Icon { Name = "home" },
        "DropdownSurface" => Dropdown(),
        "NoticeTrigger" => NoticeDetails(),
        "NavigationGuard" => Guard(),
        _ => throw new InvalidOperationException($"No executable sample is registered for {type.FullName}.")
    };

    private static F.FormLayout Stack(params UIElement[] children)
    {
        var result = new F.FormLayout { Columns = 1 };
        foreach (var child in children) result.Children.Add(child);
        return result;
    }
    private static F.Button Button(string text, ButtonVariant variant = ButtonVariant.Primary)
    {
        var button = new F.Button { Content = text, Variant = variant };
        button.Click += (_, _) => button.Content = text + " ✓";
        return button;
    }
    private static F.FormLayout Form() => new()
    {
        Columns = 2,
        Children =
        {
            new F.Field { Label = "Name", Required = true, Content = new F.TextBox { Text = "Ada Lovelace" } },
            new F.Field { Label = "Department", Content = new F.SelectBox { Options = [new("design", "Design"), new("engineering", "Engineering")], SelectedValue = "engineering" } },
            new F.Field { Label = "Quantity", Content = new F.NumberBox { Value = 24, Minimum = 0 } },
            new F.Field { Label = "Date", Content = new F.DateBox { SelectedDate = DateTime.Today } }
        }
    };
    private static object FieldValidation()
    {
        var text = new F.TextBox { Placeholder = "Required name" };
        var field = new F.Field { Label = "Name", Required = true, Content = text, Errors = ["Enter a name."] };
        var validate = Button("Validate");
        validate.Click += (_, _) => field.Errors = string.IsNullOrWhiteSpace(text.Text) ? ["Enter a name."] : [];
        return Stack(field, validate);
    }
    private static object Cards() => Stack(
        new F.Card { Title = "Ada Lovelace", Description = "Engineering · Active", Stacked = true, Content = new F.CopyText { Text = "person-0001" }, Actions = Button("Edit") },
        new F.Card { Title = "Record summary", Description = "Shared content and action structure", Content = "A record's facts belong to the consumer.", Actions = Button("Open record") },
        new F.Card { Title = "Prominent summary", Description = "Library-owned larger heading", Prominent = true, Content = "Supporting facts", SideContent = "24", Actions = Button("Open", ButtonVariant.Secondary) },
        new F.Card { Title = "Stacked content", Stacked = true, Content = "Content and actions follow the stacked card layout.", Actions = Button("Continue") });
    private static F.UniformGrid Grid() => new()
    {
        Shape = UniformGridShape.Rectangle,
        Columns = 2,
        Children =
        {
            new F.UniformGridItem { Title = "First", Description = "Read-only", Content = "24" },
            new F.UniformGridItem { Title = "Second", Description = "Read-only", Content = "48" },
            new F.UniformGridButton { Text = "Third", Description = "Interactive" },
            new F.UniformGridButton { Text = "Fourth", Description = "Interactive" }
        }
    };
    private static object Notices()
    {
        var stack = new F.FormLayout { Columns = 1 };
        foreach (var kind in Enum.GetValues<NoticeKind>()) stack.Children.Add(new F.Notice { Kind = kind, Title = kind.ToString(), Content = "Feedback reflects the current operation." });
        return stack;
    }
    private static object AccessForm() => Stack(new F.Field { Label = "Account", Content = new F.TextBox { Placeholder = "Account identifier" } }, new F.AccessActions { Children = { Button("Continue"), Button("Create account", ButtonVariant.Quiet) } });
    private static object InteractionLock()
    {
        var boundary = new F.InteractionBoundary { Content = Form() };
        var toggle = new F.ToggleSwitch { Content = "Lock while processing" };
        toggle.Click += (_, _) => boundary.Busy = toggle.IsChecked == true;
        return Stack(toggle, boundary);
    }
    private static object ToggleSettings()
    {
        var status = new F.Notice { Title = "Additional settings enabled" };
        var section = new F.ToggleSection { Header = "Additional settings", Content = Form(), Value = true };
        section.ValueChanged += (_, value) => status.Title = value ? "Additional settings enabled" : "Additional settings disabled; retained content keeps its draft";
        return Stack(section, status);
    }
    private static object NavigationButton(F.Button button)
    {
        var status = new F.Notice { Title = "Selected navigation item" };
        button.Click += (_, _) => status.Title = button.Text + " requested";
        return Stack(button, status);
    }
    private static object NavigationChoices()
    {
        var status = new F.Notice { Title = "Choose an access method" };
        var choices = new F.NavigationChoices { Items = [new("account", "Account", "/account"), new("code", "Access code", "/code"), new("unavailable", "Unavailable", "/unavailable", true)] };
        choices.Navigate += (_, destination) => status.Title = "Host destination: " + destination;
        return Stack(choices, status);
    }
    private static object Services()
    {
        var status = new F.Notice { Title = "Choose a service" };
        var services = new F.ServiceMenu { Text = "Services", Links = [new("Projects", "/projects"), new("Account", "/account")] };
        services.Navigate += (_, destination) => status.Title = "Host destination: " + destination;
        return Stack(services, status);
    }
    private static object SectionNavigation()
    {
        var sections = Stack(new F.Section { Title = "Overview", Content = "Overview facts", MinHeight = 220 }, new F.Section { Title = "Details", Content = "Record details", MinHeight = 220 }, new F.Section { Title = "History", Content = "Record history", MinHeight = 220 });
        var navigation = new F.SectionNavigator { Target = sections };
        sections.Loaded += (_, _) => navigation.Refresh();
        return Stack(navigation, sections);
    }
    private static object ScrollToTop() => Stack(new F.Section { Title = "Content", Content = "Scroll down to the BackToTop action.", MinHeight = 900 }, new F.BackToTop());
    private static ImageSource Logo()
    {
        var group = new DrawingGroup();
        group.Children.Add(new GeometryDrawing(new SolidColorBrush(Color.FromRgb(56, 94, 86)), null, new RectangleGeometry(new Rect(0, 0, 96, 96), 16, 16)));
        group.Children.Add(new GeometryDrawing(Brushes.White, null, Geometry.Parse("M 28,24 L 70,24 L 70,36 L 42,36 L 42,44 L 64,44 L 64,56 L 42,56 L 42,76 L 28,76 Z")));
        var result = new DrawingImage(group); result.Freeze(); return result;
    }

    private static object Files()
    {
        var status = new F.Notice { Title = "Choose files to inspect their paths" };
        var picker = new F.FilePicker { Text = "Choose files", Multiple = true, Filter = "Text files|*.txt;*.csv;*.json|All files|*.*" };
        picker.FilesSelected += (_, paths) => status.Title = string.Join(Environment.NewLine, paths);
        return Stack(picker, status);
    }
    private static object Dropdown() => new F.DropdownSurface { Header = "Open surface", Content = Stack(Button("First action"), Button("Second action", ButtonVariant.Secondary)) };
    private static object NoticeDetails() => new F.NoticeTrigger { Text = "Details", Notice = new F.Notice { Title = "Supporting details", Content = "Important failures must be directly visible in a Notice." } };
    private static object Guard()
    {
        var dirty = new F.ToggleSwitch { Content = "Unsaved changes" };
        var status = new F.Notice { Title = "Try a navigation request" };
        var guard = new F.NavigationGuard { CanNavigate = _ => Task.FromResult(dirty.IsChecked != true), Content = new F.TextBox { Text = "Draft content" } };
        var request = Button("Request navigation");
        request.Click += async (_, _) => status.Title = await guard.CheckAsync("/next") ? "Navigation allowed" : "Blocked by unsaved changes";
        return Stack(dirty, guard, request, status);
    }
    private static object Progress()
    {
        var bar = new F.ProgressBar { Value = 64, Status = "Processing records", MinHeight = 48 };
        var progress = new F.NumberBox { Minimum = 0, Maximum = 100, Value = 64 };
        progress.LostKeyboardFocus += (_, _) => { if (progress.Value is { } value) bar.Value = value; };
        var toggle = new F.ToggleSwitch { Content = "Indeterminate" };
        toggle.Click += (_, _) => bar.IsIndeterminate = toggle.IsChecked == true;
        return Stack(bar, progress, toggle);
    }
    private static object Rings() => new F.InlineActions
    {
        Children =
        {
            new F.ProgressRing { Value = 64, IsIndeterminate = false },
            new F.ProgressRing { IsIndeterminate = true },
            new F.ProgressRing { IsIndeterminate = true, IsRunning = false }
        }
    };
}
