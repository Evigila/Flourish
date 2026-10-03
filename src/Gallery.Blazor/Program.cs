using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Gallery.Blazor.Components;
using ArkheideSystem.Gallery.Blazor.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddFlourish(ui =>
    ui.UseTitleBar(bar => bar.SetApplicationTitle("Workspace"))
        .UseNavigation(nav =>
            nav.AddGroup(
                    "overview",
                    "概览",
                    "home",
                    group =>
                        group
                            .SetSecondaryNavigation(false)
                            .AddItem("概览", "/", "home", exact: true)
                )
                .AddGroup(
                    "workspace",
                    "登记",
                    "list",
                    group =>
                        group
                            .AddItem("列表", "/records", "list")
                            .AddItem("表单", "/forms", "edit")
                            .AddItem("记录", "/records/sample", "user")
                )
                .AddGroup(
                    "reference",
                    "组件",
                    "settings",
                    group =>
                        group
                            .AddItem("控件", "/controls", "grid")
                            .AddItem("样式", "/appearance", "palette")
                )
        )
);
builder.Services.AddScoped<RecordStore>();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStatusCodePagesWithReExecute("/not-found");
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
