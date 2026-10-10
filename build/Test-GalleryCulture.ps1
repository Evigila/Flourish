param([Uri] $BaseUri='http://localhost:5188')
$ErrorActionPreference='Stop'
if (-not $BaseUri.IsLoopback -or $BaseUri.Scheme -notin @('http','https')) { throw 'Use a local Gallery test instance.' }
$root=Split-Path -Parent $PSScriptRoot
$catalog=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::Ordinal)
$modules=@(Get-ChildItem -LiteralPath (Join-Path $root 'src/Gallery.Flourish.Blazor/Localization') -Filter 'Culture.*.json' -File | Sort-Object Name)
if (!$modules.Count) { throw 'Gallery Culture modules are missing.' }
foreach ($module in $modules) {
    $document=[System.Text.Json.JsonDocument]::Parse([IO.File]::ReadAllText($module.FullName))
    try {
        foreach ($key in $document.RootElement.EnumerateObject()) {
            if ($catalog.ContainsKey($key.Name)) { throw ($module.Name+': duplicate Gallery key '+$key.Name) }
            $catalog.Add($key.Name,($key.Value.GetRawText() | ConvertFrom-Json -AsHashtable))
        }
    } finally { $document.Dispose() }
}
$framework=Get-Content -Raw (Join-Path $root 'src/Flourish.Blazor/Flourish.Blazor.Framework/Localization/Culture.json') | ConvertFrom-Json -AsHashtable
$checks=0
function Assert([bool] $Condition,[string] $Message) {
    if (-not $Condition) { throw $Message }
    $script:checks++
}
function Get-CulturePage([string] $Path,[string] $Culture,[hashtable] $ExtraHeaders=@{}) {
    $headers=@{'Accept-Language'=$Culture}
    foreach ($entry in $ExtraHeaders.GetEnumerator()) { $headers[$entry.Key]=$entry.Value }
    $response=Invoke-WebRequest ([Uri]::new($BaseUri,$Path)) -Headers $headers -SkipHttpErrorCheck
    Assert ($response.StatusCode -eq 200) ($Path+' '+$Culture+': request failed.')
    return [Net.WebUtility]::HtmlDecode($response.Content)
}
# Browsers negotiate compressed CSS. HTML-only checks cannot detect stale asset manifests.
$index=Get-CulturePage '/' 'en-US'
$style=[regex]::Match($index,'<link\b[^>]*href="([^"]*Gallery\.Flourish\.Blazor\.[^"]*styles\.css)"')
$script=[regex]::Match($index,'<script\b[^>]*src="([^"]*blazor\.web[^"]*\.js)"')
Assert $style.Success 'The Gallery scoped stylesheet link is missing.'
Assert $script.Success 'The Blazor startup script is missing.'
$resourceCases=@(
    @{ Path='_content/Arkheide.Flourish.Blazor.Framework/framework.css'; Type='text/css'; Minimum=1000; Text='.f-icon' },
    @{ Path='_content/Arkheide.Flourish.Blazor.Design/design.css'; Type='text/css'; Minimum=1000; Text='--f-primary' },
    @{ Path='_content/Arkheide.Flourish.Extensions.Culture.Blazor/browser-preferences.js'; Type='text/javascript'; Minimum=500; Text='saveCulture' },
    @{ Path='_content/Arkheide.Flourish.Blazor.Framework/icons/material/MaterialSymbolsOutlined.woff2'; Type='font/woff2'; Minimum=1000000 },
    @{ Path=$style.Groups[1].Value; Type='text/css'; Minimum=50 },
    @{ Path=$script.Groups[1].Value; Type='text/javascript'; Minimum=10000 }
)
$handler=[Net.Http.HttpClientHandler]::new()
$handler.AutomaticDecompression=[Net.DecompressionMethods]::None
$client=[Net.Http.HttpClient]::new($handler)
try {
    foreach ($resource in $resourceCases) {
        $baselineHash=$null
        foreach ($requestedEncoding in @('identity','gzip','gzip, br')) {
            $request=[Net.Http.HttpRequestMessage]::new([Net.Http.HttpMethod]::Get,[Uri]::new($BaseUri,$resource.Path))
            $request.Headers.TryAddWithoutValidation('Accept-Encoding',$requestedEncoding) | Out-Null
            try {
                $response=$client.SendAsync($request).GetAwaiter().GetResult()
                try {
                    Assert ([int]$response.StatusCode -eq 200) ($resource.Path+': resource request failed.')
                    Assert ($response.Content.Headers.ContentType.MediaType -eq $resource.Type) ($resource.Path+': wrong or missing content type.')
                    $bytes=$response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
                    $encoding=[string]::Join(',', $response.Content.Headers.ContentEncoding)
                    if ($encoding) {
                        $input=[IO.MemoryStream]::new($bytes)
                        $output=[IO.MemoryStream]::new()
                        $decoder=switch ($encoding) {
                            'gzip' { [IO.Compression.GZipStream]::new($input,[IO.Compression.CompressionMode]::Decompress) }
                            'br' { [IO.Compression.BrotliStream]::new($input,[IO.Compression.CompressionMode]::Decompress) }
                            default { throw ($resource.Path+': unexpected content encoding '+$encoding) }
                        }
                        try { $decoder.CopyTo($output); $bytes=$output.ToArray() }
                        finally { $decoder.Dispose(); $input.Dispose(); $output.Dispose() }
                    }
                    Assert ($bytes.Length -ge $resource.Minimum) ($resource.Path+': empty or truncated resource for '+$requestedEncoding)
                    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes))
                    if ($null -eq $baselineHash) { $baselineHash=$hash }
                    Assert ($hash -ceq $baselineHash) ($resource.Path+': compressed and identity payloads differ.')
                    if ($resource.Text) {
                        Assert ([Text.Encoding]::UTF8.GetString($bytes).Contains($resource.Text)) ($resource.Path+': required CSS rules are missing.')
                    }
                    Write-Output ('PASS asset '+$resource.Path+' requested='+$requestedEncoding+' delivered='+$encoding+' bytes='+$bytes.Length)
                } finally { $response.Dispose() }
            } finally { $request.Dispose() }
        }
    }
} finally { $client.Dispose() }

