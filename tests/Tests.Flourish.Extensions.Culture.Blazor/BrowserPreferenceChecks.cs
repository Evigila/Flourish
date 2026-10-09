using ArkheideSystem.Flourish.Extensions.Culture.Blazor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using static ArkheideSystem.Tests.Flourish.Extensions.Culture.Blazor.Program;

namespace ArkheideSystem.Tests.Flourish.Extensions.Culture.Blazor;

internal static class BrowserPreferenceChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("Sessions defer imports until interaction and release unused sessions without JavaScript", async () =>
        {
            await using var root = Build(Services());
            await using var first = root.CreateAsyncScope();
            await using var second = root.CreateAsyncScope();
            var a = first.ServiceProvider.GetRequiredService<CultureSession>();
            var b = second.ServiceProvider.GetRequiredService<CultureSession>();
            var left = first.ServiceProvider.GetRequiredService<JsProbe>();
            var right = second.ServiceProvider.GetRequiredService<JsProbe>();
            Require(!ReferenceEquals(a, b), "Circuits shared a culture session.");
            Equal(0, left.Imports);
            Equal(0, right.Imports);
            Require(await a.SelectAsync("zh-CN"), "A valid preference was rejected.");
            Equal(1, left.Imports);
            Equal(365 * 86400, left.Module.Calls.Single().Arguments[3]);
            Equal(0, right.Imports);
            await b.DisposeAsync();
            Equal(0, right.Imports);
            Equal(0, right.Module.DisposeCalls);
        }));

        tests.Add(("Writes normalize independent cultures with application path and retention", async () =>
        {
            await using var root = Build(Services(culture => culture.SetRetentionDays(42), "https://example.test/gallery/"));
            await using var scope = root.CreateAsyncScope();
            var session = scope.ServiceProvider.GetRequiredService<CultureSession>();
            Require(await session.SelectAsync("ZH-cn", "PT-br"), "A normalized preference was rejected.");
            var javascript = scope.ServiceProvider.GetRequiredService<JsProbe>();
            Equal("./_content/Arkheide.Flourish.Extensions.Culture.Blazor/browser-preferences.js", javascript.ImportPaths.Single());
            var call = javascript.Module.Calls.Single();
            Equal("saveCulture", call.Identifier);
            Require(call.Arguments.SequenceEqual(new object?[] { "zh-CN", "pt-BR", "/gallery/", 42 * 86400 }), "The persisted pair, path or lifetime changed.");
            Equal("zh-CN", session.Culture);
            Equal("pt-BR", session.FormatCulture.Name);
        }));

        tests.Add(("Omitted format follows UI culture and repeated choices reuse one module", async () =>
        {
            await using var root = Build(Services());
            await using var scope = root.CreateAsyncScope();
            var session = scope.ServiceProvider.GetRequiredService<CultureSession>();
            Require(await session.SelectAsync("ZH-cn") && await session.SelectAsync("PT-br"), "A repeated supported choice was rejected.");
            var javascript = scope.ServiceProvider.GetRequiredService<JsProbe>();
            Equal(1, javascript.Imports);
            Equal(2, javascript.Module.Calls.Count);
            Equal("pt-BR", session.FormatCulture.Name);
            Require(javascript.Module.Calls.All(call => Equals(call.Arguments[0], call.Arguments[1]) && Equals(call.Arguments[2], "/")), "An omitted format used stale culture or a page path.");
        }));

        tests.Add(("Invalid or unsupported cultures fail before browser import", async () =>
        {
            await using var root = Build(Services());
            await using var scope = root.CreateAsyncScope();
            var session = scope.ServiceProvider.GetRequiredService<CultureSession>();
            foreach (var name in new[] { "", " \t", "invalid_culture!", "fr-FR" })
            {
                await ThrowsAsync<ArgumentException>(() => session.SelectAsync(name));
                await ThrowsAsync<ArgumentException>(() => session.SelectAsync("en-US", name));
                await ThrowsAsync<ArgumentException>(() => session.SelectFormatAsync(name));
            }
            var javascript = scope.ServiceProvider.GetRequiredService<JsProbe>();
            Equal(0, javascript.Imports);
            Equal(0, javascript.Module.Calls.Count);
        }));

        tests.Add(("Concurrent UI and format changes serialize persistence and apply the latest pair", async () =>
        {
            await using var root = Build(Services());
            await using var scope = root.CreateAsyncScope();
            var session = scope.ServiceProvider.GetRequiredService<CultureSession>();
            var javascript = scope.ServiceProvider.GetRequiredService<JsProbe>();
            javascript.Module.WriteGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
            var changes = new List<(string Ui, string Format)>();
            session.Changed += (_, _) => changes.Add((session.Culture, session.FormatCulture.Name));
            var first = session.SelectAsync("zh-CN");
            Task<bool> second;
            try
            {
                await javascript.Module.FirstWriteStarted.Task.WaitAsync(TimeSpan.FromSeconds(3));
                second = session.SelectFormatAsync("pt-BR");
                Equal("en-US", session.Culture);
                Equal(1, javascript.Imports);
                Equal(1, javascript.Module.Calls.Count);
                Require(!second.IsCompleted, "Formatting bypassed an unfinished UI transaction.");
            }
            finally { javascript.Module.WriteGate.TrySetResult(); }
            Require((await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(3))).All(value => value), "A queued transaction failed.");
            Equal(1, javascript.Module.MaxActiveWrites);
            Equal("zh-CN", session.Culture);
            Equal("pt-BR", session.FormatCulture.Name);
            Require(javascript.Module.Calls.Select(call => (call.Arguments[0], call.Arguments[1])).SequenceEqual(
                new (object?, object?)[] { ("zh-CN", "zh-CN"), ("zh-CN", "pt-BR") }), "A format transaction restored the UI read before the preceding save.");
            Require(changes.SequenceEqual(new[] { ("zh-CN", "zh-CN"), ("zh-CN", "pt-BR") }), "Memory and cookies applied in different orders.");
        }));

        tests.Add(("Blocked storage preserves memory and change events, then permits retry", async () =>
        {
            await using var root = Build(Services());
            await using var scope = root.CreateAsyncScope();
            var session = scope.ServiceProvider.GetRequiredService<CultureSession>();
            var javascript = scope.ServiceProvider.GetRequiredService<JsProbe>();
            var changes = 0;
            session.Changed += (_, _) => changes++;
            javascript.Module.WriteFailure = new JSException("The browser rejected its preference cookie.");
            Require(!await session.SelectAsync("zh-CN", "pt-BR"), "Storage failure reported success.");
            Equal("en-US", session.Culture);
            Equal("en-US", session.FormatCulture.Name);
            Equal(0, changes);
            javascript.Module.WriteFailure = null;
            Require(await session.SelectAsync("pt-BR"), "A failed write retained the gate.");
            Equal(1, javascript.Imports);
            Equal(2, javascript.Module.Calls.Count);
            Equal("pt-BR", session.Culture);
            Equal(1, changes);
        }));

        tests.Add(("Import failures and cancellations do not apply preferences and can be retried", async () =>
        {
            await using var root = Build(Services());
            await using var scope = root.CreateAsyncScope();
            var session = scope.ServiceProvider.GetRequiredService<CultureSession>();
            var javascript = scope.ServiceProvider.GetRequiredService<JsProbe>();
            foreach (var failure in new Exception[] { new JSException("The browser module is unavailable."),
                new JSDisconnectedException("The circuit disconnected during import."), new OperationCanceledException() })
            {
                javascript.ImportFailure = failure;
                Require(!await session.SelectAsync("zh-CN"), "A failed import reported success.");
                Equal("en-US", session.Culture);
                Equal(0, javascript.Module.Calls.Count);
                Equal(0, javascript.Module.DisposeCalls);
            }
            javascript.ImportFailure = null;
            javascript.Module.WriteFailure = new OperationCanceledException();
            Require(!await session.SelectAsync("pt-BR"), "A canceled write reported success.");
            Equal("en-US", session.Culture);
            javascript.Module.WriteFailure = null;
            Require(await session.SelectAsync("zh-CN"), "A failed import cached a failed module.");
            Equal(4, javascript.Imports);
            Equal("zh-CN", session.Culture);
        }));

        tests.Add(("Disconnection preserves selection and releases an imported module once", async () =>
        {
            await using var root = Build(Services());
            await using var scope = root.CreateAsyncScope();
            var session = scope.ServiceProvider.GetRequiredService<CultureSession>();
            var javascript = scope.ServiceProvider.GetRequiredService<JsProbe>();
            javascript.Module.WriteFailure = new JSDisconnectedException("The circuit is disconnected.");
            Require(!await session.SelectAsync("zh-CN"), "A disconnected circuit reported a persisted choice.");
            Equal("en-US", session.Culture);
            javascript.Module.DisposeFailure = new JSDisconnectedException("The circuit is disconnected.");
            await session.DisposeAsync();
            await session.DisposeAsync();
            Equal(1, javascript.Module.DisposeCalls);
            await ThrowsAsync<ObjectDisposedException>(() => session.SelectAsync("en-US"));
            Equal(1, javascript.Module.Calls.Count);
        }));

        tests.Add(("Disposal waits for an active transaction before releasing the module", async () =>
        {
            await using var root = Build(Services());
            await using var scope = root.CreateAsyncScope();
            var session = scope.ServiceProvider.GetRequiredService<CultureSession>();
            var javascript = scope.ServiceProvider.GetRequiredService<JsProbe>();
            javascript.Module.WriteGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
            var save = session.SelectAsync("zh-CN");
            Task disposal;
            try
            {
                await javascript.Module.FirstWriteStarted.Task.WaitAsync(TimeSpan.FromSeconds(3));
                disposal = session.DisposeAsync().AsTask();
                Require(!disposal.IsCompleted && javascript.Module.DisposeCalls == 0, "Disposal released an active module.");
            }
            finally { javascript.Module.WriteGate.TrySetResult(); }
            Require(await save, "Disposal interrupted an active transaction.");
            await disposal;
            Equal("zh-CN", session.Culture);
            Equal(1, javascript.Module.DisposeCalls);
        }));

        tests.Add(("Configured retention supports both documented endpoints", async () =>
        {
            foreach (var days in new[] { 1, 3650 })
            {
                await using var root = Build(Services(culture => culture.SetRetentionDays(days)));
                await using var scope = root.CreateAsyncScope();
                Require(await scope.ServiceProvider.GetRequiredService<CultureSession>().SelectAsync("zh-CN"), "A supported lifetime rejected a save.");
                Equal(days * 86400, scope.ServiceProvider.GetRequiredService<JsProbe>().Module.Calls.Single().Arguments[3]);
            }
        }));
    }
}
