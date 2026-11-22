[CmdletBinding()]
param([switch]$VerifyOnly)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$destination = Join-Path $repoRoot 'src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/fonts/noto'
$manifest = Get-Content -LiteralPath (Join-Path $destination 'sources.json') -Raw | ConvertFrom-Json

foreach ($file in $manifest.files) {
    if ($file.name -cnotmatch '^(?:noto-sans(?:-sc)?-(?:normal|italic)-[a-z0-9-]+\.woff2|OFL-NotoSans(?:SC)?\.txt)$') {
        throw "Invalid pinned Noto asset name '$($file.name)'."
    }
    $path = Join-Path $destination $file.name
    if (Test-Path -LiteralPath $path -PathType Leaf) {
        if ((Get-Item -LiteralPath $path).Length -ne $file.bytes -or (Get-FileHash -LiteralPath $path).Hash -ne $file.sha256) {
            throw "Existing asset differs from the pinned source: $path"
        }
        continue
    }
    if ($VerifyOnly) { throw "Missing pinned Noto asset: $path" }

    $source = [Uri]$file.source
    if (!$source.IsAbsoluteUri -or $source.Scheme -cne 'https' -or $source.Host -cnotin @('fonts.gstatic.com', 'raw.githubusercontent.com')) {
        throw "Invalid pinned Noto source '$source'."
    }
    $temporary = Join-Path $destination ('.download-' + [Guid]::NewGuid().ToString('N'))
    try {
        Invoke-WebRequest -Uri $source -OutFile $temporary -MaximumRedirection 0
        if ((Get-Item -LiteralPath $temporary).Length -ne $file.bytes -or (Get-FileHash -LiteralPath $temporary).Hash -ne $file.sha256) {
            throw "Downloaded asset does not match pinned source: $source"
        }
        Move-Item -LiteralPath $temporary -Destination $path
    } finally {
        if (Test-Path -LiteralPath $temporary -PathType Leaf) { Remove-Item -LiteralPath $temporary }
    }
}
Write-Host "Verified $($manifest.files.Count) pinned Noto fonts and license notices."
