param()
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$checks=0
$galleryFiles=@(Get-ChildItem -LiteralPath (Join-Path $root 'src/Gallery.Flourish.Blazor/Localization') -Filter 'Culture.*.json' -File | Sort-Object Name | ForEach-Object { [IO.Path]::GetRelativePath($root,$_.FullName) })
if (!$galleryFiles.Count) { throw 'Gallery must declare multilingual Culture modules.' }
$catalogs=@(
    @{ Name='Gallery'; Files=$galleryFiles },
    @{ Name='Framework'; Files=@('src/Flourish.Blazor/Flourish.Blazor.Framework/Localization/Culture.json') },
    @{ Name='Culture'; Files=@('src/Flourish.Extensions/Flourish.Extensions.Culture.Blazor/Localization/Culture.json') }
)
$gallery=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::Ordinal)
foreach ($catalog in $catalogs) {
  $catalogKeys=[Collections.Generic.Dictionary[string,string]]::new([StringComparer]::Ordinal)
  foreach ($file in $catalog.Files) {
    $document=[System.Text.Json.JsonDocument]::Parse([IO.File]::ReadAllText((Join-Path $root $file)))
    try {
        $keys=[Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($key in $document.RootElement.EnumerateObject()) {
            if (-not $keys.Add($key.Name)) { throw ($file+': duplicate token '+$key.Name) }
            $checks++
            if ($catalogKeys.ContainsKey($key.Name)) { throw ($file+': duplicate catalogue key '+$key.Name+' already declared in '+$catalogKeys[$key.Name]) }
            $catalogKeys.Add($key.Name,$file)
            $checks++
            if ($catalog.Name -eq 'Gallery') { $gallery.Add($key.Name,$key.Value.Clone()) }
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
  Write-Output ('PASS '+$catalog.Name+': '+$catalogKeys.Count+' unique keys across '+$catalog.Files.Count+' module(s)')
}
# Validate access-key callers as well as the resources: a complete JSON file cannot localize literals.
foreach ($source in Get-ChildItem (Join-Path $root 'src/Gallery.Flourish.Blazor') -Recurse -File | Where-Object { $_.Extension -in @('.cs','.razor') -and $_.FullName -notmatch '[\\/](bin|obj)[\\/]' }) {
    $content=[IO.File]::ReadAllText($source.FullName)
    foreach ($match in [regex]::Matches($content,'(?:TextKey\.|"Key\.)([A-Za-z_][A-Za-z0-9_]*)')) {
        if (-not $gallery.ContainsKey($match.Groups[1].Value)) { throw ($source.Name+': missing resource for '+$match.Groups[1].Value) }
        $checks++
    }
    if ($content -match '(?m)^\s*(?:using|@using|@inject|@inherits)\s+(?:\w+\s*=\s*)?ArkheideSystem\.Essential\.Culture') {
        throw ($source.Name+': Gallery must consume the Culture extension instead of exposing its Essential implementation.')
    }
    $checks++
}
$program=[IO.File]::ReadAllText((Join-Path $root 'src/Gallery.Flourish.Blazor/Program.cs'))
$registrations=@([regex]::Matches($program,'builder\.Services\.([A-Za-z][A-Za-z0-9]*)') | ForEach-Object { $_.Groups[1].Value })
if ((($registrations | Sort-Object) -join ',') -cne 'AddFlourishDesign,AddFlourishFramework,AddScoped') {
    throw 'Gallery Program must register services through AddFlourishFramework, AddFlourishDesign and RecordStore only.'
}
$checks++
if ($program -notmatch 'AddScoped<RecordStore>\s*\(' -or $program -notmatch '\.ConfigureCulture\s*\(') {
    throw 'Gallery must keep its RecordStore and configure localization inside AddFlourishFramework.'
}
$checks++
if ($program -match '\b(?:AddCultureBlazor|AddFlourishCulture|AddFlourishPreferences|UseRequestLocalization|RequestLocalizationOptions|AddJsonFile)\b') {
    throw 'Gallery Program retains localization setup that belongs to the framework/Culture extension.'
}
$checks++
if ($program -notmatch '\.AddCatalogFiles\("Gallery", CultureResources\.Files, CultureResources\.FallbackCulture\)' -or $program -match 'Gallery\.Texts\.json') {
    throw 'Gallery must load the same generated module manifest used for typed keys.'
}
$checks++
if (Test-Path -LiteralPath (Join-Path $root 'src/Gallery.Flourish.Blazor/Components/LanguagePicker.razor')) {
    throw 'Gallery retains a host language picker instead of consuming the extension component.'
}
$checks++
Write-Output ($checks.ToString()+' catalogue integrity checks passed.')
