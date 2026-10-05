using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Tests.Flourish.Blazor.Native.Components;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddFlourishFramework(framework => framework
    .ConfigureProject(project => project.SetProjectName("Native controls"))
    .ConfigureTopBar(top => top.DisplayLogo().DisplayProjectName())
    .ConfigureNavigation(navigation => navigation.AddNav("Controls", "grid", "/", exact: true)));
var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets().ShortCircuit();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
