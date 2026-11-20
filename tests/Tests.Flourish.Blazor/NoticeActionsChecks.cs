using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

internal static class NoticeActionsChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("floating notice forwards real inline actions without an empty body and retains warning semantics", async () =>
        {
            await using var fixture = new DialogFixture();
            var calls = 0;
            var output = await fixture.Render<FloatingNotice>(new()
            {
                [nameof(FloatingNotice.Title)] = "Register <company>",
                [nameof(FloatingNotice.Severity)] = NotificationSeverity.Warning,
                [nameof(FloatingNotice.Actions)] = (RenderFragment)(builder =>
                {
                    builder.OpenComponent<Button>(0);
                    builder.AddAttribute(1, nameof(Button.OnClick), EventCallback.Factory.Create<MouseEventArgs>(new object(), () => calls++));
                    builder.AddAttribute(2, nameof(Button.ChildContent), (RenderFragment)(content => content.AddContent(0, "Register & continue")));
                    builder.CloseComponent();
                }),
                [nameof(FloatingNotice.AdditionalAttributes)] = new Dictionary<string, object> { ["id"] = "registration-warning" }
            });
            var html = await fixture.Read(output);
            Require(html.Contains("f-notice-warning") && html.Contains("role=\"alert\"") && html.Contains("id=\"registration-warning\""), "The action slot lost notice semantics or host identity.");
            Require(html.Contains("f-notice-with-actions") && html.Contains("f-notice-content") && html.Contains("f-notice-actions"), "The message and actions were not rendered as separate row columns.");
            Require(!html.Contains("f-notice-body"), "A title-only action notice retained an empty body row.");
            Require(html.Contains("Register &lt;company&gt;") && html.Contains("Register &amp; continue"), "Text was not encoded.");
            var notice = fixture.Component<Notice>();
            Require(notice.Actions == fixture.Component<FloatingNotice>().Actions, "The floating wrapper did not forward the action fragment.");
            await fixture.Dispatch(() => fixture.Component<Button>().OnClick.InvokeAsync(new MouseEventArgs()));
            Require(calls == 1, "The action callback was replaced by the notification layer.");
            await fixture.Dispatch(() => fixture.Component<FloatingNotice>().SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(FloatingNotice.Actions)] = null,
                [nameof(FloatingNotice.ChildContent)] = (RenderFragment)(builder => builder.AddContent(0, "Updated message"))
            })));
            html = await fixture.Read(output);
            Require(!html.Contains("f-notice-with-actions") && !html.Contains("f-notice-actions") && html.Contains("f-notice-body"), "Removing actions retained their layout or hid ordinary body content.");
        }));

        tests.Add(("ordinary notice retains its stacked default and supports body plus independent link actions", async () =>
        {
            await using var fixture = new DialogFixture();
            var output = await fixture.Render<Notice>(new()
            {
                [nameof(Notice.Title)] = "Heading",
                [nameof(Notice.ChildContent)] = (RenderFragment)(builder => builder.AddContent(0, "Body"))
            });
            var html = await fixture.Read(output);
            Require(!html.Contains("f-notice-with-actions") && !html.Contains("f-notice-content"), "An existing caller was opted into the action layout.");
            await fixture.Dispatch(() => fixture.Component<Notice>().SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(Notice.Actions)] = (RenderFragment)(builder =>
                {
                    builder.OpenComponent<Button>(0);
                    builder.AddAttribute(1, nameof(Button.Href), "/companies/new/?from=notice&mode=create");
                    builder.AddAttribute(2, nameof(Button.ChildContent), (RenderFragment)(content => content.AddContent(0, "Register")));
                    builder.CloseComponent();
                })
            })));
            html = await fixture.Read(output);
            Require(html.IndexOf("f-notice-body", StringComparison.Ordinal) < html.IndexOf("f-notice-actions", StringComparison.Ordinal), "Actions were inserted inside the message body.");
            Require(html.Contains("href=\"/companies/new/?from=notice&amp;mode=create\""), "The standard link was not preserved.");
        }));
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
