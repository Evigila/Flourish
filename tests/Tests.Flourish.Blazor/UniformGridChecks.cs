using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class UniformGridChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("uniform grid centering is opt-in and preserves actual cell geometry", async () =>
        {
            var parameters = new Dictionary<string, object?> { [nameof(UniformGrid.Columns)] = 3, [nameof(UniformGrid.ChildContent)] = PassiveCells() };
            var start = Tag(await Render<UniformGrid>(parameters), "div", "f-uniform-grid");
            Require(!HasClass(start, "f-uniform-grid-centered"), "Existing grids were centered by default.");
            parameters[nameof(UniformGrid.Centered)] = true;
            var centered = Tag(await Render<UniformGrid>(parameters), "div", "f-uniform-grid");
            Require(HasClass(centered, "f-uniform-grid-centered"), "The centered grid has no standard alignment hook.");
            Require(Attribute(start, "style") == Attribute(centered, "style")
                && Attribute(start, "data-grid-layout") == Attribute(centered, "data-grid-layout"), "Centering changed the supplied cell geometry or layout mode.");
            parameters[nameof(UniformGrid.Centered)] = false;
            Require(!HasClass(Tag(await Render<UniformGrid>(parameters), "div", "f-uniform-grid"), "f-uniform-grid-centered"), "Explicit false did not restore start alignment.");
        }));
        tests.Add(("uniform grid defaults to automatic columns and rectangular cells without shaping its container", async () =>
        {
            var html = await Render<UniformGrid>(new() { [nameof(UniformGrid.ChildContent)] = PassiveCells() });
            var grid = Tag(html, "div", "f-uniform-grid");
            Require(Attribute(grid, "data-grid-layout") == "auto", "The default grid forces a column count.");
            Require(HasClass(grid, "f-uniform-grid-rectangle"), "The default cell shape is not Rectangle.");
            Require(HasClass(grid, "f-uniform-grid-variant-elevated"), "The default appearance is not Elevated.");
            Require(!Attribute(grid, "style").Contains("--f-grid-cell-max:", StringComparison.Ordinal), "Rectangle cells inherited the Square size cap.");
            Require(Attribute(grid, "style").Contains("--f-grid-cell-max-height:260px", StringComparison.Ordinal), "Rectangle cells lost their default 260px height cap.");
            Require(!Attribute(grid, "style").Contains("--f-grid-columns", StringComparison.Ordinal), "Automatic layout still emits a fixed column count.");
            Require(!Attribute(grid, "style").Contains("aspect-ratio", StringComparison.Ordinal), "The container itself was shaped as a cell.");
            Require(Tags(html, "f-uniform-grid-item").Count == 3, "The container lost its supplied cells.");
        }));
        tests.Add(("only square grids emit the default 280px maximum side", async () =>
        {
            var square = Tag(await Render<UniformGrid>(new() { [nameof(UniformGrid.Shape)] = UniformGridShape.Square }), "div", "f-uniform-grid");
            Require(Attribute(square, "style").Contains("--f-grid-cell-max:280px", StringComparison.Ordinal), "The default Square maximum side is not 280px.");
            var rectangle = Tag(await Render<UniformGrid>(new() { [nameof(UniformGrid.MaxCellSize)] = 180 }), "div", "f-uniform-grid");
            Require(!Attribute(rectangle, "style").Contains("--f-grid-cell-max:", StringComparison.Ordinal), "A Square size setting constrained Rectangle cells.");
            Require(!Attribute(square, "style").Contains("--f-grid-cell-max-height:", StringComparison.Ordinal), "A Rectangle height setting constrained Square cells.");
        }));
        tests.Add(("rectangular cell height can be configured independently of square size", async () =>
        {
            var rectangle = Tag(await Render<UniformGrid>(new() { [nameof(UniformGrid.MaxCellHeight)] = 160 }), "div", "f-uniform-grid");
            Require(Attribute(rectangle, "style").Contains("--f-grid-cell-max-height:160px", StringComparison.Ordinal), "The configured Rectangle height was not emitted.");
            var square = Tag(await Render<UniformGrid>(new() { [nameof(UniformGrid.Shape)] = UniformGridShape.Square,
                [nameof(UniformGrid.MaxCellHeight)] = 160 }), "div", "f-uniform-grid");
            Require(!Attribute(square, "style").Contains("--f-grid-cell-max-height:", StringComparison.Ordinal), "The Rectangle cap affected Square cells.");
        }));
        tests.Add(("uniform grid exposes independent row and column modes and preserves explicit narrow-column compatibility", async () =>
        {
            var rows = Tag(await Render<UniformGrid>(new() { [nameof(UniformGrid.Rows)] = 2 }), "div", "f-uniform-grid");
            Require(Attribute(rows, "data-grid-layout") == "rows" && Attribute(rows, "style").Contains("--f-grid-rows:2", StringComparison.Ordinal), "Rows-only layout was not retained.");
            Require(!HasClass(rows, "f-uniform-grid-columns"), "Rows-only layout invented columns.");
            var explicitGrid = Tag(await Render<UniformGrid>(new() {
                [nameof(UniformGrid.Rows)] = 2, [nameof(UniformGrid.Columns)] = 6,
                [nameof(UniformGrid.NarrowColumns)] = 1, [nameof(UniformGrid.Filled)] = true,
                [nameof(UniformGrid.Shape)] = UniformGridShape.Square
            }), "div", "f-uniform-grid");
            var style = Attribute(explicitGrid, "style");
            Require(Attribute(explicitGrid, "data-grid-layout") == "explicit" && style.Contains("--f-grid-columns:6", StringComparison.Ordinal) && style.Contains("--f-grid-rows:2", StringComparison.Ordinal), "The explicit dimensions were not preserved.");
            Require(style.Contains("--f-grid-narrow-columns:1", StringComparison.Ordinal) && HasClass(explicitGrid, "f-uniform-grid-narrow-columns"), "The existing narrow-column API stopped being explicit.");
            Require(HasClass(explicitGrid, "f-uniform-filled") && HasClass(explicitGrid, "f-uniform-grid-variant-filled") && HasClass(explicitGrid, "f-uniform-grid-square"), "Existing Filled or the selected cell shape was lost.");
        }));
        tests.Add(("uniform grid shape appearance and configurable cell maximum remain independent", async () =>
        {
            foreach (var shape in new[] { UniformGridShape.Rectangle, UniformGridShape.Square })
                foreach (var variant in new[] { UniformGridVariant.Elevated, UniformGridVariant.Filled, UniformGridVariant.Outlined, UniformGridVariant.Danger })
                {
                    var grid = Tag(await Render<UniformGrid>(new() {
                        [nameof(UniformGrid.Shape)] = shape, [nameof(UniformGrid.Variant)] = variant,
                        [nameof(UniformGrid.MaxCellSize)] = 180
                    }), "div", "f-uniform-grid");
                    Require(HasClass(grid, $"f-uniform-grid-{shape.ToString().ToLowerInvariant()}"), "Changing appearance changed the cell shape.");
                    Require(HasClass(grid, $"f-uniform-grid-variant-{AppearanceName(variant)}"), "Changing shape changed the selected appearance.");
                    Require(Attribute(grid, "style").Contains("--f-grid-cell-max:180px", StringComparison.Ordinal) == (shape == UniformGridShape.Square), "The host-supplied maximum was not confined to Square cells.");
                }
            var alias = Tag(await Render<UniformGrid>(new() { [nameof(UniformGrid.Variant)] = UniformGridVariant.Outline }), "div", "f-uniform-grid");
            Require(HasClass(alias, "f-uniform-grid-variant-outlined"), "Outline does not resolve to the canonical Outlined appearance.");
        }));
        tests.Add(("uniform grid rejects invalid dimensions and unknown shape or appearance values through the correct parameters", async () =>
        {
            foreach (var name in new[] { nameof(UniformGrid.Columns), nameof(UniformGrid.Rows), nameof(UniformGrid.NarrowColumns) })
                foreach (var value in new[] { 0, -1 }) await Reject(new() { [name] = value }, name);
            foreach (var value in new[] { 0, -1 }) await Reject(new() { [nameof(UniformGrid.MaxCellSize)] = value }, nameof(UniformGrid.MaxCellSize));
            foreach (var value in new[] { 0, -1 }) await Reject(new() { [nameof(UniformGrid.MaxCellHeight)] = value }, nameof(UniformGrid.MaxCellHeight));
            await Reject(new() { [nameof(UniformGrid.Shape)] = (UniformGridShape)91 }, nameof(UniformGrid.Shape));
            await Reject(new() { [nameof(UniformGrid.Variant)] = (UniformGridVariant)91 }, nameof(UniformGrid.Variant));
        }));
        tests.Add(("passive and interactive cells inherit grid appearance by default and accept local appearance overrides", async () =>
        {
            var inheritedItem = Tag(await Render<UniformGridItem>(new()), "div", "f-uniform-grid-item");
            var inheritedButton = Tag(await Render<UniformGridButton>(new()), "button", "f-uniform-grid-button");
            Require(!Attribute(inheritedItem, "class").Contains("f-uniform-variant-", StringComparison.Ordinal)
                && !Attribute(inheritedButton, "class").Contains("f-uniform-variant-", StringComparison.Ordinal), "Default cells override their container's appearance.");
            foreach (var variant in new[] { UniformGridVariant.Elevated, UniformGridVariant.Filled, UniformGridVariant.Outlined, UniformGridVariant.Danger })
            {
                var item = Tag(await Render<UniformGridItem>(new() { [nameof(UniformGridItem.Variant)] = variant }), "div", "f-uniform-grid-item");
                var button = Tag(await Render<UniformGridButton>(new() { [nameof(UniformGridButton.Variant)] = variant }), "button", "f-uniform-grid-button");
                var expected = $"f-uniform-variant-{AppearanceName(variant)}";
                Require(HasClass(item, expected) && HasClass(button, expected), "Passive and interactive cells disagree on their local appearance.");
            }
            var alias = Tag(await Render<UniformGridItem>(new() { [nameof(UniformGridItem.Variant)] = UniformGridVariant.Outline }), "div", "f-uniform-grid-item");
            Require(HasClass(alias, "f-uniform-variant-outlined"), "The item did not canonicalize Outline.");
        }));
        tests.Add(("uniform grid item is passive and shares encoded title icon and text composition with interactive cells", async () =>
        {
            var parameters = new Dictionary<string, object?> {
                [nameof(UniformGridItem.Title)] = "Title <unsafe>", [nameof(UniformGridItem.Icon)] = "folder",
                [nameof(UniformGridItem.Text)] = "Text & details"
            };
            var passive = await Render<UniformGridItem>(parameters);
            var active = await Render<UniformGridButton>(parameters);
            Require(!Regex.IsMatch(passive, "<(?:a|button)\\b"), "A passive item creates an interactive element.");
            Require(typeof(UniformGridItem).GetProperty("OnClick") is null && typeof(UniformGridItem).GetProperty("Href") is null, "The passive item exposes action behavior.");
            Require(Heading(passive).Length > 0 && Heading(passive) == Heading(active), "Passive and active cells use different title/icon structure.");
            Require(passive.Contains("Title &lt;unsafe&gt;", StringComparison.Ordinal) && active.Contains("Text &amp; details", StringComparison.Ordinal), "Host content was not encoded.");
            Require(Tags(passive, "f-uniform-cell-title").Count == 1 && Tags(active, "f-uniform-cell-text").Count == 1, "The standard field presenter lost title or text.");
            Require(!active.Contains("<h1", StringComparison.Ordinal), "The button contains an invalid heading element rather than the typographic title role.");
        }));
        tests.Add(("icon support defaults to two zones only when an icon is supplied", async () =>
        {
            foreach (var icon in new[] { "", "  ", "folder" })
            {
                var parameters = new Dictionary<string, object?> { [nameof(UniformGridItem.Title)] = "Title", [nameof(UniformGridItem.Icon)] = icon };
                var item = await Render<UniformGridItem>(parameters);
                var button = await Render<UniformGridButton>(parameters);
                foreach (var html in new[] { item, button })
                {
                    var split = !string.IsNullOrWhiteSpace(icon);
                    Require(Tags(html, "f-uniform-cell-icon").Count == (split ? 1 : 0)
                        && Tags(html, "f-uniform-cell-copy").Count == (split ? 1 : 0), "Automatic icon layout does not follow icon presence.");
                }
            }
        }));
        tests.Add(("explicit icon support preserves legacy flow or reserves both halves independently of icon presence", async () =>
        {
            foreach (var enabled in new[] { false, true })
                foreach (var icon in new[] { "", "folder" })
                {
                    var parameters = new Dictionary<string, object?> {
                        [nameof(UniformGridItem.Title)] = "Title", [nameof(UniformGridItem.Icon)] = icon,
                        [nameof(UniformGridItem.IconSupport)] = enabled,
                        [nameof(UniformGridItem.ChildContent)] = (RenderFragment)(b => b.AddContent(0, "Custom <content>"))
                    };
                    foreach (var html in new[] { await Render<UniformGridItem>(parameters), await Render<UniformGridButton>(parameters) })
                    {
                        Require(Tags(html, "f-uniform-cell-icon").Count == (enabled ? 1 : 0), "Explicit IconSupport was ignored.");
                        Require(html.Contains("Custom &lt;content&gt;", StringComparison.Ordinal), "Custom content was lost or stopped encoding.");
                        Require(html.Contains("data-icon=", StringComparison.Ordinal) == (icon.Length > 0), "IconSupport invented or removed an icon.");
                    }
                }
        }));
        tests.Add(("grid icon support cascades while local cell settings and nested automatic grids take precedence", async () =>
        {
            foreach (var parent in new[] { false, true })
            {
                RenderFragment cells = builder =>
                {
                    builder.OpenComponent<UniformGridItem>(0);
                    builder.AddAttribute(1, nameof(UniformGridItem.Icon), "folder");
                    builder.CloseComponent();
                    builder.OpenComponent<UniformGridButton>(2);
                    builder.AddAttribute(3, nameof(UniformGridButton.Icon), "folder");
                    builder.AddAttribute(4, nameof(UniformGridButton.IconSupport), !parent);
                    builder.CloseComponent();
                    builder.OpenComponent<UniformGrid>(5);
                    builder.AddAttribute(6, nameof(UniformGrid.ChildContent), (RenderFragment)(nested => {
                        nested.OpenComponent<UniformGridItem>(0);
                        nested.AddAttribute(1, nameof(UniformGridItem.Icon), "folder");
                        nested.CloseComponent();
                    }));
                    builder.CloseComponent();
                };
                var html = await Render<UniformGrid>(new() { [nameof(UniformGrid.IconSupport)] = parent, [nameof(UniformGrid.ChildContent)] = cells });
                var items = Tags(html, "f-uniform-grid-item");
                Require(HasClass(items[0], "f-uniform-icon-support") == parent, "Parent IconSupport did not reach its passive cell.");
                Require(HasClass(Tag(html, "button", "f-uniform-grid-button"), "f-uniform-icon-support") != parent, "A local setting did not override its grid.");
                Require(HasClass(items[1], "f-uniform-icon-support"), "An automatic nested grid inherited an unrelated parent's explicit setting.");
            }
        }));
        tests.Add(("uniform grid empty fields create no fake labels or icons and preserve custom content", async () =>
        {
            var empty = await Render<UniformGridItem>(new());
            Require(!empty.Contains("data-icon=", StringComparison.Ordinal) && Tags(empty, "f-uniform-cell-title").Count == 0 && Tags(empty, "f-uniform-cell-text").Count == 0, "An empty cell invented content.");
            var legacy = await Render<UniformGridButton>(new() { [nameof(UniformGridButton.ChildContent)] = (RenderFragment)(builder => builder.AddContent(0, "Legacy <label>")) });
            Require(legacy.Contains("Legacy &lt;label&gt;", StringComparison.Ordinal), "The existing content slot was removed or stopped encoding text.");
        }));
        tests.Add(("uniform grid action links retain shared styling and become nonnavigable while unavailable", async () =>
        {
            foreach (var unavailable in new[] { false, true })
            {
                var html = await Render<UniformGridButton>(new() {
                    [nameof(UniformGridButton.Title)] = "Records", [nameof(UniformGridButton.Href)] = "/records",
                    [nameof(UniformGridButton.Disabled)] = unavailable,
                    [nameof(UniformGridButton.AdditionalAttributes)] = new Dictionary<string, object> { ["HREF"] = "/unexpected", ["aria-label"] = "Open records" }
                });
                var link = Tag(html, "a", "f-uniform-grid-button");
                Require(HasClass(link, "f-uniform-cell") && Attribute(link, "aria-label") == "Open records", "The active cell lost its shared skin or accessible host name.");
                Require(Attribute(link, "href") == (unavailable ? string.Empty : "/records"), "Grid navigation did not follow its declared availability.");
                if (unavailable) Require(!Regex.IsMatch(link, "\\bhref=", RegexOptions.IgnoreCase) && Attribute(link, "aria-disabled") == "true" && Attribute(link, "tabindex") == "-1", "A disabled grid destination remains navigable or tabbable.");
            }
        }));
        tests.Add(("uniform grid action composes submit busy announcement and callback suppression without a DOM wrapper", async () =>
        {
            var calls = 0;
            var activator = new ButtonActivator();
            var services = new ServiceCollection();
            services.AddLogging();
        services.AddFlourishFramework();
            services.AddSingleton<NavigationManager>(new TestNavigation());
            services.AddSingleton<IComponentActivator>(activator);
            using var provider = services.BuildServiceProvider();
            await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
            await renderer.Dispatcher.InvokeAsync(async () =>
            {
                var parameters = new Dictionary<string, object?> {
                    [nameof(UniformGridButton.Type)] = "submit", [nameof(UniformGridButton.Title)] = "Save draft",
                    [nameof(UniformGridButton.Icon)] = "save", [nameof(UniformGridButton.BusyLabel)] = "Saving local draft",
                    [nameof(UniformGridButton.OnClick)] = EventCallback.Factory.Create<MouseEventArgs>(new object(), (MouseEventArgs _) => calls++)
                };
                var output = await renderer.RenderComponentAsync<UniformGridButton>(ParameterView.FromDictionary(parameters));
                var button = activator.Button ?? throw new InvalidOperationException("The grid cell did not reuse Button behavior.");
                var handler = typeof(Button).GetMethod("ClickAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
                async Task Click() => await (Task)handler.Invoke(button, [new MouseEventArgs()])!;
                await Click();
                Require(calls == 1, "A grid click did not dispatch its host action.");
                parameters[nameof(UniformGridButton.Busy)] = true;
                await activator.GridButton!.SetParametersAsync(ParameterView.FromDictionary(parameters));
                var busy = output.ToHtmlString();
                var nativeButton = Tag(busy, "button", "f-uniform-grid-button");
                Require(Attribute(nativeButton, "type") == "submit" && Regex.IsMatch(nativeButton, "\\sdisabled(?:[\\s=>])"), "Busy state lost submit or disabled semantics.");
                Require(Tags(busy, "f-sr-only").Count == 1 && busy.Contains("Saving local draft", StringComparison.Ordinal), "Busy state lost its accessible announcement.");
                Require(HasClass(nativeButton, "f-uniform-icon-support") && Tags(busy, "f-uniform-cell-icon").Count == 1,
                    "Busy state moved the text when temporarily hiding its icon.");
                Require(!Regex.IsMatch(busy, "<div\\b"), "The action introduced a wrapper that breaks grid placement.");
                await Click();
                Require(calls == 1, "A busy grid action still dispatched a callback.");
            });
        }));
    }

    private static RenderFragment PassiveCells() => builder =>
    {
        for (var index = 0; index < 3; index++)
        {
            builder.OpenComponent<UniformGridItem>(0);
            builder.AddAttribute(1, nameof(UniformGridItem.Title), $"Cell {index + 1}");
            builder.CloseComponent();
        }
    };
    private static async Task<string> Render<T>(Dictionary<string, object?> parameters) where T : IComponent
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFlourishFramework();
        services.AddSingleton<NavigationManager>(new TestNavigation());
        using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () => (await renderer.RenderComponentAsync<T>(ParameterView.FromDictionary(parameters))).ToHtmlString());
    }
    private static async Task Reject(Dictionary<string, object?> parameters, string name)
    {
        try { await Render<UniformGrid>(parameters); }
        catch (ArgumentException error) when (error.ParamName == name) { return; }
        throw new InvalidOperationException($"Invalid {name} was accepted.");
    }
    private static string Heading(string html) => Regex.Match(html, "<span class=\"f-uniform-cell-heading\">[\\s\\S]*?</strong></span>").Value;
    private static string AppearanceName(UniformGridVariant variant) => variant switch
    {
        UniformGridVariant.Elevated => "elevated", UniformGridVariant.Filled => "filled",
        UniformGridVariant.Outlined => "outlined", UniformGridVariant.Danger => "danger",
        _ => throw new ArgumentOutOfRangeException(nameof(variant))
    };
    private static IReadOnlyList<string> Tags(string html, string cssClass) => Regex.Matches(html, "<[a-z][a-z0-9]*\\b[^>]*>").Select(match => match.Value).Where(tag => HasClass(tag, cssClass)).ToArray();
    private static string Tag(string html, string element, string cssClass) => Tags(html, cssClass).Single(tag => tag.StartsWith($"<{element} ", StringComparison.Ordinal));
    private static string Attribute(string tag, string name)
    {
        var match = Regex.Match(tag, $"(?:^|\\s){Regex.Escape(name)}=\"([^\"]*)\"");
        return match.Success ? WebUtility.HtmlDecode(match.Groups[1].Value) : string.Empty;
    }
    private static bool HasClass(string tag, string cssClass) => Attribute(tag, "class").Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(cssClass);
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private sealed class ButtonActivator : IComponentActivator
    {
        internal Button? Button { get; private set; }
        internal UniformGridButton? GridButton { get; private set; }
        public IComponent CreateInstance(Type componentType)
        {
            var instance = (IComponent)Activator.CreateInstance(componentType)!;
            if (instance is Button button) Button = button;
            if (instance is UniformGridButton gridButton) GridButton = gridButton;
            return instance;
        }
    }
}
