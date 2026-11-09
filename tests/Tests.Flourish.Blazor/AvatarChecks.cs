using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class AvatarChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("avatar exposes one encoded accessible name and first last initials", async () =>
        {
            var html = await Render(new() { [nameof(Avatar.Name)] = "Renée <Évora>" });
            Require(html.Contains("role=\"img\"", StringComparison.Ordinal) && html.Contains("aria-label=\"Ren&#xE9;e &lt;&#xC9;vora&gt;\"", StringComparison.Ordinal), "Portrait lost its encoded full accessible name.");
            Require(html.Contains("R&lt;", StringComparison.Ordinal) && !html.Contains("<img", StringComparison.Ordinal), "Missing image did not use the first and last word initials.");
            Require(html.Contains("--f-avatar-size:64px", StringComparison.Ordinal), "The standard avatar is not 64px.");
        }));
        tests.Add(("avatar retains complete Unicode text elements in initials", async () =>
        {
            foreach (var (name, initials) in new[] { ("王 小明", "王小"), ("a\u0301 Zhou", "A\u0301Z"), ("👩🏽‍💻 Morgan", "👩🏽‍💻M"), ("Avery", "A") })
            {
                var html = WebUtility.HtmlDecode(await Render(new() { [nameof(Avatar.Name)] = name }));
                Require(html.Contains(">" + initials + "</span>", StringComparison.Ordinal), "A Unicode initial was split or lost.");
            }
        }));
        tests.Add(("avatar empty identity uses a named person placeholder and decorative mode removes repeat semantics", async () =>
        {
            var empty = await Render(new());
            Require(empty.Contains("data-icon=\"person\"", StringComparison.Ordinal) && empty.Contains("aria-label=\"Account\"", StringComparison.Ordinal), "Empty identity has no accessible fallback.");
            var decorative = await Render(new() { [nameof(Avatar.Name)] = "Alex Morgan", [nameof(Avatar.Decorative)] = true });
            Require(decorative.Contains("aria-hidden=\"true\"", StringComparison.Ordinal) && !decorative.Contains("aria-label=", StringComparison.Ordinal)
                && !decorative.Contains("role=\"img\"", StringComparison.Ordinal), "Decorative portrait still announces the person twice.");
        }));
        tests.Add(("avatar images preserve one wrapper name and exclude unsupported executable URL schemes", async () =>
        {
            foreach (var url in new[] { "https://example.test/photo.png", "/photos/person.png", "photos/person.jpg", "data:image/png;base64,AA==" })
            {
                var html = await Render(new() { [nameof(Avatar.Url)] = url, [nameof(Avatar.Name)] = "Alex Morgan", [nameof(Avatar.Size)] = 96 });
                Require(html.Contains("src=\"" + url + "\"", StringComparison.Ordinal) && html.Contains("alt=\"\"", StringComparison.Ordinal), "Supported image source is missing or repeats the wrapper announcement.");
                Require(html.Contains("--f-avatar-size:96px", StringComparison.Ordinal) && !html.Contains("f-avatar-initials", StringComparison.Ordinal), "A photo also rendered fallback initials.");
            }
            foreach (var url in new[] { "javascript:alert(1)", "vbscript:bad", "file:///private.png", "data:image/svg+xml,<svg/>", "https://example.test/\nphoto.png", "\\\\example.test\\photo.png" })
                Require(!(await Render(new() { [nameof(Avatar.Url)] = url, [nameof(Avatar.Name)] = "Alex Morgan" })).Contains("<img", StringComparison.Ordinal), "An unsupported URL was emitted as an image source.");
        }));
        tests.Add(("avatar validates size and retains supported host attributes", async () =>
        {
            foreach (var size in new[] { 16, 512 }) Require((await Render(new() { [nameof(Avatar.Size)] = size })).Contains($"--f-avatar-size:{size}px", StringComparison.Ordinal), "A valid size boundary was rejected.");
            foreach (var size in new[] { 0, 15, 513 })
            {
                try { await Render(new() { [nameof(Avatar.Size)] = size }); }
                catch (ArgumentOutOfRangeException error) when (error.ParamName == "Size") { continue; }
                throw new InvalidOperationException("An invalid avatar size was accepted.");
            }
            var html = await Render(new() { [nameof(Avatar.Class)] = "team-photo", [nameof(Avatar.AdditionalAttributes)] = new Dictionary<string, object> { ["data-profile"] = "person-7" } });
            Require(html.Contains("team-photo", StringComparison.Ordinal) && html.Contains("data-profile=\"person-7\"", StringComparison.Ordinal), "Host identity attributes or additional class were dropped.");
        }));
        tests.Add(("avatar image failure falls back and stale events cannot hide a changed URL", async () =>
        {
            var activator = new AvatarActivator();
            var services = Services(); services.AddSingleton<IComponentActivator>(activator);
            using var provider = services.BuildServiceProvider();
            await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
            await renderer.Dispatcher.InvokeAsync(async () =>
            {
                var parameters = new Dictionary<string, object?> { [nameof(Avatar.Name)] = "Alex Morgan", [nameof(Avatar.Url)] = "/one.png" };
                var root = await renderer.RenderComponentAsync<Avatar>(ParameterView.FromDictionary(parameters));
                var instance = activator.Instance!;
                var failed = typeof(Avatar).GetMethod("ImageFailed", BindingFlags.NonPublic | BindingFlags.Instance)!;
                failed.Invoke(instance, ["/one.png"]);
                await instance.SetParametersAsync(ParameterView.FromDictionary(parameters));
                Require(!root.ToHtmlString().Contains("<img", StringComparison.Ordinal) && root.ToHtmlString().Contains(">AM</span>", StringComparison.Ordinal), "Image failure did not render initials.");
                parameters[nameof(Avatar.Url)] = "/two.png";
                await instance.SetParametersAsync(ParameterView.FromDictionary(parameters));
                failed.Invoke(instance, ["/one.png"]);
                await instance.SetParametersAsync(ParameterView.FromDictionary(parameters));
                Require(root.ToHtmlString().Contains("src=\"/two.png\"", StringComparison.Ordinal), "A stale image event hid the new source.");
                parameters[nameof(Avatar.Url)] = "/one.png";
                await instance.SetParametersAsync(ParameterView.FromDictionary(parameters));
                Require(root.ToHtmlString().Contains("src=\"/one.png\"", StringComparison.Ordinal), "Returning to a prior source did not retry loading.");
            });
        }));
        tests.Add(("avatar renders a centered profile and opt-in native chooser with a shared help description", async () =>
        {
            var html = WebUtility.HtmlDecode(await Render(new()
            {
                [nameof(Avatar.Name)] = "Alex Morgan",
                [nameof(Avatar.Description)] = "Design & Engineering",
                [nameof(Avatar.HelpText)] = "JPEG up to 256 KiB",
                [nameof(Avatar.SaveAsync)] = (Func<string, Task<bool>>)(_ => Task.FromResult(true)),
                [nameof(Avatar.Disabled)] = true
            }));
            Require(html.Contains("f-avatar-name", StringComparison.Ordinal) && html.Contains(">Alex Morgan</strong>", StringComparison.Ordinal)
                && html.Contains(">Design & Engineering</span>", StringComparison.Ordinal), "The centered portrait lost its profile copy.");
            var picker = Regex.Match(html, "<input[^>]*type=\"file\"[^>]*>").Value;
            Require(picker.Contains("accept=\"image/jpeg,image/png,image/webp\"", StringComparison.Ordinal)
                && picker.Contains("disabled", StringComparison.Ordinal), "Native chooser bypassed the formats or disabled state.");
            var help = Regex.Match(picker, "aria-describedby=\"([^\"]+)\"").Groups[1].Value;
            Require(help.Length > 0 && html.Contains($"id=\"{help}\" role=\"tooltip\">JPEG up to 256 KiB", StringComparison.Ordinal),
                "Chooser help does not reference the visible tooltip text.");
            var readOnly = await Render(new() { [nameof(Avatar.Name)] = "Alex Morgan" });
            Require(!readOnly.Contains("type=\"file\"", StringComparison.Ordinal), "Read-only Avatar rendered an edit chooser.");
        }));
        tests.Add(("avatar reads JPEG via bounded JS stream and keeps a rejected crop open for retry", async () =>
        {
            var jpeg = new byte[64 * 1024];
            jpeg[0] = jpeg[^2] = 0xff;
            jpeg[1] = 0xd8;
            jpeg[^1] = 0xd9;
            var attempts = new List<string>();
            var activator = new AvatarActivator();
            var services = Services();
            services.AddSingleton<IComponentActivator>(activator);
            using var provider = services.BuildServiceProvider();
            await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
            await renderer.Dispatcher.InvokeAsync(async () =>
            {
                await renderer.RenderComponentAsync<Avatar>(ParameterView.FromDictionary(new Dictionary<string, object?>
                {
                    [nameof(Avatar.SaveAsync)] = (Func<string, Task<bool>>)(photo =>
                    {
                        attempts.Add(photo);
                        return Task.FromResult(attempts.Count > 1);
                    })
                }));
                var avatar = activator.Instance!;
                var module = new CropModule(jpeg);
                var type = typeof(Avatar);
                const BindingFlags members = BindingFlags.Instance | BindingFlags.NonPublic;
                var open = type.GetField("CropOpen", members)!;
                open.SetValue(avatar, true);
                type.GetField("CropReady", members)!.SetValue(avatar, true);
                type.GetField("Module", members)!.SetValue(avatar, module);
                var save = type.GetMethod("SaveCropAsync", members)!;

                await (Task)save.Invoke(avatar, null)!;
                Require((bool)open.GetValue(avatar)! && module.Releases == 0, "A rejected update discarded the open crop.");
                await (Task)save.Invoke(avatar, null)!;
                var expected = "data:image/jpeg;base64," + Convert.ToBase64String(jpeg);
                Require(attempts.SequenceEqual([expected, expected]) && !(bool)open.GetValue(avatar)! && module.Releases == 1,
                    "Retry lost the crop, altered JPEG bytes, or failed to close after saving.");
                Require(module.Streams.Count == 2 && module.Streams.All(stream => stream.Disposed && stream.MaxReadLength == 256 * 1024),
                    "The component did not use the bounded JS stream contract.");
            });
        }));

        tests.Add(("avatar composes inside a read only information grid value", async () =>
        {
            RenderFragment portrait = builder => { builder.OpenComponent<Avatar>(0); builder.AddAttribute(1, nameof(Avatar.Name), "Alex Morgan"); builder.CloseComponent(); };
            var services = Services();
            using var provider = services.BuildServiceProvider();
            await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
            var html = await renderer.Dispatcher.InvokeAsync(async () => (await renderer.RenderComponentAsync<UniformGridItem>(ParameterView.FromDictionary(new Dictionary<string, object?> {
                [nameof(UniformGridItem.Information)] = true, [nameof(UniformGridItem.Title)] = "Portrait", [nameof(UniformGridItem.ChildContent)] = portrait }))).ToHtmlString());
            Require(Regex.IsMatch(html, "<dd[^>]*>\\s*<div[^>]*class=\"f-avatar") && !Regex.IsMatch(html, "<(?:button|a)\\b"), "Portrait escaped the read-only grid value or became an action.");
        }));
    }
    private static ServiceCollection Services()
    {
        var services = new ServiceCollection(); services.AddLogging(); services.AddFlourishFramework();
        services.AddSingleton<NavigationManager>(new TestNavigation()); services.AddSingleton<IJSRuntime>(new FakeJs()); return services;
    }
    private static async Task<string> Render(Dictionary<string, object?> parameters)
    {
        using var provider = Services().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () => (await renderer.RenderComponentAsync<Avatar>(ParameterView.FromDictionary(parameters))).ToHtmlString());
    }
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    private sealed class CropModule(byte[] jpeg) : IJSObjectReference
    {
        internal int Releases { get; private set; }
        internal List<CropStream> Streams { get; } = [];
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (identifier == "release")
            {
                Releases++;
                return ValueTask.FromResult(default(TValue)!);
            }
            if (identifier != "encode" || typeof(TValue) != typeof(IJSStreamReference))
                throw new InvalidOperationException("Expected the crop encoder to return a stream.");
            var stream = new CropStream(jpeg);
            Streams.Add(stream);
            return ValueTask.FromResult((TValue)(object)stream);
        }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class CropStream(byte[] jpeg) : IJSStreamReference
    {
        public long Length => jpeg.Length;
        internal bool Disposed { get; private set; }
        internal long MaxReadLength { get; private set; }
        public ValueTask<Stream> OpenReadStreamAsync(long maxAllowedSize = 512_000, CancellationToken cancellationToken = default)
        {
            MaxReadLength = maxAllowedSize;
            return ValueTask.FromResult<Stream>(new MemoryStream(jpeg, writable: false));
        }
        public ValueTask DisposeAsync()
        {
            Disposed = true;
            return ValueTask.CompletedTask;
        }
    }


    private sealed class AvatarActivator : IComponentActivator
    {
        internal Avatar? Instance { get; private set; }
        public IComponent CreateInstance(Type componentType)
        {
            var instance = (IComponent)Activator.CreateInstance(componentType)!;
            if (instance is Avatar avatar) Instance = avatar;
            return instance;
        }
    }
}
