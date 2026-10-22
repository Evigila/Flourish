param()
$ErrorActionPreference = 'Stop'
$sourceRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
. (Join-Path $sourceRoot 'scripts/Release-Common.ps1')
$catalog = @(Get-Content -LiteralPath (Join-Path $sourceRoot 'src/Gallery.Flourish.Blazor/Models/ChangeLog.json') -Raw | ConvertFrom-Json)
$nextPrefix = $catalog[0].version -replace '-preview$', ''
$stablePrefix = @($catalog | Where-Object { $_.version -notlike '*-preview' })[0].version
$nextNumber = [version]$nextPrefix
$wrongPrefix = '{0}.{1}.{2}' -f $nextNumber.Major, $nextNumber.Minor, ($nextNumber.Build + 1)
$temporaryBase = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$fixtureRoot = Join-Path $temporaryBase ('flourish-preview-version-' + [Guid]::NewGuid().ToString('N'))
$checks = 0
function Assert([bool]$Condition, [string]$Message) {
    if (!$Condition) { throw $Message }
    $script:checks++
}
function Write-SourceVersion([string]$Prefix, [string]$Suffix = '') {
    $suffixElement = if ($Suffix) { '<VersionSuffix>' + [Security.SecurityElement]::Escape($Suffix) + '</VersionSuffix>' } else { '' }
    [IO.File]::WriteAllText((Join-Path $fixtureRoot 'Directory.Build.props'),
        '<Project><PropertyGroup><VersionPrefix>' + $Prefix + '</VersionPrefix>' + $suffixElement + '</PropertyGroup></Project>')
}
function Expect-Rejection([scriptblock]$Action, [string]$Message) {
    $rejected = $false
    try { & $Action | Out-Null } catch { $rejected = $true }
    Assert $rejected $Message
}
try {
    New-Item -ItemType Directory -Path $fixtureRoot | Out-Null
    $ReleaseRoot = $fixtureRoot
    $ReleaseSettings = @{ VersionProps = 'Directory.Build.props' }
    Write-SourceVersion $nextPrefix 'preview.portals.1'
    Assert ((Get-PackageVersion) -ceq ($nextPrefix + '-preview.portals.1')) 'Local package verification truncated the preview suffix.'
    Expect-Rejection { Get-ReleaseVersion } 'Stable release preparation accepted preview source.'
    Expect-Rejection { Assert-ReleaseTag ('v' + $nextPrefix) } 'Stable tag preparation accepted preview source.'
    Write-SourceVersion $stablePrefix
    Assert ((Get-PackageVersion) -ceq $stablePrefix) 'Stable package identity changed.'
    Assert ((Get-ReleaseVersion) -ceq $stablePrefix) 'Stable source no longer passes its existing version gate.'
    Write-SourceVersion $nextPrefix 'preview..1'
    Expect-Rejection { Get-PackageVersion } 'An empty prerelease identifier was accepted.'
    Write-SourceVersion $nextPrefix 'preview.01'
    Expect-Rejection { Get-PackageVersion } 'A noncanonical numeric prerelease identifier was accepted.'

    Write-SourceVersion $nextPrefix 'preview.portals.1'
    & (Join-Path $sourceRoot 'build/Test-ChangeLog.ps1') -VersionPropsPath (Join-Path $fixtureRoot 'Directory.Build.props') | Out-Null
    Assert $true 'Matching preview source should use the current next-version aggregate.'
    Write-SourceVersion $stablePrefix
    & (Join-Path $sourceRoot 'build/Test-ChangeLog.ps1') -VersionPropsPath (Join-Path $fixtureRoot 'Directory.Build.props') | Out-Null
    Assert $true 'Stable source should retain the next-version aggregate.'
    Write-SourceVersion $nextPrefix
    Expect-Rejection { & (Join-Path $sourceRoot 'build/Test-ChangeLog.ps1') -VersionPropsPath (Join-Path $fixtureRoot 'Directory.Build.props') } 'An unrecorded stable release was accepted.'
    Write-SourceVersion $wrongPrefix 'preview.portals.1'
    Expect-Rejection { & (Join-Path $sourceRoot 'build/Test-ChangeLog.ps1') -VersionPropsPath (Join-Path $fixtureRoot 'Directory.Build.props') } 'A source preview for the wrong aggregate version was accepted.'
    Write-SourceVersion $nextPrefix 'release.1'
    Expect-Rejection { & (Join-Path $sourceRoot 'build/Test-ChangeLog.ps1') -VersionPropsPath (Join-Path $fixtureRoot 'Directory.Build.props') } 'A non-preview suffix was mislabeled by the Gallery preview aggregate.'
    Write-Output "PASS preview package identity and stable release rejection: $checks checks."
} finally {
    $resolvedFixture = [IO.Path]::GetFullPath($fixtureRoot)
    if (!$resolvedFixture.StartsWith($temporaryBase.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -or
        [IO.Path]::GetFileName($resolvedFixture) -cnotmatch '^flourish-preview-version-[a-f0-9]{32}$') { throw 'Unexpected version fixture cleanup path.' }
    if (Test-Path -LiteralPath $resolvedFixture) { Remove-Item -LiteralPath $resolvedFixture -Recurse -Force }
}
