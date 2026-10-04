using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Gallery.Blazor.Commands;
using ArkheideSystem.Gallery.Blazor.Components;
using ArkheideSystem.Gallery.Blazor.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddFlourishFramework(framework =>
    framework
        .ConfigureTopBar(top =>
            top.SetAppName("Gallery")
                .SetIcon("gallery.svg", "")
                .AddMenu(
                    "页面",
                    menu =>
                        menu.AddMenuItem("演示记录", GalleryCommandParser.OpenRecords)
                            .AddMenuItem("公共控件", GalleryCommandParser.OpenControls)
                            .AddMenuItem("外观与接入", GalleryCommandParser.OpenAppearance)
                )
        )
        .ConfigureNavigation(navigation =>
            navigation
                .AddNav("概览", "home", "/", exact: true)
                .AddNav(
                    "登记",
                    "list",
                    "/records",
                    secondary =>
                        secondary
                            .AddSubNav("表单", "edit", "/forms")
                            .AddSubNav("记录", "user", "/records/sample")
                )
                .AddNav(
                    "组件",
                    "settings",
                    "/controls",
                    secondary =>
                        secondary
                            .AddSubNav("样式", "palette", "/appearance")
                            .AddSubNav("组合布局", "list", "/patterns")
                )
                .AddFixedNavButton("切换主题", "palette", GalleryCommandParser.ToggleTheme)
        )
        .SetCommandParser<GalleryCommandParser>()
);
builder.Services.AddFlourishDesign(appearance =>
    appearance.SetFont("'Noto Sans', 'Noto Sans CJK SC', system-ui, sans-serif")
);
builder.Services.AddScoped<RecordStore>();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStatusCodePagesWithReExecute("/not-found");
app.UseAntiforgery();
app.MapStaticAssets().ShortCircuit();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
