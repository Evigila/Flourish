using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class TextChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("framework works without a culture or design provider and never guesses tokens from strings", async () =>
        {
            using var provider = Services(framework => framework
                .ConfigureProject(project => project.SetProjectName(new TextReference("App", "Project", "Project fallback")))
                .ConfigureTopBar(top => top.AddMenu("Key.Menu", menu => menu.AddMenuItem("Key.Item", "test.command")))
                .ConfigureNavigation(nav => nav.AddNav("Key.Home", "home", "/module")), translated: false);
            using var scope = provider.CreateScope();
            var text = scope.ServiceProvider.GetRequiredService<ITextProvider>();
            Require(text.Get(new("App", "Missing", "Fallback")) == "Fallback", "Literal fallback failed.");
            Require(text.Get(new("App", "Missing")) == "Missing", "Missing literal token was not preserved.");
            var html = await Render<ApplicationShell>(scope);
            Require(html.Contains("Project fallback") && html.Contains("Key.Menu") && html.Contains("Key.Home"), "Plain strings were guessed as tokens.");
            Require(html.Contains("Skip to content"), "Framework accessibility fallback failed without Culture.");
        }));

        tests.Add(("scoped text refresh updates all shell labels without changing selection routes or commands", async () =>
        {
            using var provider = Services(framework => framework
                .ConfigureProject(project => project.SetProjectName(Ref("Project")))
                .ConfigureTopBar(top => top.SetSearch(Ref("Search")).AddMenu(Ref("Menu"), menu => menu.AddMenuItem(Ref("Item"), "test.command")))
                .ConfigureNavigation(nav => nav
                    .AddNav(Ref("Module"), "home", "/module", secondary => secondary
                        .AddSubNav(Ref("Group"), "folder", "/module/group", third => third.AddSubNav(Ref("Child"), "home", "/module/group/child")))
                    .AddNavButton(Ref("Command"), "play_arrow", "test.command")
                    .AddFixedNav(Ref("Fixed"), "home", "/fixed")
                    .AddFixedNavButton(Ref("FixedCommand"), "play_arrow", "test.command")));
            using var first = provider.CreateScope();
            using var second = provider.CreateScope();
            await using var a = Renderer(first);
            await using var b = Renderer(second);
            var outputA = await a.Dispatcher.InvokeAsync(() => a.RenderComponentAsync<ApplicationShell>());
            var outputB = await b.Dispatcher.InvokeAsync(() => b.RenderComponentAsync<ApplicationShell>());
            var original = await a.Dispatcher.InvokeAsync(outputA.ToHtmlString);
            var panels = Regex.Matches(original, "<ul id=\"([^\"]+)\" class=\"f-third-navigation\"").Select(match => match.Groups[1].Value).ToArray();
            var currentLinks = Regex.Matches(original, "href=\"([^\"]+)\"[^>]*aria-current=\"[^\"]+\"").Select(match => match.Value).ToArray();
            var textsA = (TrackingTextProvider)first.ServiceProvider.GetRequiredService<ITextProvider>();
            var textsB = (TrackingTextProvider)second.ServiceProvider.GetRequiredService<ITextProvider>();
            await Task.Run(() => textsA.Select("zh-CN"));
            var changed = await a.Dispatcher.InvokeAsync(outputA.ToHtmlString);
            foreach (var token in new[] { "Project", "Search", "Menu", "Item", "Module", "Group", "Child", "Command", "Fixed", "FixedCommand" })
                Require(WebUtility.HtmlDecode(changed).Contains("zh-CN:App/" + token), "Missing live shell label: " + token);
            Require(changed.Contains("lang=\"zh-CN\"") && changed.Contains("zh-CN:Flourish/Shell_Skip"), "Shell language/accessibility text did not refresh.");
            Require(panels.SequenceEqual(Regex.Matches(changed, "<ul id=\"([^\"]+)\" class=\"f-third-navigation\"").Select(match => match.Groups[1].Value)), "Language switch rebuilt navigation identities.");
            Require(currentLinks.SequenceEqual(Regex.Matches(changed, "href=\"([^\"]+)\"[^>]*aria-current=\"[^\"]+\"").Select(match => match.Value)), "Language switch changed selected routes.");
            Require(textsB.Culture == "en-US" && (await b.Dispatcher.InvokeAsync(outputB.ToHtmlString)).Contains("en-US:App/Module"), "One user's text change leaked to another scope.");
            var shellSubscribers = textsA.Subscribers;
            Require(shellSubscribers >= 1, "Shell and nested controls did not subscribe.");
            await Task.Run(() => textsA.Select("en-US"));
            Require(textsA.Subscribers == shellSubscribers, "Text refresh added duplicate subscriptions.");
            await a.DisposeAsync();
            Require(textsA.Subscribers == 0, "Disposed shell retained its text subscription.");
            textsA.Select("en-US");
        }));

        tests.Add(("equal menu item records keep separate explicit catalog identities", async () =>
        {
            using var provider = Services(framework => framework.ConfigureTopBar(top => top.AddMenu("Menu", menu => menu
                .AddMenuItem(new TextReference("First", "Same", "Same"), "same.command")
                .AddMenuItem(new TextReference("Second", "Same", "Same"), "same.command"))));
            using var scope = provider.CreateScope();
            var html = await Render<ApplicationShell>(scope);
            Require(html.Contains("en-US:First/Same") && html.Contains("en-US:Second/Same"), "Reference metadata collapsed equal records from separate catalogs.");
        }));

        tests.Add(("presentation footer resolves the configured project fallback without a culture provider", async () =>
        {
            using var provider = Services(framework => framework.ConfigureProject(project =>
                project.SetProjectName(new TextReference("App", "Project", "Fallback <product> & identity"))), translated: false);
            using var scope = provider.CreateScope();
            var html = await Render<PresentationFooter>(scope, new() { [nameof(PresentationFooter.Copyright)] = "Host copyright" });
            Require(html.Contains("<strong>Fallback &lt;product&gt; &amp; identity</strong>", StringComparison.Ordinal)
                && html.Contains("aria-hidden=\"true\">Fallback &lt;product&gt; &amp; identity</span>", StringComparison.Ordinal),
                "Footer title and watermark did not use the encoded project fallback without Culture.");
            Require(html.Contains("Host copyright", StringComparison.Ordinal) && !html.Contains("App/Project", StringComparison.Ordinal),
                "Footer guessed a translation token or replaced independently supplied copyright.");
        }));

        tests.Add(("presentation footer live project names are scope isolated unsubscribe and preserve explicit overrides", async () =>
        {
            using var provider = Services(framework => framework.ConfigureProject(project => project.SetProjectName(Ref("Project"))));
            using var first = provider.CreateScope();
            using var second = provider.CreateScope();
            await using var a = Renderer(first);
            await using var b = Renderer(second);
            var automaticParameters = new Dictionary<string, object?>
            {
                [nameof(PresentationFooter.Copyright)] = "Host copyright",
                [nameof(PresentationFooter.AdditionalAttributes)] = new Dictionary<string, object> { ["id"] = "project-footer" }
            };
            var outputA = await a.Dispatcher.InvokeAsync(() => a.RenderComponentAsync<PresentationFooter>(ParameterView.FromDictionary(automaticParameters)));
            var outputB = await b.Dispatcher.InvokeAsync(() => b.RenderComponentAsync<PresentationFooter>(ParameterView.FromDictionary(automaticParameters)));
            var explicitOutput = await a.Dispatcher.InvokeAsync(() => a.RenderComponentAsync<PresentationFooter>(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(PresentationFooter.BrandName)] = "Explicit <brand>",
                [nameof(PresentationFooter.Watermark)] = "Explicit <watermark>",
                [nameof(PresentationFooter.Copyright)] = "Explicit host copyright"
            })));
            var explicitBefore = await a.Dispatcher.InvokeAsync(explicitOutput.ToHtmlString);
            Require(explicitBefore.Contains("<strong>Explicit &lt;brand&gt;</strong>", StringComparison.Ordinal)
                && explicitBefore.Contains("aria-hidden=\"true\">Explicit &lt;watermark&gt;</span>", StringComparison.Ordinal),
                "Explicit footer identities were replaced or inserted without encoding.");
            var textsA = (TrackingTextProvider)first.ServiceProvider.GetRequiredService<ITextProvider>();
            var textsB = (TrackingTextProvider)second.ServiceProvider.GetRequiredService<ITextProvider>();
            Require(textsA.Subscribers == 2 && textsB.Subscribers == 1, "Each footer must subscribe once to its own scoped text provider.");
            var before = await a.Dispatcher.InvokeAsync(outputA.ToHtmlString);
            Require(before.Contains("<strong>en-US:App/Project</strong>", StringComparison.Ordinal)
                && before.Contains("aria-hidden=\"true\">en-US:App/Project</span>", StringComparison.Ordinal),
                "Footer title and watermark do not resolve the same configured project reference.");
            await Task.Run(() => textsA.Select("zh-CN"));
            var changed = await a.Dispatcher.InvokeAsync(outputA.ToHtmlString);
            Require(changed.Contains("<strong>zh-CN:App/Project</strong>", StringComparison.Ordinal)
                && changed.Contains("aria-hidden=\"true\">zh-CN:App/Project</span>", StringComparison.Ordinal),
                "Footer title and watermark did not update together on a scoped language change.");
            Require(changed.Contains("id=\"project-footer\"", StringComparison.Ordinal)
                && changed.Contains("Host copyright", StringComparison.Ordinal), "Language refresh changed native footer identity or manual copyright.");
            Require(await a.Dispatcher.InvokeAsync(explicitOutput.ToHtmlString) == explicitBefore,
                "Language refresh overwrote explicit compatibility brand, watermark or copyright.");
            Require(textsB.Culture == "en-US" && (await b.Dispatcher.InvokeAsync(outputB.ToHtmlString)).Contains("<strong>en-US:App/Project</strong>", StringComparison.Ordinal),
                "A footer language change leaked to another user scope.");
            await Task.Run(() => textsA.Select("pt-BR"));
            Require((await a.Dispatcher.InvokeAsync(outputA.ToHtmlString)).Contains("<strong>pt-BR:App/Project</strong>", StringComparison.Ordinal)
                && textsA.Subscribers == 2 && textsB.Subscribers == 1, "Repeated footer refresh accumulated subscriptions or failed to update.");
            await a.DisposeAsync();
            Require(textsA.Subscribers == 0 && textsB.Subscribers == 1, "Disposed footers retained subscriptions or detached another user's footer.");
            textsA.Select("en-US");
            await b.DisposeAsync();
            Require(textsB.Subscribers == 0, "The second disposed footer retained its text subscription.");
        }));

        tests.Add(("presentation footer literal project names stay literal through text refresh", async () =>
        {
            using var provider = Services(framework => framework.ConfigureProject(project => project.SetProjectName("App.Project")));
            using var scope = provider.CreateScope();
            var texts = (TrackingTextProvider)scope.ServiceProvider.GetRequiredService<ITextProvider>();
            await using var renderer = Renderer(scope);
            var output = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<PresentationFooter>());
            await Task.Run(() => texts.Select("zh-CN"));
            var html = await renderer.Dispatcher.InvokeAsync(output.ToHtmlString);
            Require(html.Contains("<strong>App.Project</strong>", StringComparison.Ordinal)
                && html.Contains("aria-hidden=\"true\">App.Project</span>", StringComparison.Ordinal)
                && !html.Contains("zh-CN:", StringComparison.Ordinal), "A literal project name was guessed as a culture token.");
            await renderer.DisposeAsync();
            Require(texts.Subscribers == 0, "The literal-project footer retained its subscription after disposal.");
        }));

        tests.Add(("logo displayer resolves project fallback and shared configured logo without a culture provider", async () =>
        {
            using var provider = Services(framework => framework.ConfigureProject(project => project
                .SetProjectName(new TextReference("App", "Project", "Fallback <product> & name"))
                .SetLogo("shared-logo.svg", "Shared <logo>")), translated: false);
            using var scope = provider.CreateScope();
            var html = await Render<LogoDisplayer>(scope, new() { [nameof(LogoDisplayer.TitleId)] = "fallback-logo-title" });
            Require(html.Contains("<span>Fallback &lt;product&gt; &amp; name</span>", StringComparison.Ordinal)
                && html.Contains("src=\"shared-logo.svg\"", StringComparison.Ordinal)
                && html.Contains("alt=\"Shared &lt;logo&gt;\"", StringComparison.Ordinal), "Brand display did not resolve the encoded fallback and shared project logo.");
            Require(html.Contains("id=\"fallback-logo-title\"", StringComparison.Ordinal)
                && !html.Contains("App/Project", StringComparison.Ordinal), "Brand display changed title identity or guessed a literal fallback as a token.");
        }));

        tests.Add(("logo displayer project localization is scope isolated preserves explicit identity and unsubscribes", async () =>
        {
            using var provider = Services(framework => framework.ConfigureProject(project => project
                .SetProjectName(Ref("Project")).SetLogo("project-logo.svg", "Project logo")));
            using var first = provider.CreateScope();
            using var second = provider.CreateScope();
            await using var a = Renderer(first);
            await using var b = Renderer(second);
            var automaticParameters = new Dictionary<string, object?>
            {
                [nameof(LogoDisplayer.TitleId)] = "shared-logo-title", [nameof(LogoDisplayer.ContextName)] = "Login",
                [nameof(LogoDisplayer.Description)] = "Host description"
            };
            var outputA = await a.Dispatcher.InvokeAsync(() => a.RenderComponentAsync<LogoDisplayer>(ParameterView.FromDictionary(automaticParameters)));
            var outputB = await b.Dispatcher.InvokeAsync(() => b.RenderComponentAsync<LogoDisplayer>(ParameterView.FromDictionary(automaticParameters)));
            var explicitOutput = await a.Dispatcher.InvokeAsync(() => a.RenderComponentAsync<LogoDisplayer>(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(LogoDisplayer.TitleId)] = "explicit-logo-title", [nameof(LogoDisplayer.ProjectName)] = "Explicit <brand>",
                [nameof(LogoDisplayer.LogoPath)] = "explicit-logo.svg", [nameof(LogoDisplayer.LogoAlternativeText)] = "Explicit <alternative>",
                [nameof(LogoDisplayer.ContextName)] = "Host context", [nameof(LogoDisplayer.Description)] = "Explicit description"
            })));
            var explicitBefore = await a.Dispatcher.InvokeAsync(explicitOutput.ToHtmlString);
            Require(explicitBefore.Contains("<span>Explicit &lt;brand&gt;</span>", StringComparison.Ordinal)
                && explicitBefore.Contains("src=\"explicit-logo.svg\"", StringComparison.Ordinal)
                && explicitBefore.Contains("alt=\"Explicit &lt;alternative&gt;\"", StringComparison.Ordinal), "Brand/logo/alternative text overrides did not win or were not encoded.");
            var textsA = (TrackingTextProvider)first.ServiceProvider.GetRequiredService<ITextProvider>();
            var textsB = (TrackingTextProvider)second.ServiceProvider.GetRequiredService<ITextProvider>();
            Require(textsA.Subscribers == 2 && textsB.Subscribers == 1, "Each brand display must subscribe once to its own scoped text provider.");
            Require((await a.Dispatcher.InvokeAsync(outputA.ToHtmlString)).Contains("<span>en-US:App/Project</span>", StringComparison.Ordinal), "Initial configured project reference was not resolved.");
            await Task.Run(() => textsA.Select("zh-CN"));
            var changed = await a.Dispatcher.InvokeAsync(outputA.ToHtmlString);
            Require(changed.Contains("<span>zh-CN:App/Project</span>", StringComparison.Ordinal)
                && changed.Contains("id=\"shared-logo-title\"", StringComparison.Ordinal)
                && changed.Contains("src=\"project-logo.svg\"", StringComparison.Ordinal)
                && changed.Contains("alt=\"Project logo\"", StringComparison.Ordinal)
                && changed.Contains("Login", StringComparison.Ordinal)
                && changed.Contains("Host description", StringComparison.Ordinal), "Culture refresh changed native title/logo identity or explicit context/description rather than only the project reference.");
            Require((await a.Dispatcher.InvokeAsync(explicitOutput.ToHtmlString)) == explicitBefore, "Culture refresh overwrote explicitly supplied brand-display identity.");
            Require(textsB.Culture == "en-US"
                && (await b.Dispatcher.InvokeAsync(outputB.ToHtmlString)).Contains("<span>en-US:App/Project</span>", StringComparison.Ordinal), "Brand-display culture leaked into another user scope.");
            await Task.Run(() => textsA.Select("pt-BR"));
            Require((await a.Dispatcher.InvokeAsync(outputA.ToHtmlString)).Contains("<span>pt-BR:App/Project</span>", StringComparison.Ordinal)
                && textsA.Subscribers == 2 && textsB.Subscribers == 1, "Repeated brand-display refresh accumulated subscriptions or failed to resolve text.");
            await a.DisposeAsync();
            Require(textsA.Subscribers == 0 && textsB.Subscribers == 1, "Disposed brand displays retained subscriptions or detached another user's display.");
            textsA.Select("en-US");
            await b.DisposeAsync();
            Require(textsB.Subscribers == 0, "The second brand display retained its subscription after disposal.");
        }));

        tests.Add(("logo displayer literal project identity stays literal through culture changes", async () =>
        {
            using var provider = Services(framework => framework.ConfigureProject(project => project.SetProjectName("App.Project")));
            using var scope = provider.CreateScope();
            var texts = (TrackingTextProvider)scope.ServiceProvider.GetRequiredService<ITextProvider>();
            await using var renderer = Renderer(scope);
            var output = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<LogoDisplayer>());
            await Task.Run(() => texts.Select("zh-CN"));
            var html = await renderer.Dispatcher.InvokeAsync(output.ToHtmlString);
            Require(html.Contains("<span>App.Project</span>", StringComparison.Ordinal) && !html.Contains("zh-CN:", StringComparison.Ordinal), "A literal logo-display project name was guessed as a culture token.");
            await renderer.DisposeAsync();
            Require(texts.Subscribers == 0, "The literal-project brand display retained its subscription after disposal.");
        }));

        tests.Add(("explicit shell accessibility text wins even when it equals the original default", async () =>
        {
            using var provider = Services(framework => framework.ConfigureNavigation(nav => nav.AddNav("Home", "home", "/")));
            using var scope = provider.CreateScope();
            var html = await Render<ApplicationShell>(scope, new() { [nameof(ApplicationShell.SkipLabel)] = "Skip to content" });
            Require(html.Contains(">Skip to content</a>") && !html.Contains("Flourish/Shell_Skip"), "An explicit old default string was incorrectly replaced.");
            Require(html.Contains("Flourish/Shell_MainNavigation"), "Omitted defaults did not use the text provider.");
        }));

        tests.Add(("table defaults and format culture refresh while explicit overrides remain untouched", async () =>
        {
            using var provider = Services(_ => { });
            using var scope = provider.CreateScope();
            var text = (TrackingTextProvider)scope.ServiceProvider.GetRequiredService<ITextProvider>();
            var parameters = new Dictionary<string, object?>
            {
                [nameof(DataTable<Record>.Items)] = new[] { new Record(1, "Name", "mail@example.test", 1234.5m, null) },
                [nameof(DataTable<Record>.Columns)] = new[] { new TableColumn<Record>("amount", "Amount", row => row.Amount) }
            };
            await using var renderer = Renderer(scope);
            var output = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<DataTable<Record>>(ParameterView.FromDictionary(parameters)));
            Require((await renderer.Dispatcher.InvokeAsync(output.ToHtmlString)).Contains("1234.5"), "Initial table format was unexpected.");
            await Task.Run(() => text.Select("zh-CN", "pt-BR"));
            var changed = WebUtility.HtmlDecode(await renderer.Dispatcher.InvokeAsync(output.ToHtmlString));
            Require(changed.Contains("1234,5") && changed.Contains("zh-CN:Flourish/Table_Display"), "Table default text or formatted values did not refresh.");
            parameters[nameof(DataTable<Record>.Culture)] = CultureInfo.GetCultureInfo("en-US");
            parameters[nameof(DataTable<Record>.Text)] = new TableText(Display: "Host display");
            parameters[nameof(DataTable<Record>.Label)] = "Records";
            var explicitHtml = WebUtility.HtmlDecode(await Render<DataTable<Record>>(scope, parameters));
            Require(explicitHtml.Contains("1234.5") && explicitHtml.Contains("Host display") && explicitHtml.Contains("aria-label=\"Records\""), "Explicit host Culture/Text/Label was overwritten.");
        }));

        tests.Add(("dialog and display board text updates unsubscribe correctly and preserve explicit literals", async () =>
        {
            using var provider = Services(_ => { });
            using var scope = provider.CreateScope();
            var text = (TrackingTextProvider)scope.ServiceProvider.GetRequiredService<ITextProvider>();
            await using var renderer = Renderer(scope);
            var dialog = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<Dialog>(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(Dialog.Title)] = "Title" })));
            var board = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<DisplayBoard>(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(DisplayBoard.CopyText)] = "<script>sample</script>" })));
            var controlSubscribers = text.Subscribers;
            Require(controlSubscribers >= 2, "Controls and nested buttons did not subscribe to their local provider.");
            await Task.Run(() => text.Select("zh-CN"));
            Require((await renderer.Dispatcher.InvokeAsync(dialog.ToHtmlString)).Contains("zh-CN:Flourish/Dialog_Close"), "Dialog close text did not refresh.");
            Require((await renderer.Dispatcher.InvokeAsync(board.ToHtmlString)).Contains("zh-CN:Flourish/Board_Copy"), "Copy text did not refresh.");
            Require(text.Subscribers == controlSubscribers, "Nested controls added duplicate subscriptions on text refresh.");
            await renderer.DisposeAsync();
            Require(text.Subscribers == 0, "Async-disposed controls retained text subscriptions.");
            var explicitDialog = await Render<Dialog>(scope, new() { [nameof(Dialog.Title)] = "Title", [nameof(Dialog.CloseLabel)] = "Close" });
            Require(explicitDialog.Contains("aria-label=\"Close\"") && !explicitDialog.Contains("Flourish/Dialog_Close"), "Explicit Close was treated as omitted.");
        }));

        tests.Add(("text-reference builder overloads keep startup configuration immutable and validate input", () =>
        {
            IProjectBuilder? project = null;
            ITopBarMenuBuilder? menu = null;
            ISubNavigationBuilder? secondary = null;
            using var provider = Services(framework => framework
                .ConfigureProject(builder => project = builder)
                .ConfigureTopBar(top => top.AddMenu(Ref("Menu"), builder => { menu = builder; builder.AddMenuItem(Ref("Item"), "command"); }))
                .ConfigureNavigation(nav => nav.AddNav(Ref("Root"), "home", "/", builder => { secondary = builder; builder.AddSubNav(Ref("Child"), "home", "/child"); })));
            Throws<InvalidOperationException>(() => project!.SetProjectName(Ref("Late")));
            Throws<InvalidOperationException>(() => menu!.AddMenuItem(Ref("Late"), "command"));
            Throws<InvalidOperationException>(() => secondary!.AddSubNav(Ref("Late"), "home", "/late"));
            Throws<ArgumentException>(() => new TextReference(" ", "Key"));
            Throws<ArgumentException>(() => new TextReference("App", " "));
            return Task.CompletedTask;
        }));
    }

    private static TextReference Ref(string key) => new("App", key, "Literal " + key);
    private static ServiceProvider Services(Action<IFrameworkBuilder> configure, bool translated = true)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<NavigationManager>(new TestNavigation("/module/group/child"));
        services.AddSingleton<IJSRuntime, FakeJs>();
        services.AddFlourishFramework(configure);
        if (translated) services.AddScoped<ITextProvider, TrackingTextProvider>();
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }
    private static HtmlRenderer Renderer(IServiceScope scope) => new(scope.ServiceProvider, scope.ServiceProvider.GetRequiredService<ILoggerFactory>());
    private static async Task<string> Render<T>(IServiceScope scope, Dictionary<string, object?>? parameters = null) where T : IComponent
    {
        await using var renderer = Renderer(scope);
        return await renderer.Dispatcher.InvokeAsync(async () => (await renderer.RenderComponentAsync<T>(ParameterView.FromDictionary(parameters ?? new()))).ToHtmlString());
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException("Expected " + typeof(T).Name); }
}

internal sealed class TrackingTextProvider : ITextProvider
{
    private EventHandler? changed;
    public string Culture { get; private set; } = "en-US";
    public CultureInfo FormatCulture { get; private set; } = CultureInfo.GetCultureInfo("en-US");
    public int Subscribers => changed?.GetInvocationList().Length ?? 0;
    public event EventHandler? Changed { add => changed += value; remove => changed -= value; }
    public void Select(string culture, string? format = null)
    {
        Culture = culture;
        FormatCulture = CultureInfo.GetCultureInfo(format ?? culture);
        changed?.Invoke(this, EventArgs.Empty);
    }
    public string Get(TextReference text, params object?[] arguments)
    {
        var value = text.Token == "Table_ItemRangeFormat" ? "{0} {1}-{2} / {3} {4}" : $"{Culture}:{text.CatalogId}/{text.Token}";
        return arguments.Length == 0 ? value : string.Format(FormatCulture, value, arguments);
    }
}
