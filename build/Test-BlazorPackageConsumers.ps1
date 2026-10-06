param([string]$PackageDirectory, [string]$Version, [string]$EssentialPackageDirectory)
. (Join-Path $PSScriptRoot '../scripts/Release-Common.ps1')
if (!$Version) { $Version = Get-ReleaseVersion }
if (!$PackageDirectory) { $PackageDirectory = Join-Path $ReleaseRoot 'artifacts/packages' }
$fixture = Join-Path $ReleaseRoot ('artifacts/package-consumers/' + [Guid]::NewGuid().ToString('N'))
$cache = Join-Path $fixture 'cache'
if ($EssentialPackageDirectory) {
    $EssentialPackageDirectory = [System.IO.Path]::GetFullPath($EssentialPackageDirectory)
    if (!(Test-Path -LiteralPath $EssentialPackageDirectory -PathType Container)) {
        throw "The explicit Essential package directory does not exist: $EssentialPackageDirectory"
    }
}
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
if ($EssentialPackageDirectory) {
    $sources += '<add key="Essential" value="{0}" />' -f [System.Security.SecurityElement]::Escape($EssentialPackageDirectory)
}
$sourceMapping = '<packageSource key="Flourish"><package pattern="Arkheide.Flourish.*" /></packageSource><packageSource key="nuget.org"><package pattern="*" /></packageSource>'
if ($EssentialPackageDirectory) {
    $sourceMapping += '<packageSource key="Essential"><package pattern="Arkheide.Essential.Culture*" /></packageSource>'
}
$nugetConfig = Join-Path $fixture 'NuGet.Config'
Write-Fixture $nugetConfig ('<configuration><packageSources><clear />{0}</packageSources><packageSourceMapping><clear />{1}</packageSourceMapping></configuration>' -f $sources,$sourceMapping)
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
__CULTURE_OPT_IN_CHECK__
}
__CULTURE_CHECK__
app.MapStaticAssets();
app.MapGet("/", () => new RazorComponentResult<App>());
__CULTURE_ENDPOINT__
app.Run();
'@
$razor = @'
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Web
@using ArkheideSystem.Flourish.Blazor.Components
__CULTURE_RAZOR__
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
__CULTURE_CONTENT__
    };
    private sealed record SampleRow(string Name, decimal Amount);
    private static readonly IReadOnlyList<SampleRow> Rows = Enumerable.Range(1, 25)
        .Select(index => new SampleRow($"Row {index:000}", index + .5m)).ToArray();
    private static readonly IReadOnlyList<TableColumn<SampleRow>> Columns =
    [new("name", "Name", row => row.Name), new("amount", "Amount", row => row.Amount)];
}
'@
$cultureDocument = @'
{
  "Greeting": {
    "en-US": "Hello, package consumer",
    "zh-CN": "你好，包使用者",
    "pt-BR": "Olá, consumidor do pacote"
  },
  "Amount": {
    "en-US": "Amount: {0:N2}",
    "zh-CN": "金额：{0:N2}",
    "pt-BR": "Valor: {0:N2}"
  }
}
'@
$cultureRegistration = @'
using var frameworkCatalog = typeof(ArkheideSystem.Flourish.Blazor.Components.ApplicationShell).Assembly
    .GetManifestResourceStream("Flourish.Blazor.Texts.json")
    ?? throw new InvalidOperationException("The packaged framework catalog is missing.");
using var appCatalog = typeof(App).Assembly.GetManifestResourceStream("PackageConsumer.Culture.json")
    ?? throw new InvalidOperationException("The single consumer Culture.json is missing.");
builder.Services.AddCultureBlazor(options => options
    .AddCatalog("Flourish", LocalizationCatalog.Load(frameworkCatalog))
    .AddCatalog("App", LocalizationCatalog.Load(appCatalog))
    .SetDefaultCatalog("App")
    .SetDefaultCulture("en-US")
    .AddSupportedCultures("en-US", "zh-CN", "pt-BR")
    .InitializeWith(_ => new LocalizationSelection("en-US", "en-US")));
