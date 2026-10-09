param([string] $BaseUrl = 'http://127.0.0.1:5269')

$ErrorActionPreference = 'Stop'
$baseUri = [Uri] $BaseUrl
if (!$baseUri.IsLoopback) { throw 'Use a loopback Gallery host for this regression.' }
$script:checks = 0
function Require([bool] $Condition, [string] $Message) {
    if (!$Condition) { throw $Message }
    $script:checks++
}
function Get-Page([string] $Language = '', [string] $Cookie = '') {
    $headers = @{}
    if ($Language) { $headers['Accept-Language'] = $Language }
    if ($Cookie) { $headers['Cookie'] = '.AspNetCore.Culture=' + $Cookie }
    return Invoke-WebRequest -Uri ($BaseUrl.TrimEnd('/') + '/framework/localization') -Headers $headers -TimeoutSec 5
}

$defaults = Get-Page
Require ($defaults.StatusCode -eq 200 -and $defaults.Content.Contains('<html lang="zh-CN">')) 'Configured default culture did not reach SSR.'
$english = Get-Page 'en-US'
Require ($english.Content.Contains('<html lang="en-US">')) 'The independent browser header did not initialize English.'
$cookie = [Uri]::EscapeDataString('c=pt-BR|uic=zh-CN')
$restored = Get-Page 'en-US' $cookie
Require ($restored.Content.Contains('<html lang="zh-CN">')) 'A saved UI preference did not override the browser language on the first response.'
Require ($restored.Content.Contains('12.345,67')) 'The saved independent Brazilian format did not reach the first response.'
$again = Get-Page 'en-US' $cookie
Require ($again.Content.Contains('<html lang="zh-CN">') -and $again.Content.Contains('12.345,67')) 'A repeated refresh lost the language/format pair.'
$isolated = Get-Page 'en-US'
Require ($isolated.Content.Contains('<html lang="en-US">') -and !$isolated.Content.Contains('12.345,67')) 'Another browser inherited a personal language or format choice.'
$invalid = Get-Page 'en-US' 'invalid'
Require ($invalid.Content.Contains('<html lang="en-US">')) 'An invalid cookie did not fall back to normal request negotiation.'
$asset = Invoke-WebRequest -Uri ($BaseUrl.TrimEnd('/') + '/_content/Arkheide.Flourish.Extensions.Culture.Blazor/browser-preferences.js') -TimeoutSec 5
Require ($asset.StatusCode -eq 200 -and $asset.Headers['Content-Type'] -match 'javascript' -and $asset.Content.Contains('saveCulture')) 'The library preference module is not served as usable JavaScript.'
Write-Output "$checks actual HTTP culture-preference and asset checks passed."
