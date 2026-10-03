param(
    [string] $OutputDirectory = [System.IO.Path]::GetTempPath()
)

$ErrorActionPreference = 'Stop'
$fixture = Join-Path $OutputDirectory ("css-bundle-" + [Guid]::NewGuid().ToString('N'))
$source = Join-Path $fixture 'source'
$bundle = Join-Path $fixture 'output/bundle.css'
New-Item -ItemType Directory -Path $source -Force | Out-Null
$taskSource = [System.Security.SecurityElement]::Escape((Join-Path $PSScriptRoot 'BundleCss.cs'))
$project = @'
<Project>
  <UsingTask TaskName="BundleCss" TaskFactory="RoslynCodeTaskFactory" AssemblyFile="$(MSBuildToolsPath)/Microsoft.Build.Tasks.Core.dll">
    <Task><Code Type="Class" Language="cs" Source="%%TASKSOURCE%%" /></Task>
  </UsingTask>
  <Target Name="Check">
    <BundleCss SourceRoot="$(MSBuildThisFileDirectory)source" EntryFile="$(MSBuildThisFileDirectory)source/entry.css" OutputFile="$(MSBuildThisFileDirectory)output/bundle.css" />
  </Target>
</Project>
'@
[System.IO.File]::WriteAllText((Join-Path $fixture 'check.proj'), $project.Replace('%%TASKSOURCE%%', $taskSource))
$checks = 0

function Write-Fixture([string] $Name, [string] $Content) {
    $path = Join-Path $source $Name
    New-Item -ItemType Directory -Path ([System.IO.Path]::GetDirectoryName($path)) -Force | Out-Null
    [System.IO.File]::WriteAllText($path, $Content, [System.Text.UTF8Encoding]::new($false))
}
function Invoke-Bundle([string] $Name, [string] $ExpectedError = '') {
    $output = @(dotnet msbuild (Join-Path $fixture 'check.proj') /t:Check /nologo /v:minimal 2>&1)
    $exitCode = $LASTEXITCODE
    $text = ($output | ForEach-Object { "$_" }) -join [Environment]::NewLine
    [System.IO.File]::WriteAllText((Join-Path $fixture ($Name + '.log')), $text)
    if ($ExpectedError) {
        if ($exitCode -eq 0 -or !$text.Contains($ExpectedError)) {
            throw "Expected bundle rejection '$ExpectedError' for $Name; got exit $exitCode. See $fixture."
        }
    } elseif ($exitCode -ne 0) { throw "Bundle failed for $Name. See $fixture." }
}
function Require([bool] $Condition, [string] $Message) {
    if (!$Condition) { throw $Message }
    $script:checks++
}

Write-Fixture 'entry.css' @'
/* @import 'comment-is-not-an-import.css'; */
@charset "UTF-8";
@import /* comment */ 'nested/first.css';
@import url("second.css");
.last { order: 3; content: "@import 'string-is-not-an-import.css'"; }
'@
Write-Fixture 'nested/first.css' @'
.first { order: 1; background: url('../images/image name.svg?version=1#shape'); --data: url(data:image/svg+xml;base64,PHN2Zy8+); --fragment: url('#shape'); }
'@
Write-Fixture 'second.css' '.second { order: 2; background: url("/host.svg"); --remote: url(https://example.test/remote.svg); }'
Write-Fixture 'images/image name.svg' '<svg />'
Invoke-Bundle 'order-and-urls'
$text = [System.IO.File]::ReadAllText($bundle)
Require ($text.IndexOf('order: 1') -lt $text.IndexOf('order: 2') -and $text.IndexOf('order: 2') -lt $text.IndexOf('order: 3')) 'Import order changed.'
Require ($text.Contains('url("images/image%20name.svg?version=1#shape")')) 'Relative resource URL was not rebased or encoded.'
Require ($text.Contains('url(data:image/svg+xml;base64,PHN2Zy8+)') -and $text.Contains("url('#shape')") -and $text.Contains('url("/host.svg")') -and $text.Contains('url(https://example.test/remote.svg)')) 'Data, fragment, host or external URLs changed.'
Require ($text.Contains('comment-is-not-an-import.css') -and $text.Contains('string-is-not-an-import.css') -and !$text.Contains('@charset')) 'Comments, strings or UTF-8 encoding changed.'
Require (!$text.Contains([char]13) -and [System.IO.File]::ReadAllBytes($bundle)[0] -ne 0xEF) 'Output is not deterministic UTF-8 without BOM/LF.'

$timestamp = [System.IO.File]::GetLastWriteTimeUtc($bundle)
Invoke-Bundle 'unchanged'
Require ([System.IO.File]::GetLastWriteTimeUtc($bundle) -eq $timestamp) 'An unchanged build rewrote the asset.'
Write-Fixture 'second.css' '.second { order: 20; }'
Invoke-Bundle 'changed-input'
Require ([System.IO.File]::ReadAllText($bundle).Contains('order: 20')) 'An imported content edit was missed.'
Write-Fixture 'entry.css' "@import 'second.css';"
Invoke-Bundle 'removed-import'
Require (![System.IO.File]::ReadAllText($bundle).Contains('order: 1')) 'A removed import retained stale content.'
Write-Fixture 'third.css' '.third { order: 30; }'
Write-Fixture 'entry.css' "@import 'third.css'; @import 'second.css'; @import 'third.css';"
Invoke-Bundle 'added-and-repeated-imports'
$text = [System.IO.File]::ReadAllText($bundle)
Require (($text.Split('order: 30').Length - 1) -eq 2 -and $text.IndexOf('order: 30') -lt $text.IndexOf('order: 20')) 'An added import or repeated cascade changed.'

foreach ($case in @(
    @{ Name='missing'; Css="@import 'missing.css';"; Error='Missing CSS input' },
    @{ Name='cycle'; Css="@import 'entry.css';"; Error='CSS import cycle' },
    @{ Name='traversal'; Css="@import '../outside.css';"; Error='Path leaves the CSS source root' },
    @{ Name='encoded-traversal'; Css="@import '%2e%2e/outside.css';"; Error='Path leaves the CSS source root' },
    @{ Name='media-condition'; Css="@import 'second.css' screen and (max-width: 600px);"; Error='Conditional @import is unsupported' },
    @{ Name='layer-condition'; Css="@import 'second.css' layer(controls);"; Error='Conditional @import is unsupported' },
    @{ Name='supports-condition'; Css="@import 'second.css' supports(display: grid);"; Error='Conditional @import is unsupported' },
    @{ Name='external-import'; Css="@import 'https://example.test/remote.css';"; Error='Only local unconditional CSS imports' },
    @{ Name='late-import'; Css=".first { color: red; } @import 'second.css';"; Error='@import must precede style rules' },
    @{ Name='missing-resource'; Css=".first { background: url('missing.svg'); }"; Error='Missing relative CSS resource' },
    @{ Name='resource-traversal'; Css=".first { background: url('../outside.svg'); }"; Error='Path leaves the CSS source root' },
    @{ Name='escaped-local'; Css="@import 'second\2e css';"; Error='Escaped/backslash local paths are unsupported' }
)) {
    Write-Fixture 'entry.css' $case.Css
    Invoke-Bundle $case.Name $case.Error
    $checks++
}

Write-Output "$checks CSS bundle checks passed. Evidence: $fixture"
