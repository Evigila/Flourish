using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.HtmlRendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;

internal sealed class DialogFixture : IAsyncDisposable
{
    private readonly ServiceProvider Provider;
    private readonly IServiceScope Scope;
    private readonly HtmlRenderer Renderer;
    private readonly CaptureActivator Activator = new();
    internal CountingDialogJs Javascript { get; } = new();
    internal List<IComponent> Components => Activator.Components;
    internal DialogFixture(ITextProvider? texts = null, NavigationManager? navigation = null)
    {
        var services = new ServiceCollection(); services.AddLogging(); services.AddFlourishFramework();
        services.AddSingleton<NavigationManager>(navigation ?? new TestNavigation());
        services.AddSingleton<IJSRuntime>(Javascript); services.AddSingleton<IComponentActivator>(Activator);
        if (texts is not null) services.AddScoped<ITextProvider>(_ => texts);
        Provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true }); Scope = Provider.CreateScope();
        Renderer = new(Scope.ServiceProvider, Scope.ServiceProvider.GetRequiredService<ILoggerFactory>());
    }
    internal Task<HtmlRootComponent> Render<T>(Dictionary<string, object?> parameters) where T : IComponent =>
        Renderer.Dispatcher.InvokeAsync(() => Renderer.RenderComponentAsync<T>(ParameterView.FromDictionary(parameters)));
    internal T Component<T>() where T : IComponent => Components.OfType<T>().Single();
    internal Task Dispatch(Func<Task> action) => Renderer.Dispatcher.InvokeAsync(action);
    internal Task<string> Read(HtmlRootComponent output) => Renderer.Dispatcher.InvokeAsync(output.ToHtmlString);
    internal Task AfterRender(ComponentBase component, bool first = false) => Dispatch(() =>
        (Task)component.GetType().GetMethod("OnAfterRenderAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(component, [first])!);
    public async ValueTask DisposeAsync() { await Renderer.DisposeAsync(); Scope.Dispose(); await Provider.DisposeAsync(); }
    private sealed class CaptureActivator : IComponentActivator
    {
        internal List<IComponent> Components { get; } = [];
        public IComponent CreateInstance(Type type) { var result = (IComponent)System.Activator.CreateInstance(type)!; Components.Add(result); return result; }
    }
    internal sealed class CountingDialogJs : IJSRuntime
    {
        internal int Imports { get; private set; }
        internal CountingDialogModule Module { get; } = new();
        public ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args) => InvokeAsync<T>(identifier, CancellationToken.None, args);
        public ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (identifier != "import") throw new InvalidOperationException("Unexpected browser call " + identifier);
            Imports++; return ValueTask.FromResult((T)(object)Module);
        }
    }
    internal sealed class CountingDialogModule : IJSObjectReference
    {
        internal List<(string Name, object?[]? Arguments)> Calls { get; } = [];
        public ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args) => InvokeAsync<T>(identifier, CancellationToken.None, args);
        public ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken cancellationToken, object?[]? args)
        { Calls.Add((identifier, args)); return ValueTask.FromResult(default(T)!); }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