builder.Services.AddFlourishCulture();
'@
$cultureCheck = @'
using (var left = app.Services.CreateScope())
using (var right = app.Services.CreateScope())
{
    if (!string.Equals(TextKey.Greeting, "Key.Greeting", StringComparison.Ordinal)
        || !string.Equals(TextKey.Amount, "Key.Amount", StringComparison.Ordinal))
        throw new InvalidOperationException("Transitive key generation did not use the consumer Culture.json.");
    var leftLocalization = left.ServiceProvider.GetRequiredService<ILocalizationService>();
    var rightLocalization = right.ServiceProvider.GetRequiredService<ILocalizationService>();
    leftLocalization.SetCulture("zh-CN");
    var leftText = left.ServiceProvider.GetRequiredService<ITextProvider>();
    var rightText = right.ServiceProvider.GetRequiredService<ITextProvider>();
    var greeting = new TextReference("App", TextKey.Greeting);
    if (leftText.Get(greeting) != "你好，包使用者" || rightText.Get(greeting) != "Hello, package consumer")
        throw new InvalidOperationException("Packaged Culture bridge leaked scope or failed Chinese translation.");
    leftLocalization.SetCulture("pt-BR");
    if (leftText.Get(greeting) != "Olá, consumidor do pacote"
        || rightLocalization.Parse(TextKey.Greeting) != "Hello, package consumer"
        || leftText.Get(new TextReference("App", TextKey.Amount), 12345.67m) != "Valor: 12.345,67")
        throw new InvalidOperationException("Packaged Culture bridge failed Portuguese translation, formatting or isolation.");
}
'@
$cultureEndpoint = @'
app.MapGet("/culture/{culture}", (string culture, [Microsoft.AspNetCore.Mvc.FromServices] ILocalizationService localization) =>
{
    localization.SetCulture(culture);
    return new RazorComponentResult<App>();
});
'@
$cultureRazor = @'
@inherits ArkheideSystem.Essential.Culture.Blazor.LocalizedComponentBase
@using TextKey = PackageConsumer.Texts.Key
'@
$cultureContent = @'
        builder.OpenElement(16, "p");
        builder.AddAttribute(17, "id", "package-culture-greeting");
        builder.AddContent(18, Localization.Parse(TextKey.Greeting));
        builder.CloseElement();
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
    $generatorEnabled = $mode.Culture.ToString().ToLowerInvariant()
    $cultureItems = if ($mode.Culture) {
        '<None Remove="Culture.json" /><Content Remove="Culture.json" /><AdditionalFiles Include="Culture.json" /><EmbeddedResource Include="Culture.json" LogicalName="PackageConsumer.Culture.json" />'
    } else { '' }
    $xml = '<Project Sdk="Microsoft.NET.Sdk.Web"><PropertyGroup><TargetFramework>net10.0</TargetFramework><RootNamespace>PackageConsumer</RootNamespace><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><TreatWarningsAsErrors>true</TreatWarningsAsErrors><EssentialCultureNamespace>PackageConsumer.Texts</EssentialCultureNamespace><EssentialCultureGeneratorEnabled>{1}</EssentialCultureGeneratorEnabled><EssentialCultureXamlFramework>none</EssentialCultureXamlFramework><EssentialCultureAutoCreate>false</EssentialCultureAutoCreate><EssentialCultureAutoInclude>false</EssentialCultureAutoInclude></PropertyGroup><ItemGroup>{0}</ItemGroup><ItemGroup>{2}</ItemGroup></Project>' -f $references,$generatorEnabled,$cultureItems
    Write-Fixture $project $xml
    if ($mode.Culture) { Write-Fixture (Join-Path $directory 'Culture.json') $cultureDocument }
    $usings = if ($mode.Culture) { 'using ArkheideSystem.Essential.Culture;' + [Environment]::NewLine + 'using ArkheideSystem.Essential.Culture.Blazor;' + [Environment]::NewLine + 'using TextKey = PackageConsumer.Texts.Key;' } elseif ($mode.Meta) { 'using ArkheideSystem.Essential.Culture.Blazor;' } else { '' }
    $design = if ($mode.Theme) { 'builder.Services.AddFlourishDesign();' } else { '' }
    $culture = if ($mode.Culture) { $cultureRegistration } else { '' }
    $test = if ($mode.Culture) { $cultureCheck } else { '' }
    Write-Fixture (Join-Path $directory 'Program.cs') ($program.Replace('__CULTURE_USINGS__', $usings).Replace('__DESIGN_REGISTRATION__', $design).Replace('__CULTURE_REGISTRATION__', $culture).Replace('__EXPECT_THEME__', $mode.Theme.ToString().ToLowerInvariant()).Replace('__CULTURE_CHECK__', $test).Replace('__CULTURE_ENDPOINT__', $(if ($mode.Culture) { $cultureEndpoint } else { '' })).Replace('__CULTURE_OPT_IN_CHECK__', $(if ($mode.Meta) { '    var hasCulture = scope.ServiceProvider.GetService<ILocalizationService>() is not null;' + [Environment]::NewLine + '    if (hasCulture != ' + $mode.Culture.ToString().ToLowerInvariant() + ') throw new InvalidOperationException("Culture activation does not match registration.");' } else { '' })))
    Write-Fixture (Join-Path $directory 'App.razor') ($razor.Replace('__CULTURE_RAZOR__', $(if ($mode.Culture) { $cultureRazor } else { '' })).Replace('__CULTURE_CONTENT__', $(if ($mode.Culture) { $cultureContent } else { '' })))
    $restoreArguments = @('restore', $project, '--packages', $cache, '--configfile', $nugetConfig)
    Invoke-ReleaseCommand dotnet $restoreArguments
    $assets = Get-Content -LiteralPath (Join-Path $directory 'obj/project.assets.json') -Raw | ConvertFrom-Json
    $libraries = @($assets.libraries.PSObject.Properties.Name)
    Require ($libraries -contains "Arkheide.Flourish.Core/$Version") "$($mode.Name) did not restore Core transitively."
    Require (!$assets.libraries.PSObject.Properties.Where({ $_.Value.type -eq 'project' }).Count) "$($mode.Name) has a source project dependency."
    Require (!$libraries.Where({ $_ -match '^Arkheide\.(Essential\.Culture|Flourish)(\.[^/]+)*\.Shared/' }).Count) "$($mode.Name) still depends on retired Shared."
    Require (($libraries -contains "Arkheide.Flourish.Blazor.Design/$Version") -eq $mode.Meta) "$($mode.Name) restored the wrong Design package set."
    Require (($libraries -contains "Arkheide.Flourish.Extensions.Culture.Blazor/$Version") -eq $mode.Meta) "$($mode.Name) restored the wrong Culture bridge package set."
    $directReferences = @(([xml]$xml).SelectNodes('/Project/ItemGroup/PackageReference') | ForEach-Object { $_.Include })
    $expectedReferences = @($id)
    Require (((($directReferences | Sort-Object) -join ',') -ceq (($expectedReferences | Sort-Object) -join ','))) "$($mode.Name) has an unexpected direct package reference."
    $essentialLibraries = @($libraries.Where({ $_ -match '^Arkheide\.Essential\.Culture(/|\.)' }))
    Require ($essentialLibraries.Count -eq $(if ($mode.Meta) { 3 } else { 0 })) "$($mode.Name) restored an unexpected Essential Culture package set."
    foreach ($essentialId in @('Arkheide.Essential.Culture', 'Arkheide.Essential.Culture.Blazor', 'Arkheide.Essential.Culture.Generator')) {
        Require (($libraries -contains "$essentialId/$($ReleaseSettings.EssentialVersion)") -eq $mode.Meta) "$($mode.Name) restored the wrong transitive $essentialId package."
    }
    Require (!$libraries.Where({ $_ -match '^Arkheide\.(Essential\.Culture|Flourish)(\.[^/]+)*\.(Wpf|Avalonia|WinUI|WinUI3)/' }).Count) "$($mode.Name) unexpectedly restored a desktop UI package."
    if ($mode.Culture) {
        $generatedProps = Get-Content -LiteralPath (Join-Path $directory 'obj/Consumer.csproj.nuget.g.props') -Raw
        Require ($generatedProps -match 'buildTransitive.*Arkheide\.Essential\.Culture\.Generator\.props') 'MetaCulture did not import the transitive generator props.'
        Require (@(Get-ChildItem -LiteralPath $directory -Filter Culture.json -File -Recurse).Count -eq 1) 'MetaCulture did not keep a single Culture.json source.'
    }
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
        if ($mode.Culture) {
            $greetings = @{
                'en-US' = 'Hello, package consumer'
                'zh-CN' = '你好，包使用者'
                'pt-BR' = 'Olá, consumidor do pacote'
            }
            foreach ($language in @('en-US', 'zh-CN', 'pt-BR')) {
                $localizedPage = Invoke-WebRequest -Uri "$origin/culture/$language" -UseBasicParsing
                $localizedHtml = [System.Net.WebUtility]::HtmlDecode($localizedPage.Content)
                Require ($localizedPage.StatusCode -eq 200 -and $localizedHtml.Contains('id="package-culture-greeting"') -and $localizedHtml.Contains($greetings[$language])) "MetaCulture failed generated-key SSR for $language."
            }
            $defaultPage = Invoke-WebRequest -Uri "$origin/" -UseBasicParsing
            Require ([System.Net.WebUtility]::HtmlDecode($defaultPage.Content).Contains($greetings['en-US'])) 'MetaCulture leaked a translated request into another request.'
        }
        Write-Host "Verified $($mode.Name) package graph, registration, SSR and published assets."
    } finally {
        if (!$server.HasExited) { Stop-Process -Id $server.Id }
        $server.Dispose()
    }
}
Write-Host "$checks Blazor package consumer checks passed. Evidence: $fixture"
