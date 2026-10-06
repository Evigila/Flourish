param([string]$PackageDirectory, [string]$Version)
. (Join-Path $PSScriptRoot '../scripts/Release-Common.ps1')
if (!$Version) { $Version = Get-ReleaseVersion }
if (!$PackageDirectory) { $PackageDirectory = Join-Path $ReleaseRoot 'artifacts/packages' }
$fixture = Join-Path $ReleaseRoot ('artifacts/package-consumers/' + [Guid]::NewGuid().ToString('N'))
$cache = Join-Path $fixture 'cache'
$essentialFeed = [System.IO.Path]::GetFullPath((Join-Path $ReleaseRoot '../Essential/artifacts/packages'))
New-Item -ItemType Directory -Path $fixture -Force | Out-Null
$checks = 0
function Require([bool]$Condition, [string]$Message) {
    if (!$Condition) { throw $Message }
    $script:checks++
}
function Write-Fixture([string]$Path, [string]$Content) {
    [System.IO.File]::WriteAllText($Path, $Content, [System.Text.UTF8Encoding]::new($false))
}
Write-Fixture (Join-Path $fixture 'Directory.Build.props') '<Project />'
Write-Fixture (Join-Path $fixture 'Directory.Build.targets') '<Project />'
$sources = '<add key="Flourish" value="{0}" /><add key="nuget.org" value="https://api.nuget.org/v3/index.json" />' -f [System.Security.SecurityElement]::Escape($PackageDirectory)
if (Test-Path -LiteralPath $essentialFeed) {
    $sources += '<add key="Essential" value="{0}" />' -f [System.Security.SecurityElement]::Escape($essentialFeed)
}
$nugetConfig = Join-Path $fixture 'NuGet.Config'
Write-Fixture $nugetConfig ('<configuration><packageSources><clear />{0}</packageSources></configuration>' -f $sources)
$program = @'
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using Microsoft.AspNetCore.Http.HttpResults;
using PackageConsumer;
__CULTURE_USINGS__
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();
builder.Services.AddFlourishFramework();
__DESIGN_REGISTRATION__
__CULTURE_REGISTRATION__
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var hasTheme = scope.ServiceProvider.GetService<IThemeProvider>() is not null;
    if (hasTheme != __EXPECT_THEME__) throw new InvalidOperationException("Design activation does not match registration.");
}
__CULTURE_CHECK__
app.MapStaticAssets();
app.MapGet("/", () => new RazorComponentResult<App>());
app.Run();
'@
$razor = @'
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Web
@using ArkheideSystem.Flourish.Blazor.Components
<html>
<head><HeadOutlet /></head>
<body><ApplicationLayout Body="@Body" /></body>
</html>
@code {
    private RenderFragment Body => builder =>
    {
        builder.OpenComponent<Button>(0);
        builder.AddAttribute(1, "Text", "Package consumer");
        builder.CloseComponent();
        builder.OpenComponent<ListView<SampleRow>>(3);
        builder.AddAttribute(4, "Items", Rows);
        builder.AddAttribute(5, "Columns", Columns);
        builder.AddAttribute(6, "RowHeaderKey", "name");
        builder.AddAttribute(7, "Caption", "Read-only package rows");
        builder.CloseComponent();
        builder.OpenComponent<PresentationHero>(8);
        builder.AddAttribute(9, "Title", "Package display hero");
        builder.AddAttribute(10, "Subtitle", "Library display composition");
        builder.CloseComponent();
        builder.OpenComponent<OfferStage>(11);
        builder.AddAttribute(12, "Id", "package-offers");
        builder.AddAttribute(13, "ChildContent", (RenderFragment)(offers =>
        {
            offers.OpenComponent<OfferCard>(0);
            offers.AddAttribute(1, "Id", "package-offer");
            offers.AddAttribute(2, "Title", "Package offer");
            offers.AddAttribute(3, "Description", "Readable before enhancement");
            offers.CloseComponent();
        }));
        builder.CloseComponent();
        builder.OpenComponent<AccessPanel>(14);
        builder.AddAttribute(15, "ChildContent", (RenderFragment)(panel =>
        {
            panel.OpenComponent<AccessFormSurface>(0);
            panel.AddAttribute(1, "ChildContent", (RenderFragment)(form =>
            {
                form.OpenElement(0, "form");
                form.AddAttribute(1, "method", "post");
                form.AddAttribute(2, "action", "/native-example");
                form.AddAttribute(3, "data-enhance", "false");
                form.OpenComponent<Button>(4);
                form.AddAttribute(5, "Type", "submit");
                form.AddAttribute(6, "Text", "Native package action");
                form.CloseComponent();
                form.CloseElement();
            }));
            panel.CloseComponent();
        }));
        builder.CloseComponent();
    };
    private sealed record SampleRow(string Name, decimal Amount);
    private static readonly IReadOnlyList<SampleRow> Rows = Enumerable.Range(1, 25)
        .Select(index => new SampleRow($"Row {index:000}", index + .5m)).ToArray();
    private static readonly IReadOnlyList<TableColumn<SampleRow>> Columns =
    [new("name", "Name", row => row.Name), new("amount", "Amount", row => row.Amount)];
}
'@
$cultureRegistration = @'
using var frameworkCatalog = typeof(ArkheideSystem.Flourish.Blazor.Components.ApplicationShell).Assembly
    .GetManifestResourceStream("Flourish.Blazor.Texts.json")
    ?? throw new InvalidOperationException("The packaged framework catalog is missing.");
