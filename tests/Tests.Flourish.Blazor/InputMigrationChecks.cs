using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class InputMigrationChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("standalone text and number inputs preserve server POST attributes without expressions", async () =>
        {
            var text = await Render<StandaloneTextBox>(new()
            {
                [nameof(StandaloneTextBox.Id)] = "account-address", [nameof(StandaloneTextBox.Value)] = "a<&b",
                [nameof(StandaloneTextBox.ChangeEvent)] = "change",
                [nameof(StandaloneTextBox.AdditionalAttributes)] = Attributes(("name", "address"), ("data-value", "original"), ("autocomplete", "username"))
            });
            Require(text.Contains("id=\"account-address\"") && text.Contains("name=\"address\"") && text.Contains("data-value=\"original\"")
                && text.Contains("autocomplete=\"username\"") && text.Contains("value=\"a&lt;&amp;b\""), "SSR text input lost original POST or data attributes.");
            var number = await Render<StandaloneNumberBox>(new()
            {
                [nameof(StandaloneNumberBox.Value)] = "2.75", [nameof(StandaloneNumberBox.Min)] = "0",
                [nameof(StandaloneNumberBox.Max)] = "10", [nameof(StandaloneNumberBox.Step)] = "0.25",
                [nameof(StandaloneNumberBox.AdditionalAttributes)] = Attributes(("name", "amount"), ("required", true))
            });
            Require(number.Contains("type=\"number\"") && number.Contains("value=\"2.75\"") && number.Contains("name=\"amount\"")
                && number.Contains("min=\"0\"") && number.Contains("max=\"10\"") && number.Contains("step=\"0.25\"") && number.Contains("required"), "SSR numeric input lost browser constraints or its invariant POST value.");
        }));
        tests.Add(("standalone select emits the actual selected option in static HTML", async () =>
        {
            var html = await Render<StandaloneSelectBox<string>>(new()
            {
                [nameof(StandaloneSelectBox<string>.Value)] = "PT",
                [nameof(StandaloneSelectBox<string>.Options)] = new SelectOption<string>[] { new("BR", "Brasil"), new("PT", "Portugal") },
                [nameof(StandaloneSelectBox<string>.AdditionalAttributes)] = Attributes(("name", "country"), ("data-filter", "country"))
            });
            Require(html.Contains("name=\"country\"") && html.Contains("data-filter=\"country\""), "Select lost its native POST or browser-controller attributes.");
            Require(Regex.IsMatch(html, "<option[^>]*value=\"PT\"[^>]*selected[^>]*>Portugal</option>"), "Static select did not mark the supplied typed option as selected.");
            Require(!Regex.IsMatch(html, "<option[^>]*value=\"BR\"[^>]*selected"), "Static select selected the wrong option.");
        }));
        tests.Add(("standalone checkbox preserves boolean POST value and supports existing outer labels", async () =>
        {
            var parameters = new Dictionary<string, object?>
            {
                [nameof(StandaloneCheckBox.Id)] = "remember", [nameof(StandaloneCheckBox.WrapLabel)] = false,
                [nameof(StandaloneCheckBox.AdditionalAttributes)] = Attributes(("name", "rememberAccount"), ("data-column", "status"))
            };
            var uncheckedHtml = await Render<StandaloneCheckBox>(parameters);
            Require(uncheckedHtml.Contains("name=\"rememberAccount\"") && uncheckedHtml.Contains("value=\"true\"")
                && !uncheckedHtml.Contains("<label") && !uncheckedHtml.Contains(" checked"), "Unchecked SSR checkbox changed its POST value, label ownership or selection.");
            parameters[nameof(StandaloneCheckBox.Value)] = true;
            parameters[nameof(StandaloneCheckBox.Disabled)] = true;
            var checkedHtml = await Render<StandaloneCheckBox>(parameters);
            Require(checkedHtml.Contains(" checked") && checkedHtml.Contains(" disabled") && checkedHtml.Contains("data-column=\"status\""), "Checked SSR controller lost selection, disabled state or its browser hook.");
        }));
        tests.Add(("date input preserves nullable DateOnly and DateTime bindings and native attributes", async () =>
        {
            DateOnly? date = new(2026, 10, 5);
            var dateHtml = await Render<DateBox<DateOnly?>>(new()
            {
                [nameof(DateBox<DateOnly?>.Value)] = date, [nameof(DateBox<DateOnly?>.ValueExpression)] = (Expression<Func<DateOnly?>>)(() => date),
                [nameof(DateBox<DateOnly?>.Id)] = "due-date", [nameof(DateBox<DateOnly?>.Disabled)] = true,
                [nameof(DateBox<DateOnly?>.AdditionalAttributes)] = Attributes(("name", "dueDate"), ("min", "2026-01-01"))
            });
            Require(dateHtml.Contains("value=\"2026-10-05\"") && dateHtml.Contains("type=\"date\"") && dateHtml.Contains("name=\"dueDate\"")
                && dateHtml.Contains("min=\"2026-01-01\"") && dateHtml.Contains("disabled"), "Typed DateOnly field changed its value or native attributes.");
            DateTime? timestamp = new(2026, 10, 5);
            var timestampHtml = await Render<DateBox<DateTime?>>(new()
            {
                [nameof(DateBox<DateTime?>.Value)] = timestamp,
                [nameof(DateBox<DateTime?>.ValueExpression)] = (Expression<Func<DateTime?>>)(() => timestamp)
            });
            Require(timestampHtml.Contains("value=\"2026-10-05\""), "Nullable DateTime is not supported by the date wrapper.");
        }));
        tests.Add(("file picker keeps selection events at the host boundary and renders native constraints", async () =>
        {
            var html = await Render<FilePicker>(new()
            {
                [nameof(FilePicker.Id)] = "document-file", [nameof(FilePicker.Accept)] = ".xml",
                [nameof(FilePicker.Multiple)] = true, [nameof(FilePicker.Disabled)] = true,
                [nameof(FilePicker.AdditionalAttributes)] = Attributes(("name", "document"), ("data-file", "document"))
            });
            Require(html.Contains("type=\"file\"") && html.Contains("id=\"document-file\"") && html.Contains("accept=\".xml\"")
                && html.Contains("multiple") && html.Contains("disabled") && html.Contains("name=\"document\"") && html.Contains("data-file=\"document\""), "File picker lost original selection constraints or browser attributes.");
        }));
        tests.Add(("field labels and sections provide stable directory and dropdown relationships", async () =>
        {
            var field = await Render<Field>(new()
            {
                [nameof(Field.Id)] = "reference", [nameof(Field.LabelId)] = "reference-label", [nameof(Field.Label)] = "Reference"
            });
            Require(field.Contains("id=\"reference-label\"") && field.Contains("for=\"reference\""), "Field cannot supply the dropdown's aria-labelledby target.");
            var section = await Render<Section>(new()
            {
                [nameof(Section.Id)] = "details", [nameof(Section.Title)] = "Details",
                [nameof(Section.AdditionalAttributes)] = Attributes(("data-section", "details"))
            });
            Require(section.Contains("id=\"details\"") && section.Contains("id=\"details-title\"") && section.Contains("aria-labelledby=\"details-title\"")
                && section.Contains("data-section=\"details\""), "Section lost its directory target or accessible heading relationship.");
        }));
        tests.Add(("field descriptions deduplicate explicit error links and compound controls provide focus targets", async () =>
        {
            var html = await Render<Field>(new()
            {
                [nameof(Field.Id)] = "address", [nameof(Field.Label)] = "Address", [nameof(Field.ControlId)] = "address-trigger",
                [nameof(Field.Error)] = "Invalid address",
                [nameof(Field.ChildContent)] = (RenderFragment)(builder =>
                {
                    builder.OpenComponent<StandaloneTextBox>(0);
                    builder.AddAttribute(1, nameof(StandaloneTextBox.AdditionalAttributes), Attributes(("aria-describedby", "address-help address-error address-error")));
                    builder.CloseComponent();
                })
            });
            Require(html.Contains("for=\"address-trigger\"") && html.Contains("id=\"address-error\"")
                && html.Contains("aria-describedby=\"address-help address-error\""), "Compound field label or duplicate validation descriptions are incorrect.");
        }));
        tests.Add(("bound inputs generate server form field names and preserve explicit submission names", async () =>
        {
            var model = new PostModel();
            var text = await Render<TextBox>(new() { [nameof(TextBox.ValueExpression)] = (Expression<Func<string?>>)(() => model.Email) });
            var multiline = await Render<TextBox>(new() { [nameof(TextBox.Multiline)] = true, [nameof(TextBox.ValueExpression)] = (Expression<Func<string?>>)(() => model.Email) });
            var number = await Render<NumberBox<int>>(new() { [nameof(NumberBox<int>.ValueExpression)] = (Expression<Func<int>>)(() => model.Count) });
            var select = await Render<SelectBox<string>>(new() { [nameof(SelectBox<string>.ValueExpression)] = (Expression<Func<string>>)(() => model.Choice) });
            var mask = await Render<ArkheideSystem.Flourish.Blazor.Components.Primitives.MaskedInput>(new()
            {
                [nameof(ArkheideSystem.Flourish.Blazor.Components.Primitives.MaskedInput.ValueExpression)] = (Expression<Func<string?>>)(() => model.Email),
                [nameof(ArkheideSystem.Flourish.Blazor.Components.Primitives.MaskedInput.Mask)] = "000"
            });
            Require(text.Contains("name=\"model.Email\"") && multiline.Contains("name=\"model.Email\"") && number.Contains("name=\"model.Count\"")
                && select.Contains("name=\"model.Choice\"") && mask.Contains("name=\"model.Email\""), "A bound input lost the generated SSR POST field name.");
            var explicitName = await Render<TextBox>(new() { [nameof(TextBox.ValueExpression)] = (Expression<Func<string?>>)(() => model.Email),
                [nameof(TextBox.AdditionalAttributes)] = Attributes(("name", "custom-email")) });
            Require(explicitName.Contains("name=\"custom-email\""), "Explicit POST names must take priority.");
        }));
        tests.Add(("link buttons retain caller tab semantics and disabled links leave the tab sequence", async () =>
        {
            var html = await Render<Button>(new() { [nameof(Button.Href)] = "/account/", [nameof(Button.Text)] = "Account",
                [nameof(Button.AdditionalAttributes)] = Attributes(("role", "tab"), ("tabindex", "0"), ("aria-selected", "true")) });
            Require(html.Contains("role=\"tab\"") && html.Contains("tabindex=\"0\"") && html.Contains("aria-selected=\"true\""), "Link button erased caller tab semantics.");
            var disabled = await Render<Button>(new() { [nameof(Button.Href)] = "/account/", [nameof(Button.Disabled)] = true,
                [nameof(Button.AdditionalAttributes)] = Attributes(("role", "tab"), ("tabindex", "0")) });
            Require(disabled.Contains("role=\"tab\"") && disabled.Contains("tabindex=\"-1\"") && !disabled.Contains("href="), "Disabled tab link must retain its role and prevent navigation.");
        }));
        tests.Add(("masked inputs inherit field identity validation and descriptions while explicit native ids win", async () =>
        {
            string? value = "123";
            foreach (var bound in new[] { true, false })
            {
                foreach (var explicitId in new[] { false, true })
                {
                    var native = Attributes(("name", "reference"), ("aria-describedby", "reference-help"));
                    if (explicitId) native = native.Concat(new[] { new KeyValuePair<string, object>("id", "custom-reference") }).ToDictionary(item => item.Key, item => item.Value);
                    RenderFragment child = builder =>
                    {
                        if (bound)
                        {
                            builder.OpenComponent<ArkheideSystem.Flourish.Blazor.Components.Primitives.MaskedInput>(0);
                            builder.AddAttribute(1, "ValueExpression", (Expression<Func<string?>>)(() => value));
                        }
                        else builder.OpenComponent<ArkheideSystem.Flourish.Blazor.Components.Primitives.StandaloneMaskedInput>(0);
                        builder.AddAttribute(2, "Value", value);
                        builder.AddAttribute(3, "Mask", "000-000");
                        builder.AddAttribute(4, "AdditionalAttributes", native);
                        builder.CloseComponent();
                    };
                    var html = await Render<Field>(new()
                    {
                        ["Id"] = "reference", ["Label"] = "Reference", ["Required"] = true,
                        ["Error"] = "Invalid reference", ["ChildContent"] = child
                    });
                    var input = Regex.Match(html, "<input[^>]+>").Value;
                    Require(input.Contains("id=\"" + (explicitId ? "custom-reference" : "reference") + "\"")
                        && input.Contains("name=\"reference\"") && input.Contains("required")
                        && input.Contains("aria-invalid=\"true\"") && input.Contains("aria-describedby=\"reference-help reference-error\""),
                        "Masked input lost Field or explicitly supplied native semantics.");
                }
            }
        }));
        tests.Add(("bound text and numeric fields honor change-event mode without firing input callbacks", async () =>
        {
            string? text = "before";
            await WithComponent<TextBox>(new()
            {
                [nameof(TextBox.Value)] = text, [nameof(TextBox.ValueExpression)] = (Expression<Func<string?>>)(() => text),
                [nameof(TextBox.ValueChanged)] = EventCallback.Factory.Create<string?>(new object(), value => text = value),
                [nameof(TextBox.ChangeEvent)] = "change"
            }, async component =>
            {
                var input = Callback(component, "InputCallback");
                var change = Callback(component, "ChangeCallback");
                Require(!input.HasDelegate && change.HasDelegate, "Change mode still subscribes to input events.");
                await change.InvokeAsync(new ChangeEventArgs { Value = "after" });
                Require(text == "after", "Change callback did not preserve the original model binding.");
            });
            decimal number = 1;
            await WithComponent<NumberBox<decimal>>(new()
            {
                [nameof(NumberBox<decimal>.Value)] = number, [nameof(NumberBox<decimal>.ValueExpression)] = (Expression<Func<decimal>>)(() => number),
                [nameof(NumberBox<decimal>.ValueChanged)] = EventCallback.Factory.Create<decimal>(new object(), value => number = value),
                [nameof(NumberBox<decimal>.ChangeEvent)] = "change"
            }, async component =>
            {
                Require(!Callback(component, "InputCallback").HasDelegate, "Numeric change mode still subscribes to input.");
                await Callback(component, "ChangeCallback").InvokeAsync(new ChangeEventArgs { Value = "2.75" });
                Require(number == 2.75m, "Change callback parsed a numeric value incorrectly.");
            });
        }));
    }

    private static IReadOnlyDictionary<string, object> Attributes(params (string Key, object Value)[] values)
        => values.ToDictionary(value => value.Key, value => value.Value);
    private static EventCallback<ChangeEventArgs> Callback(IComponent component, string name)
        => (EventCallback<ChangeEventArgs>)(component.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(component)
            ?? throw new InvalidOperationException("Missing callback probe: " + name));
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
    private static async Task<string> Render<TComponent>(Dictionary<string, object?> parameters) where TComponent : IComponent
    {
        string result = "";
        await WithComponent<TComponent>(parameters, _ => Task.CompletedTask, html => result = html);
        return result;
    }
    private static async Task WithComponent<TComponent>(Dictionary<string, object?> parameters, Func<IComponent, Task> verify, Action<string>? inspect = null) where TComponent : IComponent
    {
        var activator = new CaptureActivator();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFlourishFramework();
        services.AddSingleton<NavigationManager>(new StaticNavigation());
        services.AddSingleton<IComponentActivator>(activator);
        services.AddSingleton<IJSRuntime>(new StaticJs());
        using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<TComponent>(ParameterView.FromDictionary(parameters));
            inspect?.Invoke(output.ToHtmlString());
            await verify(activator.Components.First(component => component.GetType() == typeof(TComponent)));
        });
    }
    private sealed class CaptureActivator : IComponentActivator
    {
        internal List<IComponent> Components { get; } = [];
        public IComponent CreateInstance(Type componentType)
        {
            var component = (IComponent)Activator.CreateInstance(componentType)!;
            Components.Add(component);
            return component;
        }
    }
    private sealed class PostModel { public string? Email { get; set; } public int Count { get; set; } public string Choice { get; set; } = ""; }
    private sealed class StaticNavigation : NavigationManager
    {
        public StaticNavigation() => Initialize("http://localhost/", "http://localhost/");
        protected override void NavigateToCore(string uri, bool forceLoad) => throw new InvalidOperationException("Unexpected navigation.");
    }
    private sealed class StaticJs : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => throw new InvalidOperationException("Static rendering unexpectedly invoked JavaScript.");
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => InvokeAsync<TValue>(identifier, args);
    }
}
