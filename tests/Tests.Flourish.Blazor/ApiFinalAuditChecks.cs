using System.Globalization;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.HtmlRendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Primitives = ArkheideSystem.Flourish.Blazor.Components.Primitives;

internal static class ApiFinalAuditChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("bound checkbox generates its native POST name while an explicit host name takes precedence", async () =>
        {
            var model = new PostModel { Enabled = true };
            await using var fixture = new Fixture();
            var parameters = new Dictionary<string, object?>
            {
                ["Value"] = model.Enabled, ["ValueExpression"] = (Expression<Func<bool>>)(() => model.Enabled)
            };
            var generated = await fixture.Render<CheckBox>(parameters);
            await fixture.Renderer.Dispatcher.InvokeAsync(() =>
            {
                var input = Regex.Match(generated.ToHtmlString(), @"<input\b[^>]*>").Value;
                Require(input.Contains("name=\"model.Enabled\"", StringComparison.Ordinal)
                    && input.Contains("value=\"true\"", StringComparison.Ordinal)
                    && Regex.IsMatch(input, @"\schecked(?:=|\s|>)"),
                    "The bound checkbox cannot submit its selected boolean field through native POST: " + input);
            });
            parameters["AdditionalAttributes"] = new Dictionary<string, object> { ["name"] = "custom-enabled", ["data-field"] = "enabled" };
            var explicitName = await fixture.Render<CheckBox>(parameters);
            await fixture.Renderer.Dispatcher.InvokeAsync(() =>
            {
                var input = Regex.Match(explicitName.ToHtmlString(), @"<input\b[^>]*>").Value;
                Require(input.Contains("name=\"custom-enabled\"", StringComparison.Ordinal)
                    && !input.Contains("name=\"model.Enabled\"", StringComparison.Ordinal)
                    && input.Contains("data-field=\"enabled\"", StringComparison.Ordinal),
                    "Generated checkbox names must not replace an explicit native host field name: " + input);
            });
        }));

        tests.Add(("bound select formats the selected value with its scoped provider and refreshes across cultures", async () =>
        {
            decimal value = 1.5m;
            var threadCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            try
            {
                await using var fixture = new Fixture();
                fixture.Provider.Select("pt-BR");
                var output = await fixture.Render<SelectBox<decimal>>(new()
                {
                    ["Value"] = value, ["ValueExpression"] = (Expression<Func<decimal>>)(() => value),
                    ["Options"] = new SelectOption<decimal>[] { new(2.5m, "Other"), new(value, "Selected") }
                });
                await fixture.Renderer.Dispatcher.InvokeAsync(() => RequireSelected(output.ToHtmlString(), "1,5"));
                fixture.Provider.Select("en-US");
                await fixture.Renderer.Dispatcher.InvokeAsync(() => RequireSelected(output.ToHtmlString(), "1.5"));
                fixture.Provider.Select("de-DE");
                await fixture.Renderer.Dispatcher.InvokeAsync(() => RequireSelected(output.ToHtmlString(), "1,5"));
                var component = fixture.Component<SelectBox<decimal>>();
                object?[] arguments = ["2,5", null, null];
                await fixture.Renderer.Dispatcher.InvokeAsync(() =>
                {
                    Require((bool)Method(component, "TryParseValueFromString").Invoke(component, arguments)!
                        && arguments[1] is decimal parsed && parsed == 2.5m,
                        "Select parsing did not use the same provider culture as option and current-value formatting.");
                });
                Require(CultureInfo.CurrentCulture.Name == "en-US", "Scoped culture selection changed the caller's thread culture.");
            }
            finally { CultureInfo.CurrentCulture = threadCulture; }
        }));

        tests.Add(("autocomplete SSR emits explicit expanded and selected states through keyboard selection", async () =>
        {
            await using var fixture = new Fixture();
            var output = await fixture.Render<Primitives.SearchAutocomplete<string>>(AutocompleteParameters());
            var component = fixture.Component<Primitives.SearchAutocomplete<string>>();
            await fixture.Renderer.Dispatcher.InvokeAsync(() =>
            {
                Require(output.ToHtmlString().Contains("aria-expanded=\"false\"", StringComparison.Ordinal),
                    "A closed combobox must explicitly announce false rather than omit or minimize aria-expanded: " + output.ToHtmlString());
                Method(component, "Open").Invoke(component, null);
                Rerender(component);
                var html = output.ToHtmlString();
                Require(html.Contains("aria-expanded=\"true\"", StringComparison.Ordinal)
                    && Regex.Matches(html, "aria-selected=\"false\"").Count == 2,
                    "An open combobox or its initially unselected options has invalid boolean ARIA: " + html);
            });
            await fixture.Renderer.Dispatcher.InvokeAsync(async () =>
            {
                await (Task)Method(component, "HandleKeyDownAsync").Invoke(component, [new KeyboardEventArgs { Key = "ArrowDown" }])!;
                Rerender(component);
                var html = output.ToHtmlString();
                Require(Regex.Matches(html, "aria-selected=\"true\"").Count == 1
                    && Regex.Matches(html, "aria-selected=\"false\"").Count == 1,
                    "Keyboard activation must expose exactly one explicitly selected suggestion: " + html);
            });
        }));

        tests.Add(("disabled autocomplete ignores delayed input clearing and selection callbacks", async () =>
        {
            var valueChanges = 0;
            var inputChanges = 0;
            var selected = 0;
            var cleared = 0;
            await using var fixture = new Fixture();
            var parameters = AutocompleteParameters();
            parameters["Disabled"] = true;
            parameters["ValueChanged"] = EventCallback.Factory.Create<string>(fixture, _ => valueChanges++);
            parameters["InputChanged"] = EventCallback.Factory.Create<string>(fixture, _ => inputChanges++);
            parameters["ItemSelected"] = EventCallback.Factory.Create<string>(fixture, _ => selected++);
            parameters["SearchCleared"] = EventCallback.Factory.Create(fixture, () => cleared++);
            await fixture.Render<Primitives.SearchAutocomplete<string>>(parameters);
            var component = fixture.Component<Primitives.SearchAutocomplete<string>>();
            await fixture.Renderer.Dispatcher.InvokeAsync(async () =>
            {
                await (Task)Method(component, "InputAsync").Invoke(component, [new ChangeEventArgs { Value = "" }])!;
                await (Task)Method(component, "SelectAsync").Invoke(component, ["apple"])!;
            });
            Require(valueChanges == 0 && inputChanges == 0 && selected == 0 && cleared == 0,
                $"Disabled autocomplete accepted delayed events: value={valueChanges}, input={inputChanges}, selected={selected}, cleared={cleared}.");
        }));

        tests.Add(("multi-select disables existing choices and refuses their callbacks while asynchronous creation is pending", async () =>
        {
            var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var selections = 0;
            Task? creating = null;
            await using var fixture = new Fixture();
            var output = await fixture.Render<Primitives.MultiSelectDropdown<string, string>>(new()
            {
                ["Id"] = "creating-select", ["Items"] = new[] { "Existing" },
                ["SelectedValues"] = new HashSet<string>(), ["MaximumSelections"] = 1,
                ["ValueSelector"] = (Func<string, string>)(item => item), ["TextSelector"] = (Func<string, string>)(item => item),
                ["SelectionChanged"] = EventCallback.Factory.Create<string>(fixture, _ => selections++),
                ["CreateRequested"] = (Func<string, Task<bool>>)(_ => completion.Task)
            });
            var component = fixture.Component<Primitives.MultiSelectDropdown<string, string>>();
            try
            {
                await fixture.Renderer.Dispatcher.InvokeAsync(() =>
                {
                    Method(component, "Toggle").Invoke(component, null);
                    Method(component, "SearchChanged").Invoke(component, [new ChangeEventArgs { Value = "Exist" }]);
                    creating = (Task)Method(component, "CreateAsync").Invoke(component, null)!;
                    Rerender(component);
                });
                await fixture.Renderer.Dispatcher.InvokeAsync(async () =>
                {
                    var checkbox = Regex.Match(output.ToHtmlString(), @"<input\b[^>]*type=""checkbox""[^>]*>");
                    await (Task)Method(component, "ChangeAsync").Invoke(component, ["Existing", new ChangeEventArgs { Value = true }])!;
                    Require(checkbox.Success && Regex.IsMatch(checkbox.Value, @"\sdisabled(?:=|\s|>)") && selections == 0,
                        $"Pending creation left an existing choice enabled or emitted SelectionChanged ({selections}): {output.ToHtmlString()}");
                });
            }
            finally
            {
                completion.TrySetResult(true);
                if (creating is not null) await creating;
            }
            await fixture.Renderer.Dispatcher.InvokeAsync(async () =>
            {
                Rerender(component);
                var checkbox = Regex.Match(output.ToHtmlString(), @"<input\b[^>]*type=""checkbox""[^>]*>");
                Require(checkbox.Success && !Regex.IsMatch(checkbox.Value, @"\sdisabled(?:=|\s|>)"), "Completed creation left normal choices disabled.");
                await (Task)Method(component, "ChangeAsync").Invoke(component, ["Existing", new ChangeEventArgs { Value = true }])!;
            });
            Require(selections == 1, "The creation guard changed the established value-toggle callback after creation completed.");
        }));
    }

    private static Dictionary<string, object?> AutocompleteParameters() => new()
    {
        ["Id"] = "audit-search", ["Value"] = "ap", ["Items"] = new[] { "apple", "apricot" },
        ["ValueSelector"] = (Func<string, string>)(item => item), ["TextSelector"] = (Func<string, string>)(item => item)
    };
    private static MethodInfo Method(IComponent component, string name) => component.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static void Rerender(IComponent component) => typeof(ComponentBase).GetMethod("StateHasChanged", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(component, null);
    private static void RequireSelected(string html, string value)
    {
        var options = Regex.Matches(html, @"<option\b[^>]*>[^<]*</option>").Where(match => Regex.IsMatch(match.Value, @"\sselected(?:=|\s|>)")).ToArray();
        Require(options.Length == 1 && WebUtility.HtmlDecode(options[0].Value).Contains($"value=\"{value}\"", StringComparison.Ordinal)
            && options[0].Value.Contains(">Selected</option>", StringComparison.Ordinal),
            "The current typed value must match exactly one provider-formatted selected option: " + html);
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private sealed class PostModel { public bool Enabled { get; set; } }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly ServiceProvider services;
        private readonly IServiceScope scope;
        private readonly CaptureActivator activator;
        internal HtmlRenderer Renderer { get; }
        internal TrackingTextProvider Provider { get; }
        internal Fixture()
        {
            var registrations = new ServiceCollection();
            registrations.AddLogging();
            registrations.AddFlourishFramework();
            registrations.AddScoped<ITextProvider, TrackingTextProvider>();
            registrations.AddScoped<CaptureActivator>();
            registrations.AddScoped<IComponentActivator>(provider => provider.GetRequiredService<CaptureActivator>());
            registrations.AddSingleton<NavigationManager>(new TestNavigation());
            registrations.AddSingleton<IJSRuntime, NoInterop>();
            services = registrations.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
            scope = services.CreateScope();
            Provider = (TrackingTextProvider)scope.ServiceProvider.GetRequiredService<ITextProvider>();
            activator = scope.ServiceProvider.GetRequiredService<CaptureActivator>();
            Renderer = new(scope.ServiceProvider, scope.ServiceProvider.GetRequiredService<ILoggerFactory>());
        }
        internal Task<HtmlRootComponent> Render<TComponent>(Dictionary<string, object?> parameters) where TComponent : IComponent
            => Renderer.Dispatcher.InvokeAsync(() => Renderer.RenderComponentAsync<TComponent>(ParameterView.FromDictionary(parameters)));
        internal TComponent Component<TComponent>() where TComponent : IComponent => activator.Components.OfType<TComponent>().Single();
        public async ValueTask DisposeAsync()
        {
            await Renderer.DisposeAsync();
            Require(Provider.Subscribers == 0, "Audit fixture disposal retained scoped text subscribers.");
            scope.Dispose();
            services.Dispose();
        }
    }
    private sealed class CaptureActivator : IComponentActivator
    {
        internal List<IComponent> Components { get; } = [];
        public IComponent CreateInstance(Type type)
        {
            var component = (IComponent)Activator.CreateInstance(type)!;
            Components.Add(component);
            return component;
        }
    }
    private sealed class NoInterop : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => throw new InvalidOperationException("SSR unexpectedly invoked JavaScript: " + identifier);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => InvokeAsync<TValue>(identifier, args);
    }
}
