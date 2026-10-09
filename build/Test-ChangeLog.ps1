param(
    [string] $CatalogPath,
    [string] $VersionPropsPath,
    [string] $CulturePath
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
if (!$CatalogPath) { $CatalogPath = Join-Path $root 'src/Gallery.Flourish.Blazor/Models/ChangeLog.json' }
if (!$VersionPropsPath) { $VersionPropsPath = Join-Path $root 'Directory.Build.props' }
if (!$CulturePath) { $CulturePath = Join-Path $root 'src/Gallery.Flourish.Blazor/Localization/Culture.json' }

function Read-Version([string] $Value) {
    if ($Value -cnotmatch '^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$') {
        throw "Invalid stable version '$Value'; use X.Y.Z without a tag prefix."
    }
    return [version]$Value
}

[xml]$props = Get-Content -LiteralPath $VersionPropsPath -Raw
$versionPrefix = [string]($props.Project.PropertyGroup | ForEach-Object { $_.VersionPrefix } | Where-Object { $_ } | Select-Object -First 1)
$releaseVersion = Read-Version $versionPrefix
$json = Get-Content -LiteralPath $CatalogPath -Raw -Encoding UTF8
if (!$json.TrimStart().StartsWith('[')) { throw 'ChangeLog must be a JSON array.' }
$parsed = $json | ConvertFrom-Json
$entries = @($parsed)
if (!$entries.Count) { throw 'ChangeLog must contain release records and one preview.' }
$culture = Get-Content -LiteralPath $CulturePath -Raw -Encoding UTF8 | ConvertFrom-Json
$versions = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$stableVersions = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$highestStable = $releaseVersion
$previous = $null
$preview = $null
$checks = 0

foreach ($entry in $entries) {
    if ($null -eq $entry -or $entry.version -isnot [string] -or $entry.version -cnotmatch '^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(-preview)?$') {
        throw 'Every ChangeLog record must have a canonical X.Y.Z or X.Y.Z-preview version.'
    }
    $name = $entry.version
    if (!$versions.Add($name)) { throw "Duplicate ChangeLog version '$name'." }
    $number = Read-Version ($name -replace '-preview$', '')
    if ($null -ne $previous -and $number -ge $previous) { throw 'ChangeLog versions must be strictly descending.' }
    $previous = $number
    if ($name.EndsWith('-preview')) {
        if ($null -ne $preview -or $name -cne $entries[0].version) { throw 'ChangeLog must have exactly one preview as its highest entry.' }
        $preview = $name
    } else {
        [void]$stableVersions.Add($name)
        if ($number -gt $highestStable) { $highestStable = $number }
    }
    if ($entry.changeKeys -isnot [array]) { throw "$name must provide a changeKeys array." }
    if (!$name.EndsWith('-preview') -and !$entry.changeKeys.Count) { throw "$name must contain release notes." }
    $keys = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($key in $entry.changeKeys) {
        if ($key -isnot [string] -or $key -cnotmatch '^Key\.ChangeLog_[A-Za-z0-9_]+$' -or !$keys.Add($key)) {
            throw "$name contains an invalid or duplicate ChangeLog translation key."
        }
        $translation = $culture.PSObject.Properties[$key.Substring(4)]
        if ($null -eq $translation) { throw "$name references missing translation '$key'." }
        foreach ($language in @('en-US', 'zh-CN', 'pt-BR')) {
            $text = $translation.Value.PSObject.Properties[$language]
            if ($null -eq $text -or $text.Value -isnot [string] -or [string]::IsNullOrWhiteSpace($text.Value)) {
                throw "$key must contain a nonempty $language translation."
            }
            $checks++
        }
    }
    $checks++
}

# Tags from later or unrelated commits must not invalidate a historical checkout.
$tags = @(& git -C $root tag --merged HEAD --list 'v*')
if ($LASTEXITCODE -ne 0) { throw 'Cannot read local release tags reachable from HEAD.' }
$stableTags = @($tags | Where-Object { $_ -cmatch '^v(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$' } | Sort-Object { [version]$_.Substring(1) })
$firstRelease = if ($stableTags.Count) { $stableTags[0].Substring(1) } else { $null }
foreach ($tag in $stableTags | Select-Object -Skip 1) {
    $version = $tag.Substring(1)
    if (!$stableVersions.Contains($version)) { throw "Release tag '$tag' has no ChangeLog record." }
    $number = Read-Version $version
    if ($number -gt $highestStable) { $highestStable = $number }
    $checks++
}
if ($versionPrefix -cne $firstRelease -and !$stableVersions.Contains($versionPrefix)) {
    throw "VersionPrefix '$versionPrefix' needs release notes before creating its tag."
}
$expectedPreview = '{0}.{1}.{2}-preview' -f $highestStable.Major, $highestStable.Minor, ($highestStable.Build + 1)
if ($preview -cne $expectedPreview) { throw "ChangeLog must contain exactly one highest preview '$expectedPreview'." }
Write-Output "PASS ChangeLog: $($entries.Count) versions, $checks checks, $expectedPreview."
