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
    $panels = [regex]::Matches($html,'<section\b[^>]*\bid="navigation-choices-[^"]+-(organization|account|pass)-panel"[^>]*>')
    Assert ($panels.Count -eq 3 * $Instances) ($Path + ': actual Gallery choice panels were not retained.')
    foreach ($panel in $panels) {
        $key = $panel.Groups[1].Value
        $hidden = [regex]::IsMatch($panel.Value,'\shidden(?:\s|=|>)')
        Assert ($hidden -eq ($key -ne $Active)) ($Path + ': incorrect visibility for ' + $key)
    }
    $links = [regex]::Matches($html,'<a\b[^>]*\bid="navigation-choices-[^"]+-(organization|account|pass)-choice"[^>]*>')
    Assert ($links.Count -eq 3 * $Instances) ($Path + ': actual choice links are missing.')
    foreach ($link in $links) {
        $key = $link.Groups[1].Value
        Assert ($link.Value.Contains('aria-current="page"') -eq ($key -eq $Active)) ($Path + ': incorrect current link.')
        Assert ($link.Value.Contains('href="/examples/display/access-methods?choice=' + $key + '"')) ($Path + ': native GET destination changed.')
        Assert ($link.Value.Contains('data-enhance-nav="false"')) ($Path + ': native document navigation boundary disappeared.')
    }
    Write-Output ('PASS ' + $Path)
}
Test-Choices '/examples/display/access-methods' 'organization'
Test-Choices '/examples/display/access-methods?choice=account' 'account'
Test-Choices '/examples/display/access-methods?choice=pass' 'pass'
Test-Choices '/examples/display/access-methods?choice=organization' 'organization'
Test-Choices '/examples/display/access-methods?choice=missing' 'organization'
Test-Choices '/examples/display/access-methods?choice=account&choice=pass' 'organization'
Test-Choices '/controls/layout/navigationchoicessample' 'organization' 2
Test-Choices '/controls/layout/navigationchoicessample?choice=account' 'account' 2
# Repeat actual endpoint requests after selecting alternatives.
Test-Choices '/examples/display/access-methods?choice=account' 'account'
Test-Choices '/examples/display/access-methods' 'organization'
foreach ($path in @('/examples/display/accounts','/examples/display/login')) {
    $html = Get-Gallery $path
    Assert ($html.Contains('id="access-example-content"')) ($path + ': expected the access document, not another scene.')
    Assert (-not $html.Contains('f-navigation-choices-nav')) ($path + ': unexpected method selector in the account/login scene.')
}
foreach ($case in @(
    @{ Path='/controls/layout/accessbrandsample'; Pattern='<img\b[^>]*src="data:image/svg\+xml,' },
    @{ Path='/controls/layout/shellheadersample'; Pattern='<img\b[^>]*src="data:image/svg\+xml,' },
    @{ Path='/controls/inputs/fieldsample'; Pattern='name="_handler" value="sample-field-[a-f0-9]+"' },
    @{ Path='/controls/inputs/formlayoutsample'; Pattern='name="_handler" value="sample-form-layout-[a-f0-9]+"' }
)) {
    $html = Get-Gallery $case.Path
    Assert ([regex]::IsMatch([Net.WebUtility]::HtmlDecode($html),$case.Pattern)) ($case.Path + ': runtime logo/form value was rendered as a literal.')
}
$html = Get-Gallery '/controls/data/datatablesample'
Assert ($html.Contains('data-f-search-query')) 'DataTable did not render its shared search input.'
Assert (-not [regex]::IsMatch($html,'<input\b[^>]*data-f-search-query[^>]*value="RemoteQuery"')) 'Remote query was rendered as a literal instead of the current search value.'
Write-Output ($checks.ToString() + ' actual Gallery HTTP checks passed.')
