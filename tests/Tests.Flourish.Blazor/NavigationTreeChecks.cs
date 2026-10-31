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

internal static class NavigationTreeChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("dynamic tree follows absolute organization paths and preserves original href query fragment and slash", async () =>
        {
            const string branch = "/organizations/alpha/orders/";
            const string leaf = "/organizations/alpha/orders/create/?origin=list#form";
            await using var fixture = new Fixture(branch + "create/?query=current#part", "https://example.test/organizations/alpha/");
            await fixture.Run<NavigationTree>(new() { ["Items"] = new[] { new NavigationItem("Orders", branch, Children: [new("Create", leaf, Exact: true)]) } }, (_, html) =>
            {
                var output = html();
                Require(Tags(output, "a").Single(tag => Attribute(tag, "aria-current") == "page").Contains("href=\"" + leaf, StringComparison.Ordinal), "The absolute-root route did not select or preserve its canonical link.");
                Require(Attribute(Tags(output, "a").Single(tag => Attribute(tag, "href") == branch), "aria-current") == "true", "The selected child's parent is not identified as an ancestor.");
                Require(Expanded(output), "A directly loaded child did not reveal its ancestor.");
                Require(Tags(output, "ul").Count == 2 && !output.Contains("role=\"tree", StringComparison.Ordinal), "Navigation must retain native nested list semantics.");
                return Task.CompletedTask;
            });
        }));

        tests.Add(("dynamic tree triangles change expansion without navigating or notifying link activation", async () =>
        {
            var notified = 0;
            await using var fixture = new Fixture("/orders/create/");
            await fixture.Run<NavigationTree>(new() { ["Items"] = Items(), ["Navigated"] = EventCallback.Factory.Create(new object(), () => notified++) }, async (_, html) =>
            {
                var button = fixture.Branch("/orders/");
                var uri = fixture.Navigation.Uri;
                await button.OnSecondaryClick.InvokeAsync(new());
                Require(!Expanded(html()) && notified == 0 && fixture.Navigation.Uri == uri, "A triangle navigated, closed its host, or failed to collapse.");
                await button.OnSecondaryClick.InvokeAsync(new());
                Require(Expanded(html()) && notified == 0, "A triangle failed to reopen independently.");
                await button.OnClick.InvokeAsync(new());
                Require(notified == 1 && fixture.Navigation.Uri == uri, "The parent did not notify native link intent exactly once.");
            });
        }));

        tests.Add(("dynamic tree fresh equivalent records preserve manual collapse panel identity and updated labels", async () =>
        {
            var parameters = new Dictionary<string, object?> { ["Items"] = Items(), ["CurrentPath"] = "/orders/create/" };
            await using var fixture = new Fixture();
            await fixture.Run<NavigationTree>(parameters, async (tree, html) =>
            {
                var panel = Attribute(Tags(html(), "ul").Last(), "id");
                var oldToggle = fixture.Branch("/orders/").OnSecondaryClick;
                await oldToggle.InvokeAsync(new());
                Require(!Expanded(html()), "The current branch did not collapse.");
                parameters["Items"] = Items("New caption");
                parameters["ItemLabel"] = (Func<NavigationItem, string>)(item => "Localized " + item.Label);
                parameters["ExpandLabel"] = "Open";
                parameters["CollapseLabel"] = "Close";
                await tree.SetParametersAsync(ParameterView.FromDictionary(parameters));
                Require(!Expanded(html()) && Attribute(Tags(html(), "ul").Last(), "id") == panel, "A fresh equivalent record collection reset state or panel identity.");
                Require(html().Contains("Open Localized New caption", StringComparison.Ordinal), "The accessible disclosure did not refresh its host-owned label.");
                await oldToggle.InvokeAsync(new());
                Require(!Expanded(html()), "A callback from a replaced record changed the current node.");
                await fixture.Branch("/orders/").OnSecondaryClick.InvokeAsync(new());
                Require(Expanded(html()), "The current replacement record could not expand.");
            });
        }));

        tests.Add(("dynamic tree route changes reveal the active ancestor while query changes retain manual collapse", async () =>
        {
            await using var fixture = new Fixture("/orders/create/");
            await fixture.Run<NavigationTree>(new() { ["Items"] = Items() }, async (_, html) =>
            {
                await fixture.Branch("/orders/").OnSecondaryClick.InvokeAsync(new());
                fixture.Navigation.NavigateTo("/orders/list/");
                Require(Expanded(html()) && Attribute(Tags(html(), "a").Single(tag => Attribute(tag, "aria-current") == "page"), "href") == "/orders/list/", "Path navigation did not refresh selection and reveal its branch.");
                await fixture.Branch("/orders/").OnSecondaryClick.InvokeAsync(new());
                fixture.Navigation.NavigateTo("/orders/list/?filter=all#results");
                Require(!Expanded(html()), "An ignored query/fragment change overrode manual collapse.");
            });
        }));

        tests.Add(("dynamic tree rejects callbacks after removal and readdition of the same item object", async () =>
        {
            var items = Items();
            var parameters = new Dictionary<string, object?> { ["Items"] = items, ["CurrentPath"] = "/elsewhere/" };
            await using var fixture = new Fixture();
            await fixture.Run<NavigationTree>(parameters, async (tree, html) =>
            {
                var old = fixture.Branch("/orders/").OnSecondaryClick;
                parameters["Items"] = Array.Empty<NavigationItem>();
                await tree.SetParametersAsync(ParameterView.FromDictionary(parameters));
                await old.InvokeAsync(new());
                Require(Tags(html(), "ul").Count == 0, "A removed node reappeared from its stale callback.");
                parameters["Items"] = items;
                await tree.SetParametersAsync(ParameterView.FromDictionary(parameters));
                Require(!Expanded(html()), "Readding an unrelated node unexpectedly expanded it.");
                await old.InvokeAsync(new());
                Require(!Expanded(html()), "An old generation could reopen a readded object.");
                await fixture.Branch("/orders/").OnSecondaryClick.InvokeAsync(new());
                Require(Expanded(html()), "The readded current generation could not expand.");
            });
        }));

        tests.Add(("dynamic tree inherited disabled state removes all descendant targets and rejects an older enabled toggle", async () =>
        {
            var parameters = new Dictionary<string, object?> { ["Items"] = Items(), ["CurrentPath"] = "/orders/create/" };
            await using var fixture = new Fixture();
            await fixture.Run<NavigationTree>(parameters, async (tree, html) =>
            {
                var old = fixture.Branch("/orders/").OnSecondaryClick;
                parameters["Items"] = new[] { Items()[0] with { Disabled = true } };
                await tree.SetParametersAsync(ParameterView.FromDictionary(parameters));
                await old.InvokeAsync(new());
                Require(!Expanded(html()), "A disabled branch accepted an old enabled callback.");
                var links = Tags(html(), "a");
                Require(links.All(tag => Attribute(tag, "aria-disabled") == "true" && Attribute(tag, "href") is null && Attribute(tag, "tabindex") == "-1"), "A disabled ancestor left a live descendant target.");
                Require(!html().Contains("aria-current=", StringComparison.Ordinal), "A disabled descendant displaced route selection.");
            });
        }));

        tests.Add(("dynamic tree chooses destination specificity and segment boundaries instead of nesting depth", async () =>
        {
            var items = new NavigationItem[]
            {
                new("Specific parent", "/orders/special/", Children: [new("Short child", "/orders/")]),
                new("False prefix", "/orders/specialist/", Exact: true),
                new("Disabled detail", "/orders/special/detail/", Disabled: true)
            };
            await using var fixture = new Fixture();
            await fixture.Run<NavigationTree>(new() { ["Items"] = items, ["CurrentPath"] = "/orders/special/detail/?query=long#fragment" }, (_, html) =>
            {
                Require(Attribute(Tags(html(), "a").Single(tag => Attribute(tag, "aria-current") == "page"), "href") == "/orders/special/", "A shorter child, disabled item or partial segment displaced a more specific enabled destination.");
                return Task.CompletedTask;
            });
        }));

        tests.Add(("dynamic tree instance state and disclosure IDs remain independent", async () =>
        {
            await using var left = new Fixture("/orders/create/");
            await using var right = new Fixture("/orders/create/");
            string? leftPanel = null;
            await left.Run<NavigationTree>(new() { ["Items"] = Items() }, async (_, html) =>
            {
                leftPanel = Attribute(Tags(html(), "ul").Last(), "id");
                await left.Branch("/orders/").OnSecondaryClick.InvokeAsync(new());
                Require(!Expanded(html()), "The first instance did not collapse.");
                await right.Run<NavigationTree>(new() { ["Items"] = Items() }, (_, otherHtml) =>
                {
                    Require(Expanded(otherHtml()) && Attribute(Tags(otherHtml(), "ul").Last(), "id") != leftPanel, "Another instance reused expansion state or disclosure IDs.");
                    return Task.CompletedTask;
                });
                Require(!Expanded(html()), "Another renderer reset the first tree's manual state.");
            });
        }));

        tests.Add(("dynamic tree validates local unique acyclic navigation inputs", async () =>
        {
            var cyclicChildren = new List<NavigationItem>();
            var cyclic = new NavigationItem("Cycle", "/cycle/", Children: cyclicChildren);
            cyclicChildren.Add(cyclic);
            object?[] invalid = [null, new NavigationItem[] { null! }, new[] { new NavigationItem("", "/empty/") },
                new[] { new NavigationItem("A", "/orders/"), new("B", "/ORDERS") }, new[] { new NavigationItem("External", "https://elsewhere.test/") }, new[] { cyclic }];
            foreach (var items in invalid)
            {
                var rejected = false;
                await using var fixture = new Fixture();
                try { await fixture.Run<NavigationTree>(new() { ["Items"] = items }, (_, _) => Task.CompletedTask); }
                catch (ArgumentException) { rejected = true; }
                Require(rejected, "Invalid or cyclic navigation inputs were accepted.");
            }
        }));

        tests.Add(("ApplicationShell retains base-relative selection through the shared tree renderer", async () =>
        {
            await using var fixture = new Fixture("/apps/orders/create/?from=base#form", "https://example.test/apps/", framework => framework.ConfigureNavigation(navigation => navigation
                .AddNav("Orders", "list", "/orders", secondary => secondary.AddSubNav("Commercial", "shopping_cart", "/orders", children => children.AddSubNav("Create", "add", "/orders/create", exact: true)))));
            await fixture.Run<ApplicationShell>(new(), (_, html) =>
            {
                Require(Attribute(Tags(html(), "a").Single(tag => Attribute(tag, "aria-current") == "page"), "href") == "/orders/create", "Shell configuration routes lost their original base-relative matching contract.");
                Require(fixture.Capture.Instances.OfType<NavigationTree>().Single().CurrentPath == "/orders/create", "Shell did not supply its base-relative path to the shared renderer.");
                Require(Expanded(html()), "The shared renderer did not reveal the Shell's selected ancestor.");
                return Task.CompletedTask;
            });
        }));
    }

    private static IReadOnlyList<NavigationItem> Items(string label = "Orders") =>
        [new(label, "/orders/", "shopping_cart", Children: [new("List", "/orders/list/", Exact: true), new("Create", "/orders/create/", Exact: true)])];
    private static IReadOnlyList<string> Tags(string html, string tag) => Regex.Matches(html, "<" + tag + @"\b[^>]*>").Select(match => match.Value).ToArray();
    private static string? Attribute(string tag, string name) { var match = Regex.Match(tag, $"(?:^|\\s){Regex.Escape(name)}=\"([^\"]*)\""); return match.Success ? WebUtility.HtmlDecode(match.Groups[1].Value) : null; }
    private static bool Expanded(string html) => Tags(html, "button").Any(tag => Attribute(tag, "aria-expanded") == "true");
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly ServiceProvider Provider;
        private readonly HtmlRenderer Renderer;
        internal CaptureActivator Capture { get; } = new();
        internal TestNavigation Navigation { get; }
        internal Fixture(string path = "/", string baseUri = "https://example.test/", Action<IFrameworkBuilder>? configure = null)
        {
            Navigation = new(path, baseUri);
            var services = new ServiceCollection(); services.AddLogging();
            services.AddFlourishFramework(configure ?? (_ => { }));
            services.AddSingleton<NavigationManager>(Navigation); services.AddSingleton<IJSRuntime, NoInterop>(); services.AddSingleton<IComponentActivator>(Capture);
            Provider = services.BuildServiceProvider(); Renderer = new(Provider, Provider.GetRequiredService<ILoggerFactory>());
        }
        internal SplitButton Branch(string href) => Capture.Instances.OfType<SplitButton>().Last(button => button.Href == href);
        internal Task Run<T>(Dictionary<string, object?> parameters, Func<T, Func<string>, Task> verify) where T : IComponent
            => Renderer.Dispatcher.InvokeAsync(async () =>
            {
                var output = await Renderer.RenderComponentAsync<T>(ParameterView.FromDictionary(parameters));
                await verify(Capture.Instances.OfType<T>().Single(), output.ToHtmlString);
            });
        public async ValueTask DisposeAsync() { await Renderer.DisposeAsync(); await Provider.DisposeAsync(); }
    }
    private sealed class TestNavigation : NavigationManager
    {
        internal TestNavigation(string path, string baseUri) => Initialize(baseUri, new Uri(new Uri(baseUri), path).ToString());
        protected override void NavigateToCore(string uri, bool forceLoad) { Uri = ToAbsoluteUri(uri).ToString(); NotifyLocationChanged(false); }
    }
    private sealed class CaptureActivator : IComponentActivator
    {
        internal List<IComponent> Instances { get; } = [];
        public IComponent CreateInstance(Type type) { var component = (IComponent)Activator.CreateInstance(type)!; Instances.Add(component); return component; }
    }
    private sealed class NoInterop : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => throw new InvalidOperationException("Static navigation tree must not need interop: " + identifier);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => throw new InvalidOperationException("Static navigation tree must not need interop: " + identifier);
    }
}
