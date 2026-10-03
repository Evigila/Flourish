param(
    [string] $OutputDirectory = [System.IO.Path]::GetTempPath()
)

$ErrorActionPreference = 'Stop'
$fixture = Join-Path $OutputDirectory ("css-assets-" + [Guid]::NewGuid().ToString('N'))
$library = Join-Path $fixture 'Library'
$consumer = Join-Path $fixture 'ProjectConsumer'
$packageConsumer = Join-Path $fixture 'PackageConsumer'
$feed = Join-Path $fixture 'feed'
$packageId = 'Test.Generic.Assets.' + [Guid]::NewGuid().ToString('N')
$targets = [System.Security.SecurityElement]::Escape((Join-Path $PSScriptRoot 'CssBundle.targets'))
$cache = [System.Security.SecurityElement]::Escape((Join-Path $env:USERPROFILE '.nuget/packages'))
New-Item -ItemType Directory -Force -Path (Join-Path $library 'wwwroot/nested'),$consumer,$packageConsumer,$feed | Out-Null
$checks = 0

function Write-File([string] $Path, [string] $Text) {
    [System.IO.File]::WriteAllText($Path, $Text, [System.Text.UTF8Encoding]::new($false))
}
function Invoke-Sdk([string] $Name, [string[]] $Arguments) {
    $output = @(& dotnet @Arguments 2>&1)
    $exitCode = $LASTEXITCODE
    $text = ($output | ForEach-Object { "$_" }) -join [Environment]::NewLine
    Write-File (Join-Path $fixture ($Name + '.log')) $text
    if ($exitCode -ne 0) { throw "SDK fixture '$Name' failed. See $fixture." }
}
function Require([bool] $Condition, [string] $Message) {
    if (!$Condition) { throw $Message }
    $script:checks++
}

