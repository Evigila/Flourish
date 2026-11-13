#requires -Version 5.1
[CmdletBinding()]
param(
    [string]$ProjectRoot,
    [string]$Repository, [string]$Ref, [string]$SourcePath,
    [switch]$Check, [switch]$Update, [switch]$Sync, [switch]$Force, [switch]$NonInteractive
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'saf-core.ps1')
$root = $null; $lease = $null; $exitCode = 1; $result = 'error'; $action = 'origin'; $logData = $null
try {
    if (-not $ProjectRoot) { $ProjectRoot = if ($env:SAF_PROJECT_ROOT) { $env:SAF_PROJECT_ROOT } else { Split-Path -Parent $PSScriptRoot } }
    $root = Get-SafRoot -ProjectRoot $ProjectRoot
    $config = Resolve-SafConfig -Root $root
    if (([int]$Check.IsPresent + [int]$Update.IsPresent + [int]$Sync.IsPresent) -gt 1) { throw 'Choose one action: -check, -update, or -sync.' }
    if ($Force -and -not $Update) { throw '-force must accompany -update.' }
    $action = if ($Check) { 'origin.check' } elseif ($Update) { 'origin.update' } else { 'origin.sync' }
    # Check may write its log only; taking an operation lease would create another local file.
    if (-not $Check) { $lease = Enter-SafOperation -Root $root }
    $state = Read-SafState -Root $root
    $lockHash = Get-SafExistingHash (Get-SafTarget $root 'SAF.lock.json') -Raw
    $sourceState = if ($state.installation) { $state.installation } else { $state.bootstrap }
    foreach ($key in @('Repository', 'Ref')) {
        if ($PSBoundParameters.ContainsKey($key) -and [string]::IsNullOrWhiteSpace([string](Get-Variable -Name $key -ValueOnly))) { throw "An explicit $key cannot be empty." }
    }
    if (-not $Repository) { $Repository = if ($config.origin.repository) { $config.origin.repository } elseif ($sourceState) { $sourceState.repository } else { throw 'No source configured. Run scripts/init.ps1 or specify -Repository.' } }
    if (-not $Ref) { $Ref = if ($config.origin.ref) { $config.origin.ref } elseif ($sourceState) { $sourceState.ref } else { 'main' } }
    $release = Get-SafRelease -Root $root -Repository $Repository -Ref $Ref -SourcePath $SourcePath
    $version = $release.manifest.frameworkVersion
    $installed = $state.installation
    if ($installed -and [version]$version -lt [version]$installed.frameworkVersion -and -not $Force) { throw 'An intentional SAF downgrade requires -update -force.' }
    if ($installed -and $version -eq $installed.frameworkVersion -and $release.manifestHash -ne $installed.manifestSha256) { throw 'Release content changed without a version increment.' }
    $baseline = @{}
    foreach ($section in @($installed, $state.bootstrap)) {
        if (-not $section) { continue }
        foreach ($property in $section.managedFiles.PSObject.Properties) {
            Assert-SafManaged -Relative $property.Name -Legacy
            if ($property.Value -notmatch '^[0-9a-f]{64}$') { throw 'Invalid installation baseline.' }
            # Bootstrap owns only known handler paths, never arbitrary project files.
            if ($section -eq $state.bootstrap -and $property.Name -notin $script:SafHandlers) { throw 'Invalid bootstrap ownership entry.' }
            $baseline[$property.Name] = [string]$property.Value
        }
    }
    $desired = $release.payload
    $desired['SAF.json'] = [pscustomobject]@{ mode = 'managed'; bytes = $release.manifestBytes; hash = $release.manifestHash }
    $operations = New-Object 'Collections.Generic.List[object]'
    $conflicts = New-Object 'Collections.Generic.List[string]'
    $managed = [ordered]@{}
    $legacyDebug = Get-SafTarget $root 'debug-handler.bat'
    $newDebug = Get-SafTarget $root 'debug.bat'
    $hasLegacyDebug = [IO.File]::Exists($legacyDebug)
    foreach ($relative in @($desired.Keys | Sort-Object)) {
        $target = Get-SafTarget $root $relative; $entry = $desired[$relative]
        $actualRaw = Get-SafExistingHash $target -Raw
        if ($entry.mode -eq 'seed') {
            if ($relative -eq 'debug.bat' -and $hasLegacyDebug -and ([IO.File]::ReadAllBytes($legacyDebug)).Length -gt 0) {
                $legacyBytes = [IO.File]::ReadAllBytes($legacyDebug)
                if ($null -ne $actualRaw -and $actualRaw -ne (Get-SafByteHash $legacyBytes)) { throw 'Project debug menu conflict: debug.bat and debug-handler.bat differ. Preserve and resolve them manually.' }
                if ($null -eq $actualRaw) { $operations.Add([pscustomobject]@{ relative = $relative; bytes = $legacyBytes; delete = $false; expected = $null }) }
            } elseif ($null -eq $actualRaw -or ($relative -eq 'debug.bat' -and ([IO.File]::ReadAllBytes($target)).Length -eq 0)) {
                $operations.Add([pscustomobject]@{ relative = $relative; bytes = $entry.bytes; delete = $false; expected = $actualRaw })
            }
            continue
        }
        $managed[$relative] = $entry.hash
        $actual = Get-SafExistingHash $target
        if ($actual -eq $entry.hash) { continue }
        if (($baseline.ContainsKey($relative) -and $actual -ne $baseline[$relative]) -or
            (-not $baseline.ContainsKey($relative) -and $null -ne $actual)) { $conflicts.Add($relative) }
        $operations.Add([pscustomobject]@{ relative = $relative; bytes = $entry.bytes; delete = $false; expected = $actualRaw })
    }
    foreach ($relative in @($baseline.Keys | Sort-Object)) {
        if ($managed.Contains($relative)) { continue }
        $target = Get-SafTarget $root $relative; $actual = Get-SafExistingHash $target
        if ($null -eq $actual) { continue }
        if ($actual -ne $baseline[$relative]) { $conflicts.Add($relative) }
        $operations.Add([pscustomobject]@{ relative = $relative; bytes = $null; delete = $true; expected = Get-SafExistingHash $target -Raw })
    }
    # Legacy audit and installation metadata are retired only after their exact contents were merged.
    if ($state.PSObject.Properties['migration'] -and $state.migration) {
        foreach ($property in $state.migration.legacyHashes.PSObject.Properties) {
            if ($property.Name -notin @('AGENTS.lock.json', 'AGENTS.ensure.json')) { throw 'Invalid legacy retirement path.' }
            $target = Get-SafTarget $root $property.Name
            if ([IO.File]::Exists($target)) { $operations.Add([pscustomobject]@{ relative = $property.Name; bytes = $null; delete = $true; expected = $property.Value }) }
        }
    }
    if ($hasLegacyDebug) {
        $operations.Add([pscustomobject]@{ relative = 'debug-handler.bat'; bytes = $null; delete = $true; expected = Get-SafExistingHash $legacyDebug -Raw })
    }
    Assert-SafConfigUnchanged -Root $root -Config $config
    foreach ($operation in $operations) { Assert-SafWritable -Root $root -Relative $operation.relative -Config $config }
    if (-not $Check) { Assert-SafWritable -Root $root -Relative 'SAF.lock.json' -Config $config; [void](Move-SafLegacyStorage -Root $root) }
    $needsUpdate = -not $installed -or $operations.Count -gt 0 -or $installed.manifestSha256 -ne $release.manifestHash -or
        $installed.repository -ne $Repository -or $installed.ref -ne $Ref
    Write-Host "[SAF] Source $version; commit $($release.commit)."
    if ($conflicts.Count -gt 0) { Write-Host ('[SAF] Local conflicts: ' + ($conflicts -join ', ')) }
    $logData = @{ version = $version; commit = $release.commit; conflicts = @($conflicts.ToArray()) }
    if ($Check) {
        if ($conflicts.Count -gt 0) { $exitCode = 3; $result = 'conflict' }
        elseif ($needsUpdate) { Write-Host '[SAF] Update available.'; $exitCode = 2; $result = 'update-available' }
        else { Write-Host '[SAF] Current.'; $exitCode = 0; $result = 'current' }
    } elseif ($conflicts.Count -gt 0 -and -not $Force) { $exitCode = 3; $result = 'conflict'; Write-Host '[SAF] No payload changes made. Resolve customizations or explicitly use -update -force.' }
    elseif (-not $needsUpdate) { $exitCode = 0; $result = 'current'; Write-Host '[SAF] Current.' }
    else {
        $backupPaths = @($operations | ForEach-Object { $_.relative }) + @('SAF.lock.json', 'SAF.config.json')
        $backup = New-SafBackup -Root $root -RelativePaths $backupPaths -Reason $action
        Write-Host "[SAF] Recovery snapshot: $backup"
        $state.installation = [pscustomobject][ordered]@{ frameworkVersion = $version; repository = $Repository; ref = $Ref
            commit = $release.commit; manifestSha256 = $release.manifestHash; managedFiles = [pscustomobject]$managed
            synchronizedAtUtc = [DateTime]::UtcNow.ToString('o') }
        $handlerHashes = [ordered]@{}
        foreach ($relative in $script:SafHandlers) { $handlerHashes[$relative] = $managed[$relative] }
        $state.bootstrap = [pscustomobject]@{ handlerProtocolVersion = 3; configSupportVersion = 2; repository = $Repository; ref = $Ref
            commit = $release.commit; managedFiles = [pscustomobject]$handlerHashes }
        if ($state.PSObject.Properties['migration']) { $state.PSObject.Properties.Remove('migration') }
        $operations.Add([pscustomobject]@{ relative = 'SAF.lock.json'; bytes = ConvertTo-SafBytes $state; delete = $false; expected = $lockHash })
        Invoke-SafTransaction -Root $root -Operations $operations.ToArray()
        $exitCode = 0; $result = 'updated'; $logData.backup = $backup.Substring($root.Length + 1).Replace([char]92, [char]47)
        Write-Host '[SAF] Synchronized. Audit evidence preserved; a version mismatch requires a new documentation audit.'
    }
} catch {
    if ($_.Exception.Message.StartsWith('[SAF_REFRESH]')) { $exitCode = 4; $result = 'bootstrap-required' }
    Write-Host ('[SAF] ' + $_.Exception.Message) -ForegroundColor Red
} finally {
    if ($root) { Write-SafLog -Root $root -Action $action -Result $result -ExitCode $exitCode -Data $logData }
    if ($lease) { $lease.Dispose() }
}
exit $exitCode
