#requires -Version 5.1
[CmdletBinding()]
param(
    [string]$ProjectRoot,
    [switch]$Backup,
    [Alias('clean-arquived', 'clean-archived')][switch]$CleanArquived,
    [Alias('no-arquived', 'no-archived')][switch]$NoArquived,
    [switch]$NonInteractive,
    [string]$Destination
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'saf-core.ps1')
$utf8 = New-Object Text.UTF8Encoding($false, $true)
$archiveRelative = 'docs-ai/current/1_archived'
# Windows PowerShell 5.1 may bind parameter defaults before PSScriptRoot is set.
$effectiveRoot = if ($ProjectRoot) { $ProjectRoot } else { Split-Path -Parent $PSScriptRoot }

function Assert-LocalUnlinkedPath([string]$Path) {
    $cursor = [IO.Path]::GetFullPath($Path)
    while ($cursor) {
        if (Test-Path -LiteralPath $cursor) {
            if (((Get-Item -LiteralPath $cursor -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw "Refusing linked path: $cursor"
            }
        }
        $parent = [IO.Path]::GetDirectoryName($cursor)
        if ($parent -eq $cursor) { break }
        $cursor = $parent
    }
}

function Get-LocalFiles([string]$Root, [string]$Relative) {
    $path = Get-SafTarget -Root $Root -Relative $Relative
    if (-not (Test-Path -LiteralPath $path)) { return }
    if ([IO.File]::Exists($path)) { return $Relative }
    $pending = New-Object 'Collections.Generic.Stack[string]'
    $pending.Push($Relative)
    while ($pending.Count -gt 0) {
        $directoryRelative = $pending.Pop()
        $directory = Get-SafTarget -Root $Root -Relative $directoryRelative
        foreach ($item in @(Get-ChildItem -LiteralPath $directory -Force)) {
            if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) { throw "Refusing linked path: $($item.FullName)" }
            if ($item.Name -eq '.git') {
                if ($directoryRelative -eq $archiveRelative -or $directoryRelative.StartsWith($archiveRelative + '/', [StringComparison]::Ordinal)) {
                    throw 'Archive contains a Git-internal path; preserve it and resolve the unsupported content manually.'
                }
                continue
            }
            $childRelative = $directoryRelative + '/' + $item.Name
            $null = Get-SafTarget -Root $Root -Relative $childRelative
            if ($item.PSIsContainer) { $pending.Push($childRelative) }
            else { $childRelative }
        }
    }
}

function Update-LocalIgnore([string]$Root, $Config, [switch]$IgnoreArchive) {
    $path = Get-SafTarget -Root $Root -Relative '.gitignore'
    $text = if ([IO.File]::Exists($path)) { [IO.File]::ReadAllText($path, $utf8) } else { '' }
    $lines = @($text.Replace("`r`n", "`n").Split("`n"))
    $rules = @('/.agents/backup/', '/.agents/saf_log/', '/.agents/operation.lock')
    if ($IgnoreArchive) { $rules += @('/docs-ai/current/1_archived/*', '!/docs-ai/current/1_archived/.gitkeep') }
    $missing = @($rules | Where-Object { $_ -cnotin $lines })
    if ($missing.Count -eq 0) { return }
    Assert-SafWritable -Root $Root -Relative '.gitignore' -Config $Config
    if ($text.Length -gt 0 -and -not $text.EndsWith("`n")) { $text += "`n" }
    $text += ($missing -join "`n") + "`n"
    $temporary = $path + '.' + [Guid]::NewGuid().ToString('N') + '.tmp'
    try {
        [IO.File]::WriteAllText($temporary, $text, $utf8)
        if ([IO.File]::Exists($path)) { [IO.File]::Replace($temporary, $path, [NullString]::Value) }
        else { [IO.File]::Move($temporary, $path) }
    } finally { if ([IO.File]::Exists($temporary)) { [IO.File]::Delete($temporary) } }
}

function Get-LocalBackupPaths([string]$Root) {
    $paths = @('AGENTS.md', 'SAF.json', 'SAF.config.json', 'SAF.lock.json', 'origin.bat', 'local.bat', 'debug.bat', '.gitignore',
        'scripts/saf-core.ps1', 'scripts/origin.ps1', 'scripts/local.ps1')
    foreach ($relative in $paths) { Get-LocalFiles -Root $Root -Relative $relative }
    foreach ($relative in @('docs-ai', '.agents/skills')) { Get-LocalFiles -Root $Root -Relative $relative }
}

function Get-LocalDownloads {
    if ([Environment]::OSVersion.Platform -ne [PlatformID]::Win32NT) { throw 'Use -Destination outside Windows.' }
    $settings = Get-ItemProperty -LiteralPath 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders'
    $property = $settings.PSObject.Properties['{374DE290-123F-4565-9164-39C4925E467B}']
    if (-not $property -or [string]::IsNullOrWhiteSpace([string]$property.Value)) { throw 'Cannot resolve the Windows Downloads known folder. Supply -Destination.' }
    return [Environment]::ExpandEnvironmentVariables([string]$property.Value)
}

function Remove-LocalEmptyArchiveDirectories([string]$Root, $Config, [switch]$ValidateOnly) {
    $source = Get-SafTarget -Root $Root -Relative $archiveRelative
    if (-not [IO.Directory]::Exists($source)) { return }
    $directories = New-Object 'Collections.Generic.List[string]'
    $pending = New-Object 'Collections.Generic.Stack[string]'
    $pending.Push($archiveRelative)
    while ($pending.Count -gt 0) {
        $relative = $pending.Pop()
        $directory = Get-SafTarget -Root $Root -Relative $relative
        foreach ($child in Get-ChildItem -LiteralPath $directory -Directory -Force) {
            $childRelative = $relative + '/' + $child.Name
            [void](Get-SafTarget -Root $Root -Relative $childRelative)
            $directories.Add($childRelative); $pending.Push($childRelative)
        }
    }
    if ($ValidateOnly) {
        foreach ($relative in $directories) { Assert-SafWritable -Root $Root -Relative $relative -Config $Config }
        return
    }
    foreach ($relative in @($directories.ToArray() | Sort-Object Length -Descending)) {
        $directory = Get-SafTarget -Root $Root -Relative $relative
        if (@(Get-ChildItem -LiteralPath $directory -Force).Count -eq 0) {
            Assert-SafWritable -Root $Root -Relative $relative -Config $Config
            [IO.Directory]::Delete($directory)
        }
    }
}

function Export-LocalArchive([string]$Root, [string]$OutputDirectory, $Config) {
    $sourceDirectory = Get-SafTarget -Root $Root -Relative $archiveRelative
    $paths = @(Get-LocalFiles -Root $Root -Relative $archiveRelative | Where-Object {
        $_ -ne ($archiveRelative + '/.gitkeep') -or (Get-Item -LiteralPath (Get-SafTarget $Root $_) -Force).Length -gt 0
    } | Sort-Object)
    foreach ($relative in $paths) { Assert-SafWritable -Root $Root -Relative $relative -Config $Config }
    Remove-LocalEmptyArchiveDirectories -Root $Root -Config $Config -ValidateOnly
    $markerRelative = $archiveRelative + '/.gitkeep'
    $marker = Get-SafTarget -Root $Root -Relative $markerRelative
    if ([IO.Directory]::Exists($sourceDirectory) -and (-not [IO.File]::Exists($marker) -or $markerRelative -in $paths)) {
        Assert-SafWritable -Root $Root -Relative $markerRelative -Config $Config
    }
    if ($paths.Count -eq 0) { Remove-LocalEmptyArchiveDirectories -Root $Root -Config $Config; Write-Host '[SAF] No archived files to clean.'; return $null }
    if (-not $OutputDirectory) { $OutputDirectory = Get-LocalDownloads }
    $output = [IO.Path]::GetFullPath($OutputDirectory)
    Assert-LocalUnlinkedPath $output
    if ($output.Equals($sourceDirectory, [StringComparison]::OrdinalIgnoreCase) -or
        $output.StartsWith($sourceDirectory.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Archive destination must be outside the archived source directory.'
    }
    $archivePath = Join-Path $output ('SAF-archived-' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ') + '-' + [Guid]::NewGuid().ToString('N') + '.zip')
    $rootPrefix = $Root.TrimEnd([char]92, [char]47) + [IO.Path]::DirectorySeparatorChar
    if ($archivePath.StartsWith($rootPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        $outputRelative = $archivePath.Substring($rootPrefix.Length).Replace([char]92, [char]47)
        Assert-SafWritable -Root $Root -Relative $outputRelative -Config $Config
    }
    [void][IO.Directory]::CreateDirectory($output)
    $snapshots = @()
    foreach ($relative in $paths) {
        $bytes = [IO.File]::ReadAllBytes((Get-SafTarget -Root $Root -Relative $relative))
        $snapshots += [pscustomobject]@{ path = $relative; sha256 = Get-SafByteHash -Bytes $bytes; length = $bytes.LongLength }
    }
    $inventory = [ordered]@{ schemaVersion = 1; createdAtUtc = [DateTime]::UtcNow.ToString('o'); projectRoot = $Root; files = @($snapshots) }
    $inventoryBytes = $utf8.GetBytes(($inventory | ConvertTo-Json -Depth 10) + "`n")
    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $stream = [IO.File]::Open($archivePath, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
    try {
        $zip = New-Object IO.Compression.ZipArchive($stream, [IO.Compression.ZipArchiveMode]::Create, $true)
        try {
            foreach ($snapshot in $snapshots) {
                $bytes = [IO.File]::ReadAllBytes((Get-SafTarget -Root $Root -Relative $snapshot.path))
                if ((Get-SafByteHash -Bytes $bytes) -ne $snapshot.sha256) { throw "Archive source changed: $($snapshot.path)" }
                $entry = $zip.CreateEntry('files/' + $snapshot.path, [IO.Compression.CompressionLevel]::Optimal)
                $entryStream = $entry.Open()
                try { $entryStream.Write($bytes, 0, $bytes.Length) } finally { $entryStream.Dispose() }
            }
            $entry = $zip.CreateEntry('inventory.json')
            $entryStream = $entry.Open()
            try { $entryStream.Write($inventoryBytes, 0, $inventoryBytes.Length) } finally { $entryStream.Dispose() }
        } finally { $zip.Dispose() }
    } finally { $stream.Dispose() }
    $zip = [IO.Compression.ZipFile]::OpenRead($archivePath)
    try {
        if ($zip.Entries.Count -ne $snapshots.Count + 1) { throw 'Archive entry count verification failed.' }
        foreach ($snapshot in @($snapshots) + @([pscustomobject]@{ path = 'inventory.json'; sha256 = Get-SafByteHash -Bytes $inventoryBytes })) {
            $entryName = if ($snapshot.path -eq 'inventory.json') { 'inventory.json' } else { 'files/' + $snapshot.path }
            $entry = $zip.GetEntry($entryName)
            if (-not $entry) { throw "Missing archive entry: $entryName" }
            $entryStream = $entry.Open()
            $memory = New-Object IO.MemoryStream
            try {
                $entryStream.CopyTo($memory)
                if ((Get-SafByteHash -Bytes $memory.ToArray()) -ne $snapshot.sha256) { throw "Archive hash verification failed: $entryName" }
            } finally { $memory.Dispose(); $entryStream.Dispose() }
        }
    } finally { $zip.Dispose() }
    # Recheck the full snapshot before deleting any source, then preserve new or changed files.
    foreach ($snapshot in $snapshots) {
        $path = Get-SafTarget -Root $Root -Relative $snapshot.path
        if (-not [IO.File]::Exists($path) -or (Get-SafByteHash -Bytes ([IO.File]::ReadAllBytes($path))) -ne $snapshot.sha256) {
            throw "Source changed after export; no source files were cleaned: $($snapshot.path). Export: $archivePath"
        }
    }
    $deleted = New-Object 'Collections.Generic.List[object]'
    try {
        foreach ($snapshot in $snapshots) {
            $path = Get-SafTarget -Root $Root -Relative $snapshot.path
            Assert-SafWritable -Root $Root -Relative $snapshot.path -Config $Config
            if ((Get-SafByteHash -Bytes ([IO.File]::ReadAllBytes($path))) -ne $snapshot.sha256) { throw "Source changed during cleanup; preserved: $($snapshot.path). Export: $archivePath" }
            [IO.File]::Delete($path)
            $deleted.Add($snapshot)
        }
    } catch {
        $cleanupError = $_
        $zip = [IO.Compression.ZipFile]::OpenRead($archivePath)
        try {
            foreach ($snapshot in $deleted) {
                try {
                    $path = Get-SafTarget -Root $Root -Relative $snapshot.path
                    if ([IO.File]::Exists($path)) { continue }
                    $entryStream = $zip.GetEntry('files/' + $snapshot.path).Open()
                    try {
                        $restoreStream = [IO.File]::Open($path, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
                        try { $entryStream.CopyTo($restoreStream) } finally { $restoreStream.Dispose() }
                    } finally { $entryStream.Dispose() }
                } catch { Write-Warning "Source restoration needs manual review: $($snapshot.path). Verified export: $archivePath" }
            }
        } finally { $zip.Dispose() }
        throw $cleanupError
    }
    # Remove only verified-empty child directories, never recursively delete a source tree.
    Remove-LocalEmptyArchiveDirectories -Root $Root -Config $Config
    $marker = Get-SafTarget -Root $Root -Relative ($archiveRelative + '/.gitkeep')
    if (-not (Test-Path -LiteralPath $marker)) {
        Assert-SafWritable -Root $Root -Relative ($archiveRelative + '/.gitkeep') -Config $Config
        [IO.File]::WriteAllBytes($marker, [byte[]]@())
    }
    $remaining = @(Get-LocalFiles -Root $Root -Relative $archiveRelative | Where-Object { $_ -ne ($archiveRelative + '/.gitkeep') })
    if ($remaining.Count -gt 0) { throw "Export verified but new archive content remains; review $archivePath and the source directory." }
    Write-Host "[SAF] Verified archive exported to $archivePath"
    return $archivePath
}

$interactive = $PSBoundParameters.Count -eq 0
$exitCode = 1
$operation = $null
$root = $null
$action = 'local'
$data = $null
$result = 'failed'
try {
    $root = Get-SafRoot -ProjectRoot $effectiveRoot
    if (@(@($Backup, $CleanArquived, $NoArquived) | Where-Object { [bool]$_ }).Count -gt 1) { throw 'Choose one local operation.' }
    if ($interactive) {
        Write-Host '1. Backup (default)'
        Write-Host '2. Clean archived files'
        Write-Host '3. Ignore archived files in Git'
        Write-Host '0. Exit'
        while ($true) {
            Write-Host 'Select [1]:'
            $selection = [Console]::ReadLine()
            if ($null -eq $selection -or $selection.Trim() -eq '0') { $action = 'cancel'; break }
            if ($selection.Trim() -in @('', '1')) { $Backup = $true; break }
            if ($selection.Trim() -eq '2') { $CleanArquived = $true; break }
            if ($selection.Trim() -eq '3') { $NoArquived = $true; break }
            Write-Host 'Invalid selection. Enter 1, 2, 3, or 0.'
        }
    } elseif (-not ($Backup -or $CleanArquived -or $NoArquived)) { throw 'Specify -backup, -clean-arquived, or -no-arquived.' }
    if ($Destination -and -not $CleanArquived) { throw '-Destination is only valid with -clean-arquived.' }
    if ($action -eq 'cancel') { $result = 'cancelled'; $exitCode = 0 }
    else {
        Assert-LocalUnlinkedPath $root
        $operation = Enter-SafOperation -Root $root
        $config = Resolve-SafConfig -Root $root
        $null = Move-SafLegacyStorage -Root $root
        if ($Backup) {
            $action = 'backup'
            $paths = @(Get-LocalBackupPaths -Root $root | Sort-Object -Unique)
            $backupPath = New-SafBackup -Root $root -RelativePaths $paths -Reason 'local-backup'
            Update-LocalIgnore -Root $root -Config $config
            $data = @{ backupPath = $backupPath; files = $paths.Count }
            Write-Host "[SAF] Backup saved to $backupPath"
        } elseif ($NoArquived) {
            $action = 'no-arquived'
            $null = Get-LocalFiles -Root $root -Relative $archiveRelative
            Update-LocalIgnore -Root $root -Config $config -IgnoreArchive
            $data = @{ directory = $archiveRelative }
            Write-Warning 'Existing tracked archived files remain tracked. The index and existing commits were not changed.'
            Write-Host '[SAF] Added archive ignore rules.'
        } else {
            $action = 'clean-arquived'
            Update-LocalIgnore -Root $root -Config $config
            $export = Export-LocalArchive -Root $root -OutputDirectory $Destination -Config $config
            $data = @{ archivePath = $export; directory = $archiveRelative }
        }
        $result = 'success'
        $exitCode = 0
    }
} catch { Write-Host ('[SAF] Error: ' + $_.Exception.Message) -ForegroundColor Red; $data = @{ error = $_.Exception.Message } }
finally {
    if ($root) {
        try { Write-SafLog -Root $root -Action $action -Result $result -ExitCode $exitCode -Data $data }
        catch { Write-Warning ('Could not append local operation log: ' + $_.Exception.Message); $exitCode = 1 }
    }
    if ($operation) { $operation.Dispose() }
    if ($interactive) { Write-Host 'Press Enter to close...'; [void][Console]::ReadLine() }
}
exit $exitCode
