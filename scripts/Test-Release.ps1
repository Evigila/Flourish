param([switch]$VerifyOnly, [string]$ArtifactsPath, [string]$EssentialPackageDirectory)
. (Join-Path $PSScriptRoot 'Release-Common.ps1')
$version = Get-ReleaseVersion
if (!$ArtifactsPath) { $ArtifactsPath = Join-Path $ReleaseRoot 'artifacts/release-build' }
if (![IO.Path]::IsPathRooted($ArtifactsPath)) { $ArtifactsPath = Join-Path $ReleaseRoot $ArtifactsPath }
$ArtifactsPath = [IO.Path]::GetFullPath($ArtifactsPath)
if ($EssentialPackageDirectory) {
    $EssentialPackageDirectory = [IO.Path]::GetFullPath($EssentialPackageDirectory)
    if (!(Test-Path -LiteralPath $EssentialPackageDirectory -PathType Container)) { throw 'Essential package directory does not exist.' }
}
if ($VerifyOnly) { & (Join-Path $PSScriptRoot 'Verify-PackageSet.ps1') -Version $version; return }
Push-Location $ReleaseRoot
try {
    $packages = [IO.Path]::GetFullPath((Join-Path $ReleaseRoot 'artifacts/packages'))
    $expectedDirectory = [IO.Path]::GetFullPath((Join-Path $ReleaseRoot 'artifacts/packages'))
    if ($packages -ne $expectedDirectory -or !$packages.StartsWith($ReleaseRoot + [IO.Path]::DirectorySeparatorChar)) { throw 'Package directory escaped the repository.' }
    New-Item -ItemType Directory -Path $packages -Force | Out-Null
    Get-ChildItem -LiteralPath $packages -File | Where-Object { $_.Extension -in '.nupkg', '.snupkg' } | ForEach-Object { Remove-Item -LiteralPath $_.FullName }

    # Verify the production package set separately from the source-based Gallery.
    $restoreRoot = Join-Path $ArtifactsPath ('restore/' + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $restoreRoot -Force | Out-Null
    $nugetConfig = Join-Path $restoreRoot 'NuGet.Config'
    $sources = '<add key="Flourish" value="{0}" /><add key="nuget.org" value="https://api.nuget.org/v3/index.json" />' -f [Security.SecurityElement]::Escape($packages)
    $mappings = '<packageSource key="Flourish"><package pattern="Arkheide.Flourish.*" /></packageSource><packageSource key="nuget.org"><package pattern="*" /></packageSource>'
    if ($EssentialPackageDirectory) {
        $sources += '<add key="Essential" value="{0}" />' -f [Security.SecurityElement]::Escape($EssentialPackageDirectory)
        $mappings += '<packageSource key="Essential"><package pattern="Arkheide.Essential.Culture*" /></packageSource>'
    }
    $config = '<configuration><packageSources><clear />{0}</packageSources><packageSourceMapping><clear />{1}</packageSourceMapping></configuration>' -f $sources, $mappings
    [IO.File]::WriteAllText($nugetConfig, $config, [Text.UTF8Encoding]::new($false))
    $restoreArguments = @('--artifacts-path', $ArtifactsPath, '--configfile', $nugetConfig, '--packages', (Join-Path $restoreRoot 'cache'))
    $productionEntry = ($ReleaseSettings.Packages | Where-Object { $_.Id -eq 'Arkheide.Flourish.Blazor' }).Project
    if (!$productionEntry) { throw 'The Blazor production package entry is missing.' }
    Invoke-ReleaseCommand 'dotnet' (@('restore', $productionEntry) + $restoreArguments)
    Invoke-ReleaseCommand 'dotnet' @('build', $productionEntry, '-c', 'Release', '--artifacts-path', $ArtifactsPath, '--no-restore', '--no-incremental', '-p:TreatWarningsAsErrors=true', '-p:ContinuousIntegrationBuild=true')
    foreach ($package in $ReleaseSettings.Packages) {
        Invoke-ReleaseCommand 'dotnet' @('pack', $package.Project, '-c', 'Release', '--artifacts-path', $ArtifactsPath, '--output', $packages, '--no-build', '--no-restore', '-p:ContinuousIntegrationBuild=true')
    }
    & (Join-Path $PSScriptRoot 'Verify-PackageSet.ps1') -Version $version

    Invoke-ReleaseCommand 'dotnet' (@('restore', $ReleaseSettings.Solution) + $restoreArguments)
    $galleryAssets = Get-Content -LiteralPath (Join-Path $ArtifactsPath 'obj/Gallery.Flourish.Blazor/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
    foreach ($dependency in @(
        @{ Id = 'Arkheide.Flourish.Extensions.Culture.Blazor'; Version = $version; Type = 'project' }
        @{ Id = 'Arkheide.Flourish.Blazor.Framework'; Version = $version; Type = 'project' }
        @{ Id = 'Arkheide.Flourish.Blazor.Design'; Version = $version; Type = 'project' }
        @{ Id = 'Arkheide.Flourish.Blazor.Abstract'; Version = $version; Type = 'project' }
        @{ Id = 'Arkheide.Flourish.Core'; Version = $version; Type = 'project' }
        @{ Id = 'Arkheide.Essential.Culture.Blazor'; Version = $ReleaseSettings.EssentialVersion; Type = 'package' }
        @{ Id = 'Arkheide.Essential.Culture'; Version = $ReleaseSettings.EssentialVersion; Type = 'package' }
        @{ Id = 'Arkheide.Essential.Culture.Generator'; Version = $ReleaseSettings.EssentialVersion; Type = 'package' }
    )) {
        $key = $dependency.Id + '/' + $dependency.Version
        if (!$galleryAssets.libraries.ContainsKey($key) -or $galleryAssets.libraries[$key].type -ne $dependency.Type) { throw "Gallery must consume $key as $($dependency.Type)." }
    }
    Write-Host 'Verified Gallery source projects and transitive Essential package dependencies.'
    Invoke-ReleaseCommand 'dotnet' @('build', $ReleaseSettings.Solution, '-c', 'Release', '--artifacts-path', $ArtifactsPath, '--no-restore', '--no-incremental', '-p:TreatWarningsAsErrors=true', '-p:ContinuousIntegrationBuild=true')
    Invoke-ReleaseCommand 'dotnet' @('test', $ReleaseSettings.Solution, '-c', 'Release', '--artifacts-path', $ArtifactsPath, '--no-build', '--no-restore')
    foreach ($project in $ReleaseSettings.ConsoleTests) { Invoke-ReleaseCommand 'dotnet' @('run', '--project', $project, '-c', 'Release', '--artifacts-path', $ArtifactsPath, '--no-build', '--no-restore') }
    if ($ReleaseSettings.JavaScriptTests.Count) { Invoke-ReleaseCommand 'node' (@('--test') + $ReleaseSettings.JavaScriptTests) }
    foreach ($check in $ReleaseSettings.CheckScripts) { Invoke-ReleaseCommand 'pwsh' @('-NoLogo', '-NoProfile', '-File', $check) }
    $consumerArguments = @('-NoLogo', '-NoProfile', '-File', 'build/Test-BlazorPackageConsumers.ps1', '-Version', $version)
    if ($EssentialPackageDirectory) { $consumerArguments += @('-EssentialPackageDirectory', $EssentialPackageDirectory) }
    Invoke-ReleaseCommand 'pwsh' $consumerArguments
    Write-Host "Release preparation completed for v$version. No Git or publishing changes were made."
} finally { Pop-Location }
