# Program-configured Blazor shell

The public quick-start path now treats the application shell as framework configuration rather than host-authored layout markup. `AddFlourishFramework` configures the top bar, explicit primary and secondary routes, command navigation, fixed navigation, layout and one scoped Core `ICommandParser`. The previous `AddFlourish` builder remains as an obsolete migration adapter for existing consumers.

Top-bar configuration supports application name, application-local logo, command menus and typed left/center/right component injection. Startup options retain component types rather than instances or render fragments; `DynamicComponent` creates them in the active scope. Navigation route entries and command buttons have distinct immutable models, and fixed entries render in a separate bottom region.

Framework now provides a scoped command runtime. The configured parser and its dependencies are created per Blazor scope, so Interactive Server circuits do not share registrations or mutable handler state. Menu commands use `CommandSource.TitleBar`; rail command buttons use `CommandSource.Navigation`.

`ApplicationLayout` creates the full shell and adds Framework CSS through `HeadContent`. Design registers an optional `IThemeProvider`; the same layout then adds Design CSS and applies scoped theme class/variables without a Framework-to-Design reference. `ThemeScope` remains available for independent nested previews.

Gallery now declares its logo, application name, page menu, primary/secondary navigation and fixed theme command in `Program.cs`. `GalleryCommandParser` owns menu navigation and theme toggling. The hand-authored `MainLayout` was removed, App no longer lists library stylesheets, and Routes selects the framework layout. Gallery remains a generic consumer with no product-specific library asset.

Verification: Gallery Debug build completed with zero warnings and errors. The console suite passed 51/51 checks, including scoped command isolation and static rendering of the configured logo, menu, secondary selection and fixed command region. Browser interaction remains covered by the manual checklist; Computer Use was not used.
