# Optional Blazor design

Package: Arkheide.Flourish.Blazor.Design. Target: .NET 10 Razor Class Library.

Design references Framework, Abstract and Shared. It owns the visual skin, appearance palette, theme scopes and scoped appearance runtime. Framework has no reverse dependency.

## Explicit opt-in

~~~csharp
using ArkheideSystem.Flourish.Blazor;

builder.Services.AddFlourishFramework();
builder.Services.AddFlourishDesign(appearance => appearance
    .SetColors("#153A32", "#16745F")
    .SetTheme(ArkheideSystem.Flourish.Abstract.ApplicationTheme.Light)
    .SetFont("'Noto Sans', 'Noto Sans CJK SC', system-ui, sans-serif"));
~~~

`ApplicationLayout` detects the optional `IThemeProvider`, adds `design.css` after `framework.css`, and applies its class and variables to the shell. Consumers do not manually wrap the default shell in `ThemeScope` or maintain library stylesheet tags.

`ThemeScope` remains public for a nested preview or a deliberately independent subtree. Its Primary, Accent and Theme parameters override that scope. `IAppearanceService.SetColors` and `SetTheme` update the current circuit without replacing host navigation or business state.

The skin owns typography, colors, control and row sizes, spacing, radii, shadows, states and light/dark/system foundations. Font configuration selects an already available host font; it does not download a font or add a dependency. Noto Sans and Noto Sans CJK can be named in the stack when the host or device provides them.

`AddFlourishDesign` does not register the shell. Its immutable configuration is singleton and mutable appearance state is scoped to the current circuit/client host. There is no implicit persistence.

The SDK-only build combines modular skin sources into one public `design.css` asset. Application pages, routes, branding DTOs, business compositions and isolated page CSS remain in their host. No consumer-specific skin or compatibility entry is published.
