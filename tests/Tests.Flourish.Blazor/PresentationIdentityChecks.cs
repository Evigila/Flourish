using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using ArkheideSystem.Flourish.Blazor.Hosting;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class PresentationIdentityChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("ordinary Cards compose encoded account titles and CopyText identifiers without page chrome", async () =>
        {
            using var services = Services();
            await using var renderer = Renderer(services);
            var html = await Render<Card>(renderer, new()
            {
                [nameof(Card.Title)] = "Member <safe> & name",
                [nameof(Card.ChildContent)] = (RenderFragment)(builder =>
                {
                    builder.OpenComponent<CopyText>(0);
                    builder.AddAttribute(1, nameof(CopyText.Value), "ID-<safe>&001");
                    builder.CloseComponent();
                })
            });
            Require(html.Contains("<article class=\"f-card\">", StringComparison.Ordinal), "An account summary bypassed the ordinary Card renderer.");
            Require(html.Contains("<h3>Member &lt;safe&gt; &amp; name</h3>", StringComparison.Ordinal), "The Card discarded its existing H3 title semantics or encoded account content.");
            Require(html.Contains("<code class=\"f-copy-text\">ID-&lt;safe&gt;&amp;001</code>", StringComparison.Ordinal), "The real CopyText identifier was changed or rendered unsafely.");
            Require(!html.Contains("f-page-heading", StringComparison.Ordinal) && !Regex.IsMatch(html, @"<h[12]\b"), "A Card account title inherited page heading chrome or introduced a second page heading.");
            var untitled = await Render<Card>(renderer, new() { [nameof(Card.Title)] = "" });
            Require(!Regex.IsMatch(untitled, @"<h[1-6]\b"), "An absent name produced an empty heading.");
        }));
        tests.Add(("retired IdentityCard has no exported renderer catalog entry or dedicated style family", () =>
        {
            Require(typeof(Card).Assembly.GetType("ArkheideSystem.Flourish.Blazor.Components.IdentityCard") is null,
                "The overlapping IdentityCard renderer is still exported.");
            Require(!ComponentUsageCatalog.Entries.Keys.Any(type => type.Name == "IdentityCard"), "The retired renderer remains in production usage guidance.");
            var framework = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/framework.css");
            var design = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/controls.css");
            Require(!framework.Contains("f-identity-", StringComparison.Ordinal) && !design.Contains("f-identity-", StringComparison.Ordinal),
                "Dedicated IdentityCard geometry or paint survives the renderer retirement.");
            return Task.CompletedTask;
        }));
        tests.Add(("footer instance project names are encoded and leave global project identity and optional copyright untouched", async () =>
        {
            using var services = Services();
            await using var renderer = Renderer(services);
            var options = services.GetRequiredService<ApplicationOptions>();
            var original = options.Project.Name;
            var configured = await Render<PresentationFooter>(renderer, new());
            var custom = await Render<PresentationFooter>(renderer, new()
            {
                [nameof(PresentationFooter.ProjectName)] = "Organization <safe> & name",
                [nameof(PresentationFooter.Copyright)] = " "
            });
            Require(custom.Contains("<strong>Organization &lt;safe&gt; &amp; name</strong>", StringComparison.Ordinal)
                && custom.Contains("aria-hidden=\"true\">Organization &lt;safe&gt; &amp; name</span>", StringComparison.Ordinal), "Footer instance identity or watermark did not use the same encoded override.");
            Require(!custom.Contains("©", StringComparison.Ordinal) && !custom.Contains("<span> </span>", StringComparison.Ordinal), "An omitted copyright created a notice or empty content.");
            Require(options.Project.Name == original && (await Render<PresentationFooter>(renderer, new())) == configured, "A footer override mutated shared project configuration or another footer.");
        }));
        tests.Add(("bounded access layout separates stable identity from native operation transport without a second main", async () =>
        {
            using var services = Services();
            await using var renderer = Renderer(services);
            RenderFragment brand = builder =>
            {
                builder.OpenComponent<LogoDisplayer>(0);
                builder.AddAttribute(1, nameof(LogoDisplayer.Compact), true);
                builder.AddAttribute(2, nameof(LogoDisplayer.TitleId), "bounded-title");
                builder.AddAttribute(3, nameof(LogoDisplayer.ContextName), "Service <safe>");
                builder.CloseComponent();
                builder.OpenComponent<Button>(4);
                builder.AddAttribute(5, nameof(Button.Href), "/product");
                builder.AddAttribute(6, nameof(Button.Variant), ButtonVariant.Quiet);
                builder.AddAttribute(7, nameof(Button.Text), "Return");
                builder.CloseComponent();
            };
            RenderFragment form = builder =>
            {
                builder.OpenElement(0, "form");
                builder.AddAttribute(1, "method", "post");
                builder.AddAttribute(2, "action", "/auth/local");
                builder.AddAttribute(3, "data-enhance", "false");
                builder.AddMarkupContent(4, "<input type=\"hidden\" name=\"__RequestVerificationToken\" value=\"fixture\" />");
                builder.OpenComponent<Button>(5);
                builder.AddAttribute(6, nameof(Button.Type), "submit");
                builder.AddAttribute(7, nameof(Button.Text), "Continue");
                builder.CloseComponent();
                builder.CloseElement();
            };
            var plain = await Render<AccessLayout>(renderer, new() { [nameof(AccessLayout.ChildContent)] = form });
            Require(!plain.Contains("f-access-layout-wide", StringComparison.Ordinal)
                && !plain.Contains("f-access-layout-dotted", StringComparison.Ordinal)
                && !plain.Contains("f-access-layout-brand", StringComparison.Ordinal), "AccessLayout enabled optional branding, width or pattern by default.");
            var html = await Render<AccessLayout>(renderer, new()
            {
                [nameof(AccessLayout.Brand)] = brand, [nameof(AccessLayout.ChildContent)] = form,
                [nameof(AccessLayout.Wide)] = true, [nameof(AccessLayout.Dotted)] = true,
                [nameof(AccessLayout.AdditionalAttributes)] = new Dictionary<string, object> { ["aria-labelledby"] = "bounded-title" }
            });
            Require(html.Contains("f-access-layout-wide f-access-layout-dotted", StringComparison.Ordinal)
                && html.Contains("aria-labelledby=\"bounded-title\"", StringComparison.Ordinal), "Bounded operation variants or native attributes disappeared.");
            var body = html.IndexOf("class=\"f-access-layout-body\"", StringComparison.Ordinal);
            Require(body > html.IndexOf("class=\"f-access-layout-brand\"", StringComparison.Ordinal)
                && html.IndexOf("href=\"/product\"", StringComparison.Ordinal) < body,
                "Stable return navigation moved into the variable-height operation body.");
            Require(Regex.Matches(html, "<main\\b").Count == 0 && Regex.Matches(html, "<form\\b").Count == 1
                && html.Contains("method=\"post\" action=\"/auth/local\" data-enhance=\"false\"", StringComparison.Ordinal)
                && html.Contains("name=\"__RequestVerificationToken\" value=\"fixture\"", StringComparison.Ordinal)
                && Regex.IsMatch(html, @"<button\b[^>]*type=""submit"""), "Layout replaced the sole main boundary, native POST or real submit control.");
        }));
        tests.Add(("compact identity keeps configured project heading semantics and optional description", async () =>
        {
            using var services = Services();
            await using var renderer = Renderer(services);
            var plain = await Render<LogoDisplayer>(renderer, new() { [nameof(LogoDisplayer.TitleId)] = "ordinary-title" });
            var compact = await Render<LogoDisplayer>(renderer, new()
            {
                [nameof(LogoDisplayer.Compact)] = true, [nameof(LogoDisplayer.HeadingLevel)] = 2,
                [nameof(LogoDisplayer.TitleId)] = "compact-title", [nameof(LogoDisplayer.ContextName)] = "Service <safe>",
                [nameof(LogoDisplayer.Description)] = "Operation & details"
            });
            Require(!plain.Contains("f-logo-displayer-compact", StringComparison.Ordinal) && plain.Contains("<h1 id=\"ordinary-title\"", StringComparison.Ordinal), "Default artistic identity opted into Compact.");
            Require(compact.Contains("f-logo-displayer-compact", StringComparison.Ordinal)
                && compact.Contains("<h2 id=\"compact-title\" class=\"f-logo-displayer-title\"", StringComparison.Ordinal)
                && compact.Contains("Configured project", StringComparison.Ordinal)
                && compact.Contains("Service &lt;safe&gt;", StringComparison.Ordinal)
                && compact.Contains("Operation &amp; details", StringComparison.Ordinal), "Compact changed identity, encoding or heading semantics.");
            var noImage = await Render<LogoDisplayer>(renderer, new() { [nameof(LogoDisplayer.Compact)] = true, [nameof(LogoDisplayer.LogoPath)] = "" });
            Require(!noImage.Contains("<img", StringComparison.Ordinal) && noImage.Contains("Configured project", StringComparison.Ordinal), "Compact requires an image or drops identity when it is absent.");
        }));
        tests.Add(("context-primary identity keeps one encoded heading and leaves ordinary project identity unchanged", async () =>
        {
            using var services = Services();
            await using var renderer = Renderer(services);
            foreach (var compact in new[] { false, true })
            {
                var parameters = new Dictionary<string, object?>
                {
                    [nameof(LogoDisplayer.Compact)] = compact,
                    [nameof(LogoDisplayer.LogoSize)] = 96,
                    [nameof(LogoDisplayer.TitleId)] = "service-identity",
                    [nameof(LogoDisplayer.ProjectName)] = "Brand <safe> & name",
                    [nameof(LogoDisplayer.ContextName)] = "Long service <safe> & name",
                    [nameof(LogoDisplayer.Description)] = "Service description"
                };
                var ordinary = await Render<LogoDisplayer>(renderer, parameters);
                parameters[nameof(LogoDisplayer.ContextPrimary)] = false;
                Require(await Render<LogoDisplayer>(renderer, parameters) == ordinary,
                    "Explicit false changed ordinary Account or Compact identity.");
                parameters[nameof(LogoDisplayer.ContextPrimary)] = true;
                var html = await Render<LogoDisplayer>(renderer, parameters);
                Require(html.Contains("f-logo-displayer-context-primary", StringComparison.Ordinal)
                    && Regex.Matches(html, @"<h[1-6]\b").Count == 1
                    && Regex.IsMatch(html, @"<h1\b[^>]*id=""service-identity""[^>]*>\s*<span class=""f-logo-displayer-project"">Brand &lt;safe&gt; &amp; name</span>\s*<span class=""f-logo-displayer-context"">Long service &lt;safe&gt; &amp; name</span>\s*</h1>"),
                    "Context emphasis dropped the single heading, accessible brand/service order or safe text encoding.");
                Require(Regex.IsMatch(html, @"<img\b[^>]*width=""96""[^>]*height=""96""[^>]*style=""--f-logo-size:96px;""")
                    && html.IndexOf("f-logo-displayer-logo", StringComparison.Ordinal) < html.IndexOf("<h1", StringComparison.Ordinal)
                    && html.IndexOf("</h1>", StringComparison.Ordinal) < html.IndexOf("f-logo-displayer-description", StringComparison.Ordinal),
                    "Context emphasis altered explicit logo dimensions or reordered identity and description.");
                parameters[nameof(LogoDisplayer.LogoPath)] = "";
                var noImage = await Render<LogoDisplayer>(renderer, parameters);
                Require(!noImage.Contains("<img", StringComparison.Ordinal)
                    && noImage.Contains("f-logo-displayer-context-primary", StringComparison.Ordinal)
                    && noImage.Contains("Long service &lt;safe&gt; &amp; name", StringComparison.Ordinal),
                    "An omitted image removed the primary service identity.");
                foreach (var context in new string?[] { null, "", " \t " })
                {
                    parameters[nameof(LogoDisplayer.ContextName)] = context;
                    parameters[nameof(LogoDisplayer.ContextPrimary)] = false;
                    var baseline = await Render<LogoDisplayer>(renderer, parameters);
                    parameters[nameof(LogoDisplayer.ContextPrimary)] = true;
                    Require(await Render<LogoDisplayer>(renderer, parameters) == baseline,
                        "An absent context changed ordinary identity or emitted an empty primary context.");
                }
            }
        }));
        tests.Add(("height-filled native choices remain opt-in and retain forms and available current route in every layout", async () =>
        {
            using var services = Services();
            await using var renderer = Renderer(services);
            foreach (var layout in Enum.GetValues<NavigationChoicesLayout>())
            {
                var parameters = new Dictionary<string, object?>
                {
                    [nameof(NavigationChoices.Id)] = "bounded-methods", [nameof(NavigationChoices.Label)] = "Methods",
                    [nameof(NavigationChoices.Layout)] = layout, [nameof(NavigationChoices.ActiveKey)] = "organization",
                    [nameof(NavigationChoices.Items)] = new NavigationChoiceItem[] { new("organization", "Organization", "/login?method=organization"), new("account", "Account", "/login?method=account") },
                    [nameof(NavigationChoices.ChildContent)] = (RenderFragment<NavigationChoiceItem>)(choice => builder =>
                    {
                        builder.OpenElement(0, "form"); builder.AddAttribute(1, "method", "post"); builder.AddAttribute(2, "action", "/auth/" + choice.Key);
                        builder.OpenComponent<Button>(3); builder.AddAttribute(4, nameof(Button.Type), "submit"); builder.AddAttribute(5, nameof(Button.Text), choice.Label); builder.CloseComponent(); builder.CloseElement();
                    })
                };
                var natural = await Render<NavigationChoices>(renderer, parameters);
                Require(!natural.Contains("f-navigation-choices-fill-height", StringComparison.Ordinal), "Natural native navigation acquired a height constraint.");
                parameters[nameof(NavigationChoices.FillHeight)] = true;
                var bounded = await Render<NavigationChoices>(renderer, parameters);
                Require(bounded.Contains("f-navigation-choices-fill-height", StringComparison.Ordinal)
                    && Regex.Matches(bounded, "<form\\b").Count == 2
                    && Regex.Matches(bounded, "aria-current=\"page\"").Count == 1
                    && Regex.Matches(bounded, @"<section\b[^>]*\shidden(?:\s|=|>)").Count == 1
                    && bounded.Contains("data-enhance-nav=\"false\"", StringComparison.Ordinal)
                    && !Regex.IsMatch(bounded, @"role=""(?:tablist|tab|tabpanel)"""), "FillHeight altered native forms, hidden panels or route semantics.");
            }
        }));
        tests.Add(("explicit LogoSize affects only this instance image and rejects nonpositive sizes", async () =>
        {
            using var services = Services();
            await using var renderer = Renderer(services);
            foreach (var compact in new[] { false, true })
            {
                var baseline = await Render<LogoDisplayer>(renderer, new() { [nameof(LogoDisplayer.Compact)] = compact, [nameof(LogoDisplayer.TitleId)] = "stable-size-default" });
                var expected = compact ? 48 : 96;
                Require(baseline.Contains($"width=\"{expected}\" height=\"{expected}\"", StringComparison.Ordinal)
                    && !baseline.Contains("--f-logo-size:", StringComparison.Ordinal), "Omitted LogoSize changed layout defaults or emitted an instance override.");
                var parameters = new Dictionary<string, object?> { [nameof(LogoDisplayer.Compact)] = compact, [nameof(LogoDisplayer.LogoSize)] = 96 };
                var explicitSize = await Render<LogoDisplayer>(renderer, parameters);
                Require(Regex.IsMatch(explicitSize, @"<img\b[^>]*width=""96""[^>]*height=""96""[^>]*style=""--f-logo-size:96px;"""), "LogoSize did not update the actual image dimensions and image-local supported variable.");
                Require(!Regex.IsMatch(explicitSize, @"<header\b[^>]*--f-logo-size")
                    && explicitSize.Contains("f-logo-displayer-compact", StringComparison.Ordinal) == compact,
                    "An image override leaked to the brand container or changed Compact text mode.");
                Require(await Render<LogoDisplayer>(renderer, new() { [nameof(LogoDisplayer.Compact)] = compact, [nameof(LogoDisplayer.TitleId)] = "stable-size-default" }) == baseline,
                    "An instance size mutated later identity defaults.");
            }
            foreach (var invalid in new[] { 0, -1 })
            {
                try { await Render<LogoDisplayer>(renderer, new() { [nameof(LogoDisplayer.LogoSize)] = invalid }); }
                catch (ArgumentOutOfRangeException exception) when (exception.ParamName == nameof(LogoDisplayer.LogoSize)) { continue; }
                throw new InvalidOperationException("Nonpositive LogoSize was accepted.");
            }
        }));
        tests.Add(("decorative band images preserve empty defaults native forms and every semantic Tone", async () =>
        {
            using var services = Services();
            await using var renderer = Renderer(services);
            RenderFragment form = builder =>
            {
                builder.OpenElement(0, "form"); builder.AddAttribute(1, "method", "post"); builder.AddAttribute(2, "action", "/native-operation");
                builder.AddMarkupContent(3, "<input type=\"hidden\" name=\"__RequestVerificationToken\" value=\"fixture\" />");
                builder.OpenComponent<Button>(4); builder.AddAttribute(5, nameof(Button.Type), "submit"); builder.AddAttribute(6, nameof(Button.Text), "Continue"); builder.CloseComponent(); builder.CloseElement();
            };
            var defaults = new Dictionary<string, object?> { [nameof(PresentationBand.Title)] = "Plain band", [nameof(PresentationBand.TitleId)] = "stable-media-band", [nameof(PresentationBand.ChildContent)] = form };
            var ordinary = await Render<PresentationBand>(renderer, defaults);
            foreach (var empty in new string?[] { null, "", " \t " })
            {
                defaults[nameof(PresentationBand.BackgroundImageUrl)] = empty;
                Require(await Render<PresentationBand>(renderer, defaults) == ordinary, "An absent background image changed ordinary band markup.");
            }
            foreach (var tone in Enum.GetValues<PresentationTone>())
            {
                var parameters = new Dictionary<string, object?>(defaults)
                {
                    [nameof(PresentationBand.BackgroundImageUrl)] = "/images/<safe>&\"poster.svg",
                    [nameof(PresentationBand.Tone)] = tone, [nameof(PresentationBand.MinHeight)] = 320,
                    [nameof(PresentationBand.FullHeight)] = false, [nameof(PresentationBand.IncludeInProgress)] = false,
                    [nameof(PresentationBand.Dotted)] = true
                };
                var html = await Render<PresentationBand>(renderer, parameters);
                var media = html.IndexOf("class=\"f-presentation-band-media\" aria-hidden=\"true\"", StringComparison.Ordinal);
                Require(media > 0 && media < html.IndexOf("class=\"f-content-container\"", StringComparison.Ordinal), "Decorative media moved into the bounded foreground or disappeared.");
                Require(DecorativeImageMatches(html, "/images/&lt;safe&gt;&amp;&quot;poster.svg"),
                    "Media URL is unsafe, lazy loaded, interactive or exposed as meaningful content." + Environment.NewLine + "Renderer markup: " + html);
                Require(html.Contains("f-presentation-" + tone.ToString().ToLowerInvariant(), StringComparison.Ordinal)
                    && html.Contains("f-presentation-dotted", StringComparison.Ordinal)
                    && html.Contains("--f-band-min-height:320px", StringComparison.Ordinal)
                    && !html.Contains("f-presentation-fullheight", StringComparison.Ordinal)
                    && !html.Contains("data-presentation-band=", StringComparison.Ordinal), "Background media changed Tone, Dotted, height or progress ownership.");
                Require(Regex.Matches(html, "<section\\b").Count == 1 && Regex.Matches(html, "<form\\b").Count == 1
                    && html.Contains("method=\"post\" action=\"/native-operation\"", StringComparison.Ordinal)
                    && html.Contains("name=\"__RequestVerificationToken\" value=\"fixture\"", StringComparison.Ordinal)
                    && Regex.IsMatch(html, "<button\\b[^>]*type=\"submit\""), "The decorative renderer introduced a second band or changed native operation transport.");
            }
        }));
        tests.Add(("hero forwards decorative media while preserving its single heading progress side slot and real actions", async () =>
        {
            using var services = Services();
            await using var renderer = Renderer(services);
            RenderFragment actions = builder =>
            {
                builder.OpenComponent<Button>(0); builder.AddAttribute(1, nameof(Button.Text), "Start"); builder.AddAttribute(2, nameof(Button.Href), "/native-start"); builder.AddAttribute(3, "data-enhance-nav", "false"); builder.CloseComponent();
            };
            RenderFragment side = builder => builder.AddMarkupContent(0, "<p data-fixture-side>Bounded side</p>");
            var html = await Render<PresentationHero>(renderer, new()
            {
                [nameof(PresentationHero.Title)] = "Hero <safe>", [nameof(PresentationHero.TitleId)] = "media-hero-title",
                [nameof(PresentationHero.BackgroundImageUrl)] = "/images/hero.svg", [nameof(PresentationHero.Actions)] = actions,
                [nameof(PresentationHero.SideContent)] = side, [nameof(PresentationHero.StackTitleWords)] = true,
                [nameof(PresentationHero.Tone)] = PresentationTone.Primary
            });
            Require(Regex.Matches(html, "<section\\b").Count == 1 && Regex.Matches(html, "<h1\\b").Count == 1
                && Regex.Matches(html, "data-presentation-band=\"true\"").Count == 1, "Hero media duplicated its semantic band, page heading or progress entry.");
            Require(DecorativeImageMatches(html, "/images/hero.svg")
                && html.Contains("f-presentation-primary", StringComparison.Ordinal)
                && html.Contains("f-presentation-fullheight", StringComparison.Ordinal)
                && html.Contains("aria-labelledby=\"media-hero-title\"", StringComparison.Ordinal)
                && html.Contains("aria-label=\"Hero &lt;safe&gt;\"", StringComparison.Ordinal), "Hero did not forward the actual media/Tone or preserve its accessible stacked title." + Environment.NewLine + "Renderer markup: " + html);
            Require(html.Contains("f-presentation-hero-side", StringComparison.Ordinal) && html.Contains("data-fixture-side", StringComparison.Ordinal)
                && html.Contains("href=\"/native-start\"", StringComparison.Ordinal) && html.Contains("data-enhance-nav=\"false\"", StringComparison.Ordinal), "Media displaced the separate side slot or native action.");
            var plain = await Render<PresentationHero>(renderer, new() { [nameof(PresentationHero.Title)] = "Plain" });
            Require(!plain.Contains("f-presentation-band-media", StringComparison.Ordinal) && !plain.Contains("<img", StringComparison.Ordinal), "Default Hero unexpectedly adds a media layer.");
        }));
    }

    private static bool DecorativeImageMatches(string html, string encodedSource)
    {
        var images = Regex.Matches(html, @"<img\b[^>]*>");
        if (images.Count != 1) return false;
        var image = images[0].Value;
        // HtmlRenderer may minimize a static empty alt; both forms retain the empty alternative.
        return Regex.IsMatch(image, @"\ssrc=""" + Regex.Escape(encodedSource) + @"""(?=\s|/?>)")
            && Regex.IsMatch(image, @"\salt(?:="""")?(?=\s|/?>)")
            && Regex.IsMatch(image, @"\saria-hidden=""true""(?=\s|/?>)")
            && !Regex.IsMatch(image, @"\s(?:loading|tabindex|on[a-z]+)(?=\s|=|/?>)");
    }
    private static ServiceProvider Services()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<NavigationManager>(new TestNavigation());
        services.AddFlourishFramework(builder => builder.ConfigureProject(project => project.SetProjectName("Configured project")));
        return services.BuildServiceProvider();
    }
    private static HtmlRenderer Renderer(IServiceProvider services) => new(services, services.GetRequiredService<ILoggerFactory>());
    private static Task<string> Render<T>(HtmlRenderer renderer, Dictionary<string, object?> parameters) where T : IComponent => renderer.Dispatcher.InvokeAsync(async () =>
        (await renderer.RenderComponentAsync<T>(ParameterView.FromDictionary(parameters))).ToHtmlString());
    private static string ReadSource(string path)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "src", "Flourish.Blazor"))) directory = directory.Parent;
        return File.ReadAllText(Path.Combine(directory?.FullName ?? throw new InvalidOperationException("Repository root not found."), path));
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
