using System.Reflection;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class ButtonStructuredLabelChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("ordinary buttons retain simple and icon-only labels without structured layout", async () =>
        {
            var plain = await Render(new() { ["Text"] = "Save", ["Type"] = "submit" });
            Require(plain.Contains("type=\"submit\"") && plain.Contains("f-button-text") && !plain.Contains("f-button-structured"), "Normal button composition changed.");
            var icon = await Render(new() { ["Icon"] = "add", ["AdditionalAttributes"] = new Dictionary<string, object> { ["aria-label"] = "Add" } });
            Require(icon.Contains("f-button-icon") && !icon.Contains("f-button-label"), "Icon-only presentation changed.");
        }));
        tests.Add(("structured labels are encoded passive spans within one native button", async () =>
        {
            var html = await Render(Structured());
            Require(Regex.Matches(html, "<button\\b").Count == 1 && Regex.Matches(html, "</button>").Count == 1
                && !Regex.IsMatch(html, "<(?:p|div|input|a)\\b"), "Structured content is not phrasing-only inside one native button.");
            Require(html.Contains("f-button-label") && html.Contains("f-button-description") && html.Contains("f-button-trailing")
                && html.Contains("Name &lt;safe&gt;") && html.Contains("mail&lt;safe&gt;@example.test") && html.Contains("Connected &lt;safe&gt;"), "Structured text is missing or not encoded.");
        }));
        tests.Add(("structured native actions and links preserve disabled busy and click transport", async () =>
        {
            foreach (var href in new[] { false, true })
            foreach (var state in new[] { "available", "disabled", "busy" })
            {
                var calls = 0;
                var parameters = Structured();
                parameters["Type"] = "submit";
                parameters["Disabled"] = state == "disabled";
                parameters["Busy"] = state == "busy";
                parameters["BusyLabel"] = "Processing";
                if (href) parameters["Href"] = "/target";
                parameters["OnClick"] = EventCallback.Factory.Create<MouseEventArgs>(new object(), () => calls++);
                var html = await Render(parameters, async (button, renderer) => await renderer.Dispatcher.InvokeAsync(async () =>
                    await (Task)typeof(Button).GetMethod("ClickAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(button, [new MouseEventArgs()])!));
                Require(calls == (state == "available" ? 1 : 0), "Unavailable structured action dispatched a callback.");
                Require(html.Contains("f-button-trailing") && html.Contains("aria-busy=\"" + (state == "busy" ? "true" : "false") + "\""), "Structured content or busy announcement disappeared.");
                if (href) Require(html.Contains("<a ") && (state == "available" ? html.Contains("href=\"/target\"") : !html.Contains("href=\"/target\"") && html.Contains("tabindex=\"-1\"")), "Structured navigation changed unavailable-link transport.");
                else Require(html.Contains("type=\"submit\"") && (state == "available" ? !html.Contains(" disabled") : html.Contains(" disabled")), "Structured button changed native type or disabled state.");
                if (state == "busy") Require(html.Contains("f-spinner") && html.Contains("Processing"), "Busy state lost its spinner or status text.");
            }
        }));
        tests.Add(("structured button requires a primary label and rejects mixed render fragments", async () =>
        {
            await Reject(new() { ["Description"] = "Secondary", ["Icon"] = "person" });
            var parameters = Structured();
            parameters["ChildContent"] = (RenderFragment)(builder => builder.AddContent(0, "Other label"));
            await Reject(parameters);
        }));
        tests.Add(("structured label layout is logical bounded and demonstrated by the real Gallery button", () =>
        {
            var framework = Read("src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/framework.css");
            Require(framework.Contains(".f-button.f-button-structured") && framework.Contains("width:100%; max-width:100%; min-width:0")
                && framework.Contains("flex-direction:column; align-items:flex-start; flex:1 1 0") && framework.Contains("margin-inline-start:auto; text-align:end")
                && framework.Contains("overflow-wrap:anywhere"), "Structured layout lacks bounded wrapping or logical alignment.");
            var sample = Read("src/Gallery.Flourish.Blazor/Components/Samples/Data/ButtonSample.razor");
            Require(sample.Contains("Description=\"") && sample.Contains("TrailingText=\"") && sample.Contains("OnClick=\"SelectAccount\""), "Gallery lacks an executable structured account action.");
            return Task.CompletedTask;
        }));
    }

    private static Dictionary<string, object?> Structured() => new()
    {
        ["Text"] = "Name <safe>", ["Description"] = "mail<safe>@example.test", ["TrailingText"] = "Connected <safe>"
    };

    private static async Task<string> Render(Dictionary<string, object?> parameters, Func<Button, HtmlRenderer, Task>? exercise = null)
    {
        var services = new ServiceCollection().AddLogging();
        services.AddFlourishFramework();
        services.AddSingleton<NavigationManager>(new TestNavigation());
        var capture = new Capture();
        services.AddSingleton<IComponentActivator>(capture);
        using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        var root = await renderer.Dispatcher.InvokeAsync(async () => await renderer.RenderComponentAsync<Button>(ParameterView.FromDictionary(parameters)));
        if (exercise is not null) await exercise(capture.Button!, renderer);
        return await renderer.Dispatcher.InvokeAsync(root.ToHtmlString);
    }

    private static async Task Reject(Dictionary<string, object?> parameters)
    {
        try { await Render(parameters); }
        catch (InvalidOperationException error) when (error.Message.Contains("structured button")) { return; }
        throw new InvalidOperationException("Invalid structured label was accepted.");
    }

    private sealed class Capture : IComponentActivator
    {
        internal Button? Button;
        public IComponent CreateInstance(Type type)
        {
            var component = (IComponent)Activator.CreateInstance(type)!;
            if (component is Button button) Button = button;
            return component;
        }
    }

    private static string Read(string path)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "src", "Flourish.Blazor"))) directory = directory.Parent;
        return File.ReadAllText(Path.Combine(directory?.FullName ?? throw new InvalidOperationException("Repository root not found."), path));
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
