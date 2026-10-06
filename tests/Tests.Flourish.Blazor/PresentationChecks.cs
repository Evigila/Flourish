using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using ArkheideSystem.Flourish.Blazor.Components.Patterns;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Primitives = ArkheideSystem.Flourish.Blazor.Components.Primitives;

internal static class PresentationChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("all chrome slots center injected roots without button-specific paint or document margins", () =>
        {
            var layout = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/framework.css");
            const string scope = ":is(.shell-header,.f-titlebar) .f-topbar-slot";
            var slot = Rule(layout, scope);
            Check(slot.Contains("display:flex") && slot.Contains("align-items:center") && slot.Contains("align-self:center"), "Chrome slots do not own their common centering contract.");
            var roots = Rule(layout, scope + " > *, " + scope + " > form > *");
            Check(roots.Contains("align-self:center") && roots.Contains("margin-block:0"), "Injected buttons or form controls can override chrome centering with document-flow alignment.");
            var groupedRoots = Rule(layout, scope + " > .f-inline-actions > *, " + scope + " > form > .f-inline-actions > *");
            Check(groupedRoots.Contains("align-self:center") && groupedRoots.Contains("margin-block:0"), "Different-height grouped controls retain start alignment inside chrome.");
            Check(Rule(layout, scope + " > form").Contains("align-items:center"), "Native form transport loses chrome alignment.");
            var design = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/controls.css");
            Check(Rule(design, ".f-button").Contains("align-self:start") && Regex.IsMatch(design, @"\.f-inline-actions\s*\{[^}]*margin-top:\s*16px"), "The regression no longer exercises the real document defaults overridden by stronger chrome selectors.");
            foreach (var rule in new[] { slot, roots, groupedRoots })
                Check(!Regex.IsMatch(rule, @"background|color:|border|height:|!important"), "Chrome alignment reskins, fixes the height of, or force-overrides an injected control.");
            Check(!ReadSource("src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/primitives/behavior.css").Contains(".shell-header-actions > .f-inline-actions"), "The old group-only alignment patch survived.");
            var shell = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Framework/Components/ApplicationShell.razor");
            foreach (var track in new[] { "start", "center", "end" })
                Check(shell.Contains($"class=\"f-titlebar-{track} f-topbar-slot\""), "ApplicationShell injection bypassed the common slot: " + track);
            return Task.CompletedTask;
        }));

        tests.Add(("shell header centers direct controls of every variant beside the account identity", async () =>
        {
            foreach (var variant in Enum.GetValues<ButtonVariant>())
            {
                var html = await Render<Primitives.ShellHeader>(new()
                {
                    ["HomeHref"] = "/", ["HomeAriaLabel"] = "Home", ["LogoPath"] = "/logo.svg", ["BrandName"] = "Project",
                    ["IdentityName"] = "Account", ["IdentityHref"] = "/account/",
                    ["EndActions"] = ButtonFragment(variant, "Sign out", "/logout/", native: true)
                });
                Check(Regex.Matches(html, "class=\"f-topbar-slot\"").Count == 1, "A direct end action did not receive exactly one library alignment slot.");
                Check(Regex.IsMatch(html, @"<div class=""f-topbar-slot"">\s*<a\b[^>]*href=""/logout/"""), "Direct native sign-out escaped the standard slot.");
                Check(html.Contains("f-button-" + variant.ToString().ToLowerInvariant()) && html.Contains("data-enhance-nav=\"false\""), "Alignment changed a control variant or native navigation.");
                Check(html.Contains("shell-header-identity") && html.Contains("href=\"/account/\""), "Account identity was dropped or replaced with a host control.");
            }
        }));

        tests.Add(("shell header service leading grouped and native form actions share the same slot", async () =>
        {
            RenderFragment grouped = builder =>
            {
                builder.OpenComponent<InlineActions>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(actions =>
                {
                    actions.AddContent(0, ButtonFragment(ButtonVariant.Elevated, "Enter", "/login/"));
                    actions.AddContent(1, ButtonFragment(ButtonVariant.Elevated, "Create", "/signup/"));
                }));
                builder.CloseComponent();
            };
            RenderFragment native = builder =>
            {
                builder.OpenElement(0, "form");
                builder.AddAttribute(1, "method", "post");
                builder.AddAttribute(2, "action", "/logout/");
                builder.AddMarkupContent(3, "<input type=\"hidden\" name=\"__RequestVerificationToken\" value=\"fixture\">");
                builder.OpenComponent<Button>(4);
                builder.AddAttribute(5, "Type", "submit");
                builder.AddAttribute(6, "Variant", ButtonVariant.Quiet);
                builder.AddAttribute(7, "Icon", "logout");
                builder.CloseComponent();
                builder.CloseElement();
            };
            var parameters = new Dictionary<string, object?> { ["HomeHref"] = "/", ["HomeAriaLabel"] = "Home", ["LogoPath"] = "/logo.svg", ["BrandName"] = "Project" };
            var empty = await Render<Primitives.ShellHeader>(parameters);
            Check(!empty.Contains("f-topbar-slot") && !empty.Contains("shell-header-actions"), "Omitted fragments leave empty chrome tracks.");
            parameters["Services"] = ButtonFragment(ButtonVariant.Quiet, "Services");
            parameters["LeadingActions"] = grouped;
            parameters["EndActions"] = native;
            var html = await Render<Primitives.ShellHeader>(parameters);
            Check(Regex.Matches(html, "class=\"f-topbar-slot\"").Count == 3, "One of the service, leading or end fragments bypasses common alignment.");
            Check(html.Contains("class=\"f-inline-actions\"") && Regex.Matches(html, "f-button-elevated").Count == 2, "Grouped actions were flattened or reskinned.");
            Check(Regex.Matches(html, "<form\\b").Count == 1 && html.Contains("method=\"post\" action=\"/logout/\"") && html.Contains("name=\"__RequestVerificationToken\" value=\"fixture\""), "Alignment changed the single native POST transport.");
            Check(Regex.IsMatch(html, @"<button\b[^>]*type=""submit""[^>]*class=""[^""]*f-button-quiet"), "Native sign-out lost its real library submit control.");
        }));

        tests.Add(("card retains its original default title and child DOM", async () =>
        {
            var plain = await Render<Card>(new() { ["Title"] = "Title <safe>", ["ChildContent"] = Text("Body <safe>") });
            Check(Regex.IsMatch(plain, "^<article class=\"f-card\">\\s*<h3>Title &lt;safe&gt;</h3>\\s*Body &lt;safe&gt;\\s*</article>$"), "Default Card shape or encoding changed.");
            Check(!plain.Contains("f-card-prominent", StringComparison.Ordinal), "An ordinary business card opted into presentation geometry.");
            var empty = await Render<Card>(new());
            Check(Regex.IsMatch(empty, "^<article class=\"f-card\">\\s*</article>$"), "Empty Card generated imaginary copy or actions.");
        }));

        tests.Add(("prominent card puts standard actions before encoded large paragraph copy without adding a heading", async () =>
        {
            var card = await Render<Card>(new() { ["Prominent"] = true, ["Text"] = "Start <free>", ["Actions"] = ButtonFragment(ButtonVariant.Elevated, "Create", "/signup/", native: true), ["ChildContent"] = Text("Extra <copy>") });
            Check(card.StartsWith("<article class=\"f-card f-card-prominent\">", StringComparison.Ordinal), "Prominent Card lost its original card identity.");
            Check(card.IndexOf("f-card-prominent-actions", StringComparison.Ordinal) < card.IndexOf("f-card-prominent-copy", StringComparison.Ordinal), "Action/copy visual and keyboard order differ.");
            Check(card.Contains("<p class=\"f-card-prominent-text\">Start &lt;free&gt;</p>", StringComparison.Ordinal) && card.Contains("Extra &lt;copy&gt;", StringComparison.Ordinal), "Prominent Card lost or failed to encode consumer copy.");
            Check(card.Contains("f-button-elevated", StringComparison.Ordinal) && card.Contains("href=\"/signup/\"", StringComparison.Ordinal) && card.Contains("data-enhance-nav=\"false\"", StringComparison.Ordinal), "Presentation card cloned or blocked its native action.");
            Check(!Regex.IsMatch(card, @"<(?:h[1-6]|main|form)\b"), "Large body type introduced a page heading or transport boundary.");
        }));

        tests.Add(("stacked prominent card keeps copy before a single real action without changing ordinary cards", async () =>
        {
            var card = await Render<Card>(new() { ["Prominent"] = true, ["Stacked"] = true, ["Text"] = "Support <safe>", ["Actions"] = ButtonFragment(ButtonVariant.Elevated, "Request") });
            Check(card.Contains("f-card f-card-prominent f-card-stacked") && card.IndexOf("f-card-prominent-copy", StringComparison.Ordinal) < card.IndexOf("f-card-prominent-actions", StringComparison.Ordinal), "Stacked prominent card lost copy/action DOM order.");
            Check(Regex.Matches(card, "f-card-prominent-actions").Count == 1 && Regex.Matches(card, "<button\\b").Count == 1 && card.Contains("f-button-elevated"), "Stacking duplicates or replaces the real action.");
            Check(card.Contains("Support &lt;safe&gt;") && !Regex.IsMatch(card, @"<h[1-6]\b"), "Stacked copy lost encoding or paragraph semantics.");
            var empty = await Render<Card>(new() { ["Prominent"] = true, ["Stacked"] = true, ["Text"] = new string('x', 400) });
            Check(!empty.Contains("f-card-prominent-actions") && empty.Contains(new string('x', 400)), "Stacked no-action card clips copy or leaves an empty action slot.");
            var ordinary = await Render<Card>(new() { ["Stacked"] = true, ["Title"] = "Ordinary" });
            Check(ordinary.Contains("<h3>Ordinary</h3>") && !ordinary.Contains("f-card-stacked"), "Stacked changed ordinary card geometry.");
            var layout = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/presentation/layout.css");
            Check(Rule(layout, ".f-card.f-card-prominent.f-card-stacked").Contains("flex-direction:column") && Rule(layout, ".f-card-stacked > .f-card-prominent-actions, .f-card-stacked > .f-card-prominent-copy").Contains("flex:0 1 auto"), "Stacked card still uses the horizontal fixed-basis action column.");
        }));

        tests.Add(("prominent card supports no actions and optional title copy while ordinary new slots remain additive", async () =>
        {
            var card = await Render<Card>(new() { ["Prominent"] = true, ["Title"] = "Title <safe>", ["Text"] = new string('x', 300) });
            Check(!card.Contains("f-card-prominent-actions", StringComparison.Ordinal) && !Regex.IsMatch(card, @"<h[1-6]\b"), "No-action Card retained an empty action column or separate heading.");
            Check(card.Contains("Title &lt;safe&gt;", StringComparison.Ordinal) && card.Contains(new string('x', 300), StringComparison.Ordinal), "Long text was truncated during rendering.");
            var ordinary = await Render<Card>(new() { ["Title"] = "Heading", ["Text"] = "Body", ["Actions"] = ButtonFragment(ButtonVariant.Secondary, "Action") });
            Check(ordinary.Contains("<h3>Heading</h3>", StringComparison.Ordinal) && ordinary.Contains("<p>Body</p>", StringComparison.Ordinal) && ordinary.Contains("f-inline-actions", StringComparison.Ordinal), "Additive ordinary Card slots changed their meaning.");
            Check(!ordinary.Contains("f-card-prominent", StringComparison.Ordinal), "Optional text/actions alone enabled prominent geometry.");
        }));

        tests.Add(("prominent card reuses role paint and H1 body scale with naturally growing responsive geometry", () =>
        {
            var layout = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/presentation/layout.css");
            var design = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/presentation.css");
            var card = Rule(layout, ".f-card.f-card-prominent");
            Check(card.Contains("display:flex", StringComparison.Ordinal) && card.Contains("min-height:260px", StringComparison.Ordinal) && card.Contains("gap:32px", StringComparison.Ordinal), "Card lost prominent action/copy geometry.");
            Check(!Regex.IsMatch(card, @"(?:^|;)\s*(?:height|max-height|overflow)\s*:"), "Prominent Card clips natural text growth.");
            Check(Regex.IsMatch(layout, @"@media\(max-width:760px\)\s*\{\s*\.f-card\.f-card-prominent\s*\{[^}]*flex-direction:column"), "Prominent Card lost small-screen stacking.");
            Check(Rule(layout, ".f-card-prominent > .f-card-prominent-copy").Contains("overflow-wrap:anywhere", StringComparison.Ordinal), "Framework-only long copy cannot wrap.");
            var paint = Rule(design, ".f-card.f-card-prominent");
            Check(!Regex.IsMatch(paint, @"\b(?:background|color|border-color)\s*:"), "Prominent Card created an independent skin.");
            var text = Rule(design, ".f-card-prominent-copy > .f-card-prominent-text");
            Check(text.Contains("font-size:var(--f-type-h1,34px)", StringComparison.Ordinal) && text.Contains("margin:0", StringComparison.Ordinal), "Prominent body copy no longer follows H1 type tokens.");
            return Task.CompletedTask;
        }));

        tests.Add(("document presentation keeps standard page titles aligned and section headings at H2", () =>
        {
            var layout = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/presentation/layout.css");
            var design = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/presentation.css");
            var heading = Rule(layout, ".f-presentation-band-heading");
            Check(heading.Contains("align-items:flex-start", StringComparison.Ordinal) && heading.Contains("text-align:start", StringComparison.Ordinal), "Section headings use a centered host-specific default.");
            Check(Rule(design, ".f-presentation-band-heading :is(h1,h2,h3,h4,h5,h6)").Contains("font-size:var(--f-type-h2,28px)", StringComparison.Ordinal), "Band section titles do not use the standard H2 scale.");
            Check(Rule(layout, ".f-document-surface .f-page-heading").Contains("position:static", StringComparison.Ordinal), "A website page title competes with its sticky top bar.");
            var gutter = Rule(layout, ".f-document-surface .f-page-body.f-page-full > .f-page-heading");
            Check(gutter.Contains("var(--f-content-width,1180px)", StringComparison.Ordinal) && gutter.Contains("24px", StringComparison.Ordinal), "Full-width page titles drift away from the centered band track.");
            Check(Regex.IsMatch(layout, @"@media\(max-width:560px\)[\s\S]*?\.f-document-surface \.f-page-body\.f-page-full > \.f-page-heading\s*\{\s*padding-inline:18px"), "Small-screen title and content gutters differ.");
            return Task.CompletedTask;
        }));

        tests.Add(("document and access root overscroll disables vertical bounce without locking native or business scroll", () =>
        {
            var layout = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/presentation/layout.css");
            var roots = Rule(layout, "html:has(.f-document-surface), body:has(.f-document-surface)");
            Check(roots.Trim() == "overscroll-behavior-y:none;", "Root policy changed horizontal navigation, native scrolling or unrelated documents.");
            var document = Rule(layout, ".content-surface.f-document-surface");
            Check(document.Contains("height:auto", StringComparison.Ordinal) && document.Contains("overflow:visible", StringComparison.Ordinal), "Bounce suppression changed document flow into a clipped business shell.");
            Check(Rule(layout, ".content-surface.f-document-surface > .content-stage").Contains("overflow:visible", StringComparison.Ordinal), "Document now has competing nested scroll ownership.");
            return Task.CompletedTask;
        }));

        tests.Add(("footer underline links use the existing primary foreground without reskinning other variants", () =>
        {
            var design = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/controls.css");
            Check(Rule(design, ".f-presentation-footer .f-button-underline").Trim() == "color:var(--f-primary-ink);", "Footer Underline text does not follow its actual Primary surface.");
            var press = Rule(design, ".f-presentation-footer .f-button-underline:active:not(:disabled):not([aria-disabled=true])");
            Check(press.Contains("color:var(--f-primary-ink)", StringComparison.Ordinal) && press.Contains("background:var(--f-primary-click)", StringComparison.Ordinal), "Footer press loses the Primary contrast role.");
            Check(!design.Contains(".f-presentation-footer .f-button-elevated", StringComparison.Ordinal), "Underline context changed explicit unrelated button variants.");
            return Task.CompletedTask;
        }));

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
                    ["ChildContent"] = Text("Content"), ["Actions"] = ButtonFragment(ButtonVariant.Secondary, "Learn"),
                    ["AdditionalAttributes"] = new Dictionary<string, object> { ["id"] = "features" }
                });
                Check(Regex.IsMatch(html, "^<section[^>]*class=\"f-presentation-band f-presentation-" + tone.ToString().ToLowerInvariant()), "Band tone or full-width root differs.");
                Check(html.Contains("aria-labelledby=\"features-title\"", StringComparison.Ordinal)
                    && html.Contains("<h2 id=\"features-title\">Features &lt;safe&gt;</h2>", StringComparison.Ordinal), "Band title naming or encoding was lost.");
                Check(Regex.IsMatch(html, @"<section\b[^>]*>\s*<div\b[^>]*class=""f-content-container(?:\s[^""]*)?"""), "Full-width background and inner limit were not separate layers: " + html);
                Check(html.Contains("f-button-secondary", StringComparison.Ordinal), "A band reskinned its standard action.");
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

        tests.Add(("presentation banners share an 800px minimum and opt-in dots without losing native attributes", async () =>
        {
            var bandDefault = await Render<PresentationBand>(new());
            var heroDefault = await Render<PresentationHero>(new() { ["Title"] = "Product" });
            var accessDefault = await RenderDocumentBand(new());
            foreach (var html in new[] { bandDefault, heroDefault, accessDefault })
            {
                Check(html.Contains("--f-band-min-height:800px;", StringComparison.Ordinal), "The standard banner minimum is not 800px.");
                Check(!html.Contains("f-presentation-dotted", StringComparison.Ordinal), "A production banner enables decorative dots by default.");
            }
            foreach (var minimum in new[] { 0, 240, 450, 960 })
            {
                var attributes = new Dictionary<string, object>
                {
                    ["id"] = "configured-banner", ["data-enhance-nav"] = "false", ["style"] = "--host-marker:1"
                };
                var band = await Render<PresentationBand>(new()
                {
                    ["MinHeight"] = minimum, ["Dotted"] = true, ["AdditionalAttributes"] = attributes
                });
                var hero = await Render<PresentationHero>(new()
                {
                    ["Title"] = "Product", ["MinHeight"] = minimum, ["Dotted"] = true, ["AdditionalAttributes"] = attributes
                });
                foreach (var html in new[] { band, hero })
                {
                    Check(html.Contains($"--f-band-min-height:{minimum}px;", StringComparison.Ordinal)
                        && html.Contains("f-presentation-dotted", StringComparison.Ordinal), "A banner lost its configured minimum or opt-in dot state.");
                    Check(html.Contains("id=\"configured-banner\"", StringComparison.Ordinal)
                        && html.Contains("data-enhance-nav=\"false\"", StringComparison.Ordinal)
                        && html.Contains("--host-marker:1;", StringComparison.Ordinal), "The shared banner renderer dropped native protocol or style attributes.");
                    Check(Regex.Matches(html, @"\sstyle=""").Count == 1, "Banner style merging generated duplicate native attributes.");
                }
            }
            foreach (var minimum in new[] { -1, int.MinValue })
            {
                await Reject<PresentationBand>(new() { ["MinHeight"] = minimum }, "MinHeight");
                await Reject<PresentationHero>(new() { ["Title"] = "Product", ["MinHeight"] = minimum }, "MinHeight");
            }
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

        tests.Add(("full-height banners opt in without changing ordinary band or hero defaults", async () =>
        {
            var ordinaryBand = await Render<PresentationBand>(new() { ["Title"] = "Ordinary band" });
            var ordinaryHero = await Render<PresentationHero>(new() { ["Title"] = "Ordinary hero" });
            Check(!ordinaryBand.Contains("f-presentation-fullheight", StringComparison.Ordinal)
                && !ordinaryHero.Contains("f-presentation-fullheight", StringComparison.Ordinal), "Ordinary presentation content became a full-height access scene by default.");
            foreach (var dotted in new[] { false, true })
            {
                var band = await Render<PresentationBand>(new() { ["Title"] = "Full band", ["FullHeight"] = true, ["Dotted"] = dotted, ["MinHeight"] = 620 });
                var hero = await Render<PresentationHero>(new() { ["Title"] = "Full hero", ["FullHeight"] = true, ["Dotted"] = dotted, ["MinHeight"] = 620 });
                foreach (var html in new[] { band, hero })
                {
                    Check(html.Contains("f-presentation-fullheight", StringComparison.Ordinal)
                        && html.Contains("--f-band-min-height:620px;", StringComparison.Ordinal), "Full height discarded the explicit minimum-height contract.");
                    Check(html.Contains("f-presentation-dotted", StringComparison.Ordinal) == dotted, "Full height changed the independent dot setting.");
                    Check(Regex.Matches(html, @"<section\b").Count == 1
                        && Regex.Matches(html, @"class=""f-content-container(?:\s[^""]*)?""").Count == 1,
                        "Full height introduced an independent banner or duplicate center-width container.");
                }
            }
        }));

        tests.Add(("access surface owns one main and delegates full-height dots and palette roles to the standard band", async () =>
        {
            foreach (var tone in Enum.GetValues<PresentationTone>())
            {
                var automatic = await RenderDocumentBand(new() { ["Tone"] = tone, ["ChildContent"] = NativeAccessForm() });
                Check(!automatic.Contains("f-presentation-dotted", StringComparison.Ordinal), "An access surface enables dots without an explicit request.");
                foreach (var dotted in new[] { false, true })
                {
                    var html = await RenderDocumentBand(new()
                    {
                        ["Dotted"] = dotted, ["Tone"] = tone, ["ChildContent"] = NativeAccessForm()
                    });
                    Check(Regex.Matches(html, @"<main\b").Count == 1 && Regex.Matches(html, @"<section\b").Count == 1
                        && html.Contains("f-document-surface", StringComparison.Ordinal), "Access surface nested a second main or duplicated the standard band.");
                    Check(html.Contains("f-presentation-fullheight", StringComparison.Ordinal)
                        && html.Contains("f-presentation-" + tone.ToString().ToLowerInvariant(), StringComparison.Ordinal)
                        && html.Contains("f-presentation-dotted", StringComparison.Ordinal) == dotted, "Access surface did not forward full height, role tone or explicit dots.");
                    Check(Regex.Matches(html, @"class=""f-content-container(?:\s[^""]*)?""").Count == 1,
                        "Access surface created a second content-width layer.");
                    Check(Regex.Matches(html, @"<form\b").Count == 1
                        && html.Contains("method=\"post\" action=\"/auth/login\" data-enhance=\"false\"", StringComparison.Ordinal)
                        && html.Contains("name=\"__RequestVerificationToken\" value=\"fixture-token\"", StringComparison.Ordinal), "Access scene rewrote or nested native transport or anti-forgery content.");
                }
            }
            await Reject<PresentationBand>(new() { ["Tone"] = (PresentationTone)int.MaxValue }, "Tone");
            Check(typeof(PresentationBand).Assembly.GetType("ArkheideSystem.Flourish.Blazor.Components.Primitives.AccessSurface") is null,
                "The generic document composition retained an access-only wrapper.");
        }));

        tests.Add(("logo displayer encodes configured identity and preserves image title context and description order", async () =>
        {
            var configured = await Render<LogoDisplayer>(new()
            {
                ["TitleId"] = "product-logo-title", ["ContextName"] = "Context <safe>", ["Description"] = "Description <safe>",
                ["Class"] = "scene-brand", ["AdditionalAttributes"] = new Dictionary<string, object> { ["id"] = "product-brand", ["aria-labelledby"] = "product-logo-title" }
            }, "Product <safe> & name", "configured-logo.svg", "Logo <safe> & alternative");
            Check(configured.StartsWith("<header", StringComparison.Ordinal) && configured.Contains("class=\"f-logo-displayer scene-brand\"", StringComparison.Ordinal)
                && configured.Contains("id=\"product-brand\"", StringComparison.Ordinal)
                && configured.Contains("aria-labelledby=\"product-logo-title\"", StringComparison.Ordinal), "Brand display discarded standard classes or native identity attributes.");
            Check(configured.Contains("src=\"configured-logo.svg\"", StringComparison.Ordinal)
                && configured.Contains("alt=\"Logo &lt;safe&gt; &amp; alternative\"", StringComparison.Ordinal)
                && configured.Contains("width=\"96\" height=\"96\"", StringComparison.Ordinal), "Brand display did not use the configured logo and encoded alternative text.");
            Check(Regex.IsMatch(configured, @"<h1\b[^>]*id=""product-logo-title""[^>]*>\s*<span>Product &lt;safe&gt; &amp; name</span>\s*<span class=""f-logo-displayer-context"">Context &lt;safe&gt;</span>\s*</h1>"),
                "Project title and context lost encoding, heading semantics or their stacked order.");
            Check(configured.IndexOf("<img", StringComparison.Ordinal) < configured.IndexOf("<h1", StringComparison.Ordinal)
                && configured.IndexOf("</h1>", StringComparison.Ordinal) < configured.IndexOf("f-logo-displayer-description", StringComparison.Ordinal)
                && configured.Contains("Description &lt;safe&gt;", StringComparison.Ordinal), "Logo, title and description are not in the intended document order.");
            for (var level = 1; level <= 6; level++)
            {
                var html = await Render<LogoDisplayer>(new() { ["HeadingLevel"] = level, ["TitleId"] = "semantic-brand", ["ProjectName"] = "Explicit <brand>", ["LogoPath"] = "" });
                Check(Regex.IsMatch(html, $"<h{level}\\b[^>]*id=\"semantic-brand\"[^>]*><span>Explicit &lt;brand&gt;</span></h{level}>"), "Brand heading level or explicit name was lost.");
                Check(!html.Contains("<img", StringComparison.Ordinal) && !html.Contains("f-logo-displayer-logo", StringComparison.Ordinal)
                    && !html.Contains("f-logo-displayer-context", StringComparison.Ordinal), "An explicitly omitted logo or context leaves an empty placeholder.");
            }
            foreach (var level in new[] { 0, 7 }) await Reject<LogoDisplayer>(new() { ["HeadingLevel"] = level }, "HeadingLevel");
            var defaultLogo = await Render<LogoDisplayer>(new() { ["TitleId"] = "literal-brand" }, "App.Project");
            Check(defaultLogo.Contains("<span>App.Project</span>", StringComparison.Ordinal)
                && defaultLogo.Contains("_content/Arkheide.Flourish.Blazor.Framework/browse.svg", StringComparison.Ordinal), "Unspecified logo did not use the framework logo or a literal name was guessed as a token.");
            var unconfigured = await Render<LogoDisplayer>(new() { ["TitleId"] = "default-brand" });
            Check(unconfigured.Contains("<span>Application</span>", StringComparison.Ordinal), "The unconfigured brand display lost the framework's literal project-name default.");
            var whitespaceLogo = await Render<LogoDisplayer>(new() { ["ProjectName"] = "No image", ["LogoPath"] = " " });
            Check(!whitespaceLogo.Contains("<img", StringComparison.Ordinal) && !whitespaceLogo.Contains("f-logo-displayer-logo", StringComparison.Ordinal),
                "A whitespace logo override leaves an image or blank logo placeholder.");
            var explicitLogo = await Render<LogoDisplayer>(new()
            {
                ["ProjectName"] = "Explicit brand", ["LogoPath"] = "explicit-logo.svg", ["LogoAlternativeText"] = "Explicit <alternative>"
            }, "Configured brand", "configured-logo.svg", "Configured alternative");
            Check(explicitLogo.Contains("<span>Explicit brand</span>", StringComparison.Ordinal)
                && explicitLogo.Contains("src=\"explicit-logo.svg\"", StringComparison.Ordinal)
                && explicitLogo.Contains("alt=\"Explicit &lt;alternative&gt;\"", StringComparison.Ordinal)
                && !explicitLogo.Contains("Configured", StringComparison.Ordinal), "Explicit brand/logo/alternative text overrides were overwritten.");
        }));

        tests.Add(("full-height access and artistic branding CSS retain natural growth inherited fonts and separate action spacing", () =>
        {
            var layout = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/presentation/layout.css");
            var design = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/presentation.css");
            var full = Rule(layout, ".f-presentation-band.f-presentation-fullheight");
            Check(Regex.IsMatch(full, @"min-height\s*:\s*max\(\s*100dvh\s*,\s*var\(--f-band-min-height,\s*800px\)\s*\)"),
                "Full-height mode does not combine the viewport and configured minimum without a fixed height.");
            foreach (var declarations in new[] { full, Rule(layout, ".content-surface.f-document-surface"), Rule(layout, ".f-logo-displayer"), Rule(layout, ".f-logo-displayer-title") })
                Check(!Regex.IsMatch(declarations, @"(?:^|;)\s*(?:height|max-height)\s*:\s*(?!auto\b)|(?:^|;)\s*overflow(?:-[xy])?\s*:\s*(?:hidden|clip)\b"), "Access banner or brand copy is fixed-height or clipped.");
            var title = Rule(design, ".f-logo-displayer > .f-logo-displayer-title");
            Check(Regex.IsMatch(title, @"font-size\s*:\s*clamp\(\s*56px\s*,\s*8vw\s*,\s*80px\s*\)"), "Brand title stopped using its artistic 56–80px scale.");
            Check(Regex.IsMatch(Rule(layout, ".f-logo-displayer"), @"(?:^|;)\s*display\s*:\s*grid\s*(?:;|$)")
                && Regex.IsMatch(Rule(layout, ".f-logo-displayer-title"), @"(?:^|;)\s*display\s*:\s*grid\s*(?:;|$)"),
                "Logo, project title and context no longer use the intended vertically stacked composition.");
            var surface = Rule(layout, ".f-access-form-surface");
            var actions = Rule(layout, ".f-access-form-actions");
            Check(Regex.IsMatch(surface, @"(?:^|;)\s*gap\s*:\s*16px\s*(?:;|$)")
                && Regex.IsMatch(actions, @"(?:^|;)\s*margin-top\s*:\s*16px\s*(?:;|$)"), "Access action separation no longer combines 16px grid gap and 16px additional spacing.");
            foreach (var declarations in new[]
            {
                Rule(layout, ".f-logo-displayer"), Rule(layout, ".f-logo-displayer-title"), Rule(design, ".f-logo-displayer"), title,
                Rule(design, ".f-logo-displayer-context"), Rule(design, ".f-logo-displayer > .f-logo-displayer-description"), surface, actions
            })
                Check(!Regex.IsMatch(declarations, @"(?:^|;)\s*font-family\s*:"), "Access composition installs a new font skin instead of inheriting the standard root font.");
            return Task.CompletedTask;
        }));

        tests.Add(("presentation hero uses one shared banner and container for every tone and keeps long copy readable in SSR", async () =>
        {
            var longCopy = string.Join(" ", Enumerable.Repeat("Long product copy <safe> & complete.", 80));
            foreach (var tone in Enum.GetValues<PresentationTone>())
            {
                var html = await Render<PresentationHero>(new()
                {
                    ["Title"] = "Product", ["TitleId"] = "hero-title", ["Subtitle"] = "Artistic subtitle",
                    ["Description"] = longCopy, ["Tone"] = tone, ["MinHeight"] = 450,
                    ["ChildContent"] = Text("Additional product content"),
                    ["Actions"] = ButtonFragment(ButtonVariant.Elevated, "Pricing", "/pricing/", native: true),
                    ["AdditionalAttributes"] = new Dictionary<string, object>
                    {
                        ["id"] = "hero", ["aria-labelledby"] = "obsolete-title", ["data-scene"] = "product"
                    }
                });
                Check(Regex.Matches(html, @"<section\b").Count == 1
                    && Regex.Matches(html, @"class=""f-content-container(?:\s[^""]*)?""").Count == 1,
                    "Hero nested an independent banner or center-width renderer.");
                Check(html.Contains("f-presentation-" + tone.ToString().ToLowerInvariant(), StringComparison.Ordinal)
                    && html.Contains("f-presentation-hero-content", StringComparison.Ordinal), "Hero lost the shared palette role or its content composition.");
                Check(Regex.Matches(html, @"\saria-labelledby=""").Count == 1
                    && html.Contains("aria-labelledby=\"hero-title\"", StringComparison.Ordinal)
                    && !html.Contains("obsolete-title", StringComparison.Ordinal), "Hero's accessible title does not name its actual h1.");
                Check(html.Contains("id=\"hero\"", StringComparison.Ordinal) && html.Contains("data-scene=\"product\"", StringComparison.Ordinal), "Hero did not pass native attributes to its single banner.");
                Check(System.Net.WebUtility.HtmlDecode(html).Contains(longCopy, StringComparison.Ordinal)
                    && html.Contains("Additional product content", StringComparison.Ordinal)
                    && !html.Contains("<safe>", StringComparison.Ordinal), "Hero shortened, hid or failed to encode long copy.");
                Check(html.Contains("f-button-elevated", StringComparison.Ordinal)
                    && html.Contains("href=\"/pricing/\"", StringComparison.Ordinal)
                    && html.Contains("data-enhance-nav=\"false\"", StringComparison.Ordinal), "Hero changed the native standard CTA.");
            }
            await Reject<PresentationHero>(new() { ["Title"] = "Product", ["Tone"] = (PresentationTone)int.MaxValue }, "Tone");
        }));

        tests.Add(("presentation CSS uses minimum height and grid gaps rather than clipping content or collapsing paragraph margins", () =>
        {
            var layout = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/presentation/layout.css");
            var design = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/presentation.css");
            var band = Rule(layout, ".f-presentation-band");
            var content = Rule(layout, ".f-presentation-hero-content");
            Check(Regex.IsMatch(band, @"(?:^|;)\s*min-height\s*:\s*var\(--f-band-min-height,\s*800px\)"), "Banner CSS does not consume the shared minimum-height contract.");
            Check(Regex.IsMatch(content, @"(?:^|;)\s*display\s*:\s*grid\s*(?:;|$)")
                && Regex.IsMatch(content, @"(?:^|;)\s*gap\s*:\s*40px\s*(?:;|$)"), "Artistic copy spacing relies on paragraph margins that the foundation reset can override.");
            foreach (var declarations in new[]
            {
                band, content, Rule(layout, ".f-content-container"), Rule(design, ".f-presentation-band"),
                Rule(design, ".f-presentation-hero > .f-content-container"), Rule(design, ".f-presentation-hero-content > h1"),
                Rule(design, ".f-presentation-hero-content > .f-presentation-hero-subtitle"),
                Rule(design, ".f-presentation-hero-content > .f-presentation-hero-description")
            })
                Check(!Regex.IsMatch(declarations, @"(?:^|;)\s*(?:height|max-height)\s*:|(?:^|;)\s*overflow(?:-[xy])?\s*:\s*(?:hidden|clip)\b"),
                    "Presentation content gained a fixed-height or clipping rule instead of natural growth.");
            foreach (var (selector, foreground, background) in new[]
            {
                (".f-presentation-canvas", "--f-text", "--f-canvas"),
                (".f-presentation-surface", "--f-text", "--f-surface"),
                (".f-presentation-primary", "--f-primary-ink", "--f-primary")
            })
            {
                var role = Rule(design, selector);
                Check(role.Contains($"color:var({foreground})", StringComparison.Ordinal)
                    && role.Contains($"background-color:var({background})", StringComparison.Ordinal), "A banner tone stopped using the active palette's foreground/background pair.");
                Check(!Regex.IsMatch(role, @"(?:^|;)\s*background\s*:"),
                    "A banner tone resets the opt-in dot background image with a background shorthand.");
            }
            var boardDesign = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/display-board.css");
            var sharedDots = Regex.Match(boardDesign, @"\.f-display-board-dotted\s*,\s*\.f-presentation-dotted\s*\{([^}]*)\}");
            Check(sharedDots.Success && sharedDots.Groups[1].Value.Contains("background-image:radial-gradient", StringComparison.Ordinal)
                && sharedDots.Groups[1].Value.Contains("background-size:18px 18px", StringComparison.Ordinal),
                "Production banners lost the shared opt-in dot decoration independent of preview behavior.");
            var primaryDots = Rule(design, ".f-presentation-primary.f-presentation-dotted");
            Check(Regex.IsMatch(primaryDots, @"background-image\s*:\s*radial-gradient\(\s*circle,\s*var\(--f-primary-preview\)")
                && !primaryDots.Contains("rgb(from", StringComparison.Ordinal),
                "Primary banner dots derive a new color instead of consuming the configured primary preview role.");
            Check(!Regex.IsMatch(design, @"^\s*\.f-presentation-hero\s+(?:\.f-content-container|h1|\.f-presentation-hero-subtitle|\.f-presentation-hero-description)\s*\{", RegexOptions.Multiline)
                && !Regex.IsMatch(design, @"^\s*\.f-presentation-hero\.f-presentation-primary\s+:is\(", RegexOptions.Multiline),
                "Hero styles leak into nested ChildContent containers, headings or another presentation tone.");
            Check(Rule(design, ".f-presentation-hero.f-presentation-primary > .f-content-container > .f-presentation-hero-content > :is(.f-presentation-hero-subtitle,.f-presentation-hero-description)")
                .Contains("color:inherit", StringComparison.Ordinal), "Primary hero copy does not inherit its paired contrast color.");
            Check(Rule(design, ".f-presentation-hero.f-presentation-primary > .f-content-container > .f-presentation-hero-layout > .f-presentation-hero-content > :is(.f-presentation-hero-subtitle,.f-presentation-hero-description)")
                .Contains("color:inherit", StringComparison.Ordinal), "Split Primary hero copy does not inherit its paired contrast color.");
            return Task.CompletedTask;
        }));

        tests.Add(("presentation footer separates decorative wordmark identity and native navigation", async () =>
        {
            var html = await Render<PresentationFooter>(new()
            {
                ["Copyright"] = "Copyright 2026", ["LinksLabel"] = "Footer links",
                ["Actions"] = ButtonFragment(ButtonVariant.Underline, "Pricing", "/pricing/", native: true)
            }, "Product");
            Check(Regex.IsMatch(html, "^<footer class=\"f-presentation-footer\""), "Footer lost native semantics.");
            Check(html.Contains("<span class=\"f-presentation-watermark\" aria-hidden=\"true\">Product</span>", StringComparison.Ordinal), "Decorative project wordmark is not hidden from accessibility text.");
            Check(html.Contains("f-content-container f-presentation-footer-content", StringComparison.Ordinal)
                && html.Contains("<strong>Product</strong>", StringComparison.Ordinal)
                && html.Contains("aria-label=\"Footer links\"", StringComparison.Ordinal), "Footer content lost limit, identity or link naming.");
            Check(html.Contains("f-button-underline", StringComparison.Ordinal) && html.Contains("data-enhance-nav=\"false\"", StringComparison.Ordinal), "Footer did not retain the actual standard native link.");
            var fallback = await Render<PresentationFooter>(new());
            Check(fallback.Contains("<strong>Application</strong>", StringComparison.Ordinal)
                && fallback.Contains("aria-hidden=\"true\">Application</span>", StringComparison.Ordinal), "An unconfigured footer did not use the shared default project identity.");
        }));

        tests.Add(("presentation footer derives encoded identity from project configuration and keeps copyright independent", async () =>
        {
            const string project = "Configured <project> & name";
            var automatic = await Render<PresentationFooter>(new() { ["Copyright"] = "© 2026 ARKHEIDE SYSTEM <owner>" }, project);
            Check(automatic.Contains("<strong>Configured &lt;project&gt; &amp; name</strong>", StringComparison.Ordinal)
                && automatic.Contains("aria-hidden=\"true\">Configured &lt;project&gt; &amp; name</span>", StringComparison.Ordinal),
                "Footer identity and decorative wordmark do not share the configured, encoded project name.");
            Check(System.Net.WebUtility.HtmlDecode(automatic).Contains("© 2026 ARKHEIDE SYSTEM <owner>", StringComparison.Ordinal)
                && !automatic.Contains("<owner>", StringComparison.Ordinal), "Host copyright was replaced or inserted as unsafe markup.");
            var noCopyright = await Render<PresentationFooter>(new(), project);
            Check(!noCopyright.Contains("ARKHEIDE", StringComparison.Ordinal) && !noCopyright.Contains("©", StringComparison.Ordinal)
                && !noCopyright.Contains("<span></span>", StringComparison.Ordinal), "A generic footer invented an organization, copyright or empty copyright element.");
            Check(typeof(PresentationFooter).GetProperty("BrandName") is null
                && typeof(PresentationFooter).GetProperty("Watermark") is null,
                "Footer exposes alternate identity parameters instead of the configured project name.");
        }));

        tests.Add(("offer stage SSR leaves every offer and action readable before progressive enhancement", async () =>
        {
            var html = await Render<OfferStage>(new()
            {
                ["Id"] = "offers", ["Label"] = "Five offers", ["LinkScopeId"] = "prices", ["ChildContent"] = Cards(5)
            });
            Check(Regex.Matches(html, @"<article\b[^>]*class=""f-offer-card""").Count == 5, "SSR dropped or duplicated an offer.");
            Check(Regex.Matches(html, "class=\"f-offer-details\"").Count == 5 && Regex.Matches(html, "f-button-primary").Count == 5, "SSR concealed offer details or controls.");
            Check(!Regex.IsMatch(html, @"data-offer-ready|data-active|\shidden(?:=|\s|>)|class=""f-offer-details""[^>]*aria-hidden=""true""|aria-hidden=""true""[^>]*class=""f-offer-details"""), "Enhancement-only inactive state escaped into SSR: " + html);
            Check(html.Contains("role=\"list\"", StringComparison.Ordinal) && Regex.Matches(html, "role=\"listitem\"").Count == 5,
                "Offer collection lost list semantics.");
            Check(Regex.Matches(html, "class=\"f-offer-compact-title\" aria-hidden=\"true\"").Count == 5,
                "Decorative duplicate titles are not hidden from screen readers.");
            Check(Regex.Matches(html, "<p class=\"f-offer-title\"").Count == 5 && !Regex.IsMatch(html, @"<h[1-6]\b"),
                "Offer labels generated extra page headings or disappeared.");
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

        tests.Add(("offer icon-only rotation keeps standard sizing accessible labels and unavailable SSR without visible label clones", async () =>
        {
            var html = await Render<OfferStage>(new()
            {
                ["RotationControlIconOnly"] = true, ["PauseRotationLabel"] = "Pause <offers>", ["ChildContent"] = Cards(2)
            });
            var control = Regex.Match(html, @"<button\b[^>]*class=""[^""]*\bf-button-quiet\b[^""]*\bf-button-icon\b[^""]*""[^>]*>[\s\S]*?</button>");
            Check(control.Success, "Icon rotation bypassed the standard Quiet icon-only Button: " + html);
            Check(control.Value.Contains("type=\"button\"", StringComparison.Ordinal)
                && control.Value.Contains("aria-pressed=\"false\"", StringComparison.Ordinal)
                && Regex.IsMatch(control.Value, @"\sdisabled(?:=|\s|>)"), "SSR icon rotation became submitting or available before enhancement.");
            Check(control.Value.Contains("aria-label=\"Pause &lt;offers&gt;\"", StringComparison.Ordinal)
                && control.Value.Contains("data-tooltip=\"Pause &lt;offers&gt;\"", StringComparison.Ordinal)
                && control.Value.Contains("data-icon=\"pause\"", StringComparison.Ordinal)
                && control.Value.Contains("aria-hidden=\"true\"", StringComparison.Ordinal), "The pause icon lost its encoded accessible name, tooltip or decorative icon semantics.");
            Check(!control.Value.Contains("f-button-text", StringComparison.Ordinal)
                && !Regex.IsMatch(control.Value, @">\s*Pause &lt;offers&gt;"), "Icon-only rotation still renders a second visible text label.");
            Check(Regex.Matches(html, @"<article\b[^>]*class=""f-offer-card""").Count == 2
                && !html.Contains("data-offer-ready", StringComparison.Ordinal), "Icon mode concealed SSR offer content or pretended enhancement completed.");
            var manual = await Render<OfferStage>(new() { ["AutoRotate"] = false, ["RotationControlIconOnly"] = true, ["ChildContent"] = Cards(2) });
            Check(!manual.Contains("f-offer-controls", StringComparison.Ordinal), "Icon-only mode re-enabled a disabled automatic-rotation feature.");
        }));

        tests.Add(("offer icon-only rotation suppresses SSR activation and updates pause resume icons and scoped labels", async () =>
        {
            var registrations = new ServiceCollection();
            registrations.AddLogging();
            registrations.AddFlourishFramework();
            registrations.AddScoped<ArkheideSystem.Flourish.Blazor.Abstract.ITextProvider, TrackingTextProvider>();
            registrations.AddScoped<OfferStageActivator>();
            registrations.AddScoped<IComponentActivator>(provider => provider.GetRequiredService<OfferStageActivator>());
            registrations.AddSingleton<NavigationManager>(new TestNavigation("/pricing/"));
            registrations.AddSingleton<IJSRuntime, NoInterop>();
            using var services = registrations.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
            using var scope = services.CreateScope();
            var texts = (TrackingTextProvider)scope.ServiceProvider.GetRequiredService<ArkheideSystem.Flourish.Blazor.Abstract.ITextProvider>();
            var activator = scope.ServiceProvider.GetRequiredService<OfferStageActivator>();
            await using var renderer = new HtmlRenderer(scope.ServiceProvider, services.GetRequiredService<ILoggerFactory>());
            var output = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<OfferStage>(ParameterView.FromDictionary(
                new Dictionary<string, object?> { ["RotationControlIconOnly"] = true })));
            var stage = activator.Components.OfType<OfferStage>().Single();
            var control = activator.Components.OfType<Button>().Single();
            var activate = typeof(Button).GetMethod("ClickAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            var rerender = typeof(ComponentBase).GetMethod("StateHasChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            await renderer.Dispatcher.InvokeAsync(async () =>
            {
                RequireOfferIconState(output.ToHtmlString(), "en-US:Flourish/Offer_PauseRotation", "pause", false);
                await (Task)activate.Invoke(control, [new MouseEventArgs()])!;
                RequireOfferIconState(output.ToHtmlString(), "en-US:Flourish/Offer_PauseRotation", "pause", false);
                typeof(OfferStage).GetField("initialized", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(stage, true);
                rerender.Invoke(stage, null);
                Check(!Regex.IsMatch(output.ToHtmlString(), @"<button\b[^>]*\sdisabled(?:=|\s|>)"), "An initialized icon rotation control remains disabled.");
                await (Task)activate.Invoke(control, [new MouseEventArgs()])!;
                RequireOfferIconState(output.ToHtmlString(), "en-US:Flourish/Offer_ResumeRotation", "play_arrow", true);
            });
            await Task.Run(() => texts.Select("pt-BR"));
            await renderer.Dispatcher.InvokeAsync(async () =>
            {
                RequireOfferIconState(output.ToHtmlString(), "pt-BR:Flourish/Offer_ResumeRotation", "play_arrow", true);
                await (Task)activate.Invoke(control, [new MouseEventArgs()])!;
                RequireOfferIconState(output.ToHtmlString(), "pt-BR:Flourish/Offer_PauseRotation", "pause", false);
                await stage.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?>
                {
                    ["RotationControlIconOnly"] = true, ["PauseRotationLabel"] = "Pause rotation", ["ResumeRotationLabel"] = "Resume rotation"
                }));
                RequireOfferIconState(output.ToHtmlString(), "Pause rotation", "pause", false);
                await (Task)activate.Invoke(control, [new MouseEventArgs()])!;
                RequireOfferIconState(output.ToHtmlString(), "Resume rotation", "play_arrow", true);
            });
            await Task.Run(() => texts.Select("zh-CN"));
            await renderer.Dispatcher.InvokeAsync(() => RequireOfferIconState(output.ToHtmlString(), "Resume rotation", "play_arrow", true));
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

        tests.Add(("access form actions remain inside the host native form without inventing a second transport boundary", async () =>
        {
            var html = await RenderDocumentBand(new() { ["ChildContent"] = NativeAccessForm() });
            Check(Regex.Matches(html, @"<form\b").Count == 1
                && Regex.IsMatch(html, @"<form\b[^>]*>[\s\S]*<div class=""f-access-form-actions"">[\s\S]*</div>[\s\S]*</form>"),
                "The Actions slot escaped the host's single native form.");
            Check(Regex.IsMatch(html, @"<button\b[^>]*type=""submit""[^>]*>[\s\S]*Enter[\s\S]*</button>")
                && html.Contains("f-button-elevated", StringComparison.Ordinal)
                && html.Contains("href=\"/forgot-password/\"", StringComparison.Ordinal)
                && html.Contains("data-enhance-nav=\"false\"", StringComparison.Ordinal), "The action slot changed standard submit or native recovery-link behavior.");
            Check(html.Contains("name=\"__RequestVerificationToken\" value=\"fixture-token\"", StringComparison.Ordinal)
                && html.Contains("name=\"returnUrl\" value=\"/account/\"", StringComparison.Ordinal)
                && html.Contains("name=\"email\"", StringComparison.Ordinal), "Action composition discarded protocol fields or anti-forgery content.");
            var noForm = await Render<AccessFormSurface>(new() { ["Actions"] = ButtonFragment(ButtonVariant.Elevated, "Action") });
            Check(noForm.Contains("f-access-form-actions", StringComparison.Ordinal) && !Regex.IsMatch(noForm, @"<form\b"),
                "The Actions slot created an implicit form rather than leaving transport host-owned.");
            var noActions = await Render<AccessFormSurface>(new() { ["ChildContent"] = Text("Fields") });
            Check(!noActions.Contains("f-access-form-actions", StringComparison.Ordinal), "Omitted actions leave an empty action layout or spacing placeholder.");
        }));

        tests.Add(("access actions preserve standard button variants and native links without grid tiles", async () =>
        {
            RenderFragment actions = builder =>
            {
                builder.AddContent(0, ButtonFragment(ButtonVariant.Elevated, "Login", "/login/", native: true));
                builder.AddContent(1, ButtonFragment(ButtonVariant.Secondary, "Cancel"));
            };
            var html = await Render<AccessActions>(new() { ["Label"] = "Entry actions", ["ChildContent"] = actions });
            Check(html.StartsWith("<nav class=\"f-access-actions\" aria-label=\"Entry actions\"", StringComparison.Ordinal), "Access actions lost their named group.");
            Check(html.Contains("f-button-elevated", StringComparison.Ordinal) && html.Contains("f-button-secondary", StringComparison.Ordinal), "An action wrapper changed the consumer's chosen variants.");
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
    private static RenderFragment NativeAccessForm() => builder =>
    {
        builder.OpenElement(0, "form");
        builder.AddAttribute(1, "method", "post");
        builder.AddAttribute(2, "action", "/auth/login");
        builder.AddAttribute(3, "data-enhance", "false");
        builder.OpenComponent<AccessFormSurface>(4);
        builder.AddAttribute(5, "ChildContent", Markup("<input type=\"hidden\" name=\"__RequestVerificationToken\" value=\"fixture-token\"><input type=\"hidden\" name=\"returnUrl\" value=\"/account/\"><input type=\"email\" name=\"email\" required>"));
        builder.AddAttribute(6, "Actions", (RenderFragment)(actions =>
        {
            actions.OpenComponent<Button>(0);
            actions.AddAttribute(1, "Type", "submit");
            actions.AddAttribute(2, "Variant", ButtonVariant.Elevated);
            actions.AddAttribute(3, "Text", "Enter");
            actions.CloseComponent();
            actions.AddContent(4, ButtonFragment(ButtonVariant.Underline, "Recovery", "/forgot-password/", native: true));
        }));
        builder.CloseComponent();
        builder.CloseElement();
    };
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
            builder.AddAttribute(4, "ChildContent", ButtonFragment(ButtonVariant.Primary, $"Action {index}"));
            builder.CloseComponent();
        }
    };
    private static void RequireOfferIconState(string html, string label, string icon, bool paused)
    {
        var encoded = System.Net.WebUtility.HtmlEncode(label);
        Check(html.Contains($"aria-label=\"{encoded}\"", StringComparison.Ordinal)
            && html.Contains($"data-tooltip=\"{encoded}\"", StringComparison.Ordinal)
            && html.Contains($"data-icon=\"{icon}\"", StringComparison.Ordinal)
            && html.Contains($"aria-pressed=\"{paused.ToString().ToLowerInvariant()}\"", StringComparison.Ordinal),
            "Offer icon, accessible name, tooltip and pressed state disagree: " + html);
        Check(html.Contains("f-button-icon", StringComparison.Ordinal) && !html.Contains("f-button-text", StringComparison.Ordinal), "An updated icon control reverted to a text button.");
    }
    private sealed class OfferStageActivator : IComponentActivator
    {
        internal List<IComponent> Components { get; } = [];
        public IComponent CreateInstance(Type type)
        {
            var component = (IComponent)Activator.CreateInstance(type)!;
            Components.Add(component);
            return component;
        }
    }
    private static Task<string> RenderDocumentBand(Dictionary<string, object?> parameters) =>
        Render<ContentSurface>(new()
        {
            ["DocumentFlow"] = true,
            ["ChildContent"] = (RenderFragment)(builder =>
            {
                builder.OpenComponent<PresentationBand>(0);
                builder.AddAttribute(1, nameof(PresentationBand.FullHeight), true);
                builder.AddMultipleAttributes(2, parameters);
                builder.CloseComponent();
            })
        });
    private static string ReadSource(string relativePath)
    {
        foreach (var start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
            for (DirectoryInfo? directory = new(start); directory is not null; directory = directory.Parent)
            {
                var path = Path.Combine(directory.FullName, relativePath.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(path)) return File.ReadAllText(path);
            }
        throw new FileNotFoundException("Presentation regression check requires its repository source.", relativePath);
    }
    private static string Rule(string css, string selector)
    {
        var match = Regex.Match(css, @"^\s*" + Regex.Escape(selector) + @"\s*\{([^}]*)\}", RegexOptions.Multiline);
        Check(match.Success, "Missing presentation CSS role: " + selector);
        return match.Groups[1].Value;
    }
    private static async Task<string> Render<TComponent>(Dictionary<string, object?> parameters, string? projectName = null,
        string? logoPath = null, string? logoAlternativeText = null) where TComponent : IComponent
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFlourishFramework(framework =>
        {
            if (projectName is not null || logoPath is not null || logoAlternativeText is not null)
                framework.ConfigureProject(project =>
                {
                    if (projectName is not null) project.SetProjectName(projectName);
                    if (logoPath is not null || logoAlternativeText is not null) project.SetLogo(logoPath, logoAlternativeText);
                });
        });
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
