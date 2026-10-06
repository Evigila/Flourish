using System.Linq.Expressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class ValidationMessagesChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("hidden and cross-field validation uses the Field renderer and updates on validation events", async () =>
        {
            var model = new Model(); var context = new EditContext(model);
            var messages = new ValidationMessageStore(context);
            using var provider = Services(); using var scope = provider.CreateScope();
            await using var renderer = new HtmlRenderer(scope.ServiceProvider, scope.ServiceProvider.GetRequiredService<ILoggerFactory>());
            RenderFragment body = builder =>
            {
                builder.OpenComponent<ValidationMessages>(0);
                builder.AddAttribute(1, "Id", "token-error");
                builder.AddAttribute(2, "For", (Expression<Func<object?>>)(() => model.Token));
                builder.CloseComponent();
            };
            var output = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<CascadingValue<EditContext>>(
                ParameterView.FromDictionary(new Dictionary<string, object?> { ["Value"] = context, ["ChildContent"] = body })));
            await renderer.Dispatcher.InvokeAsync(() => Require(!output.ToHtmlString().Contains("role=\"alert\""), "An empty error container was rendered."));
            await renderer.Dispatcher.InvokeAsync(() =>
            {
                messages.Add(new FieldIdentifier(model, nameof(Model.Token)), "Token <invalid>");
                context.NotifyValidationStateChanged();
            });
            await renderer.Dispatcher.InvokeAsync(() => Require(output.ToHtmlString().Contains("Token &lt;invalid&gt;")
                && output.ToHtmlString().Contains("id=\"token-error\"") && output.ToHtmlString().Contains("f-field-errors"), "A hidden field error was lost or rendered as HTML."));
            await renderer.Dispatcher.InvokeAsync(() => { messages.Clear(); context.NotifyValidationStateChanged(); });
            await renderer.Dispatcher.InvokeAsync(() => Require(!output.ToHtmlString().Contains("role=\"alert\""), "Cleared errors survived."));
        }));
        tests.Add(("Field and standalone messages use one production error renderer", async () =>
        {
            using var provider = Services(); using var scope = provider.CreateScope();
            await using var renderer = new HtmlRenderer(scope.ServiceProvider, scope.ServiceProvider.GetRequiredService<ILoggerFactory>());
            var html = await renderer.Dispatcher.InvokeAsync(async () => (await renderer.RenderComponentAsync<Field>(
                ParameterView.FromDictionary(new Dictionary<string, object?> { ["Id"] = "name", ["Label"] = "Name", ["Error"] = "Invalid & name" }))).ToHtmlString());
            Require(System.Text.RegularExpressions.Regex.Matches(html, "f-field-errors").Count == 1
                && html.Contains("id=\"name-error\"") && html.Contains("Invalid &amp; name"), "Field duplicated or detached its production validation renderer.");
        }));
        tests.Add(("validation component releases and switches EditContext subscriptions", async () =>
        {
            using var provider = Services(); using var scope = provider.CreateScope();
            var activator = scope.ServiceProvider.GetRequiredService<Capture>();
            await using var renderer = new HtmlRenderer(scope.ServiceProvider, scope.ServiceProvider.GetRequiredService<ILoggerFactory>());
            var firstModel = new Model(); var first = new EditContext(firstModel);
            var secondModel = new Model(); var second = new EditContext(secondModel);
            var output = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<SwitchingForm>(
                ParameterView.FromDictionary(new Dictionary<string, object?> { ["Context"] = first })));
            var parent = activator.Items.OfType<SwitchingForm>().Single();
            var component = activator.Items.OfType<ValidationMessages>().Single();
            // SetParametersAsync is the real lifecycle, not a substitute renderer.
            await renderer.Dispatcher.InvokeAsync(() => parent.SetParametersAsync(ParameterView.FromDictionary(
                new Dictionary<string, object?> { ["Context"] = second })));
            var oldMessages = new ValidationMessageStore(first); oldMessages.Add(new FieldIdentifier(firstModel, nameof(Model.Token)), "Old scope");
            var newMessages = new ValidationMessageStore(second); newMessages.Add(new FieldIdentifier(secondModel, nameof(Model.Token)), "New scope");
            await renderer.Dispatcher.InvokeAsync(() => { first.NotifyValidationStateChanged(); second.NotifyValidationStateChanged(); });
            await renderer.Dispatcher.InvokeAsync(() => Require(output.ToHtmlString().Contains("New scope") && !output.ToHtmlString().Contains("Old scope"), "Context changes retained another form's messages."));
            await renderer.Dispatcher.InvokeAsync(() => { component.Dispose(); second.NotifyValidationStateChanged(); });
        }));
    }
    private static ServiceProvider Services()
    {
        var services = new ServiceCollection().AddLogging().AddFlourishFramework();
        services.AddScoped<Capture>(); services.AddScoped<IComponentActivator>(provider => provider.GetRequiredService<Capture>());
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private sealed class Model { public string Token { get; set; } = ""; }
    private sealed class SwitchingForm : ComponentBase
    {
        [Parameter] public EditContext Context { get; set; } = null!;
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            builder.OpenComponent<CascadingValue<EditContext>>(0);
            builder.AddAttribute(1, "Value", Context);
            builder.AddAttribute(2, "ChildContent", (RenderFragment)(child =>
            {
                var model = (Model)Context.Model;
                child.OpenComponent<ValidationMessages>(0);
                child.AddAttribute(1, "For", (Expression<Func<object?>>)(() => model.Token));
                child.CloseComponent();
            }));
            builder.CloseComponent();
        }
    }
    private sealed class Capture : IComponentActivator
    {
        internal List<IComponent> Items { get; } = [];
        public IComponent CreateInstance(Type type) { var item = (IComponent)Activator.CreateInstance(type)!; Items.Add(item); return item; }
    }
}
