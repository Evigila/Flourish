Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$ReleaseRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$ReleaseSettings = Import-PowerShellDataFile (Join-Path $PSScriptRoot 'ReleaseSettings.psd1')
function Get-PackageVersion {
    [xml]$props = Get-Content -LiteralPath (Join-Path $ReleaseRoot $ReleaseSettings.VersionProps)
    $prefix = [string]($props.Project.PropertyGroup | ForEach-Object { $_.VersionPrefix } | Where-Object { $_ } | Select-Object -First 1)
    $suffix = [string]($props.SelectNodes('/Project/PropertyGroup/VersionSuffix') | ForEach-Object { $_.InnerText } | Where-Object { $_ } | Select-Object -First 1)
    if ($prefix -cnotmatch '^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$') { throw "VersionPrefix '$prefix' must be a canonical three-part version." }
    if ([string]::IsNullOrEmpty($suffix)) { return $prefix }
    if ($suffix -cnotmatch '^[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*$' -or
        @($suffix.Split('.') | Where-Object { $_ -cmatch '^0\d+$' }).Count) {
        throw "Invalid prerelease VersionSuffix '$suffix'."
    }
    return "$prefix-$suffix"
}
function Get-ReleaseVersion {
    $version = Get-PackageVersion
    if ($version.Contains('-')) {
        throw "Prerelease source '$version' cannot enter the stable release, tag or publication flow. Build and verify local preview packages explicitly."
    }
    return $version
}
function Invoke-ReleaseCommand([string]$File, [string[]]$Arguments) {
    & $File @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$File failed with exit code $LASTEXITCODE." }
}
function Assert-ReleaseTag([string]$Tag, [string]$Commit = 'HEAD') {
    $version = Get-ReleaseVersion
    if ($Tag -ne "v$version") { throw "Tag '$Tag' must match VersionPrefix as 'v$version'." }
    & git -C $ReleaseRoot merge-base --is-ancestor $Commit origin/master
    if ($LASTEXITCODE -ne 0) { throw "Release commit '$Commit' must be contained in origin/master." }
}
