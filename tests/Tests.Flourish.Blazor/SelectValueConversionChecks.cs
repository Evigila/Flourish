using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.HtmlRendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class SelectValueConversionChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("bound Boolean select handles native string events and recovers parsing validation", async () =>
        {
            var model = new Selection<bool> { Value = true };
            var context = new EditContext(model);
            await using var fixture = new Fixture();
            var output = await fixture.RenderBound(model, context, BooleanOptions);
            var control = fixture.Component<SelectBox<bool>>();
            RequireSelected(await fixture.Html(output), "true", "Enabled");

            await fixture.Change(control, "false");
            Require(!model.Value && model.Changes == 1, "A native false option did not update the bound Boolean.");
            RequireSelected(await fixture.Html(output), "false", "Disabled");
            await fixture.Change(control, "TRUE");
            Require(model.Value && model.Changes == 2, "A native Boolean string did not update the bound value.");
            RequireSelected(await fixture.Html(output), "true", "Enabled");

            foreach (var invalid in new[] { "invalid", "", (string?)null })
            {
                await fixture.Change(control, invalid);
                Require(model.Value && model.Changes == 2, "An invalid nonnullable Boolean overwrote the bound value.");
                Require(context.GetValidationMessages().Single() == "Choose a valid selection.", "A parsing failure did not use InputBase validation.");
            }

            await fixture.Change(control, "false");
            Require(!model.Value && model.Changes == 3 && !context.GetValidationMessages().Any(), "A valid choice did not clear the parsing failure.");
            RequireSelected(await fixture.Html(output), "false", "Disabled");
            Require(context.IsModified(new FieldIdentifier(model, nameof(Selection<bool>.Value))), "The bound selection bypassed EditContext field changes.");
        }));

        tests.Add(("standalone Boolean select ignores invalid strings and preserves its displayed value", async () =>
        {
            var model = new Selection<bool> { Value = false };
            await using var fixture = new Fixture();
            var output = await fixture.RenderStandalone(model, BooleanOptions);
            var control = fixture.Component<StandaloneSelectBox<bool>>();
            RequireSelected(await fixture.Html(output), "false", "Disabled");
            await fixture.Change(control, "true");
            Require(model.Value && model.Changes == 1, "Standalone native true was not parsed as a Boolean.");
            RequireSelected(await fixture.Html(output), "true", "Enabled");

            foreach (var invalid in new[] { "invalid", "", (string?)null })
            {
                await fixture.Change(control, invalid);
                Require(model.Value && model.Changes == 1, "An invalid standalone Boolean notified its owner.");
                RequireSelected(await fixture.Html(output), "true", "Enabled");
            }

            await fixture.Change(control, "false");
            Require(!model.Value && model.Changes == 2, "Standalone parsing did not recover after invalid input.");
            RequireSelected(await fixture.Html(output), "false", "Disabled");
        }));

        tests.Add(("bound nullable Boolean select preserves true false and undeclared choices", async () =>
        {
            var model = new Selection<bool?>();
            var context = new EditContext(model);
            await using var fixture = new Fixture();
            var output = await fixture.RenderBound(model, context, NullableBooleanOptions);
            var control = fixture.Component<SelectBox<bool?>>();
            RequireSelected(await fixture.Html(output), "", "Undeclared");

            await fixture.Change(control, "true");
            Require(model.Value == true && model.Changes == 1, "Nullable native true was not preserved.");
            RequireSelected(await fixture.Html(output), "true", "Enabled");
            await fixture.Change(control, "false");
            Require(model.Value == false && model.Changes == 2, "Nullable native false became null or failed to notify.");
            RequireSelected(await fixture.Html(output), "false", "Disabled");
            await fixture.Change(control, "invalid");
            Require(model.Value == false && model.Changes == 2 && context.GetValidationMessages().Any(), "An invalid nullable choice lost the prior false value or validation.");

            await fixture.Change(control, "");
            Require(model.Value is null && model.Changes == 3 && !context.GetValidationMessages().Any(), "The empty option did not restore an undeclared value and clear validation.");
            RequireSelected(await fixture.Html(output), "", "Undeclared");
            await fixture.Change(control, "true");
            await fixture.Change(control, null);
            Require(model.Value is null && model.Changes == 5 && !context.GetValidationMessages().Any(), "A null native event did not retain the nullable contract.");
            RequireSelected(await fixture.Html(output), "", "Undeclared");
        }));

        tests.Add(("standalone nullable Boolean select renders and changes all three states", async () =>
        {
            var model = new Selection<bool?>();
            await using var fixture = new Fixture();
            var output = await fixture.RenderStandalone(model, NullableBooleanOptions);
            var control = fixture.Component<StandaloneSelectBox<bool?>>();
            RequireSelected(await fixture.Html(output), "", "Undeclared");
            await fixture.Change(control, "true");
            Require(model.Value == true && model.Changes == 1, "Standalone nullable true was not delivered.");
            RequireSelected(await fixture.Html(output), "true", "Enabled");
            await fixture.Change(control, "false");
            Require(model.Value == false && model.Changes == 2, "Standalone nullable false was not delivered.");
            RequireSelected(await fixture.Html(output), "false", "Disabled");
            await fixture.Change(control, "invalid");
            Require(model.Value == false && model.Changes == 2, "An invalid standalone nullable choice notified its owner.");
            RequireSelected(await fixture.Html(output), "false", "Disabled");

            await fixture.Change(control, "");
            Require(model.Value is null && model.Changes == 3, "Standalone empty selection became a Boolean default.");
            RequireSelected(await fixture.Html(output), "", "Undeclared");
            await fixture.Change(control, "true");
            await fixture.Change(control, null);
            Require(model.Value is null && model.Changes == 5, "Standalone null native value did not clear nullable selection.");
            RequireSelected(await fixture.Html(output), "", "Undeclared");
        }));

        tests.Add(("disabled bound and standalone Boolean selects refuse native events", async () =>
        {
            foreach (var bound in new[] { true, false })
            {
                var model = new Selection<bool?> { Value = false };
                var context = new EditContext(model);
                await using var fixture = new Fixture();
                var output = bound
                    ? await fixture.RenderBound(model, context, NullableBooleanOptions)
                    : await fixture.RenderStandalone(model, NullableBooleanOptions);
                IComponent control = bound ? fixture.Component<SelectBox<bool?>>() : fixture.Component<StandaloneSelectBox<bool?>>();
                await fixture.Parameters(control, new() { ["Disabled"] = true, ["Value"] = model.Value });
                foreach (var value in new[] { "true", "", "invalid" }) await fixture.Change(control, value);
                Require(model.Value == false && model.Changes == 0 && !context.GetValidationMessages().Any() && !context.IsModified(), "Disabled selection changed its owner or validation state.");
                RequireSelected(await fixture.Html(output), "false", "Disabled");
                Require(Regex.IsMatch(await fixture.Html(output), @"<select\b[^>]*\sdisabled(?:=|\s|>)"), "Disabled state was not rendered on the real select.");
                await fixture.Parameters(control, new() { ["Disabled"] = false, ["Value"] = model.Value });
                await fixture.Change(control, "true");
                Require(model.Value == true && model.Changes == 1, "Enabling the original control did not restore its native event.");
                RequireSelected(await fixture.Html(output), "true", "Enabled");
            }
        }));

        tests.Add(("Boolean child options match the library's lowercase current value", async () =>
        {
            foreach (var bound in new[] { true, false })
            {
                var model = new Selection<bool?> { Value = false };
                await using var fixture = new Fixture();
                var child = (RenderFragment)(builder =>
                {
                    builder.OpenElement(0, "option"); builder.AddAttribute(1, "value", ""); builder.AddContent(2, "Undeclared"); builder.CloseElement();
                    builder.OpenElement(3, "option"); builder.AddAttribute(4, "value", "true"); builder.AddContent(5, "Enabled"); builder.CloseElement();
                    builder.OpenElement(6, "option"); builder.AddAttribute(7, "value", "false"); builder.AddContent(8, "Disabled"); builder.CloseElement();
                });
                var output = bound
                    ? await fixture.RenderBound(model, new EditContext(model), NullableBooleanOptions, child)
                    : await fixture.RenderStandalone(model, NullableBooleanOptions, child);
                IComponent control = bound ? fixture.Component<SelectBox<bool?>>() : fixture.Component<StandaloneSelectBox<bool?>>();
                RequireSelected(await fixture.Html(output), "false", "Disabled");
                await fixture.Change(control, "true");
                RequireSelected(await fixture.Html(output), "true", "Enabled");
                await fixture.Change(control, "");
                Require(model.Value is null, "Handwritten empty options lost their nullable binding.");
                RequireSelected(await fixture.Html(output), "", "Undeclared");
            }
        }));

        tests.Add(("select Boolean repair retains nullable numeric enum Guid and string conversion", async () =>
        {
            await ScalarRoundTrip<int?>(1, 2, "2", clearsEmpty: true);
            await ScalarRoundTrip<Choice?>(Choice.First, Choice.Second, "Second", clearsEmpty: true);
            await ScalarRoundTrip(Choice.First, Choice.Second, "Second");
            await ScalarRoundTrip(Guid.Parse("edeb6c85-2c90-44c2-8a01-32ebca1634df"), Guid.Parse("770f83a9-ddce-46d2-90ce-9ca32cbb1b68"), "770f83a9-ddce-46d2-90ce-9ca32cbb1b68");
            await ScalarRoundTrip("before", "after", "after");
        }));
    }

    private static readonly SelectOption<bool>[] BooleanOptions = [new(true, "Enabled"), new(false, "Disabled")];
    private static readonly SelectOption<bool?>[] NullableBooleanOptions = [new(null, "Undeclared"), new(true, "Enabled"), new(false, "Disabled")];

    private static async Task ScalarRoundTrip<TValue>(TValue before, TValue after, string nativeValue, bool clearsEmpty = false)
    {
        foreach (var bound in new[] { true, false })
        {
            var model = new Selection<TValue> { Value = before };
            var context = new EditContext(model);
            SelectOption<TValue>[] options = clearsEmpty
                ? [new(default!, "Undeclared"), new(before, "Before"), new(after, "After")]
                : [new(before, "Before"), new(after, "After")];
            await using var fixture = new Fixture();
            var output = bound ? await fixture.RenderBound(model, context, options) : await fixture.RenderStandalone(model, options);
            IComponent control = bound ? fixture.Component<SelectBox<TValue>>() : fixture.Component<StandaloneSelectBox<TValue>>();
            await fixture.Change(control, nativeValue);
            Require(EqualityComparer<TValue>.Default.Equals(model.Value, after) && model.Changes == 1, $"{typeof(TValue).Name} lost its existing native conversion.");
            RequireSelected(await fixture.Html(output), nativeValue, "After");
            if (typeof(TValue) != typeof(string))
            {
                await fixture.Change(control, "invalid");
                Require(EqualityComparer<TValue>.Default.Equals(model.Value, after) && model.Changes == 1, $"{typeof(TValue).Name} no longer retains its value after invalid input.");
                Require(context.GetValidationMessages().Any() == bound, $"{typeof(TValue).Name} changed its bound-versus-standalone validation contract.");
                if (!bound) RequireSelected(await fixture.Html(output), nativeValue, "After");
            }
            if (clearsEmpty)
            {
                await fixture.Change(control, "");
                Require(model.Value is null && model.Changes == 2 && !context.GetValidationMessages().Any(), $"{typeof(TValue).Name} lost its empty nullable selection.");
            }
            else if (typeof(TValue) != typeof(string))
            {
                await fixture.Change(control, nativeValue);
                Require(EqualityComparer<TValue>.Default.Equals(model.Value, after) && !context.GetValidationMessages().Any(), $"{typeof(TValue).Name} could not recover its valid selection.");
                RequireSelected(await fixture.Html(output), nativeValue, "After");
            }
        }
    }

    private static void RequireSelected(string html, string value, string text)
    {
        var options = Regex.Matches(html, @"<option\b[^>]*>[^<]*</option>").Cast<Match>().ToArray();
        Require(options.Length > 0 && options.All(option => Regex.IsMatch(option.Value, "\\svalue=\"[^\"]*\"")), "A typed option lost its string value attribute: " + html);
        var selected = options.Where(option => Regex.IsMatch(option.Value, @"\sselected(?:=|\s|>)")).ToArray();
        Require(selected.Length == 1 && selected[0].Value.Contains($"value=\"{value}\"", StringComparison.Ordinal)
            && selected[0].Value.EndsWith($">{text}</option>", StringComparison.Ordinal), "The current value did not match the actual option: " + html);
    }

