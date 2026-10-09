using System.Linq.Expressions;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using ArkheideSystem.Gallery.Flourish.Blazor.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.HtmlRendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class FieldActionsChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("Field without actions retains its ordinary label control and validation structure", async () =>
        {
            await using var fixture = new Fixture();
            var output = await fixture.Render<Field>(new()
            {
                [nameof(Field.Id)] = "draft-name", [nameof(Field.ControlId)] = "actual-name",
                [nameof(Field.LabelId)] = "actual-label", [nameof(Field.Label)] = "Name", [nameof(Field.Required)] = true,
                [nameof(Field.FullWidth)] = true, [nameof(Field.Error)] = "Invalid <name>",
                [nameof(Field.ChildContent)] = (RenderFragment)(builder =>
                {
                    builder.OpenComponent<StandaloneTextBox>(0);
                    builder.AddAttribute(1, nameof(StandaloneTextBox.Id), "actual-name");
                    builder.CloseComponent();
                })
            });
            var html = await fixture.Html(output);
            Require(!html.Contains("f-field-with-actions") && !html.Contains("f-field-actions"), "Omitted Actions selected a different field layout.");
            Require(html.Contains("f-field-full") && html.Contains("id=\"actual-label\"") && html.Contains("for=\"actual-name\""), "Ordinary field identities or full-width placement changed.");
            Require(Regex.IsMatch(html, "<div class=\"f-field-control\">\\s*<input[^>]*>\\s*</div>\\s*<div[^>]*id=\"draft-name-error\""), "An ordinary field changed the label/control/error hierarchy.");
            Require(html.Contains("Invalid &lt;name&gt;") && Regex.Matches(html, "f-field-errors").Count == 1, "The original encoded validation renderer was duplicated or lost.");
        }));

        tests.Add(("Field action slot excludes operation controls from its required and error context", async () =>
        {
            await using var fixture = new Fixture();
            var output = await fixture.Render<Field>(new()
            {
                [nameof(Field.Id)] = "create-name", [nameof(Field.Label)] = "Name", [nameof(Field.Required)] = true,
                [nameof(Field.Error)] = "Name is required.", [nameof(Field.ChildContent)] = StandaloneInput(),
                [nameof(Field.Actions)] = Actions(includeInputProbe: true)
            });
            var html = await fixture.Html(output);
            var input = Input(html, "create-name");
            var actionInput = Regex.Match(html, "<input\\b(?=[^>]*data-action-probe=\"true\")[^>]*>").Value;
            var button = Regex.Match(html, "<button\\b[^>]*>[\\s\\S]*?</button>").Value;
            Require(html.Contains("f-field-with-actions") && Regex.IsMatch(html,
                "<label\\b[^>]*for=\"create-name\"[^>]*>[\\s\\S]*?f-required[\\s\\S]*?</label>\\s*<div class=\"f-field-control\">"), "The marker does not identify the input's label.");
            Require(Regex.Matches(html, "class=\"f-required\"").Count == 1 && !button.Contains("f-required"), "An operation button acquired the field's required marker.");
            Require(Regex.IsMatch(html, "id=\"create-name-error\"[\\s\\S]*?</div>\\s*</div>\\s*<div class=\"f-field-actions\">"), "The error is not anchored inside the input control before the independent actions.");
            Require(Has(input, "required") && input.Contains("aria-invalid=\"true\"") && input.Contains("aria-describedby=\"create-name-error\""), "The labeled input lost its actual required/error semantics.");
            Require(actionInput.Length > 0 && !Has(actionInput, "required") && !Has(actionInput, "id")
                && !Has(actionInput, "aria-invalid") && !Has(actionInput, "aria-describedby"), "FieldContext leaked into action content.");
            Require(button.Contains("type=\"submit\"") && button.Contains("Create &lt;safe&gt;"), "The action slot changed native submission or text encoding.");
        }));

        tests.Add(("Field Actions preserve bound input events and live accessible validation messages", async () =>
        {
            var model = new Model(); var context = new EditContext(model); var messages = new ValidationMessageStore(context);
            await using var fixture = new Fixture();
            var output = await fixture.Render<CascadingValue<EditContext>>(new()
            {
                [nameof(CascadingValue<EditContext>.Value)] = context,
                [nameof(CascadingValue<EditContext>.ChildContent)] = BoundField(model)
            });
            await fixture.Renderer.Dispatcher.InvokeAsync(async () =>
            {
                await fixture.Components.OfType<TextBox>().Single().ValueChanged.InvokeAsync("New name");
                Require(model.Name == "New name", "The real input's binding callback was lost in the named slot.");
                messages.Add(new FieldIdentifier(model, nameof(Model.Name)), "Invalid <name>");
                context.NotifyValidationStateChanged();
            });
            var html = await fixture.Html(output);
            var input = Input(html, "bound-name");
            Require(Has(input, "required") && input.Contains("maxlength=\"100\"") && input.Contains("aria-invalid=\"true\""), "Bound input semantics were replaced by action-slot layout.");
            Require(input.Contains("aria-describedby=\"name-hint bound-name-error\"") && html.Contains("Invalid &lt;name&gt;")
                && Regex.Matches(html, "f-field-errors").Count == 1, "For validation lost its existing description, encoding or unique error renderer.");
            await fixture.Renderer.Dispatcher.InvokeAsync(() => { messages.Clear(); context.NotifyValidationStateChanged(); });
            html = await fixture.Html(output);
            input = Input(html, "bound-name");
            Require(!Has(input, "aria-invalid") && input.Contains("aria-describedby=\"name-hint\"") && !html.Contains("f-field-errors"), "Cleared errors remained attached to the input.");
            Require(html.Contains("class=\"f-field-actions\"") && html.Contains("type=\"submit\""), "Validation updates removed the actual operation slot.");
        }));

        tests.Add(("Field with actions switches and releases its form validation subscriptions", async () =>
        {
            var firstModel = new Model(); var first = new EditContext(firstModel);
            var secondModel = new Model(); var second = new EditContext(secondModel);
            var firstMessages = new ValidationMessageStore(first); firstMessages.Add(new FieldIdentifier(firstModel, nameof(Model.Name)), "First scope");
            var secondMessages = new ValidationMessageStore(second); secondMessages.Add(new FieldIdentifier(secondModel, nameof(Model.Name)), "Second scope");
            await using var fixture = new Fixture();
            var output = await fixture.Render<SwitchingField>(new() { [nameof(SwitchingField.Context)] = first });
            var parent = fixture.Components.OfType<SwitchingField>().Single();
            await fixture.Renderer.Dispatcher.InvokeAsync(() => parent.SetParametersAsync(ParameterView.FromDictionary(
                new Dictionary<string, object?> { [nameof(SwitchingField.Context)] = second })));
            var html = await fixture.Html(output);
            Require(html.Contains("Second scope") && !html.Contains("First scope"), "Switching form context retained another form's errors.");
            await fixture.Renderer.Dispatcher.InvokeAsync(() => { firstMessages.Clear(); first.NotifyValidationStateChanged(); });
            Require((await fixture.Html(output)).Contains("Second scope"), "A retired context changed the current field.");
            await fixture.Renderer.Dispatcher.InvokeAsync(() =>
            {
                fixture.Components.OfType<Field>().Single().Dispose();
                fixture.Components.OfType<ValidationMessages>().Single().Dispose();
                secondMessages.Clear(); second.NotifyValidationStateChanged();
            });
            Require((await fixture.Html(output)).Contains("Second scope"), "Disposed validation subscribers still rerendered the field.");
        }));

        tests.Add(("Field action and input slots have explicit Gallery API meanings and production guidance", () =>
        {
            var field = ComponentCatalog.Groups.SelectMany(group => group.Entries).Single(entry => entry.ComponentType == typeof(Field));
            Require(field.ApiParameters.Single(parameter => parameter.Name == nameof(Field.Actions)).DescriptionKey == "Key.Parameter_Actions_Field"
                && field.ApiParameters.Single(parameter => parameter.Name == nameof(Field.ChildContent)).DescriptionKey == "Key.Parameter_ChildContent_Field", "Named slots use ambiguous generic API descriptions.");
            Require(ComponentUsageCatalog.For(typeof(Field)).Guidance.FallbackText!.Contains("Optional Actions")
                && ComponentUsageCatalog.For(typeof(InlineActions)).Guidance.FallbackText!.Contains("Field.Actions"), "Production usage still recommends putting operations inside the labeled input.");
            return Task.CompletedTask;
        }));
    }

    private static RenderFragment StandaloneInput() => builder =>
    {
        builder.OpenComponent<StandaloneTextBox>(0);
        builder.AddAttribute(1, nameof(StandaloneTextBox.Value), "Draft");
        builder.CloseComponent();
    };

    private static RenderFragment Actions(bool includeInputProbe = false) => builder =>
    {
        builder.OpenComponent<InlineActions>(0);
        builder.AddAttribute(1, nameof(InlineActions.Alignment), HorizontalAlignment.End);
        builder.AddAttribute(2, nameof(InlineActions.ChildContent), (RenderFragment)(row =>
        {
            row.OpenComponent<Button>(0); row.AddAttribute(1, nameof(Button.Type), "submit");
            row.AddAttribute(2, nameof(Button.Text), "Create <safe>"); row.CloseComponent();
            if (includeInputProbe)
            {
                row.OpenComponent<StandaloneTextBox>(3);
                row.AddAttribute(4, nameof(StandaloneTextBox.AdditionalAttributes), new Dictionary<string, object> { ["data-action-probe"] = "true" });
                row.CloseComponent();
            }
        }));
        builder.CloseComponent();
    };

    private static RenderFragment BoundField(Model model) => builder =>
    {
        builder.OpenComponent<Field>(0); builder.AddAttribute(1, nameof(Field.Id), "bound-name");
        builder.AddAttribute(2, nameof(Field.Label), "Name"); builder.AddAttribute(3, nameof(Field.Required), true);
        builder.AddAttribute(4, nameof(Field.For), (Expression<Func<object?>>)(() => model.Name));
        builder.AddAttribute(5, nameof(Field.ChildContent), (RenderFragment)(input =>
        {
            input.OpenComponent<TextBox>(0); input.AddAttribute(1, nameof(TextBox.Value), model.Name);
            input.AddAttribute(2, nameof(TextBox.ValueChanged), EventCallback.Factory.Create<string?>(model, name => model.Name = name ?? ""));
            input.AddAttribute(3, nameof(TextBox.ValueExpression), (Expression<Func<string?>>)(() => model.Name));
            input.AddAttribute(4, nameof(TextBox.MaxLength), 100); input.AddAttribute(5, nameof(TextBox.ChangeEvent), "change");
            input.AddAttribute(6, nameof(TextBox.AdditionalAttributes), new Dictionary<string, object> { ["aria-describedby"] = "name-hint" });
            input.CloseComponent();
        }));
        builder.AddAttribute(6, nameof(Field.Actions), Actions()); builder.CloseComponent();
    };

    private static string Input(string html, string id) => Regex.Match(html, "<input\\b(?=[^>]*id=\"" + Regex.Escape(id) + "\")[^>]*>").Value;
    private static bool Has(string html, string attribute) => Regex.IsMatch(html, @"\s" + Regex.Escape(attribute) + @"(?:=|\s|>)");
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private sealed class Model { public string Name { get; set; } = ""; }

    private sealed class SwitchingField : ComponentBase
    {
        [Parameter] public EditContext Context { get; set; } = null!;
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            builder.OpenComponent<CascadingValue<EditContext>>(0);
            builder.AddAttribute(1, nameof(CascadingValue<EditContext>.Value), Context);
            builder.AddAttribute(2, nameof(CascadingValue<EditContext>.ChildContent), (RenderFragment)(child =>
            {
                var model = (Model)Context.Model;
                child.OpenComponent<Field>(0); child.AddAttribute(1, nameof(Field.Id), "switching-name");
                child.AddAttribute(2, nameof(Field.Label), "Name"); child.AddAttribute(3, nameof(Field.Required), true);
                child.AddAttribute(4, nameof(Field.For), (Expression<Func<object?>>)(() => model.Name));
                child.AddAttribute(5, nameof(Field.ChildContent), StandaloneInput());
                child.AddAttribute(6, nameof(Field.Actions), Actions()); child.CloseComponent();
            }));
            builder.CloseComponent();
        }
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly ServiceProvider services;
        private readonly Capture capture;
        internal HtmlRenderer Renderer { get; }
        internal IReadOnlyList<IComponent> Components => capture.Items;
        internal Fixture()
        {
            var registrations = new ServiceCollection().AddLogging().AddFlourishFramework();
            registrations.AddSingleton<Capture>(); registrations.AddSingleton<IComponentActivator>(provider => provider.GetRequiredService<Capture>());
            registrations.AddSingleton<NavigationManager, Navigation>(); registrations.AddSingleton<IJSRuntime, NoJs>();
            services = registrations.BuildServiceProvider(); capture = services.GetRequiredService<Capture>();
            Renderer = new(services, services.GetRequiredService<ILoggerFactory>());
        }
        internal Task<HtmlRootComponent> Render<T>(Dictionary<string, object?> parameters) where T : IComponent =>
            Renderer.Dispatcher.InvokeAsync(() => Renderer.RenderComponentAsync<T>(ParameterView.FromDictionary(parameters)));
        internal Task<string> Html(HtmlRootComponent output) => Renderer.Dispatcher.InvokeAsync(output.ToHtmlString);
        public async ValueTask DisposeAsync() { await Renderer.DisposeAsync(); await services.DisposeAsync(); }
    }

    private sealed class Capture : IComponentActivator
    {
        internal List<IComponent> Items { get; } = [];
        public IComponent CreateInstance(Type type) { var component = (IComponent)Activator.CreateInstance(type)!; Items.Add(component); return component; }
    }
    private sealed class Navigation : NavigationManager { public Navigation() => Initialize("https://example.test/", "https://example.test/form"); }
    private sealed class NoJs : IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args) => throw new InvalidOperationException("Static rendering must not import browser behavior.");
        public ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken cancellationToken, object?[]? args) => InvokeAsync<T>(identifier, args);
    }
}