builder.Services.AddCultureBlazor(options => options
    .AddCatalog("Flourish", LocalizationCatalog.Load(frameworkCatalog))
    .AddCatalog("App", LocalizationCatalog.FromJson("""
        { "Greeting": { "en-US": "Hello", "zh-CN": "你好" } }
        """))
    .SetDefaultCatalog("App")
    .SetDefaultCulture("en-US")
    .AddSupportedCultures("en-US", "zh-CN")
    .InitializeWith(_ => new LocalizationSelection("en-US", "en-US")));
builder.Services.AddFlourishCulture();
'@
$cultureCheck = @'
using (var left = app.Services.CreateScope())
using (var right = app.Services.CreateScope())
{
    left.ServiceProvider.GetRequiredService<ArkheideSystem.Essential.Culture.Blazor.ILocalizationService>().SetCulture("zh-CN");
    var leftText = left.ServiceProvider.GetRequiredService<ITextProvider>();
    var rightText = right.ServiceProvider.GetRequiredService<ITextProvider>();
    var greeting = new TextReference("App", "Greeting");
    if (leftText.Get(greeting) != "你好" || rightText.Get(greeting) != "Hello")
        throw new InvalidOperationException("Packaged Culture bridge leaked scope or failed translation.");
}
'@
$modes = @(
    @{ Name='FrameworkOnly'; Meta=$false; Theme=$false; Culture=$false },
    @{ Name='MetaNative'; Meta=$true; Theme=$false; Culture=$false },
    @{ Name='MetaDesign'; Meta=$true; Theme=$true; Culture=$false },
    @{ Name='MetaCulture'; Meta=$true; Theme=$false; Culture=$true }
)
foreach ($mode in $modes) {
    $directory = Join-Path $fixture $mode.Name
    New-Item -ItemType Directory -Path $directory -Force | Out-Null
    $project = Join-Path $directory 'Consumer.csproj'
    $id = if ($mode.Meta) { 'Arkheide.Flourish.Blazor' } else { 'Arkheide.Flourish.Blazor.Framework' }
    $references = '<PackageReference Include="{0}" Version="{1}" />' -f $id,$Version
    if ($mode.Culture) { $references += '<PackageReference Include="Arkheide.Flourish.Extensions.Culture.Blazor" Version="{0}" />' -f $Version }
    $xml = '<Project Sdk="Microsoft.NET.Sdk.Web"><PropertyGroup><TargetFramework>net10.0</TargetFramework><RootNamespace>PackageConsumer</RootNamespace><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><TreatWarningsAsErrors>true</TreatWarningsAsErrors><EssentialCultureGeneratorEnabled>false</EssentialCultureGeneratorEnabled><EssentialCultureAutoCreate>false</EssentialCultureAutoCreate><EssentialCultureAutoInclude>false</EssentialCultureAutoInclude></PropertyGroup><ItemGroup>{0}</ItemGroup></Project>' -f $references
    Write-Fixture $project $xml
    $usings = if ($mode.Culture) { 'using ArkheideSystem.Essential.Culture;' + [Environment]::NewLine + 'using ArkheideSystem.Essential.Culture.Blazor;' } else { '' }
    $design = if ($mode.Theme) { 'builder.Services.AddFlourishDesign();' } else { '' }
    $culture = if ($mode.Culture) { $cultureRegistration } else { '' }
    $test = if ($mode.Culture) { $cultureCheck } else { '' }
    Write-Fixture (Join-Path $directory 'Program.cs') ($program.Replace('__CULTURE_USINGS__', $usings).Replace('__DESIGN_REGISTRATION__', $design).Replace('__CULTURE_REGISTRATION__', $culture).Replace('__EXPECT_THEME__', $mode.Theme.ToString().ToLowerInvariant()).Replace('__CULTURE_CHECK__', $test))
    Write-Fixture (Join-Path $directory 'App.razor') $razor
    $restoreArguments = @('restore', $project, '--packages', $cache, '--configfile', $nugetConfig)
    Invoke-ReleaseCommand dotnet $restoreArguments
    $assets = Get-Content -LiteralPath (Join-Path $directory 'obj/project.assets.json') -Raw | ConvertFrom-Json
    $libraries = @($assets.libraries.PSObject.Properties.Name)
    Require ($libraries -contains "Arkheide.Flourish.Core/$Version") "$($mode.Name) did not restore Core transitively."
    Require (!$assets.libraries.PSObject.Properties.Where({ $_.Value.type -eq 'project' }).Count) "$($mode.Name) has a source project dependency."
    Require (!$libraries.Where({ $_ -like '*Flourish.Blazor.Shared/*' }).Count) "$($mode.Name) still depends on retired Shared."
    Require (($libraries -contains "Arkheide.Flourish.Blazor.Design/$Version") -eq $mode.Meta) "$($mode.Name) restored the wrong Design package set."
    Require (($libraries -contains "Arkheide.Flourish.Extensions.Culture.Blazor/$Version") -eq $mode.Culture) "$($mode.Name) restored the wrong Culture bridge package set."
    $publish = Join-Path $directory 'publish'
    Invoke-ReleaseCommand dotnet @('publish', $project, '-c', 'Release', '--no-restore', '-o', $publish)
    $stdout = Join-Path $directory 'server.log'
    $stderr = Join-Path $directory 'server-error.log'
    $launch = @{
        FilePath=(Get-Command dotnet).Source; ArgumentList=@('Consumer.dll', '--urls', 'http://127.0.0.1:0')
        WorkingDirectory=$publish; RedirectStandardOutput=$stdout; RedirectStandardError=$stderr
        Environment=@{ ASPNETCORE_ENVIRONMENT='Production'; DOTNET_ENVIRONMENT='Production' }; PassThru=$true
    }
    if ($IsWindows) { $launch.WindowStyle='Hidden' }
    $server = Start-Process @launch
    try {
        $deadline = [DateTime]::UtcNow.AddSeconds(25)
        $origin = ''
        while (!$origin -and [DateTime]::UtcNow -lt $deadline) {
            if ($server.HasExited) { throw "$($mode.Name) exited; see $stderr." }
            if (Test-Path -LiteralPath $stdout) {
                $startup = Get-Content -LiteralPath $stdout -Raw
                if ($startup) {
                    $match = [regex]::Match($startup, 'Now listening on:\s*(http://127\.0\.0\.1:\d+)')
                    if ($match.Success) { $origin = $match.Groups[1].Value }
                }
            }
            if (!$origin) { Start-Sleep -Milliseconds 100 }
        }
        Require ([bool]$origin) "$($mode.Name) did not start; see $stdout."
        $page = Invoke-WebRequest -Uri "$origin/" -UseBasicParsing
        Require ($page.StatusCode -eq 200 -and $page.Content.Contains('Package consumer')) "$($mode.Name) failed SSR."
        Require ($page.Content.Contains('f-list-view') -and $page.Content.Contains('f-data-table') -and $page.Content.Contains('Read-only package rows')) "$($mode.Name) omitted the packaged static list."
        Require ([regex]::Matches($page.Content, '<th scope="row">').Count -eq 25 -and $page.Content.Contains('Row 025')) "$($mode.Name) paginated or omitted static list rows."
        Require (!$page.Content.Contains('data-f-table') -and !$page.Content.Contains('f-data-pager') -and !$page.Content.Contains('f-data-actions')) "$($mode.Name) introduced dynamic list behavior."
        Require ($page.Content.Contains('f-presentation-hero') -and $page.Content.Contains('Package display hero') -and $page.Content.Contains('f-content-container')) "$($mode.Name) omitted packaged display composition."
        Require ($page.Content.Contains('f-offer-stage') -and $page.Content.Contains('Readable before enhancement') -and !$page.Content.Contains('data-offer-ready')) "$($mode.Name) changed the readable pre-enhancement offer fallback."
        Require ($page.Content.Contains('f-access-form-surface') -and $page.Content.Contains('action="/native-example"') -and $page.Content.Contains('data-enhance="false"')) "$($mode.Name) altered the native access form boundary."
        Require ($page.Content.Contains('_content/Arkheide.Flourish.Blazor.Framework/framework.css')) "$($mode.Name) omitted functional Framework CSS."
        Require ($page.Content.Contains('_content/Arkheide.Flourish.Blazor.Design/design.css') -eq $mode.Theme) "$($mode.Name) changed Design activation."
        $assetPaths = @('framework.css','shell.js','controls.js','data.js','multi-select-box.js','primitives/editing-grid.js','presentation/offers.js','icons/material/MaterialSymbolsOutlined.woff2','icons/material/LICENSE.txt')
        foreach ($asset in $assetPaths) {
            $response = Invoke-WebRequest -Uri "$origin/_content/Arkheide.Flourish.Blazor.Framework/$asset" -UseBasicParsing
            Require ($response.StatusCode -eq 200 -and $response.RawContentLength -gt 0) "$($mode.Name) cannot serve Framework $asset."
        }
        if ($mode.Meta) {
            $response = Invoke-WebRequest -Uri "$origin/_content/Arkheide.Flourish.Blazor.Design/design.css" -UseBasicParsing
            Require ($response.StatusCode -eq 200) "$($mode.Name) cannot serve restored Design assets."
        }
        Write-Host "Verified $($mode.Name) package graph, registration, SSR and published assets."
    } finally {
        if (!$server.HasExited) { Stop-Process -Id $server.Id }
        $server.Dispose()
    }
}
Write-Host "$checks Blazor package consumer checks passed. Evidence: $fixture"