function Assert-PublishedCompression([string] $Directory, [string] $AssemblyName, [string] $Mode) {
    $manifest = Read-Json (Join-Path $Directory ($AssemblyName + '.staticwebassets.endpoints.json'))
    $cssEndpoints = @($manifest.Endpoints | Where-Object { $_.Route -match '/entry(\.[a-z0-9]+)?\.css$' })
    $routes = @($cssEndpoints | Select-Object -ExpandProperty Route -Unique)
    Require ($routes.Count -eq 2) "$Mode publish lost the stable or fingerprint CSS route."
    foreach ($route in $routes) {
        foreach ($encoding in @('gzip','br')) {
            $alternative = @($cssEndpoints | Where-Object { $_.Route -eq $route -and @($_.Selectors | Where-Object { $_.Name -eq 'Content-Encoding' -and $_.Value -eq $encoding }).Count -eq 1 })
            Require ($alternative.Count -eq 1) "$Mode publish has no $encoding negotiation endpoint for $route."
        }
    }

    $stdout = Join-Path $fixture ($Mode + '-http-server.log')
    $stderr = Join-Path $fixture ($Mode + '-http-error.log')
    $launch = @{
        FilePath = (Get-Command dotnet).Source
        ArgumentList = @(($AssemblyName + '.dll'), '--urls', 'http://127.0.0.1:0')
        WorkingDirectory = $Directory
        RedirectStandardOutput = $stdout
        RedirectStandardError = $stderr
        Environment = @{ ASPNETCORE_ENVIRONMENT='Production'; DOTNET_ENVIRONMENT='Production' }
        PassThru = $true
    }
    if ($IsWindows) { $launch.WindowStyle = 'Hidden' }
    $server = Start-Process @launch
    $handler = [System.Net.Http.HttpClientHandler]::new()
    $handler.AutomaticDecompression = [System.Net.DecompressionMethods]::None
    $client = [System.Net.Http.HttpClient]::new($handler)
    $client.Timeout = [TimeSpan]::FromSeconds(10)
    $evidence = @()
    try {
        $deadline = [DateTime]::UtcNow.AddSeconds(15)
        $origin = ''
        while (!$origin -and [DateTime]::UtcNow -lt $deadline) {
            if ($server.HasExited) { throw "$Mode fixture server exited; see $stderr." }
            if (Test-Path -LiteralPath $stdout) {
                $startupText = Get-Content -LiteralPath $stdout -Raw
                if ($startupText) {
                    $match = [regex]::Match($startupText, 'Now listening on:\s*(http://127\.0\.0\.1:\d+)')
                    if ($match.Success) { $origin = $match.Groups[1].Value }
                }
            }
            if (!$origin) { Start-Sleep -Milliseconds 100 }
        }
        if (!$origin) { throw "$Mode fixture server did not start; see $stdout." }
        foreach ($route in $routes) {
            $primary = $cssEndpoints | Where-Object { $_.Route -eq $route -and $_.Selectors.Count -eq 0 } | Select-Object -First 1
            $raw = [System.IO.File]::ReadAllBytes((Join-Path $Directory ('wwwroot/' + $primary.AssetFile)))
            foreach ($encoding in @('gzip','br')) {
                $request = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::Get, "$origin/$route")
                $request.Headers.AcceptEncoding.ParseAdd($encoding)
                $response = $client.SendAsync($request).GetAwaiter().GetResult()
                try {
                    $wire = $response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
                    $actualEncoding = $response.Content.Headers.ContentEncoding -join ','
                    Require ([int]$response.StatusCode -eq 200 -and $actualEncoding -eq $encoding) "$Mode failed HTTP $encoding negotiation for $route."
                    Require ($wire.Length -lt $raw.Length -and $response.Content.Headers.ContentLength -eq $wire.Length) "$Mode HTTP $encoding did not transfer fewer bytes."
                    $inputStream = [System.IO.MemoryStream]::new($wire)
                    $decoded = [System.IO.MemoryStream]::new()
                    $decoder = if ($encoding -eq 'gzip') {
                        [System.IO.Compression.GZipStream]::new($inputStream, [System.IO.Compression.CompressionMode]::Decompress)
                    } else {
                        [System.IO.Compression.BrotliStream]::new($inputStream, [System.IO.Compression.CompressionMode]::Decompress)
                    }
                    try {
                        $decoder.CopyTo($decoded)
                        Require ([Convert]::ToBase64String($decoded.ToArray()) -eq [Convert]::ToBase64String($raw)) "$Mode decoded $encoding bytes differ from the stylesheet."
                    } finally {
                        $decoder.Dispose()
                        $decoded.Dispose()
                        $inputStream.Dispose()
                    }
                    Require (($response.Headers.Vary -join ',').Contains('Accept-Encoding')) "$Mode lacks compression Vary metadata."
                    if ($route -match '/entry\.[a-z0-9]+\.css$') {
                        Require ($response.Headers.CacheControl.ToString().Contains('immutable')) "$Mode compressed fingerprint URL lacks immutable caching."
                    }
                    $evidence += [PSCustomObject]@{ Mode=$Mode; Route=$route; Encoding=$actualEncoding; RawBytes=$raw.Length; WireBytes=$wire.Length; DecodedEqual=$true }
                } finally {
                    $response.Dispose()
                    $request.Dispose()
                }
            }
        }
    } finally {
        $client.Dispose()
        $handler.Dispose()
        $server.Refresh()
        if (!$server.HasExited) { $server.Kill(); $server.WaitForExit() }
        $server.Dispose()
    }
    $evidence | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $fixture ($Mode + '-http-compression.json')) -Encoding utf8
}


