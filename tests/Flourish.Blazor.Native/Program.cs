using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.NativeGallery.Components;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddFlourishFramework(framework => framework
    .ConfigureTopBar(top => top.SetAppName("Native controls"))
    .ConfigureNavigation(navigation => navigation.AddNav("Controls", "grid", "/", exact: true)));
var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets().ShortCircuit();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
