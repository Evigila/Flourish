param([Uri] $BaseUri = 'http://localhost:5188')
$ErrorActionPreference = 'Stop'
if (-not $BaseUri.IsLoopback -or $BaseUri.Scheme -notin @('http','https')) {
    throw 'Use an independently started local Gallery instance.'
}
$checks = 0
function Assert([bool] $Condition, [string] $Message) {
    if (-not $Condition) { throw $Message }
    $script:checks++
}
function Get-Gallery([string] $Path) {
    $response = Invoke-WebRequest ([Uri]::new($BaseUri,$Path)) -SkipHttpErrorCheck
    Assert ($response.StatusCode -eq 200) ($Path + ': expected HTTP 200, received ' + $response.StatusCode)
    return $response.Content
}
function Test-Choices([string] $Path, [string] $Active, [int] $Instances = 1) {
    $html = Get-Gallery $Path
    $panels = [regex]::Matches($html,'<section\b[^>]*\bid="navigation-choices-[^"]+-(organization|account)-panel"[^>]*>')
    Assert ($panels.Count -eq 2 * $Instances) ($Path + ': actual Gallery choice panels were not retained.')
    foreach ($panel in $panels) {
        $key = $panel.Groups[1].Value
        $hidden = [regex]::IsMatch($panel.Value,'\shidden(?:\s|=|>)')
        Assert ($hidden -eq ($key -ne $Active)) ($Path + ': incorrect visibility for ' + $key)
    }
    $links = [regex]::Matches($html,'<a\b[^>]*\bid="navigation-choices-[^"]+-(organization|account)-choice"[^>]*>')
    Assert ($links.Count -eq 2 * $Instances) ($Path + ': actual choice links are missing.')
    foreach ($link in $links) {
        $key = $link.Groups[1].Value
        Assert ($link.Value.Contains('aria-current="page"') -eq ($key -eq $Active)) ($Path + ': incorrect current link.')
        Assert ($link.Value.Contains('href="/examples/display/access-methods?choice=' + $key + '"')) ($Path + ': native GET destination changed.')
        Assert ($link.Value.Contains('data-enhance-nav="false"')) ($Path + ': native document navigation boundary disappeared.')
    }
    Write-Output ('PASS ' + $Path)
}
$changeLog = [Net.WebUtility]::HtmlDecode((Get-Gallery '/changelog'))
$releases = Get-Content -Raw (Join-Path (Split-Path -Parent $PSScriptRoot) 'src/Gallery.Flourish.Blazor/Models/ChangeLog.json') | ConvertFrom-Json
Assert ($changeLog.Contains('f-no-secondary')) 'ChangeLog must have no secondary navigation.'
Assert (-not $changeLog.Contains('class="f-secondary-navigation"')) 'ChangeLog unexpectedly rendered secondary navigation.'
Assert ([regex]::IsMatch($changeLog, '<a\b[^>]*class="f-primary-item is-selected"[^>]*href="/changelog"[^>]*aria-current="true"')) 'ChangeLog is not the selected primary navigation entry.'
$selectorIndex = $changeLog.IndexOf('id="changelog-version"')
$subtitleIndex = $changeLog.IndexOf('<h2')
Assert ($selectorIndex -ge 0 -and $subtitleIndex -ge 0 -and $selectorIndex -lt $subtitleIndex) 'ChangeLog version selector must precede its first subtitle.'
Assert ($changeLog.Contains('>' + $releases[0].version + '</h2>')) 'ChangeLog must initially display the preview.'
foreach ($version in $releases.version) {
    Assert ($changeLog.Contains('<option value="' + $version + '"')) ('ChangeLog is missing version ' + $version)
}
Assert (-not $changeLog.Contains('<option value="1.1.0"')) 'The first tag must not have a ChangeLog entry.'
$overviewHtml = [Net.WebUtility]::HtmlDecode((Get-Gallery '/'))
$startSection = $overviewHtml.IndexOf('href="/examples"', $overviewHtml.IndexOf('<h1'))
$changeLogSection = $overviewHtml.IndexOf('href="/changelog"', $overviewHtml.IndexOf('<h1'))
$setupSection = $overviewHtml.IndexOf('f-code-block', $overviewHtml.IndexOf('<h1'))
Assert ($startSection -ge 0 -and $changeLogSection -gt $startSection -and $setupSection -gt $changeLogSection) 'Home ChangeLog section must follow Start here and precede setup.'
Test-Choices '/examples/display/access-methods' 'account'
Test-Choices '/examples/display/access-methods?choice=account' 'account'
Test-Choices '/examples/display/access-methods?choice=pass' 'account'
Test-Choices '/examples/display/access-methods?choice=organization' 'organization'
Test-Choices '/examples/display/access-methods?choice=missing' 'account'
Test-Choices '/examples/display/access-methods?choice=account&choice=organization' 'account'
Test-Choices '/controls/layout/navigationchoicessample' 'account' 2
Test-Choices '/controls/layout/navigationchoicessample?choice=account' 'account' 2
# Repeat actual endpoint requests after selecting alternatives.
Test-Choices '/examples/display/access-methods?choice=account' 'account'
Test-Choices '/examples/display/access-methods' 'account'
foreach ($path in @('/examples/display/access-methods','/examples/display/access-methods?choice=account','/examples/display/access-methods?choice=organization')) {
    $html = Get-Gallery $path
    Assert ($html.Contains('f-navigation-choices-sidebar')) ($path+': responsive sidebar layout missing.')
    Assert (-not $html.Contains('f-presentation-hero')) ($path+': retired organization hero returned.')
    Assert (-not [regex]::IsMatch($html,'navigation-choices-[^"]+-pass-(choice|panel)')) ($path+': retired pass demo returned.')
    Assert ($html.Contains('href="/examples/display/login"')) ($path+': platform account demo lost its native destination.')
    Assert ([regex]::Matches($html,'data-icon="check"').Count -eq 1) ($path+': current-item check missing or duplicated.')
    foreach ($field in @('username','password')) {
        Assert ([regex]::IsMatch($html,'<label\b[^>]*for="navigation-choices-[a-f0-9]+-'+$field+'"')) ($path+': '+$field+' label is not linked to its actual input.')
        Assert ([regex]::IsMatch($html,'<input\b[^>]*id="navigation-choices-[a-f0-9]+-'+$field+'"')) ($path+': '+$field+' input lost its standard identity.')
    }
}
Write-Output ($checks.ToString() + ' actual Gallery HTTP checks passed.')