function Assert-DevelopmentAssets([string] $Directory, [string] $AssemblyName, [string] $Mode) {
    $buildRoot = Join-Path $Directory 'bin/Debug/net10.0'
    $manifest = Read-Json (Join-Path $buildRoot ($AssemblyName + '.staticwebassets.endpoints.json'))
    $endpoints = @($manifest.Endpoints | Where-Object { $_.Route -match '/(entry(\.[a-z0-9]+)?\.css|behavior(\.[a-z0-9]+)?\.js)$' })
    Require (@($endpoints | Where-Object { @($_.Selectors | Where-Object { $_.Name -eq 'Content-Encoding' -and $_.Value -eq 'br' }).Count -gt 0 }).Count -eq 0) "$Mode development contains publish-only Brotli endpoints."
    $routes = @($endpoints | Select-Object -ExpandProperty Route -Unique)
    Require ($routes.Count -eq 4) "$Mode development lost CSS/JS stable or fingerprint aliases."
    $sourceAssets = (Read-Json (Join-Path $Directory 'obj/Debug/net10.0/staticwebassets.build.json')).Assets
    $stdout = Join-Path $fixture ($Mode + '-development-server.log')
    $stderr = Join-Path $fixture ($Mode + '-development-error.log')
    $quote = [char]34
    $assembly = Join-Path $buildRoot ($AssemblyName + '.dll')
    $launch = @{
        FilePath = (Get-Command dotnet).Source
        ArgumentList = @(($quote + $assembly + $quote), '--urls', 'http://127.0.0.1:0', '--contentRoot', ($quote + $Directory + $quote))
        WorkingDirectory = $Directory
        RedirectStandardOutput = $stdout
        RedirectStandardError = $stderr
        Environment = @{ ASPNETCORE_ENVIRONMENT='Development'; DOTNET_ENVIRONMENT='Development' }
        PassThru = $true
    }
    if ($IsWindows) { $launch.WindowStyle = 'Hidden' }
    $server = Start-Process @launch
    $handler = [System.Net.Http.HttpClientHandler]::new()
    $handler.AutomaticDecompression = [System.Net.DecompressionMethods]::None
    $client = [System.Net.Http.HttpClient]::new($handler)
    $client.Timeout = [TimeSpan]::FromSeconds(10)
    $evidence = @()
    try {
        $deadline = [DateTime]::UtcNow.AddSeconds(15)
        $origin = ''
        while (!$origin -and [DateTime]::UtcNow -lt $deadline) {
            if ($server.HasExited) { throw "$Mode development server exited; see $stderr." }
            if (Test-Path -LiteralPath $stdout) {
                $startupText = Get-Content -LiteralPath $stdout -Raw
                if ($startupText) {
                    $match = [regex]::Match($startupText, 'Now listening on:\s*(http://127\.0\.0\.1:\d+)')
                    if ($match.Success) { $origin = $match.Groups[1].Value }
                }
            }
            if (!$origin) { Start-Sleep -Milliseconds 100 }
        }
        if (!$origin) { throw "$Mode development server did not start; see $stdout." }
        foreach ($route in $routes) {
            $primary = $endpoints | Where-Object { $_.Route -eq $route -and $_.Selectors.Count -eq 0 } | Select-Object -First 1
            $leaf = ($primary.AssetFile -split '/')[-1]
            $asset = $sourceAssets | Where-Object { $_.SourceId -eq $packageId -and $_.AssetRole -eq 'Primary' -and [System.IO.Path]::GetFileName($_.Identity) -eq $leaf } | Select-Object -First 1
            $raw = [System.IO.File]::ReadAllBytes($asset.Identity)
            foreach ($accept in @('identity','gzip','br','gzip, deflate, br')) {
                $request = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::Get, "$origin/$route")
                $request.Headers.AcceptEncoding.ParseAdd($accept)
                $response = $client.SendAsync($request).GetAwaiter().GetResult()
                try {
                    $wire = $response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
                    $encoding = $response.Content.Headers.ContentEncoding -join ','
                    Require ([int]$response.StatusCode -eq 200) "$Mode failed development request for $route / $accept."
                    $mediaType = $response.Content.Headers.ContentType.MediaType
                    Require ($(if ($route.EndsWith('.js')) { $mediaType -eq 'text/javascript' } else { $mediaType -eq 'text/css' })) "$Mode returned an invalid module/style content type."
                    Require ($encoding -eq '' -or $encoding -eq 'gzip') "$Mode development returned unsupported $encoding for $route."
                    $decoded = $wire
                    if ($encoding -eq 'gzip') {
                        $inputStream = [System.IO.MemoryStream]::new($wire)
                        $outputStream = [System.IO.MemoryStream]::new()
                        $decoder = [System.IO.Compression.GZipStream]::new($inputStream, [System.IO.Compression.CompressionMode]::Decompress)
                        try { $decoder.CopyTo($outputStream); $decoded = $outputStream.ToArray() }
                        finally { $decoder.Dispose(); $outputStream.Dispose(); $inputStream.Dispose() }
                    }
                    Require ([Convert]::ToBase64String($decoded) -eq [Convert]::ToBase64String($raw)) "$Mode development decoded bytes differ from $leaf."
                    $evidence += [PSCustomObject]@{ Mode=$Mode; Route=$route; AcceptEncoding=$accept; ContentEncoding=$encoding; RawBytes=$raw.Length; WireBytes=$wire.Length; DecodedEqual=$true }
                } finally { $response.Dispose(); $request.Dispose() }
            }
        }
    } finally {
        $client.Dispose(); $handler.Dispose(); $server.Refresh()
        if (!$server.HasExited) { $server.Kill(); $server.WaitForExit() }
        $server.Dispose()
    }
    $evidence | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $fixture ($Mode + '-development-assets.json')) -Encoding utf8
}