#pragma warning disable BL0006
    private static Task DispatchChange(IComponent component, string? value)
    {
        using var tree = new RenderTreeBuilder();
        component.GetType().GetMethod("BuildRenderTree", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(component, [tree]);
        var frames = tree.GetFrames();
        for (var index = 0; index < frames.Count; index++)
        {
            var frame = frames.Array[index];
            if (frame.FrameType != RenderTreeFrameType.Attribute || frame.AttributeName != "onchange") continue;
            var args = new ChangeEventArgs { Value = value };
            return frame.AttributeValue switch
            {
                EventCallback<ChangeEventArgs> callback => callback.InvokeAsync(args),
                EventCallback callback => callback.InvokeAsync(args),
                MulticastDelegate callback => ((IHandleEvent)component).HandleEventAsync(new EventCallbackWorkItem(callback), args),
                _ => throw new InvalidOperationException("The production select's native onchange binding cannot be invoked.")
            };
        }
        throw new InvalidOperationException("The production select lost its native onchange binding.");
    }
#pragma warning restore BL0006

    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private enum Choice { First, Second }
    private sealed class Selection<TValue> { public TValue Value { get; set; } = default!; public int Changes { get; set; } }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly ServiceProvider services;
        private readonly Capture capture = new();
        private readonly HtmlRenderer renderer;
        internal Fixture()
        {
            var registrations = new ServiceCollection().AddLogging().AddFlourishFramework();
            registrations.AddSingleton<IComponentActivator>(capture);
            registrations.AddSingleton<NavigationManager, Navigation>();
            registrations.AddSingleton<IJSRuntime, NoJs>();
            services = registrations.BuildServiceProvider();
            renderer = new(services, services.GetRequiredService<ILoggerFactory>());
        }

        internal Task<HtmlRootComponent> RenderBound<TValue>(Selection<TValue> model, EditContext context, IReadOnlyList<SelectOption<TValue>> options, RenderFragment? child = null)
        {
            var parameters = Parameters(model, options, child);
            parameters[nameof(SelectBox<TValue>.ValueExpression)] = (Expression<Func<TValue>>)(() => model.Value);
            parameters[nameof(SelectBox<TValue>.ParsingErrorMessage)] = "Choose a valid selection.";
            return renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<CascadingValue<EditContext>>(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(CascadingValue<EditContext>.Value)] = context,
                [nameof(CascadingValue<EditContext>.IsFixed)] = true,
                [nameof(CascadingValue<EditContext>.ChildContent)] = (RenderFragment)(builder =>
                {
                    builder.OpenComponent<SelectBox<TValue>>(0);
                    builder.AddMultipleAttributes(1, parameters.Select(parameter => new KeyValuePair<string, object>(parameter.Key, parameter.Value!)));
                    builder.CloseComponent();
                })
            })));
        }

        internal Task<HtmlRootComponent> RenderStandalone<TValue>(Selection<TValue> model, IReadOnlyList<SelectOption<TValue>> options, RenderFragment? child = null) =>
            renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<StandaloneSelectBox<TValue>>(ParameterView.FromDictionary(Parameters(model, options, child))));

        private static Dictionary<string, object?> Parameters<TValue>(Selection<TValue> model, IReadOnlyList<SelectOption<TValue>> options, RenderFragment? child) => new()
        {
            ["Value"] = model.Value, ["Options"] = options, ["ChildContent"] = child,
            ["ValueChanged"] = EventCallback.Factory.Create<TValue>(model, value => { model.Value = value; model.Changes++; })
        };

        internal TComponent Component<TComponent>() where TComponent : IComponent => capture.Items.OfType<TComponent>().Single();
        internal Task<string> Html(HtmlRootComponent output) => renderer.Dispatcher.InvokeAsync(output.ToHtmlString);
        internal Task Change(IComponent component, string? value) => renderer.Dispatcher.InvokeAsync(() => DispatchChange(component, value));
        internal Task Parameters(IComponent component, Dictionary<string, object?> parameters) => renderer.Dispatcher.InvokeAsync(() => component.SetParametersAsync(ParameterView.FromDictionary(parameters)));
        public async ValueTask DisposeAsync() { await renderer.DisposeAsync(); await services.DisposeAsync(); }
    }

    private sealed class Capture : IComponentActivator
    {
        internal List<IComponent> Items { get; } = [];
        public IComponent CreateInstance(Type type) { var component = (IComponent)Activator.CreateInstance(type)!; Items.Add(component); return component; }
    }
    private sealed class Navigation : NavigationManager { public Navigation() => Initialize("https://example.test/", "https://example.test/form"); }
    private sealed class NoJs : IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args) => throw new InvalidOperationException("Static rendering must not call browser services.");
        public ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken cancellationToken, object?[]? args) => InvokeAsync<T>(identifier, args);
    }
}
