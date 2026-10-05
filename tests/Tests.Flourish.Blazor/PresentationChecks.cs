using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using ArkheideSystem.Flourish.Blazor.Components.Patterns;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class PresentationChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("presentation container owns center-width markup without host control skins", async () =>
        {
            var html = await Render<ContentContainer>(new()
            {
                ["Class"] = "scene-content", ["ChildContent"] = Text("Readable content"),
                ["AdditionalAttributes"] = new Dictionary<string, object> { ["id"] = "scene", ["aria-label"] = "Scene content" }
            });
            Check(Regex.IsMatch(html, "^<div[^>]*class=\"f-content-container scene-content\""), "The container lost its standard center-width contract.");
            Check(html.Contains("id=\"scene\"", StringComparison.Ordinal) && html.Contains("aria-label=\"Scene content\"", StringComparison.Ordinal), "Container protocol attributes were lost.");
            Check(html.Contains("Readable content", StringComparison.Ordinal) && !Regex.IsMatch(html, @"<(?:main|form|button)\b"), "The width container introduced an unrelated page or control.");
        }));

        tests.Add(("presentation bands separate full-width surface from center-width content and title semantics", async () =>
        {
            foreach (var tone in Enum.GetValues<PresentationTone>())
            {
                var html = await Render<PresentationBand>(new()
                {
                    ["Title"] = "Features <safe>", ["TitleId"] = "features-title", ["Tone"] = tone,
                    ["ChildContent"] = Text("Content"), ["Actions"] = ButtonFragment(ButtonVariant.Outlined, "Learn"),
                    ["AdditionalAttributes"] = new Dictionary<string, object> { ["id"] = "features" }
                });
                Check(Regex.IsMatch(html, "^<section[^>]*class=\"f-presentation-band f-presentation-" + tone.ToString().ToLowerInvariant()), "Band tone or full-width root differs.");
                Check(html.Contains("aria-labelledby=\"features-title\"", StringComparison.Ordinal)
                    && html.Contains("<h2 id=\"features-title\">Features &lt;safe&gt;</h2>", StringComparison.Ordinal), "Band title naming or encoding was lost.");
                Check(Regex.IsMatch(html, @"<section\b[^>]*>\s*<div\b[^>]*class=""f-content-container(?:\s[^""]*)?"""), "Full-width background and inner limit were not separate layers: " + html);
                Check(html.Contains("f-button-outlined", StringComparison.Ordinal), "A band reskinned its standard action.");
            }
            var untitled = await Render<PresentationBand>(new() { ["ChildContent"] = Text("Plain") });
            Check(!untitled.Contains("aria-labelledby", StringComparison.Ordinal) && !untitled.Contains("<h2", StringComparison.Ordinal), "An untitled band references a missing title.");
            for (var level = 1; level <= 6; level++)
            {
                var heading = await Render<PresentationBand>(new() { ["Title"] = "Ordinary heading", ["TitleId"] = "ordinary-title", ["HeadingLevel"] = level });
                Check(heading.Contains($"<h{level} id=\"ordinary-title\">Ordinary heading</h{level}>", StringComparison.Ordinal), "Band heading level was not semantic or preserved.");
            }
            foreach (var level in new[] { 0, 7 }) await Reject<PresentationBand>(new() { ["HeadingLevel"] = level }, "HeadingLevel");
            await Reject<PresentationBand>(new() { ["Tone"] = (PresentationTone)int.MaxValue }, "Tone");
        }));

        tests.Add(("presentation hero preserves artistic roles and real standard button variants", async () =>
        {
            var html = await Render<PresentationHero>(new()
            {
                ["Title"] = "Product <name>", ["TitleId"] = "product-title", ["Subtitle"] = "A clear subtitle",
                ["Description"] = "Ordinary reading text", ["Actions"] = ButtonFragment(ButtonVariant.Elevated, "Pricing", "/pricing/", native: true)
            });
            Check(html.Contains("aria-labelledby=\"product-title\"", StringComparison.Ordinal)
                && html.Contains("<h1 id=\"product-title\">Product &lt;name&gt;</h1>", StringComparison.Ordinal), "Hero heading lost identity or text encoding.");
            Check(html.Contains("f-presentation-hero-subtitle", StringComparison.Ordinal)
                && html.Contains("f-presentation-hero-description", StringComparison.Ordinal), "Artistic and ordinary copy roles were merged.");
            Check(html.Contains("f-button-elevated", StringComparison.Ordinal) && html.Contains("href=\"/pricing/\"", StringComparison.Ordinal)
                && html.Contains("data-enhance-nav=\"false\"", StringComparison.Ordinal), "Hero replaced the button variant or native route.");
            Check(!html.Contains("f-uniform-grid", StringComparison.Ordinal) && !html.Contains("f-form-actions", StringComparison.Ordinal), "Marketing actions became business grid tiles.");
        }));

        tests.Add(("presentation footer separates decorative wordmark identity and native navigation", async () =>
        {
            var html = await Render<PresentationFooter>(new()
            {
                ["BrandName"] = "Product", ["Watermark"] = "Large brand", ["Copyright"] = "Copyright 2026", ["LinksLabel"] = "Footer links",
                ["Actions"] = ButtonFragment(ButtonVariant.Elevated, "Pricing", "/pricing/", native: true)
            });
            Check(Regex.IsMatch(html, "^<footer class=\"f-presentation-footer\""), "Footer lost native semantics.");
            Check(html.Contains("<span class=\"f-presentation-watermark\" aria-hidden=\"true\">Large brand</span>", StringComparison.Ordinal), "Decorative background wordmark is not hidden from accessibility text.");
            Check(html.Contains("f-content-container f-presentation-footer-content", StringComparison.Ordinal)
                && html.Contains("<strong>Product</strong>", StringComparison.Ordinal)
                && html.Contains("aria-label=\"Footer links\"", StringComparison.Ordinal), "Footer content lost limit, identity or link naming.");
            Check(html.Contains("f-button-elevated", StringComparison.Ordinal) && html.Contains("data-enhance-nav=\"false\"", StringComparison.Ordinal), "Footer did not retain the actual standard native link.");
            var fallback = await Render<PresentationFooter>(new() { ["BrandName"] = "Default wordmark" });
            Check(fallback.Contains("aria-hidden=\"true\">Default wordmark</span>", StringComparison.Ordinal), "An omitted watermark did not use the brand text.");
        }));

        tests.Add(("offer stage SSR leaves every offer and action readable before progressive enhancement", async () =>
        {
            var html = await Render<OfferStage>(new()
            {
                ["Id"] = "offers", ["Label"] = "Five offers", ["LinkScopeId"] = "prices", ["ChildContent"] = Cards(5)
            });
            Check(Regex.Matches(html, @"<article\b[^>]*class=""f-offer-card""").Count == 5, "SSR dropped or duplicated an offer.");
            Check(Regex.Matches(html, "class=\"f-offer-details\"").Count == 5 && Regex.Matches(html, "f-button-filled").Count == 5, "SSR concealed offer details or controls.");
            Check(!Regex.IsMatch(html, @"data-offer-ready|data-active|\shidden(?:=|\s|>)|class=""f-offer-details""[^>]*aria-hidden=""true""|aria-hidden=""true""[^>]*class=""f-offer-details"""), "Enhancement-only inactive state escaped into SSR: " + html);
            Check(html.Contains("role=\"list\"", StringComparison.Ordinal) && Regex.Matches(html, "role=\"listitem\"").Count == 5,
                "Offer collection lost list semantics.");
            Check(Regex.Matches(html, "class=\"f-offer-compact-title\" aria-hidden=\"true\"").Count == 5,
                "Decorative duplicate titles are not hidden from screen readers.");
        }));

        tests.Add(("offer contracts reject invalid or duplicate target IDs and too-fast rotations", async () =>
        {
            foreach (var id in new[] { string.Empty, " ", "two ids", "line\nbreak", "tab\tid" })
                await Reject<OfferCard>(new() { ["Id"] = id, ["Title"] = "Offer" }, "Id");
            foreach (var interval in new[] { -1, 0, 999 })
                await Reject<OfferStage>(new() { ["RotationIntervalMilliseconds"] = interval }, "RotationIntervalMilliseconds");
            var valid = await Render<OfferStage>(new() { ["RotationIntervalMilliseconds"] = 1000, ["ChildContent"] = Cards(1) });
            Check(valid.Contains("offer-0", StringComparison.Ordinal), "The minimum valid interval was rejected.");
            await Reject<OfferStage>(new() { ["ChildContent"] = Cards(2, duplicateIds: true) }, null);
        }));

        tests.Add(("offer rotation exposes a real standard pause control only when automatic rotation is enabled", async () =>
        {
            var html = await Render<OfferStage>(new()
            {
                ["Id"] = "rotating-offers", ["ChildContent"] = Cards(2), ["PauseRotationLabel"] = "Pause offers"
            });
            var control = Regex.Match(html, @"<button\b[^>]*class=""[^""]*\bf-button-quiet\b[^""]*""[^>]*>[^<]*Pause offers\s*</button>");
            Check(control.Success, "Rotation did not expose an actual Flourish Quiet button: " + html);
            Check(control.Value.Contains("type=\"button\"", StringComparison.Ordinal)
                && control.Value.Contains("aria-pressed=\"false\"", StringComparison.Ordinal)
                && Regex.IsMatch(control.Value, @"\sdisabled(?:=|\s|>)"),
                "The uninitialized SSR rotation control must be non-submitting, unpressed and unavailable: " + html);
            Check(Regex.IsMatch(html, @"</div>\s*<div class=""f-offer-controls"">"),
                "Rotation control was inserted among the list items instead of the presentation frame: " + html);
            var manual = await Render<OfferStage>(new() { ["AutoRotate"] = false, ["ChildContent"] = Cards(2) });
            Check(!manual.Contains("f-offer-controls", StringComparison.Ordinal) && !manual.Contains("f-button-quiet", StringComparison.Ordinal),
                "A manually navigated offer stage invented an automatic-rotation control.");
        }));

        tests.Add(("access panel is reusable inside existing main and keeps compact wide and emphasis explicit", async () =>
        {
            var compact = await Render<AccessPanel>(new() { ["Brand"] = Text("Brand"), ["ChildContent"] = Text("Content") });
            Check(compact.StartsWith("<article class=\"f-access-panel", StringComparison.Ordinal)
                && !compact.Contains("f-access-panel-wide", StringComparison.Ordinal), "Compact panel has the wrong width mode.");
            var wide = await Render<AccessPanel>(new() { ["Wide"] = true, ["Emphasized"] = true, ["ChildContent"] = Text("Wide content") });
            Check(wide.Contains("f-access-panel-wide", StringComparison.Ordinal) && wide.Contains("f-access-panel-emphasized", StringComparison.Ordinal), "Explicit access modes were lost.");
            Check(!Regex.IsMatch(compact + wide, @"<(?:main|form)\b"), "A nested access panel introduced an extra main or form.");
        }));

        tests.Add(("access form surface preserves native transport and anti-forgery content without owning a form", async () =>
        {
            var html = await Render<AccessFormSurface>(new()
            {
                ["ChildContent"] = Markup("<form method=\"post\" action=\"/auth/login\" data-enhance=\"false\"><input type=\"hidden\" name=\"__RequestVerificationToken\" value=\"fixture-token\"><input name=\"returnUrl\" value=\"/account/\"></form>"),
                ["AdditionalAttributes"] = new Dictionary<string, object> { ["aria-labelledby"] = "login-title" }
            });
            Check(html.StartsWith("<div class=\"f-access-form-surface\"", StringComparison.Ordinal)
                && html.Contains("aria-labelledby=\"login-title\"", StringComparison.Ordinal), "The access form wrapper lost its standard surface or accessible attributes.");
            Check(Regex.Matches(html, @"<form\b").Count == 1 && html.Contains("method=\"post\" action=\"/auth/login\" data-enhance=\"false\"", StringComparison.Ordinal), "The component nested or rewrote the host's native form.");
            Check(html.Contains("name=\"__RequestVerificationToken\" value=\"fixture-token\"", StringComparison.Ordinal)
                && html.Contains("name=\"returnUrl\" value=\"/account/\"", StringComparison.Ordinal), "Protocol content was discarded.");
            var empty = await Render<AccessFormSurface>(new());
            Check(!Regex.IsMatch(empty, @"<form\b"), "The presentation component invented a transport boundary.");
        }));

        tests.Add(("access actions preserve standard button variants and native links without grid tiles", async () =>
        {
            RenderFragment actions = builder =>
            {
                builder.AddContent(0, ButtonFragment(ButtonVariant.Elevated, "Login", "/login/", native: true));
                builder.AddContent(1, ButtonFragment(ButtonVariant.Outlined, "Cancel"));
            };
            var html = await Render<AccessActions>(new() { ["Label"] = "Entry actions", ["ChildContent"] = actions });
            Check(html.StartsWith("<nav class=\"f-access-actions\" aria-label=\"Entry actions\"", StringComparison.Ordinal), "Access actions lost their named group.");
            Check(html.Contains("f-button-elevated", StringComparison.Ordinal) && html.Contains("f-button-outlined", StringComparison.Ordinal), "An action wrapper changed the consumer's chosen variants.");
            Check(html.Contains("href=\"/login/\"", StringComparison.Ordinal) && html.Contains("data-enhance-nav=\"false\"", StringComparison.Ordinal), "Native entry navigation changed.");
            Check(!html.Contains("f-uniform-grid", StringComparison.Ordinal) && !html.Contains("f-form-actions", StringComparison.Ordinal), "Access actions became oversized business action tiles.");
        }));

        tests.Add(("document-flow content surface keeps skip target and footer outside the single main", async () =>
        {
            var html = await Render<ContentSurface>(new()
            {
                ["DocumentFlow"] = true, ["ContentId"] = "display-main", ["SkipLabel"] = "Skip display content",
                ["TitleBar"] = Markup("<header>Top bar</header>"), ["ChildContent"] = Text("Display content"),
                ["Footer"] = Markup("<footer>Page footer</footer>")
            });
            Check(html.Contains("content-surface f-document-surface", StringComparison.Ordinal), "Document flow was not represented by a source-owned mode.");
            Check(html.Contains("<a class=\"skip-link\" href=\"#display-main\">Skip display content</a>", StringComparison.Ordinal), "Skip link does not target the document content.");
            Check(html.Contains("class=\"f-document-surface-header\"", StringComparison.Ordinal), "The document header did not use its sticky wrapper.");
            Check(Regex.Matches(html, @"<main\b").Count == 1 && Regex.IsMatch(html, @"</main>\s*<footer>Page footer</footer>"), "The footer was hidden in, or introduced another, scrolling main.");
            var business = await Render<ContentSurface>(new() { ["ChildContent"] = Text("Business content") });
            Check(!business.Contains("f-document-surface", StringComparison.Ordinal), "The new display mode changed the existing default business surface.");
        }));
    }

    private static RenderFragment Text(string value) => builder => builder.AddContent(0, value);
    private static RenderFragment Markup(string value) => builder => builder.AddMarkupContent(0, value);
    private static RenderFragment ButtonFragment(ButtonVariant variant, string text, string? href = null, bool native = false) => builder =>
    {
        builder.OpenComponent<Button>(0);
        builder.AddAttribute(1, "Variant", variant);
        builder.AddAttribute(2, "Text", text);
        if (href is not null) builder.AddAttribute(3, "Href", href);
        if (native) builder.AddAttribute(4, "AdditionalAttributes", new Dictionary<string, object> { ["data-enhance-nav"] = "false" });
        builder.CloseComponent();
    };
    private static RenderFragment Cards(int count, bool duplicateIds = false) => builder =>
    {
        for (var index = 0; index < count; index++)
        {
            builder.OpenComponent<OfferCard>(0);
            builder.AddAttribute(1, "Id", duplicateIds ? "duplicate-offer" : $"offer-{index}");
            builder.AddAttribute(2, "Title", $"Offer {index}");
            builder.AddAttribute(3, "Description", $"Description {index}");
            builder.AddAttribute(4, "ChildContent", ButtonFragment(ButtonVariant.Filled, $"Action {index}"));
            builder.CloseComponent();
        }
    };
    private static async Task<string> Render<TComponent>(Dictionary<string, object?> parameters) where TComponent : IComponent
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFlourishFramework();
        services.AddSingleton<NavigationManager>(new TestNavigation("/pricing/"));
        services.AddSingleton<IJSRuntime, NoInterop>();
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var scope = provider.CreateScope();
        await using var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<TComponent>(ParameterView.FromDictionary(parameters))).ToHtmlString());
    }
    private static async Task Reject<TComponent>(Dictionary<string, object?> parameters, string? parameter) where TComponent : IComponent
    {
        try { await Render<TComponent>(parameters); }
        catch (ArgumentException error) when (parameter is null || error.ParamName == parameter) { return; }
        throw new InvalidOperationException($"Invalid {typeof(TComponent).Name} {parameter} was accepted.");
    }
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private sealed class NoInterop : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => throw new InvalidOperationException("SSR must not initialize presentation JavaScript: " + identifier);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
            => throw new InvalidOperationException("SSR must not initialize presentation JavaScript: " + identifier);
    }
}
