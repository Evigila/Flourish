# Blazor application shell and controls

A .NET 10 Razor Class Library extracted from the reusable presentation and interaction patterns of Colligere. Its API follows the existing WPF library's public-contract/internal-implementation approach. Web geometry comes from Colligere, not WPF. The library has no dependency on Colligere, authentication, Workspace contracts or business DTOs.

## Quick start

Reference `Flourish.Blazor.csproj` (or the locally packed `Arkheide.Flourish.Blazor` package together with its matching `Arkheide.Flourish.Core` dependency) from a .NET 10 Blazor Web App. Keep the ASP.NET host in your application:

```csharp
using ArkheideSystem.Flourish.Blazor;

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddFlourish(ui => ui
    .UseTitleBar(bar => bar.SetApplicationTitle("My application"))
    .UseNavigation(nav => nav.AddGroup("main", "Main", "home", group => group
        .AddItem("Home", "/", "home", exact: true)
        .AddItem("Records", "/records", "list"))));
```

Import in `_Imports.razor`:

```razor
@using ArkheideSystem.Flourish.Blazor.Components
@using ArkheideSystem.Flourish.Blazor.Abstract
```

Link the stylesheet once in your document head:

```html
<link rel="stylesheet" href="_content/Arkheide.Flourish.Blazor/flourish.css" />
```

Use `<body class="f-document">` when the full-height shell owns document scrolling. Configure `Router`/`RouteView` to use `ApplicationLayout` for an immediate shell, or make your own layout with one component:

```razor
@inherits LayoutComponentBase
<ApplicationShell>
    <ChildContent>@Body</ChildContent>
    <TitleBarEnd><!-- application-owned commands --></TitleBarEnd>
</ApplicationShell>
```

Use `TitleBarBrand` for a host-owned logo/glyph and `TitleBarStart` for a service menu. The shell owns their alignment and dividers. `Button IconOnly="true"` supplies a 48px target; always give it an accessible name. `ActionMenu.TriggerContent` supplies a text trigger while its default remains the record ellipsis.

A group can call `SetSecondaryNavigation(false)` for an overview module with only the primary rail. This also suppresses its mobile expansion trigger. `PageHeading Compact="true"` requests a compact heading independently of the normal 96/24px scroll hysteresis. `FactList` supplies one/two-column definition facts with responsive stacking; use one `div` per `dt`/`dd` pair.

The shell creates the top bar, primary/secondary navigation, content scroll track, mobile navigation and heading compaction. `UseTitleBar()` opts in; consumers do not assemble these regions from HTML borders. Keep the shell and pages inside the same interactive render boundary. The host Router may keep `FocusOnNavigate` with `Selector="h1"`: the library suppresses only the programmatically focused route title outline, while keyboard controls keep theirs. The stylesheet is a static web asset; no extra script tag is needed because components import their own modules. This first release is verified on Blazor Web App Interactive Server with prerendering. Other hosting modes require their own acceptance checks.

## Pages and forms

```razor
<PageBody>
    <PageHeading Title="Create record" ParentHref="/records" ParentLabel="Records" />
    <EditForm Model="draft" OnValidSubmit="Save" FormName="record">
        <DataAnnotationsValidator />
        <FormLayout>
            <Field Id="name" Label="Name" Required For="@(() => draft.Name)">
                <TextBox Id="name" @bind-Value="draft.Name" />
            </Field>
        </FormLayout>
        <FormActions>
            <Button Type="submit">Save</Button>
            <Button Variant="ButtonVariant.Secondary" OnClick="Cancel">Cancel</Button>
        </FormActions>
    </EditForm>
</PageBody>
```

Controls retain native HTML semantics and InputBase/EditContext validation. A Field's Id must match its input Id. The consumer owns the model, validation policy, save operation, access rules and any decision to discard changes. Supply `ApplicationShell.NavigationGroups` and `ApplicationTitle` for runtime application context; the host filters navigation according to its own access rules. An empty navigation collection renders no startup routes. Set `Dialog.CanClose` to veto close while dirty, and use native `NavigationLock` for page navigation; the Gallery demonstrates this combination.

NumberBox<TValue>, UniformGrid, ToggleSection, ProgressBar and ProgressRing provide numeric entry, equal cells, retained optional settings and known/unknown progress presentation.

## Local data lists

`DataTable<TItem>` accepts Items, Columns, row actions and optional OnRowOpen. It owns column-based local search, typed stable three-state sorting, 20-item pagination, list/card modes, column visibility and resizing. TableColumn delegates project real values; no domain field names are built in. Supply Culture and TableText for application-specific localization. Hidden searchable columns remain searchable. The table processes the complete supplied local Items collection: it does not pretend a server page is the complete dataset. Remote queries, authorization, virtualized worklists and bulk editing remain consumer concerns.

## Appearance and extension

Configure colors, theme, font stack and layout through ConfigureAppearance/ConfigureLayout. Inject scoped IAppearanceService for runtime SetColors/SetTheme. Core ApplicationTheme and NotificationSeverity are reused, but Core's desktop singleton registrations are not imported. Theme changes remain in the current circuit; there is no implicit localStorage or database persistence.

Default body/labels use 17 px Segoe UI and 400-weight labels, 48 px inputs with 12 px radius, 68/60 px title bars, 76/58 px primary navigation, 232 px secondary navigation and a 1180 px content baseline. The fluid business track uses one sixth of excess width as each gutter. Large FormActions reproduce the approved Colligere action-grid pattern. Foundations expose --f-* variables; components use f-* classes. ThemeScope scopes controls outside a full shell. Light, dark and system surface tokens are provided, with derived contrasting chrome/action ink and focus/accent text. A configured font stack uses fonts already supplied by the host; no remote font or icon library is loaded.

Public components are extensible through named content slots, typed parameters and callbacks. Builders, configuration and runtime implementation remain internal. Application-owned route labels and content are never embedded in styles. See the runnable Gallery and docs-ai/current/blazor-extraction.md for source correspondence, limits and the Colligere adoption sequence.

## Run and verify

```powershell
dotnet run --project src/Gallery.Blazor
dotnet run --project tests/Flourish.Blazor.Test
node --test tests/Flourish.Blazor.Test/tests.js
node tests/Flourish.Blazor.Test/controls-dom.mjs
dotnet pack src/Flourish.Blazor -c Release -p:Platform=AnyCPU
```

See docs-ai/current/blazor-manual-tests.md for browser acceptance. Gallery data is scoped, in-memory demonstration data and resets on a new circuit; it is not a production storage or authentication implementation.