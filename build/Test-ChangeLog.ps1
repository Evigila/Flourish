param(
    [string] $CatalogPath,
    [string] $VersionPropsPath,
    [string[]] $CulturePath
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
. (Join-Path $root 'scripts/Release-Common.ps1')
$publishedVersionPattern = "$VersionPrefixPattern(?:-$ReleaseSuffixPattern)?"
if (!$CatalogPath) { $CatalogPath = Join-Path $root 'src/Gallery.Flourish.Blazor/Models/ChangeLog.json' }
if (!$VersionPropsPath) { $VersionPropsPath = Join-Path $root 'Directory.Build.props' }
if (!$CulturePath) { $CulturePath = @(Get-ChildItem -LiteralPath (Join-Path $root 'src/Gallery.Flourish.Blazor/Localization') -Filter 'Culture.*.json' -File | Sort-Object Name | ForEach-Object FullName) }
if (!$CulturePath.Count) { throw 'ChangeLog validation needs Gallery Culture modules.' }

$sourceVersion = Get-PackageVersion -VersionPropsPath $VersionPropsPath
$versionPrefix, $versionSuffix = $sourceVersion -split '-', 2
$isPreviewSource = $versionSuffix -cmatch '^preview(?:\.[0-9A-Za-z-]+)*$'
if (!$isPreviewSource -and $sourceVersion -cnotmatch "^$publishedVersionPattern$") {
    throw "Gallery source version '$sourceVersion' must use a public channel or a canonical preview suffix."
}
$json = Get-Content -LiteralPath $CatalogPath -Raw -Encoding UTF8
if (!$json.TrimStart().StartsWith('[')) { throw 'ChangeLog must be a JSON array.' }
$parsed = $json | ConvertFrom-Json
$entries = @($parsed)
if (!$entries.Count) { throw 'ChangeLog must contain release records and one preview.' }
$culture = [Collections.Generic.Dictionary[string,object]]::new([StringComparer]::Ordinal)
foreach ($file in $CulturePath) {
    $document = [System.Text.Json.JsonDocument]::Parse([IO.File]::ReadAllText($file))
    try {
        foreach ($key in $document.RootElement.EnumerateObject()) {
            if ($culture.ContainsKey($key.Name)) { throw "Duplicate Culture key '$($key.Name)' in '$file'." }
            $culture.Add($key.Name,($key.Value.GetRawText() | ConvertFrom-Json))
        }
    } finally { $document.Dispose() }
}
$versions = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$publishedVersions = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$highestPublished = if ($isPreviewSource) { [version]'0.0.0' } else { [version]$versionPrefix }
$previous = $null
$preview = $null
$checks = 0

foreach ($entry in $entries) {
    if ($null -eq $entry -or $entry.version -isnot [string] -or $entry.version -cnotmatch "^(?:$publishedVersionPattern|$VersionPrefixPattern-preview)$") {
        throw 'Every ChangeLog record must have a canonical published version or X.Y.Z-preview.'
    }
    $name = $entry.version
    if (!$versions.Add($name)) { throw "Duplicate ChangeLog version '$name'." }
    $number = [version](($name -split '-', 2)[0])
    if ($null -ne $previous -and $number -ge $previous) { throw 'ChangeLog versions must be strictly descending.' }
    $previous = $number
    if ($name.EndsWith('-preview')) {
        if ($null -ne $preview -or $name -cne $entries[0].version) { throw 'ChangeLog must have exactly one preview as its highest entry.' }
        $preview = $name
    } else {
        [void]$publishedVersions.Add($name)
        if ($number -gt $highestPublished) { $highestPublished = $number }
    }
    if ($entry.changeKeys -isnot [array]) { throw "$name must provide a changeKeys array." }
    if (!$name.EndsWith('-preview') -and !$entry.changeKeys.Count) { throw "$name must contain release notes." }
    $keys = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($key in $entry.changeKeys) {
        if ($key -isnot [string] -or $key -cnotmatch '^Key\.ChangeLog_[A-Za-z0-9_]+$' -or !$keys.Add($key)) {
            throw "$name contains an invalid or duplicate ChangeLog translation key."
        }
        $translation = $culture[$key.Substring(4)]
        if ($null -eq $translation) { throw "$name references missing translation '$key'." }
        foreach ($language in @('en-US', 'zh-CN', 'pt-BR')) {
            $text = $translation.PSObject.Properties[$language]
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
$publishedTags = @($tags | Where-Object { $_ -cmatch "^v$publishedVersionPattern$" })
$firstLegacyTag = $publishedTags | Where-Object { $_ -cmatch "^v$VersionPrefixPattern$" } |
    Sort-Object { [version]$_.Substring(1) } | Select-Object -First 1
foreach ($tag in $publishedTags | Where-Object { $_ -cne $firstLegacyTag }) {
    $version = $tag.Substring(1)
    if (!$publishedVersions.Contains($version)) { throw "Release tag '$tag' has no ChangeLog record." }
    $checks++
}
if (!$isPreviewSource -and "v$sourceVersion" -cne $firstLegacyTag -and !$publishedVersions.Contains($sourceVersion)) {
    throw "Version '$sourceVersion' needs release notes before creating its tag."
}
$expectedPreview = '{0}.{1}.{2}-preview' -f $highestPublished.Major, $highestPublished.Minor, ($highestPublished.Build + 1)
if ($preview -cne $expectedPreview) { throw "ChangeLog must contain exactly one highest preview '$expectedPreview'." }
if ($isPreviewSource -and ($versionPrefix + '-preview') -cne $expectedPreview) {
    throw "Source preview '$versionPrefix-$versionSuffix' must belong to aggregate '$expectedPreview'."
}
Write-Output "PASS ChangeLog: $($entries.Count) versions, $checks checks, $expectedPreview."
