# Optional Blazor design

Package: Arkheide.Flourish.Blazor.Design. Target: .NET 10 Razor Class Library.

Design references Framework, Abstract and Shared. It owns the visual skin, appearance palette, theme scope and scoped appearance runtime. Framework has no reverse dependency.

## Explicit opt-in

~~~csharp
using ArkheideSystem.Flourish.Blazor;
builder.Services.AddFlourish(app => app.UseTitleBar());
builder.Services.AddFlourishDesign(appearance => appearance
    .SetColors("#153A32", "#16745F")
    .SetTheme(ArkheideSystem.Flourish.Abstract.ApplicationTheme.Light)
    .SetFont("'Segoe UI', system-ui, sans-serif"));
~~~

Load both stylesheets in order:

~~~html
<link rel="stylesheet" href="_content/Arkheide.Flourish.Blazor.Framework/framework.css" />
<link rel="stylesheet" href="_content/Arkheide.Flourish.Blazor.Design/design.css" />
~~~

Wrap the shell or an interactive subtree:

~~~razor
<ThemeScope>
    <ApplicationShell>@ChildContent</ApplicationShell>
</ThemeScope>
~~~

An unconfigured ThemeScope reads the registered scoped IAppearanceService. Primary, Accent and Theme parameters override that scope explicitly. SetColors/SetTheme updates the scope without replacing host navigation or business state.

The skin owns 17px typography, colors, control/row sizes, spacing, radii, shadows, states and light/dark/system foundations. Font configuration selects an already available host font; it does not download dependencies. Generic framework portal behavior propagates the current inherited styles to menus/dialog fallbacks without depending on Design.

AddFlourishDesign does not silently register the shell. Its immutable configuration is singleton and mutable appearance state is scoped to the current circuit/client host. There is no implicit persistence.

Existing public namespaces for ThemeScope, AppearancePalette and appearance contracts are preserved. Package and assembly identity changed: use the new Design package/asset path.

The SDK-only build combines modular skin sources into one public design.css asset. It contains generic foundations, controls, surface layouts and theme roles. Application pages, routes, branding DTOs, business compositions and isolated page CSS remain in their host. No consumer-specific skin or compatibility entry is published.
