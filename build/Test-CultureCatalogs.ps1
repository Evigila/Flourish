param()
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$checks=0
$files=@(
    'src/Gallery.Flourish.Blazor/Localization/Culture.json',
    'src/Flourish.Blazor/Flourish.Blazor.Framework/Localization/Culture.json'
)
foreach ($file in $files) {
    $document=[System.Text.Json.JsonDocument]::Parse([IO.File]::ReadAllText((Join-Path $root $file)))
    try {
        $keys=[Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($key in $document.RootElement.EnumerateObject()) {
            if (-not $keys.Add($key.Name)) { throw ($file+': duplicate token '+$key.Name) }
            $checks++
            $languages=@($key.Value.EnumerateObject() | ForEach-Object Name)
            if (@(Compare-Object @('en-US','zh-CN','pt-BR') $languages).Count -gt 0) { throw ($key.Name+': provide exactly en-US, zh-CN and pt-BR.') }
            $checks++
            $baseline=$key.Value.GetProperty('en-US').GetString()
            $pattern='(?<!\{)\{\d+(?:,[^}:]+)?(?::[^}]+)?\}(?!\})'
            $placeholders=@([regex]::Matches($baseline,$pattern) | ForEach-Object Value | Sort-Object)
            $breaks=[regex]::Matches($baseline,"\n").Count
            foreach ($culture in @('en-US','zh-CN','pt-BR')) {
                $text=$key.Value.GetProperty($culture).GetString()
                if ([string]::IsNullOrWhiteSpace($text)) { throw ($key.Name+': empty '+$culture+' translation.') }
                $checks++
                $current=@([regex]::Matches($text,$pattern) | ForEach-Object Value | Sort-Object)
                if (@(Compare-Object $placeholders $current).Count -gt 0) { throw ($key.Name+': placeholder mismatch for '+$culture) }
                $checks++
                if ([regex]::Matches($text,"\n").Count -ne $breaks) { throw ($key.Name+': paragraph-break mismatch for '+$culture) }
                $checks++
            }
        }
        if ($keys.Count -eq 0) { throw ($file+': empty catalogue.') }
        Write-Output ('PASS '+$file+': '+$keys.Count+' complete three-language keys')
    } finally { $document.Dispose() }
}
# Validate access-key callers as well as the resources: a complete JSON file cannot localize literals.
$gallery=Get-Content -Raw (Join-Path $root $files[0]) | ConvertFrom-Json -AsHashtable
foreach ($source in Get-ChildItem (Join-Path $root 'src/Gallery.Flourish.Blazor') -Recurse -File | Where-Object { $_.Extension -in @('.cs','.razor') -and $_.FullName -notmatch '[\\/](bin|obj)[\\/]' }) {
    $content=[IO.File]::ReadAllText($source.FullName)
    foreach ($match in [regex]::Matches($content,'(?:TextKey\.|"Key\.)([A-Za-z_][A-Za-z0-9_]*)')) {
        if (-not $gallery.Contains($match.Groups[1].Value)) { throw ($source.Name+': missing resource for '+$match.Groups[1].Value) }
        $checks++
    }
}
Write-Output ($checks.ToString()+' catalogue integrity checks passed.')
