using System.Net;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;

internal static class DialogViewChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("browser controlled Dialog renders actual keyed views and native production actions without circuit interop", async () =>
        {
            await using var f = new DialogFixture();
            var html = await f.Read(await f.Render<Dialog>(new()
            {
                ["Id"] = "native-protocol-dialog", ["Title"] = "Connection <status>", ["BrowserControlled"] = true, ["Dismissible"] = false,
                ["Views"] = Views(), ["Actions"] = Actions(),
                ["AdditionalAttributes"] = new Dictionary<string, object> { ["data-nosnippet"] = true, ["data-native-protocol"] = "connection" }
            }));
            var dialog = f.Component<Dialog>();
            await f.AfterRender(dialog, true); await f.AfterRender(dialog);
            Require(f.Javascript.Imports == 0 && f.Javascript.Module.Calls.Count == 0, "Browser-owned state imported a circuit controller.");
            var root = Tag(html, "dialog", "native-protocol-dialog");
            Require(root.Contains("f-dialog") && !HasAttribute(root, "open") && root.Contains("data-f-dialog-browser-controlled=\"true\""), "Static protocol lost its closed native dialog.");
            Require(HasAttribute(root, "data-nosnippet") && root.Contains("data-native-protocol=\"connection\""), "Host protocol markers were dropped.");
            Require(!html.Contains("f-dialog-close") && !html.Contains("<status>") && html.Contains("Connection &lt;status&gt;"), "Non-dismissable header or encoded host text changed.");
            Require(f.Components.OfType<Notice>().Count() == 2 && f.Components.OfType<ProgressBar>().Count() == 1 && f.Components.OfType<Button>().Count() == 2,
                "Generic view composition cloned its production controls.");
            Require(f.Components.OfType<Button>().All(button => !button.OnClick.HasDelegate), "Native protocol actions gained circuit callbacks.");
            foreach (var id in new[] { "native-retry", "native-resume" })
            {
                var button = Tag(html, "button", id);
                Require(button.Contains("f-button") && button.Contains("type=\"button\"") && !HasAttribute(button, "disabled"), "Native action lost standard button behavior.");
            }
            Require(!HasAttribute(ViewTag(html, "pending"), "hidden") && HasAttribute(ViewTag(html, "failed"), "hidden"), "The first keyed view is not initially active.");
        }));
        tests.Add(("Dialog nested action rows retain native form transport and production Buttons", async () =>
        {
            foreach (var (nativeForm, nestedRow) in new[] { (false, true), (true, true), (true, false) })
            {
                await using var fixture = new DialogFixture();
                RenderFragment buttons = content =>
                {
                    content.OpenComponent<Button>(0);
                    content.AddAttribute(1, nameof(Button.Text), "Confirm");
                    content.AddAttribute(2, nameof(Button.Type), nativeForm ? "submit" : "button");
                    content.CloseComponent();
                    content.OpenComponent<Button>(3);
                    content.AddAttribute(4, nameof(Button.Text), "Cancel");
                    content.CloseComponent();
                };
                RenderFragment actions = builder =>
                {
                    if (nativeForm)
                    {
                        builder.OpenElement(0, "form");
                        builder.AddAttribute(1, "method", "post");
                        builder.AddAttribute(2, "action", "/native-confirm");
                    }
                    if (nestedRow)
                    {
                        builder.OpenComponent<InlineActions>(3);
                        builder.AddAttribute(4, nameof(InlineActions.ChildContent), buttons);
                        builder.CloseComponent();
                    }
                    else buttons(builder);
                    if (nativeForm) builder.CloseElement();
                };
                var html = await fixture.Read(await fixture.Render<Dialog>(new()
                {
                    [nameof(Dialog.Title)] = "Nested actions", [nameof(Dialog.BrowserControlled)] = true,
                    [nameof(Dialog.Actions)] = actions
                }));
                Require(html.Contains("<footer class=\"f-dialog-actions\">"), "The Dialog action footer was lost.");
                if (nestedRow) Require(Regex.IsMatch(html, @"<footer class=""f-dialog-actions"">[\s\S]*class=""f-inline-actions"" data-alignment=""end"""), "Dialog no longer contains the real shared action row with its trailing-edge default.");
                Require(fixture.Components.OfType<Button>().Count() == 2 && fixture.Components.OfType<Button>().All(button => !button.OnClick.HasDelegate), "Action layout changed native action ownership.");
                Require(!html.Contains("style="), "Nested actions require host geometry.");
                if (nativeForm) Require(html.Contains("method=\"post\" action=\"/native-confirm\"") && html.Contains("type=\"submit\""), "Dialog changed native form transport.");
            }
        }));
        tests.Add(("Dialog selected views encode multiline host content and retain independent dialog state", async () =>
        {
            await using var first = new DialogFixture(); await using var second = new DialogFixture();
            var a = await first.Render<Dialog>(new() { ["Title"] = "First", ["BrowserControlled"] = true, ["Views"] = Views(), ["View"] = "failed" });
            var b = await second.Render<Dialog>(new() { ["Title"] = "Second", ["BrowserControlled"] = true, ["Views"] = Views() });
            var html = await first.Read(a);
            Require(HasAttribute(ViewTag(html, "pending"), "hidden") && !HasAttribute(ViewTag(html, "failed"), "hidden"), "An explicit active key was ignored.");
            Require(!html.Contains("<unsafe>") && WebUtility.HtmlDecode(html).Contains("Try <unsafe> & later\nSecond line"), "Multiline semantic text became unsafe markup or was merged.");
            await first.Dispatch(() => first.Component<Dialog>().SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { ["View"] = "pending" })));
            Require(!HasAttribute(ViewTag(await first.Read(a), "pending"), "hidden") && !HasAttribute(ViewTag(await second.Read(b), "pending"), "hidden"), "Changing one dialog mutated another instance.");
        }));
        tests.Add(("Dialog views reject ambiguous keys and lifecycle owners", async () =>
        {
            foreach (var views in new IReadOnlyList<DialogView>[] { [new("", Text("empty"))], [new("same", Text("a")), new("same", Text("b"))] })
            {
                await using var invalid = new DialogFixture(); await RejectAsync<ArgumentException>(() => invalid.Render<Dialog>(new() { ["Title"] = "Invalid", ["Views"] = views }));
            }
            await using var unknown = new DialogFixture(); await RejectAsync<ArgumentException>(() => unknown.Render<Dialog>(new() { ["Title"] = "Unknown", ["Views"] = Views(), ["View"] = "absent" }));
            await using var mixed = new DialogFixture(); await RejectAsync<InvalidOperationException>(() => mixed.Render<Dialog>(new() { ["Title"] = "Mixed", ["BrowserControlled"] = true, ["IsOpen"] = true }));
            await using var changing = new DialogFixture(); await changing.Render<Dialog>(new() { ["Title"] = "Stable", ["BrowserControlled"] = true });
            await changing.Dispatch(() => RejectAsync<InvalidOperationException>(() => changing.Component<Dialog>().SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { ["BrowserControlled"] = false }))));
        }));
        tests.Add(("browser Dialog close defaults follow scoped culture while host title remains literal and disposal releases subscriptions", async () =>
        {
            var texts = new TrackingTextProvider(); var other = new TrackingTextProvider();
            var f = new DialogFixture(texts);
            try
            {
                var output = await f.Render<Dialog>(new() { ["Title"] = "Host literal <title>", ["BrowserControlled"] = true, ["Views"] = Views() });
                var subscribers = texts.Subscribers;
                Require(subscribers >= 2 && (await f.Read(output)).Contains("en-US:Flourish/Dialog_Close"), "The production text controls did not subscribe to their scope.");
                await Task.Run(() => texts.Select("pt-BR"));
                var html = await f.Read(output);
                Require(html.Contains("pt-BR:Flourish/Dialog_Close") && html.Contains("Host literal &lt;title&gt;") && other.Culture == "en-US" && texts.Subscribers == subscribers,
                    "Culture refresh changed literal content, leaked scopes or duplicated subscriptions.");
                await f.Dispatch(() => f.Component<Dialog>().SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { ["CloseLabel"] = "Literal close" })));
                await Task.Run(() => texts.Select("zh-CN"));
                Require((await f.Read(output)).Contains("aria-label=\"Literal close\""), "Explicit close text was treated as a default.");
            }
            finally { await f.DisposeAsync(); }
            Require(texts.Subscribers == 0, "Dialog composition retained culture subscriptions after disposal.");
        }));
    }
    private static IReadOnlyList<DialogView> Views() => [new("pending", builder =>
    {
        builder.OpenComponent<Notice>(0); builder.AddAttribute(1, "ChildContent", Text("Trying connection")); builder.CloseComponent();
        builder.OpenComponent<ProgressBar>(2);  builder.CloseComponent();
    }), new("failed", builder => { builder.OpenComponent<Notice>(0); builder.AddAttribute(1, "ChildContent", Text("Try <unsafe> & later\nSecond line")); builder.CloseComponent(); })];
    private static RenderFragment Actions() => builder =>
    {
        builder.OpenComponent<Button>(0); builder.AddAttribute(2, "Text", "Retry");
        builder.AddAttribute(3, "AdditionalAttributes", new Dictionary<string, object> { ["id"] = "native-retry", ["data-f-dialog-views"] = "failed" }); builder.CloseComponent();
        builder.OpenComponent<Button>(4); builder.AddAttribute(6, "Text", "Resume");
        builder.AddAttribute(7, "AdditionalAttributes", new Dictionary<string, object> { ["id"] = "native-resume", ["data-f-dialog-views"] = "failed paused" }); builder.CloseComponent();
    };
    private static RenderFragment Text(string text) => builder => builder.AddContent(0, text);
    private static string Tag(string html, string tag, string id) => Regex.Matches(html, $"<{tag}\\b[^>]*>").Select(match => match.Value).Single(value => value.Contains($"id=\"{id}\""));
    private static string ViewTag(string html, string key) => Regex.Matches(html, "<div\\b[^>]*>").Select(match => match.Value).Single(value => value.Contains($"data-f-dialog-view=\"{key}\""));
    private static bool HasAttribute(string tag, string name) => Regex.IsMatch(tag, $"(?:^|\\s){Regex.Escape(name)}(?:=|\\s|>)");
    private static async Task RejectAsync<T>(Func<Task> action) where T : Exception { try { await action(); } catch (T) { return; } throw new InvalidOperationException($"Expected {typeof(T).Name}."); }
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
}