# Assert the actual collapsible PageHeading, rather than accepting translated navigation text.
$headingCases=@{
    '/changelog'='Nav_ChangeLog'
    '/framework'='Nav_GetStarted'; '/framework/topbar'='Nav_TopBar'
    '/framework/navigation'='Nav_Navigation'; '/framework/commands'='Nav_Commands'
    '/framework/layout'='Nav_PageLayout'; '/framework/interactions'='Nav_Interactions'
    '/controls'='Nav_Actions'; '/controls/inputs'='Nav_Inputs'; '/controls/data'='Nav_Data'
    '/controls/overlays'='Nav_Overlays'; '/controls/feedback'='Nav_Feedback'
    '/controls/progress'='Nav_Progress'; '/controls/content'='Nav_Content'; '/controls/layout'='Nav_Layout'
    '/foundations'='Nav_Colors'; '/foundations/typography'='Nav_Typography'
    '/foundations/spacing'='Nav_Spacing'; '/foundations/shape'='Nav_Shape'; '/foundations/dimensions'='Nav_Dimensions'
    '/foundations/icons'='Nav_Icons'; '/examples'='Nav_ExampleIndex'; '/examples/form'='Nav_FormPage'
    '/examples/enum'='Nav_Enums'; '/examples/search'='Nav_Search'
    '/records'='Nav_Records'; '/forms'='Page_RecordCreate'; '/patterns'='Page_Products'
    '/examples/overlays/reconnect'='Page_Reconnect'; '/examples/shell'='Page_Shell'
    '/not-found'='Page_NotFound'; '/error'='Page_Error'
}
function Get-Heading([string] $Html) {
    $header=[regex]::Match($Html,'<header\b[^>]*\bdata-page-heading\b[^>]*>(.*?)</header>','Singleline')
    Assert $header.Success 'The expected production PageHeading is missing.'
    $title=[regex]::Match($header.Groups[1].Value,'<h1\b[^>]*>(.*?)</h1>','Singleline')
    Assert $title.Success 'PageHeading has no semantic H1.'
    return [regex]::Replace($title.Groups[1].Value,'<[^>]+>','').Trim()
}
foreach ($culture in @('en-US','zh-CN','pt-BR')) {
    $html=Get-CulturePage '/' $culture
    Assert ($html.Contains('<html lang="'+$culture+'"')) ($culture+': SSR language negotiation failed.')
    Assert ($html.Contains($catalog.Home_Setup[$culture])) ($culture+': homepage key was not translated.')
    Assert ($html.Contains('<option value="pt-BR"')) ($culture+': Portuguese is missing from the language picker.')
    $methods=Get-CulturePage '/examples/display/access-methods?choice=account' $culture
    Assert ($methods.Contains('<html lang="'+$culture+'"')) ($culture+': method page did not use the same scoped language.')
    Assert ($methods.Contains('aria-label="'+$catalog.Access_MethodLabel[$culture]+'"')) ($culture+': method label is not keyed.')
    Assert ($methods.Contains($catalog.Access_MethodAccountAction[$culture])) ($culture+': method action is not translated.')
    $localization=Get-CulturePage '/framework/localization' $culture
    $formatted=[string]::Format([Globalization.CultureInfo]::GetCultureInfo($culture),$catalog.Formatted_Sample[$culture],[object[]]@([decimal]12345.67,[datetime]::new(2026,10,5)))
    Assert ($localization.Contains($formatted)) ($culture+': translated placeholders/date/number formatting mismatch.')
    $expectedCopy=$framework.Board_Copy[$culture]
    Assert ($localization.Contains('aria-label="'+$expectedCopy+'"')) ($culture+': actual Framework copy caption was not supplied by the bridge.')
    $accounts=Get-CulturePage '/examples/display/accounts' $culture
    Assert ($accounts.Contains($catalog.Access_AccountsDescription[$culture])) ($culture+': account scene still fell back to another language.')
    foreach ($case in $headingCases.GetEnumerator()) {
        $page=Get-CulturePage $case.Key $culture
        $actual=Get-Heading $page
        Assert ($actual -ceq $catalog[$case.Value][$culture]) ($case.Key+' '+$culture+': actual PageHeading was not translated: '+$actual)
    }
    $component=Get-CulturePage '/controls/actions/buttonsample' $culture
    Assert ((Get-Heading $component) -ceq 'Button') ($culture+': component API identity should remain unchanged.')
    Assert ($component.Contains($catalog.Page_BackToCategory[$culture] -f $catalog.Nav_Actions[$culture])) ($culture+': category back link was not translated.')
    $organizationTitle=$catalog.Access_OrganizationName[$culture]
    Assert ($methods.Contains('aria-label="'+$organizationTitle+'"')) ($culture+': stacked organization title was not translated.')
    Write-Output ('PASS '+$culture+' HTTP negotiation, actual PageHeading titles, generated keys, methods and formatting')
}
# Check section titles/prose inside their semantic elements, excluding code samples/navigation.
$bodyCases=@(
    @{Path='/framework'; Heading='Nav_GetStarted'; Body='Framework_StartFramework'},
    @{Path='/framework/layout'; Heading='Nav_PageLayout'; Body='Framework_ContentLayout'},
    @{Path='/examples'; Heading='Examples_CompletePages'; Body='Examples_SessionData'},
    @{Path='/examples/form'; Heading='Nav_FormPage'; Body='Examples_FormValidation'},
    @{Path='/foundations'; Heading='Nav_Colors'; Body='Appearance_ColorDescription'},
    @{Path='/foundations/icons'; Heading='Icons_IntroductionTitle'; Body='Icons_FontDescription'}
)
foreach ($culture in @('en-US','zh-CN','pt-BR')) {
    foreach ($case in $bodyCases) {
        $html=Get-CulturePage $case.Path $culture
        $headings=@([regex]::Matches($html,'<h2\b[^>]*>(.*?)</h2>','Singleline') | ForEach-Object { [regex]::Replace($_.Groups[1].Value,'<[^>]+>','').Trim() })
        Assert ($headings -ccontains $catalog[$case.Heading][$culture]) ($case.Path+' '+$culture+': H2 is not translated.')
        $paragraphs=@([regex]::Matches($html,'<p\b[^>]*>(.*?)</p>','Singleline') | ForEach-Object { [regex]::Replace($_.Groups[1].Value,'<[^>]+>','') })
        Assert (@($paragraphs.Where({ $_.Contains($catalog[$case.Body][$culture]) })).Count -gt 0) ($case.Path+' '+$culture+': explanatory paragraph is not translated.')
    }
}
# A separate request must not inherit the preceding Portuguese request's session.
$again=Get-CulturePage '/' 'en-US'
Assert ($again.Contains('<html lang="en-US"')) 'Separate requests leaked a previous UI language.'
# Existing culture-cookie negotiation has priority over the Accept-Language header.
$cookie=Get-CulturePage '/' 'en-US' @{'Cookie'='.AspNetCore.Culture=c=pt-BR|uic=pt-BR'}
Assert ($cookie.Contains('<html lang="pt-BR"')) 'The existing culture cookie was ignored.'
Write-Output ($checks.ToString()+' three-language Gallery HTTP checks passed.')
