param([string]$PackageDirectory, [string]$Version)
. (Join-Path $PSScriptRoot 'Release-Common.ps1')
if (!$Version) { $Version = Get-PackageVersion }
if (!$PackageDirectory) { $PackageDirectory = Join-Path $ReleaseRoot 'artifacts/packages' }
$expected = @($ReleaseSettings.Packages | ForEach-Object { "$($_.Id).$Version.nupkg" } | Sort-Object)
$actual = @(Get-ChildItem -LiteralPath $PackageDirectory -Filter '*.nupkg' -File | Select-Object -ExpandProperty Name | Sort-Object)
if (Compare-Object $expected $actual) { throw "Package set must be exactly: $($expected -join ', '). Found: $($actual -join ', ')." }
Add-Type -AssemblyName System.IO.Compression.FileSystem
foreach ($package in $ReleaseSettings.Packages) {
    $archive = [System.IO.Compression.ZipFile]::OpenRead((Join-Path $PackageDirectory "$($package.Id).$Version.nupkg"))
    try {
        $nuspec = @($archive.Entries | Where-Object { $_.FullName.EndsWith('.nuspec') })
        if ($nuspec.Count -ne 1) { throw "$($package.Id) must contain one nuspec." }
        $reader = [System.IO.StreamReader]::new($nuspec[0].Open())
        try { [xml]$xml = $reader.ReadToEnd() } finally { $reader.Dispose() }
        $metadata = $xml.SelectSingleNode('/*[local-name()="package"]/*[local-name()="metadata"]')
        if ($metadata.id -ne $package.Id -or $metadata.version -ne $Version) { throw "Package metadata mismatch for $($package.Id)." }
        $dependencies = @($metadata.SelectNodes('.//*[local-name()="dependency"]'))
        $internal = @($dependencies | Where-Object { $_.id -like "$($ReleaseSettings.PackagePrefix)*" })
        foreach ($dependency in $internal) {
            $lowerBound = ($dependency.version.Trim('[]() ') -split ',')[0].Trim()
            if ($lowerBound -ne $Version) { throw "$($package.Id) depends on $($dependency.id) at '$($dependency.version)', expected $Version." }
            if ($dependency.id -notin $ReleaseSettings.Packages.Id) { throw "Unexpected internal dependency $($dependency.id)." }
        }
        $actualLibraryDependencies = @($dependencies | Where-Object {
            $_.id -like "$($ReleaseSettings.PackagePrefix)*" -or $_.id -like 'Arkheide.Essential.Culture*'
        } | Select-Object -ExpandProperty id -Unique | Sort-Object)
        if ((@($package.Dependencies | Sort-Object) -join ',') -cne ($actualLibraryDependencies -join ',')) {
            throw "$($package.Id) library dependencies differ from the configured package contract."
        }
        if ($ReleaseSettings.PackagePrefix -eq 'Arkheide.Flourish.' -and $package.Id -notlike 'Arkheide.Flourish.Extensions.*' -and @($dependencies | Where-Object { $_.id -like 'Arkheide.Essential.Culture*' }).Count) {
            throw "$($package.Id) must remain independent of optional Culture integration."
        }
        foreach ($dependency in $dependencies | Where-Object { $_.id -like 'Arkheide.Essential.Culture*' -and $ReleaseSettings.PackagePrefix -eq 'Arkheide.Flourish.' }) {
            $lowerBound = ($dependency.version.Trim('[]() ') -split ',')[0].Trim()
            if ($lowerBound -ne $ReleaseSettings.EssentialVersion) { throw "$($package.Id) must depend on Essential $($ReleaseSettings.EssentialVersion), found $($dependency.version)." }
        }
        foreach ($path in $package.Assets) {
            if (!$archive.GetEntry($path)) { throw "$($package.Id) is missing packaged asset '$path'." }
        }
        if ($package.Id -ceq 'Arkheide.Flourish.Blazor.Design') {
            if ($metadata.license.InnerText -cne 'MIT AND OFL-1.1') { throw 'Design package must declare the bundled font licenses.' }
            $prefix = 'staticwebassets/fonts/noto/'
            $manifest = $archive.GetEntry($prefix + 'sources.json')
            if (!$manifest) { throw 'Design package is missing the Noto source manifest.' }
            $reader = [IO.StreamReader]::new($manifest.Open())
            try { $fontFiles = ($reader.ReadToEnd() | ConvertFrom-Json).files } finally { $reader.Dispose() }
            $expectedFonts = @($fontFiles | ForEach-Object { $prefix + $_.name } | Sort-Object)
            $actualFonts = @($archive.Entries | Where-Object { $_.FullName.StartsWith($prefix) -and $_.Name -ne 'sources.json' } | ForEach-Object FullName | Sort-Object)
            if (Compare-Object $expectedFonts $actualFonts) { throw 'Design package does not match the pinned Noto assets.' }
            foreach ($file in $fontFiles) {
                $entry = $archive.GetEntry($prefix + $file.name)
                $stream = $entry.Open()
                try { $hash = (Get-FileHash -InputStream $stream).Hash } finally { $stream.Dispose() }
                if ($entry.Length -ne $file.bytes -or $hash -ne $file.sha256) { throw "Packaged Noto asset differs from its pinned source: $($file.name)." }
            }
            Write-Host "Verified $($fontFiles.Count) Noto fonts and license notices in Design."
        }
        if ($package.Managed) {
            $assemblies = @($archive.Entries | Where-Object { $_.FullName -like 'lib/*.dll' })
            if (!$assemblies.Count) { throw "$($package.Id) contains no managed library." }
        } elseif (@($archive.Entries | Where-Object { $_.FullName -like 'lib/*' -or $_.FullName -like 'ref/*' -or $_.FullName -like 'staticwebassets/*' }).Count) {
            throw "$($package.Id) must remain a dependency-only package without managed output or static assets."
        }
        Write-Host "Verified $($package.Id) $Version"
    } finally { $archive.Dispose() }
}
Write-Host "Verified $($expected.Count) packages, dependencies, and required assets."
