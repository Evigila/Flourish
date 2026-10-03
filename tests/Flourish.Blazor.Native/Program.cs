using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.NativeGallery.Components;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddFlourish(app => app.UseTitleBar(bar => bar.SetApplicationTitle("Native controls"))
    .UseNavigation(nav => nav.AddGroup("controls", "Controls", "grid", group => group.SetSecondaryNavigation(false).AddItem("Controls", "/", exact: true))));
var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets().ShortCircuit();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