function Read-Json([string] $Path) {
    Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
}
function Assert-Consumer([string] $Directory, [string] $Mode) {
    $manifest = Read-Json (Join-Path $Directory 'obj/Debug/net10.0/staticwebassets.build.json')
    $css = @($manifest.Assets | Where-Object { $_.SourceId -eq $packageId -and $_.AssetRole -eq 'Primary' -and $_.RelativePath -like '*.css' })
    Require ($css.Count -eq 1) "$Mode consumer publishes extra source CSS."
    Require ((Get-Content -LiteralPath $css[0].Identity -Raw).Contains('.first') -and !(Get-Content -LiteralPath $css[0].Identity -Raw).Contains('@import')) "$Mode consumer does not map the complete bundle."
    $endpoints = Read-Json (Join-Path $Directory 'obj/Debug/net10.0/staticwebassets.build.endpoints.json')
    Require (@($endpoints.Endpoints | Where-Object Route -Match '/entry\.[a-z0-9]+\.css$').Count -ge 1) "$Mode consumer has no versioned CSS endpoint."
    Require (@($endpoints.Endpoints | Where-Object Route -Like '*/entry.css').Count -ge 1) "$Mode consumer lost the stable CSS alias."
}

Write-File (Join-Path $library 'Library.csproj') (@'
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework><PackageId>%%PACKAGE%%</PackageId><Version>1.0.0</Version>
    <StaticWebAssetBasePath>_content/%%PACKAGE%%</StaticWebAssetBasePath><CssBundleEntry>entry.css</CssBundleEntry>
  </PropertyGroup>
  <ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App" /></ItemGroup>
  <Import Project="%%TARGETS%%" />
</Project>
'@.Replace('%%PACKAGE%%', $packageId).Replace('%%TARGETS%%', $targets))
Write-File (Join-Path $library 'wwwroot/entry.css') "@import 'nested/first.css'; .last { order: 2; }"
Write-File (Join-Path $library 'wwwroot/nested/first.css') ('.first { order: 1; }' + ('.shape { display: grid; padding: 16px; margin: 24px; color: CanvasText; background: Canvas; }' * 120))
Write-File (Join-Path $library 'wwwroot/behavior.js') 'export const value = 1;'
Write-File (Join-Path $consumer 'ProjectConsumer.csproj') @'
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
  <ItemGroup><ProjectReference Include="../Library/Library.csproj" /></ItemGroup>
</Project>
'@
$program = @'
using Microsoft.AspNetCore.Builder;
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapStaticAssets();
app.Run();
'@
Write-File (Join-Path $consumer 'Program.cs') $program
$config = Join-Path $fixture 'NuGet.Config'
Write-File $config (@'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources><clear /><add key="fixture" value="%%FEED%%" /><add key="cache" value="%%CACHE%%" /></packageSources>
</configuration>
'@.Replace('%%FEED%%', [System.Security.SecurityElement]::Escape($feed)).Replace('%%CACHE%%', $cache))
Invoke-Sdk 'project-build' @('build',(Join-Path $consumer 'ProjectConsumer.csproj'),'--configfile',$config,'--verbosity','minimal')
Assert-Consumer $consumer 'ProjectReference'
Assert-DevelopmentAssets $consumer 'ProjectConsumer' 'ProjectReference'
$manifest = Read-Json (Join-Path $library 'obj/Debug/net10.0/staticwebassets.build.json')
Require (@($manifest.DiscoveryPatterns).Count -eq 0) 'Development directory discovery can expose internal CSS.'
$css = @($manifest.Assets | Where-Object { $_.AssetRole -eq 'Primary' -and $_.RelativePath -like '*.css' })
Require ($css.Count -eq 1 -and $css[0].SourceType -eq 'Computed' -and $css[0].Identity.Contains('bundled-css')) 'CSS is served from source rather than generated output.'

Invoke-Sdk 'no-build-pack' @('pack',(Join-Path $library 'Library.csproj'),'--no-build','--no-restore','-c','Debug','-o',$feed,'--verbosity','minimal')
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead((Join-Path $feed ($packageId + '.1.0.0.nupkg')))
try {
    $cssEntries = @($zip.Entries | Where-Object FullName -Like '*.css')
    Require ($cssEntries.Count -eq 1 -and $cssEntries[0].FullName -eq 'staticwebassets/entry.css') 'NuGet publishes source CSS or omits the bundle.'
    Require (@($zip.Entries | Where-Object FullName -eq 'staticwebassets/behavior.js').Count -eq 1) 'NuGet omitted normal JS.'

    $compressedEntries = @($zip.Entries | Where-Object { $_.FullName -eq 'staticwebassets/entry.css.gz' -or $_.FullName -eq 'staticwebassets/entry.css.br' })
    Require ($compressedEntries.Count -eq 1 -and $compressedEntries[0].FullName -eq 'staticwebassets/entry.css.gz') 'NuGet must preserve gzip and leave Brotli to publish.'
} finally { $zip.Dispose() }
Invoke-Sdk 'project-no-build-publish' @('publish',(Join-Path $consumer 'ProjectConsumer.csproj'),'--no-build','--no-restore','-c','Debug','-o',(Join-Path $fixture 'project-publish'),'--verbosity','minimal')
Require (@(Get-ChildItem -LiteralPath (Join-Path $fixture 'project-publish/wwwroot') -Recurse -File | Where-Object Extension -eq '.css').Count -eq 1) 'Project publish leaked modular CSS.'

Assert-PublishedCompression (Join-Path $fixture 'project-publish') 'ProjectConsumer' 'ProjectReference'

Write-File (Join-Path $packageConsumer 'PackageConsumer.csproj') (@'
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
  <ItemGroup><PackageReference Include="%%PACKAGE%%" Version="1.0.0" /></ItemGroup>
</Project>
'@.Replace('%%PACKAGE%%', $packageId))
Write-File (Join-Path $packageConsumer 'Program.cs') $program
Invoke-Sdk 'package-build' @('build',(Join-Path $packageConsumer 'PackageConsumer.csproj'),'--configfile',$config,('-p:RestorePackagesPath='+(Join-Path $fixture 'package-cache')),'--verbosity','minimal')
Assert-Consumer $packageConsumer 'PackageReference'
Assert-DevelopmentAssets $packageConsumer 'PackageConsumer' 'PackageReference'
Invoke-Sdk 'package-no-build-publish' @('publish',(Join-Path $packageConsumer 'PackageConsumer.csproj'),'--no-build','--no-restore','-c','Debug','-o',(Join-Path $fixture 'package-publish'),'--verbosity','minimal')
Require (@(Get-ChildItem -LiteralPath (Join-Path $fixture 'package-publish/wwwroot') -Recurse -File | Where-Object Extension -eq '.css').Count -eq 1) 'Package publish leaked modular CSS.'

Assert-PublishedCompression (Join-Path $fixture 'package-publish') 'PackageConsumer' 'PackageReference'

$before = [System.IO.File]::ReadAllText((Join-Path $library 'obj/Debug/net10.0/bundled-css/entry.css'))
Write-File (Join-Path $library 'wwwroot/nested/first.css') '.first { order: 10; }'
Invoke-Sdk 'incremental-project-build' @('build',(Join-Path $consumer 'ProjectConsumer.csproj'),'--no-restore','--verbosity','minimal')
$after = [System.IO.File]::ReadAllText((Join-Path $library 'obj/Debug/net10.0/bundled-css/entry.css'))
Require ($after -ne $before -and $after.Contains('order: 10')) 'ProjectReference incremental build missed a CSS edit.'
Write-Output "$checks CSS SDK integration checks passed. Evidence: $fixture"
