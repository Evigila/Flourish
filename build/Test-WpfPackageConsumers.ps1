param(
    [string]$PackageDirectory,
    [string]$Version
)

$ErrorActionPreference = 'Stop'
if (-not $IsWindows -and $env:OS -ne 'Windows_NT') { throw 'Native WPF package consumers require Windows.' }
$taskRoot = Split-Path $PSScriptRoot -Parent
if (-not $PackageDirectory) { $PackageDirectory = Join-Path $taskRoot 'artifacts/wpf-packages' }
$PackageDirectory = (Resolve-Path -LiteralPath $PackageDirectory).Path
if (-not $Version) { $Version = ([xml](Get-Content (Join-Path $taskRoot 'Directory.Build.props') -Raw)).Project.PropertyGroup.VersionPrefix }
if ($Version -notmatch '^\d+\.\d+\.\d+(?:-[A-Za-z0-9.-]+)?$') { throw 'Expected a concrete package version.' }
$taskRun = Join-Path $taskRoot ('artifacts/wpf-package-consumers/' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $taskRun -Force | Out-Null
Set-Content (Join-Path $taskRun 'Directory.Build.props') '<Project />'
Set-Content (Join-Path $taskRun 'Directory.Build.targets') '<Project />'
$taskEscapedFeed = [System.Security.SecurityElement]::Escape($PackageDirectory)
Set-Content (Join-Path $taskRun 'NuGet.Config') "<configuration><packageSources><clear/><add key='native-candidates' value='$taskEscapedFeed'/></packageSources><fallbackPackageFolders><clear/></fallbackPackageFolders></configuration>"

$taskPrograms = [ordered]@{
    Abstract = @'
using ArkheideSystem.Flourish.WPF.Abstract;
var change = new MultiSelectChange(new[] { "first" }, new HashSet<string> { "first" });
if (!change.SelectedKeys.Contains("first") || new TextReference("Application", "Title", "Title").CatalogId != "Application") throw new Exception("Contract package failed.");
Console.WriteLine("Abstract-only native contracts passed.");
'@
    Framework = @'
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArkheideSystem.Flourish.WPF;
using F = ArkheideSystem.Flourish.WPF.Controls;
internal static class Program {
 [STAThread] static void Main() {
  var button = new F.Button { Text = "Native consumer", Icon = "home" };
  var host = new Border { Child = button }; FrameworkResources.Apply(host);
  host.Measure(new Size(480, 200)); host.Arrange(new Rect(0, 0, 480, 200)); host.UpdateLayout(); button.ApplyTemplate();
  if (button.Template is null || button.ActualHeight < 48 || !IconCatalog.Contains("home")) throw new Exception("Native templates or bundled icon names failed.");
  foreach (var filled in new[] { false, true }) {
   var icon = new F.Icon { Name = "home", Filled = filled, Foreground = Brushes.Black, Size = 26 };
   icon.Measure(new Size(32, 32)); icon.Arrange(new Rect(0, 0, 32, 32));
   var bitmap = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32); bitmap.Render(icon);
   var pixels = new byte[32 * 32 * 4]; bitmap.CopyPixels(pixels, 32 * 4, 0);
   if (!pixels.Any(value => value != 0)) throw new Exception("Bundled native outline/filled artwork did not render.");
  }
  if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "Flourish.WPF.Design")) throw new Exception("Framework-only consumer loaded optional Design.");
  Console.WriteLine("Framework-only native templates and icon passed; Design absent.");
 }
}
'@
    Design = @'
using System.Windows.Media;
using System.Windows;
using ArkheideSystem.Flourish.WPF;
internal static class Program {
 [STAThread] static void Main() {
  var resources = DesignResources.Load(ArkheideSystem.Flourish.WPF.Abstract.ThemeMode.Light);
  var brush = (SolidColorBrush)resources["Flourish.Brush.Primary"];
  var expected = SystemParameters.HighContrast ? SystemColors.HighlightColor : (Color)ColorConverter.ConvertFromString("#153A32");
  if (!brush.IsFrozen || brush.Color != expected) throw new Exception("Optional native Design palette failed.");
  if (ComponentUsageCatalog.Entries.Count != 75) throw new Exception("Native inventory missing.");
  Console.WriteLine("Design package and transitive Framework passed.");
 }
}
'@
    Aggregate = @'
using System.Windows;
using ArkheideSystem.Flourish.WPF;
internal static class Program {
 [STAThread] static void Main() {
  var scope = new System.Windows.Controls.Border(); FrameworkResources.Apply(scope);
  using var theme = DesignResources.Apply(scope, ArkheideSystem.Flourish.WPF.Abstract.ThemeMode.Dark);
  var configuration = new FrameworkBuilder().ConfigureProject(p => p.SetProjectName("Package consumer")).Build();
  if (configuration.ProjectName != "Package consumer" || scope.Resources.MergedDictionaries.Count != 2) throw new Exception("Aggregate did not supply three native layers.");
  Console.WriteLine("Dependency-only aggregate consumer passed.");
 }
}
'@
}
foreach ($taskEntry in $taskPrograms.GetEnumerator()) {
    $taskId = if ($taskEntry.Key -eq 'Aggregate') { 'Arkheide.Flourish.WPF' } else { 'Arkheide.Flourish.WPF.' + $taskEntry.Key }
    $taskPackage = Join-Path $PackageDirectory "$taskId.$Version.nupkg"
    if (-not (Test-Path -LiteralPath $taskPackage -PathType Leaf)) { throw "Missing native candidate $taskPackage" }
    $taskConsumer = Join-Path $taskRun $taskEntry.Key
    New-Item -ItemType Directory -Path $taskConsumer | Out-Null
    $taskProject = Join-Path $taskConsumer 'Consumer.csproj'
    Set-Content $taskProject "<Project Sdk='Microsoft.NET.Sdk'><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0-windows</TargetFramework><UseWPF>true</UseWPF><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup><ItemGroup><PackageReference Include='$taskId' Version='$Version'/></ItemGroup></Project>"
    Set-Content (Join-Path $taskConsumer 'Program.cs') $taskEntry.Value
    & dotnet restore $taskProject --configfile (Join-Path $taskRun 'NuGet.Config') --packages (Join-Path $taskRun 'packages') -p:NuGetAudit=false --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "Native $($taskEntry.Key) consumer restore failed." }
    & dotnet run --project $taskProject -c Release --no-restore -p:NuGetAudit=false -p:TreatWarningsAsErrors=true -nr:false
    if ($LASTEXITCODE -ne 0) { throw "Native $($taskEntry.Key) consumer failed." }
    $taskAssets = Get-Content (Join-Path $taskConsumer 'obj/project.assets.json') -Raw | ConvertFrom-Json
    $taskUnexpected = @($taskAssets.libraries.PSObject.Properties.Name | Where-Object { $_ -notlike 'Arkheide.Flourish.WPF*' })
    if ($taskUnexpected.Count) { throw "Native consumer restored unexpected packages: $($taskUnexpected -join ', ')." }
}
Write-Host "Four NuGet-only native consumers passed. Evidence: $taskRun"
