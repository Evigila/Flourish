#requires -Version 5.1
# Shared repository-local primitives; dot-source, do not execute a remote controller.
Set-StrictMode -Version Latest
$script:SafUtf8 = New-Object Text.UTF8Encoding($false, $true)
$script:SafOperationLocks = @{}
$script:SafStartedAtUtc = [DateTime]::UtcNow.ToString('o')
$script:SafOperationId = [Guid]::NewGuid().ToString('N')
$script:SafHandlers = @('origin.bat', 'local.bat', 'scripts/origin.ps1', 'scripts/local.ps1', 'scripts/saf-core.ps1')

function Get-SafByteHash([byte[]]$Bytes) {
    $algorithm = [Security.Cryptography.SHA256]::Create()
    try { return ([BitConverter]::ToString($algorithm.ComputeHash($Bytes))).Replace('-', '').ToLowerInvariant() }
    finally { $algorithm.Dispose() }
}

function Get-SafHash([byte[]]$Bytes) {
    return Get-SafByteHash ($script:SafUtf8.GetBytes($script:SafUtf8.GetString($Bytes).TrimStart([char]0xFEFF).Replace("`r`n", "`n")))
}

function Read-SafJson([byte[]]$Bytes) {
    $value = $script:SafUtf8.GetString($Bytes).TrimStart([char]0xFEFF)
    if ($value -notmatch '\A[ \t\r\n]*\{') { throw 'SAF data requires a top-level JSON object.' }
    $parsed = if ((Get-Command ConvertFrom-Json).Parameters.ContainsKey('DateKind')) { ConvertFrom-Json -InputObject $value -DateKind String }
        else { ConvertFrom-Json -InputObject $value }
    if ($null -eq $parsed -or $parsed -isnot [pscustomobject]) { throw 'SAF data requires a top-level JSON object.' }
    return $parsed
}

function ConvertTo-SafBytes($Value) { return ,$script:SafUtf8.GetBytes(($Value | ConvertTo-Json -Depth 30) + "`n") }

function ConvertTo-SafArgument([string]$Value) {
    $builder = New-Object Text.StringBuilder
    [void]$builder.Append('"'); $slashes = 0
    foreach ($character in $Value.ToCharArray()) {
        if ($character -eq '\') { $slashes++; continue }
        if ($character -eq '"') { [void]$builder.Append(('\' * (2 * $slashes + 1))) }
        else { [void]$builder.Append(('\' * $slashes)) }
        [void]$builder.Append($character); $slashes = 0
    }
    [void]$builder.Append(('\' * (2 * $slashes))); [void]$builder.Append('"')
    return $builder.ToString()
}

function Invoke-SafGit([string[]]$Arguments, [switch]$Text) {
    $start = New-Object Diagnostics.ProcessStartInfo
    $start.FileName = 'git'
    $start.Arguments = (($Arguments | ForEach-Object { ConvertTo-SafArgument $_ }) -join ' ')
    $start.UseShellExecute = $false; $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true; $start.RedirectStandardError = $true
    $start.EnvironmentVariables['GIT_TERMINAL_PROMPT'] = '0'
    $process = New-Object Diagnostics.Process; $process.StartInfo = $start
    $buffer = New-Object IO.MemoryStream
    try {
        [void]$process.Start(); $errorTask = $process.StandardError.ReadToEndAsync()
        $process.StandardOutput.BaseStream.CopyTo($buffer); $process.WaitForExit()
        # Do not echo a native error that may contain credentials in a source URL.
        if ($process.ExitCode -ne 0) { throw "Git operation failed (exit $($process.ExitCode)). Check source access and ref." }
        [void]$errorTask.Result
        if ($Text) { return $script:SafUtf8.GetString($buffer.ToArray()).Trim() }
        return ,$buffer.ToArray()
    } finally { $buffer.Dispose(); $process.Dispose() }
}

function Assert-SafRelative([string]$Relative) {
    if ([string]::IsNullOrWhiteSpace($Relative) -or $Relative -match '[\\:<>"|?*\x00-\x1f]' -or $Relative.StartsWith('/') -or
        $Relative.EndsWith('/') -or @($Relative.Split('/') | Where-Object {
            $_ -in @('', '.', '..', '.git') -or $_ -match '[. ]$' -or $_ -match '^(?i:CON|PRN|AUX|NUL|COM[0-9]|LPT[0-9])(?:\.|$)'
        }).Count -gt 0) { throw "Invalid SAF relative path: $Relative" }
}

function Get-SafTarget([string]$Root, [string]$Relative) {
    Assert-SafRelative $Relative
    $base = [IO.Path]::GetFullPath($Root).TrimEnd([char]92, [char]47)
    $target = [IO.Path]::GetFullPath((Join-Path $base $Relative))
    if (-not $target.StartsWith($base + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw 'SAF path escapes project root.' }
    $cursor = $target
    while ($cursor) {
        if (Test-Path -LiteralPath $cursor) {
            $item = Get-Item -LiteralPath $cursor -Force
            if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) { throw "Refusing linked path: $cursor" }
        }
        $cursor = [IO.Path]::GetDirectoryName($cursor)
    }
    return $target
}

function Get-SafRoot([string]$ProjectRoot) {
    $root = [IO.Path]::GetFullPath($ProjectRoot).TrimEnd([char]92, [char]47)
    if (-not [IO.Directory]::Exists($root)) { throw 'ProjectRoot must be an existing directory.' }
    $gitRoot = Invoke-SafGit -Arguments @('-C', $root, 'rev-parse', '--show-toplevel') -Text
    if (-not $root.Equals([IO.Path]::GetFullPath($gitRoot).TrimEnd([char]92, [char]47), [StringComparison]::OrdinalIgnoreCase)) { throw 'Use the target Git repository root.' }
    [void](Get-SafTarget -Root $root -Relative 'SAF.lock.json')
    return $root
}

function Assert-SafRepository([string]$Repository) {
    if ([string]::IsNullOrWhiteSpace($Repository) -or $Repository.StartsWith('-')) { throw 'Invalid source repository.' }
    $uri = $null
    if ([Uri]::TryCreate($Repository, [UriKind]::Absolute, [ref]$uri) -and $uri.Scheme -in @('http', 'https') -and
        ($uri.UserInfo -or $uri.Query -or $uri.Fragment)) { throw 'Use a credential-free repository URL; authentication belongs in the existing Git credential helper.' }
}

function Get-SafFiles([string]$Root, [string]$Relative) {
    $path = Get-SafTarget -Root $Root -Relative $Relative
    if (-not (Test-Path -LiteralPath $path)) { return }
    if ([IO.File]::Exists($path)) { return $Relative }
    foreach ($child in Get-ChildItem -LiteralPath $path -Force) {
        $childRelative = $Relative + '/' + $child.Name
        [void](Get-SafTarget -Root $Root -Relative $childRelative)
        if ($child.PSIsContainer) { Get-SafFiles -Root $Root -Relative $childRelative }
        else { $childRelative }
    }
}

function Assert-SafManaged([string]$Relative, [switch]$Legacy) {
    Assert-SafRelative $Relative
    if ($Relative -in (@('AGENTS.md', 'SAF.json') + $script:SafHandlers) -or
        $Relative.StartsWith('docs-ai/common/', [StringComparison]::Ordinal) -or
        $Relative.StartsWith('.agents/skills/', [StringComparison]::Ordinal)) { return }
    if ($Legacy -and $Relative -in @('AGENTS.framework.json', 'fetch-agents.bat', 'scripts/fetch-agents.ps1')) { return }
    throw "Path is outside SAF ownership: $Relative"
}

function Get-SafExistingHash([string]$Path, [switch]$Raw) {
    if (-not (Test-Path -LiteralPath $Path)) { return $null }
    if (-not [IO.File]::Exists($Path)) { throw "Expected a regular file: $Path" }
    $bytes = [IO.File]::ReadAllBytes($Path)
    if ($Raw) { return Get-SafByteHash $bytes }
    try { return Get-SafHash $bytes } catch [Text.DecoderFallbackException] { return '(invalid UTF-8)' }
}

function Assert-SafConfigObject($Value, [string[]]$Allowed, [string]$Location) {
    if ($null -eq $Value -or $Value -isnot [pscustomobject]) { throw "Expected a configuration object: $Location" }
    foreach ($property in $Value.PSObject.Properties) {
        if ($property.Name -cnotin $Allowed) { throw "Unknown configuration field: $Location.$($property.Name)" }
    }
}

function Assert-SafConfigString($Value, [string]$Location) {
    if ($Value -isnot [string] -or [string]::IsNullOrWhiteSpace($Value) -or $Value -match '[\x00-\x1f]') { throw "Expected a nonempty configuration string: $Location" }
}

function Resolve-SafConfig([string]$Root) {
    $path = Get-SafTarget -Root $Root -Relative 'SAF.config.json'
    $bytes = $null; $hash = $null
    if (Test-Path -LiteralPath $path) {
        if (-not [IO.File]::Exists($path)) { throw "Expected a regular file: $path" }
        $bytes = [IO.File]::ReadAllBytes($path)
        $hash = Get-SafByteHash $bytes
    }
    $effective = [pscustomobject]@{
        schemaVersion = 2; origin = [pscustomobject]@{ repository = $null; ref = $null }
        project = [pscustomobject]@{ protectedPaths = @()
            verification = [pscustomobject]@{ computerUse = 'deny' }
            ui = [pscustomobject]@{ framework = 'Material Design 3' }
            commit = [pscustomobject]@{ style = 'follow-history'; initialStyle = 'english-imperative' } }
        sourceHash = $hash
    }
    if ($null -eq $hash) { return $effective }
    $config = Read-SafJson $bytes
    Assert-SafConfigObject $config @('schemaVersion', 'origin', 'project') 'config'
    if (-not $config.PSObject.Properties['schemaVersion'] -or ($config.schemaVersion -isnot [int] -and $config.schemaVersion -isnot [long]) -or $config.schemaVersion -ne 2) { throw 'Unsupported SAF.config.json schema. Migrate project configuration to schema 2 before initializing SAF 2.' }
    if ($config.PSObject.Properties['origin']) {
        Assert-SafConfigObject $config.origin @('repository', 'ref') 'origin'
        foreach ($key in @('repository', 'ref')) {
            if (-not $config.origin.PSObject.Properties[$key]) { continue }
            $value = $config.origin.$key; Assert-SafConfigString $value "origin.$key"
            if ($key -eq 'repository') { Assert-SafRepository $value }
            elseif ($value.StartsWith('-')) { throw 'Invalid configured source ref.' }
            $effective.origin.$key = $value
        }
    }
    if ($config.PSObject.Properties['project']) {
        Assert-SafConfigObject $config.project @('protectedPaths', 'verification', 'ui', 'commit') 'project'
        if ($config.project.PSObject.Properties['protectedPaths']) {
            if ($config.project.protectedPaths -isnot [array]) { throw 'project.protectedPaths must be an array.' }
            $protected = @()
            foreach ($relative in $config.project.protectedPaths) {
                Assert-SafConfigString $relative 'project.protectedPaths'
                $bare = $relative.TrimEnd([char]47)
                $protectedPath = Get-SafTarget -Root $Root -Relative $bare
                if ($relative -ne $bare -and $relative -ne ($bare + '/')) { throw 'Invalid protected directory path.' }
                if ([IO.Directory]::Exists($protectedPath) -and -not $relative.EndsWith('/')) { throw "Protected directories require a trailing slash: $relative" }
                if ([IO.File]::Exists($protectedPath) -and $relative.EndsWith('/')) { throw "Protected files cannot have a trailing slash: $relative" }
                $protected += $relative
            }
            $effective.project.protectedPaths = @($protected | Select-Object -Unique)
        }
        if ($config.project.PSObject.Properties['verification']) {
            Assert-SafConfigObject $config.project.verification @('computerUse') 'project.verification'
            if ($config.project.verification.PSObject.Properties['computerUse']) {
                Assert-SafConfigString $config.project.verification.computerUse 'project.verification.computerUse'
                if ($config.project.verification.computerUse -cnotin @('inherit', 'deny')) { throw 'Invalid computerUse preference.' }
                $effective.project.verification.computerUse = $config.project.verification.computerUse
            }
        }
        if ($config.project.PSObject.Properties['ui']) {
            Assert-SafConfigObject $config.project.ui @('framework') 'project.ui'
            if ($config.project.ui.PSObject.Properties['framework']) {
                Assert-SafConfigString $config.project.ui.framework 'project.ui.framework'
                $effective.project.ui.framework = $config.project.ui.framework
            }
        }
        if ($config.project.PSObject.Properties['commit']) {
            Assert-SafConfigObject $config.project.commit @('style', 'initialStyle') 'project.commit'
            foreach ($key in @('style', 'initialStyle')) {
                if ($config.project.commit.PSObject.Properties[$key]) {
                    Assert-SafConfigString $config.project.commit.$key "project.commit.$key"
                    $effective.project.commit.$key = $config.project.commit.$key
                }
            }
        }
    }
    return $effective
}

function Assert-SafConfigUnchanged([string]$Root, $Config) {
    if ((Get-SafExistingHash (Get-SafTarget $Root 'SAF.config.json') -Raw) -ne $Config.sourceHash) { throw 'SAF.config.json changed during the operation. Retry with the new configuration.' }
}

function Assert-SafWritable([string]$Root, [string]$Relative, $Config) {
    $target = Get-SafTarget -Root $Root -Relative $Relative
    Assert-SafConfigUnchanged -Root $Root -Config $Config
    $base = [IO.Path]::GetFullPath($Root).TrimEnd([char]92, [char]47)
    if (-not [IO.Directory]::Exists($base)) { throw 'The project root must already exist.' }
    $writePaths = New-Object 'Collections.Generic.List[string]'; $writePaths.Add($Relative)
    $parent = [IO.Path]::GetDirectoryName($target)
    while (-not [IO.Directory]::Exists($parent)) {
        $writePaths.Add($parent.Substring($base.Length + 1).Replace([char]92, [char]47))
        $parent = [IO.Path]::GetDirectoryName($parent)
    }
    foreach ($writePath in $writePaths) {
        foreach ($protected in $Config.project.protectedPaths) {
            $bare = $protected.TrimEnd([char]47)
            if ($writePath.Equals($bare, [StringComparison]::OrdinalIgnoreCase) -or
                ($protected.EndsWith('/') -and $writePath.StartsWith($protected, [StringComparison]::OrdinalIgnoreCase))) {
                throw "Project-protected path cannot be changed by this operation: $writePath"
            }
        }
    }
}

function Enter-SafOperation([string]$Root) {
    $config = Resolve-SafConfig -Root $Root
    Assert-SafWritable -Root $Root -Relative '.agents/operation.lock' -Config $config
    $path = Get-SafTarget -Root $Root -Relative '.agents/operation.lock'
    $legacyPath = Get-SafTarget -Root $Root -Relative '.agent/operation.lock'
    $legacy = $null; $stream = $null
    try {
        if ([IO.File]::Exists($legacyPath)) { $legacy = [IO.File]::Open($legacyPath, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None) }
        [void][IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($path))
        $stream = [IO.File]::Open($path, [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    } catch {
        if ($legacy) { $legacy.Dispose() }; if ($stream) { $stream.Dispose() }
        throw 'Another SAF operation is running, or an operation lock is inaccessible. Retry after it finishes.'
    }
    $lease = [pscustomobject]@{ Primary = $stream; Legacy = $legacy; LegacyPath = $legacyPath
        Root = $Root; FrozenConfig = $config; CanWrite = $true }
    $lease | Add-Member -MemberType ScriptMethod -Name Dispose -Value {
        if (-not $this.CanWrite) { return }
        try {
            if ($this.Legacy) {
                $empty = $this.Legacy.Length -eq 0; $this.Legacy.Dispose()
                if ($empty) {
                    try {
                        # Bootstrap may have created a neutral config during this lease.
                        # Both the original and current protection sets still apply to cleanup.
                        $currentConfig = Resolve-SafConfig -Root $this.Root
                        $frozenPolicy = [pscustomobject]@{ sourceHash = $currentConfig.sourceHash; project = $this.FrozenConfig.project }
                        $legacyPath = Get-SafTarget -Root $this.Root -Relative '.agent/operation.lock'
                        if ([IO.File]::Exists($legacyPath) -and (Get-Item -LiteralPath $legacyPath -Force).Length -eq 0) {
                            Assert-SafWritable -Root $this.Root -Relative '.agent/operation.lock' -Config $frozenPolicy
                            Assert-SafWritable -Root $this.Root -Relative '.agent/operation.lock' -Config $currentConfig
                            [IO.File]::Delete($legacyPath)
                        }
                        $directory = Get-SafTarget -Root $this.Root -Relative '.agent'
                        if ([IO.Directory]::Exists($directory) -and @(Get-ChildItem -LiteralPath $directory -Force).Count -eq 0) {
                            Assert-SafWritable -Root $this.Root -Relative '.agent' -Config $frozenPolicy
                            Assert-SafWritable -Root $this.Root -Relative '.agent' -Config $currentConfig
                            [IO.Directory]::Delete($directory)
                        }
                    } catch { Write-Warning ('Legacy runtime lock or directory retained: ' + $_.Exception.Message) }
                }
            }
        } finally {
            try { $this.Primary.Dispose() } finally { $this.CanWrite = $false }
        }
    }
    $script:SafOperationLocks[$Root] = $lease
    return $lease
}

function Move-SafLegacyStorage([string]$Root) {
    if (-not $script:SafOperationLocks.ContainsKey($Root) -or -not $script:SafOperationLocks[$Root].CanWrite) { throw 'Legacy migration requires the SAF operation lease.' }
    $config = Resolve-SafConfig -Root $Root
    $plans = New-Object 'Collections.Generic.List[object]'
    foreach ($kind in @('backup', 'saf_log')) {
        foreach ($relative in @(Get-SafFiles -Root $Root -Relative ('.agent/' + $kind))) {
            $source = Get-SafTarget $Root $relative
            $hash = Get-SafExistingHash $source -Raw
            $tail = $relative.Substring(('.agent/' + $kind + '/').Length)
            $destinationRelative = '.agents/' + $kind + '/' + $tail
            $destination = Get-SafTarget $Root $destinationRelative
            $existing = Get-SafExistingHash $destination -Raw
            if ($null -ne $existing -and $existing -ne $hash) {
                if ($kind -ne 'saf_log') { throw "Legacy backup collision; both copies retained: $relative" }
                $destinationRelative = '.agents/saf_log/legacy/' + $tail + '.' + $hash + '.jsonl'
                $destination = Get-SafTarget $Root $destinationRelative
                $existing = Get-SafExistingHash $destination -Raw
                if ($null -ne $existing -and $existing -ne $hash) { throw 'Legacy log collision; both copies retained.' }
            }
            Assert-SafWritable -Root $Root -Relative $relative -Config $config
            Assert-SafWritable -Root $Root -Relative $destinationRelative -Config $config
            $plans.Add([pscustomobject]@{ source = $source; relative = $relative; target = $destination; targetRelative = $destinationRelative; hash = $hash; existing = $existing })
        }
    }
    foreach ($plan in $plans) {
        if ((Get-SafExistingHash $plan.source -Raw) -ne $plan.hash) { throw 'Legacy storage changed during migration planning.' }
        Assert-SafWritable -Root $Root -Relative $plan.targetRelative -Config $config
        if ($null -eq $plan.existing) {
            [void][IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($plan.target))
            $temporary = $plan.target + '.' + [Guid]::NewGuid().ToString('N') + '.tmp'
            try {
                [IO.File]::WriteAllBytes($temporary, [IO.File]::ReadAllBytes($plan.source))
                # Move fails if another process creates the destination; never replace it.
                [IO.File]::Move($temporary, $plan.target)
            } finally { if ([IO.File]::Exists($temporary)) { [IO.File]::Delete($temporary) } }
        }
        if ((Get-SafExistingHash $plan.target -Raw) -ne $plan.hash) { throw 'Legacy storage copy verification failed; source retained.' }
    }
    foreach ($plan in $plans) {
        [void](Get-SafTarget $Root $plan.relative)
        if ((Get-SafExistingHash $plan.source -Raw) -ne $plan.hash -or (Get-SafExistingHash $plan.target -Raw) -ne $plan.hash) { throw 'Legacy storage changed; remaining sources retained.' }
        Assert-SafWritable -Root $Root -Relative $plan.relative -Config $config
        [IO.File]::Delete($plan.source)
    }
    foreach ($kind in @('backup', 'saf_log')) {
        $relative = '.agent/' + $kind; $base = Get-SafTarget $Root $relative
        if (-not [IO.Directory]::Exists($base)) { continue }
        $pending = New-Object 'Collections.Generic.Stack[string]'; $directories = New-Object 'Collections.Generic.List[string]'
        $pending.Push($relative); $directories.Add($relative)
        while ($pending.Count -gt 0) {
            $parent = $pending.Pop()
            foreach ($child in Get-ChildItem -LiteralPath (Get-SafTarget $Root $parent) -Directory -Force) {
                $childRelative = $parent + '/' + $child.Name
                [void](Get-SafTarget $Root $childRelative); $pending.Push($childRelative); $directories.Add($childRelative)
            }
        }
        foreach ($directory in @($directories.ToArray() | Sort-Object Length -Descending)) {
            $path = Get-SafTarget $Root $directory
            if (@(Get-ChildItem -LiteralPath $path -Force).Count -eq 0) { Assert-SafWritable $Root $directory $config; [IO.Directory]::Delete($path) }
        }
    }
    $legacyRoot = Get-SafTarget $Root '.agent'
    if ([IO.Directory]::Exists($legacyRoot) -and @(Get-ChildItem -LiteralPath $legacyRoot -Force).Count -eq 0) { Assert-SafWritable $Root '.agent' $config; [IO.Directory]::Delete($legacyRoot) }
    return $plans.Count
}

function Read-SafState([string]$Root) {
    $statePath = Get-SafTarget -Root $Root -Relative 'SAF.lock.json'
    $legacyPaths = @('AGENTS.lock.json', 'AGENTS.ensure.json')
    if ([IO.File]::Exists($statePath)) {
        $state = Read-SafJson ([IO.File]::ReadAllBytes($statePath))
        if ($state.schemaVersion -ne 1 -or -not $state.PSObject.Properties['bootstrap'] -or
            -not $state.PSObject.Properties['installation'] -or -not $state.PSObject.Properties['audit']) { throw 'Invalid SAF.lock.json schema.' }
        if ($state.PSObject.Properties['migration'] -and $state.migration) {
            if (-not $state.migration.PSObject.Properties['legacyHashes']) { throw 'Invalid migration state.' }
            foreach ($property in $state.migration.legacyHashes.PSObject.Properties) {
                if ($property.Name -notin $legacyPaths -or $property.Value -notmatch '^[0-9a-f]{64}$') { throw 'Invalid legacy metadata ownership.' }
            }
        }
        foreach ($relative in $legacyPaths) {
            $path = Get-SafTarget -Root $Root -Relative $relative
            if ([IO.File]::Exists($path)) {
                if (-not $state.PSObject.Properties['migration'] -or -not $state.migration -or
                    -not $state.migration.legacyHashes.PSObject.Properties[$relative] -or
                    (Get-SafExistingHash $path -Raw) -ne $state.migration.legacyHashes.$relative) { throw "Conflicting legacy and SAF state: $relative" }
            }
        }
        return $state
    }
    $state = [pscustomobject][ordered]@{ schemaVersion = 1; bootstrap = $null; installation = $null; audit = [pscustomobject]@{
        auditedFrameworkVersion = $null; lastMissingMaterialAskedAtUtc = $null; lastCompletedAtUtc = $null
    } }
    $legacyHashes = [ordered]@{}
    foreach ($relative in $legacyPaths) {
        $path = Get-SafTarget -Root $Root -Relative $relative
        if (-not [IO.File]::Exists($path)) { continue }
        $legacy = Read-SafJson ([IO.File]::ReadAllBytes($path))
        $legacyHashes[$relative] = Get-SafExistingHash $path -Raw
        if ($relative -eq 'AGENTS.lock.json') {
            if ($legacy.schemaVersion -ne 1 -or $legacy.frameworkVersion -notmatch '^\d+\.\d+\.\d+$' -or
                $legacy.commit -notmatch '^[0-9a-f]{40}$' -or $legacy.manifestSha256 -notmatch '^[0-9a-f]{64}$') { throw 'Invalid legacy installation state.' }
            $state.installation = $legacy
        } else {
            if ($legacy.schemaVersion -ne 2) { throw 'Unsupported legacy audit schema; resolve it before migrating.' }
            $state.audit = [pscustomobject]@{ auditedFrameworkVersion = $legacy.auditedFrameworkVersion
                lastMissingMaterialAskedAtUtc = $legacy.lastMissingMaterialAskedAtUtc; lastCompletedAtUtc = $legacy.lastCompletedAtUtc }
        }
    }
    if ($legacyHashes.Count -gt 0) { $state | Add-Member -NotePropertyName migration -NotePropertyValue ([pscustomobject]@{ legacyHashes = [pscustomobject]$legacyHashes }) }
    return $state
}

function Write-SafAtomic([string]$Path, [byte[]]$Bytes, [ValidateSet('Auto', 'Create', 'Replace')][string]$Mode = 'Auto', [ref]$Committed) {
    if ($null -ne $Committed) { $Committed.Value = $false }
    $temporary = $Path + '.' + [Guid]::NewGuid().ToString('N') + '.tmp'
    try {
        [IO.File]::WriteAllBytes($temporary, $Bytes)
        if ($Mode -eq 'Create') { [IO.File]::Move($temporary, $Path) }
        elseif ($Mode -eq 'Replace') { [IO.File]::Replace($temporary, $Path, [NullString]::Value) }
        elseif ([IO.File]::Exists($Path)) { [IO.File]::Replace($temporary, $Path, [NullString]::Value) }
        else { [IO.File]::Move($temporary, $Path) }
        if ($null -ne $Committed) { $Committed.Value = $true }
    } finally { if ([IO.File]::Exists($temporary)) { [IO.File]::Delete($temporary) } }
}

function Invoke-SafTransaction([string]$Root, [object[]]$Operations) {
    $Root = [IO.Path]::GetFullPath($Root).TrimEnd([char]92, [char]47)
    $config = Resolve-SafConfig -Root $Root
    $paths = @{}; $plans = New-Object 'Collections.Generic.List[object]'
    $journal = New-Object 'Collections.Generic.List[object]'; $parents = New-Object 'Collections.Generic.List[string]'
    foreach ($operation in $Operations) {
        Assert-SafWritable -Root $Root -Relative $operation.relative -Config $config
        $path = Get-SafTarget -Root $Root -Relative $operation.relative
        if ($paths.ContainsKey($path)) { throw "Duplicate transaction path: $($operation.relative)" }
        $beforeBytes = $null; $actual = $null
        if (Test-Path -LiteralPath $path) {
            if (-not [IO.File]::Exists($path)) { throw "Expected a regular file: $path" }
            $beforeBytes = [IO.File]::ReadAllBytes($path)
            $actual = Get-SafByteHash $beforeBytes
        }
        if ($operation.PSObject.Properties['expected'] -and $actual -ne $operation.expected) { throw "File changed during planning: $($operation.relative)" }
        $afterHash = if ($operation.delete) { $null } else { Get-SafByteHash $operation.bytes }
        $plan = [pscustomobject]@{ relative = $operation.relative; path = $path; operation = $operation
            beforeBytes = $beforeBytes; beforeHash = $actual; afterHash = $afterHash; committed = $false }
        $paths[$path] = $true; $plans.Add($plan)
    }
    try {
        foreach ($plan in $plans) {
            Assert-SafWritable -Root $Root -Relative $plan.relative -Config $config
            $path = Get-SafTarget -Root $Root -Relative $plan.relative
            if ((Get-SafExistingHash $path -Raw) -ne $plan.beforeHash) { throw "File changed during transaction: $($plan.relative)" }
            if ($plan.operation.delete -and $null -eq $plan.beforeHash) { continue }
            if (-not $plan.operation.delete) {
                $parent = [IO.Path]::GetDirectoryName($path); $missing = New-Object 'Collections.Generic.List[string]'
                while (-not [IO.Directory]::Exists($parent)) { $missing.Add($parent); $parent = [IO.Path]::GetDirectoryName($parent) }
                for ($i = $missing.Count - 1; $i -ge 0; $i--) {
                    $parentRelative = $missing[$i].Substring($Root.Length + 1).Replace([char]92, [char]47)
                    Assert-SafWritable -Root $Root -Relative $parentRelative -Config $config
                    if (-not [IO.Directory]::Exists($missing[$i])) { [void][IO.Directory]::CreateDirectory($missing[$i]); $parents.Add($missing[$i]) }
                }
            }
            Assert-SafWritable -Root $Root -Relative $plan.relative -Config $config
            if ((Get-SafExistingHash $path -Raw) -ne $plan.beforeHash) { throw "File changed before transaction write: $($plan.relative)" }
            # The atomic helper reports commitment before any later exception;
            # attempted writes alone never establish ownership for recovery.
            $journal.Add($plan)
            if ($plan.operation.delete) { [IO.File]::Delete($path); $plan.committed = $true }
            else {
                $mode = if ($null -eq $plan.beforeHash) { 'Create' } else { 'Replace' }
                $committed = $false
                try { Write-SafAtomic -Path $path -Bytes $plan.operation.bytes -Mode $mode -Committed ([ref]$committed) }
                finally { $plan.committed = $committed }
            }
            if ((Get-SafExistingHash $path -Raw) -ne $plan.afterHash) { throw "File changed after transaction write: $($plan.relative)" }
            if ($plan.relative -eq 'SAF.config.json') {
                $config.sourceHash = $plan.afterHash
            }
        }
    } catch {
        $failure = $_
        for ($i = $journal.Count - 1; $i -ge 0; $i--) {
            $plan = $journal[$i]
            if (-not $plan.committed) { continue }
            try {
                $path = Get-SafTarget -Root $Root -Relative $plan.relative
                $actual = Get-SafExistingHash $path -Raw
                if ($actual -eq $plan.beforeHash) { continue }
                if ($actual -ne $plan.afterHash) { Write-Warning "Recovery preserved externally changed content: $($plan.relative)"; continue }
                $currentConfig = Resolve-SafConfig -Root $Root
                $originalPolicy = [pscustomobject]@{ sourceHash = $currentConfig.sourceHash; project = $config.project }
                Assert-SafWritable -Root $Root -Relative $plan.relative -Config $originalPolicy
                Assert-SafWritable -Root $Root -Relative $plan.relative -Config $currentConfig
                if ((Get-SafExistingHash $path -Raw) -ne $plan.afterHash) { throw 'Recovery target changed; contents retained.' }
                if ($null -eq $plan.beforeBytes) { [IO.File]::Delete($path) }
                elseif ($null -eq $plan.afterHash) { Write-SafAtomic -Path $path -Bytes $plan.beforeBytes -Mode Create }
                else { Write-SafAtomic -Path $path -Bytes $plan.beforeBytes -Mode Replace }
            } catch { Write-Warning "Recovery needs manual review: $($plan.relative)" }
        }
        for ($i = $parents.Count - 1; $i -ge 0; $i--) {
            try {
                $relative = $parents[$i].Substring($Root.Length + 1).Replace([char]92, [char]47)
                $parent = Get-SafTarget -Root $Root -Relative $relative
                if ([IO.Directory]::Exists($parent) -and @(Get-ChildItem -LiteralPath $parent -Force).Count -eq 0) {
                    $currentConfig = Resolve-SafConfig -Root $Root
                    $originalPolicy = [pscustomobject]@{ sourceHash = $currentConfig.sourceHash; project = $config.project }
                    Assert-SafWritable -Root $Root -Relative $relative -Config $originalPolicy
                    Assert-SafWritable -Root $Root -Relative $relative -Config $currentConfig
                    [IO.Directory]::Delete($parent)
                }
            } catch { Write-Warning "Recovery retained directory for manual review: $($parents[$i])" }
        }
        throw $failure
    }
}

function Set-SafState([string]$Root, [ValidateSet('bootstrap', 'installation', 'audit')][string]$Section, $Value) {
    $lease = $null
    if (-not $script:SafOperationLocks.ContainsKey($Root) -or -not $script:SafOperationLocks[$Root].CanWrite) { $lease = Enter-SafOperation -Root $Root }
    try {
        $config = Resolve-SafConfig -Root $Root
        Assert-SafWritable -Root $Root -Relative 'SAF.lock.json' -Config $config
        $state = Read-SafState -Root $Root; $state.$Section = $Value
        $path = Get-SafTarget -Root $Root -Relative 'SAF.lock.json'
        Invoke-SafTransaction -Root $Root -Operations @([pscustomobject]@{ relative = 'SAF.lock.json'; bytes = ConvertTo-SafBytes $state
            delete = $false; expected = Get-SafExistingHash $path -Raw })
    } finally { if ($lease) { $lease.Dispose() } }
}

function New-SafBackup([string]$Root, [string[]]$RelativePaths, [string]$Reason) {
    $config = Resolve-SafConfig -Root $Root
    $relativeBase = '.agents/backup/' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ') + '-' + [Guid]::NewGuid().ToString('N')
    $base = Get-SafTarget -Root $Root -Relative $relativeBase
    Assert-SafWritable -Root $Root -Relative ($relativeBase + '/inventory.json') -Config $config
    $items = New-Object 'Collections.Generic.List[object]'
    foreach ($relative in @($RelativePaths | Sort-Object -Unique)) {
        if ($relative.StartsWith('.agent/', [StringComparison]::OrdinalIgnoreCase) -or $relative.StartsWith('.agents/backup/', [StringComparison]::OrdinalIgnoreCase) -or
            $relative.StartsWith('.agents/backups/', [StringComparison]::OrdinalIgnoreCase) -or $relative.StartsWith('.agents/saf_log/', [StringComparison]::OrdinalIgnoreCase) -or
            $relative -eq '.agents/operation.lock') { throw 'Backup cannot contain local runtime storage.' }
        $source = Get-SafTarget -Root $Root -Relative $relative
        if (-not [IO.File]::Exists($source)) { continue }
        $bytes = [IO.File]::ReadAllBytes($source)
        $target = Get-SafTarget -Root $Root -Relative ($relativeBase + '/files/' + $relative)
        Assert-SafWritable -Root $Root -Relative ($relativeBase + '/files/' + $relative) -Config $config
        [void][IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($target))
        [IO.File]::WriteAllBytes($target, $bytes)
        if ((Get-SafByteHash ([IO.File]::ReadAllBytes($target))) -ne (Get-SafByteHash $bytes)) { throw 'Backup verification failed.' }
        $items.Add([pscustomobject]@{ path = $relative; sha256 = Get-SafByteHash $bytes; size = $bytes.Length })
    }
    [void][IO.Directory]::CreateDirectory($base)
    Assert-SafWritable -Root $Root -Relative ($relativeBase + '/inventory.json') -Config $config
    [IO.File]::WriteAllBytes((Join-Path $base 'inventory.json'), (ConvertTo-SafBytes ([ordered]@{
        schemaVersion = 1; createdAtUtc = [DateTime]::UtcNow.ToString('o'); reason = $Reason; files = @($items.ToArray())
    })))
    return $base
}

function Write-SafLog([string]$Root, [string]$Action, [string]$Result, [int]$ExitCode, $Data) {
    try {
        $relative = '.agents/saf_log/' + [DateTime]::UtcNow.ToString('yyyy-MM-dd') + '.jsonl'
        $config = Resolve-SafConfig -Root $Root
        Assert-SafWritable -Root $Root -Relative $relative -Config $config
        $path = Get-SafTarget -Root $Root -Relative $relative
        [void][IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($path))
        # Callers supply only relative paths/status data, never arbitrary argv or remote URLs.
        $line = ([ordered]@{ schemaVersion = 1; operationId = $script:SafOperationId; startedAtUtc = $script:SafStartedAtUtc; completedAtUtc = [DateTime]::UtcNow.ToString('o')
            action = $Action; result = $Result; exitCode = $ExitCode; data = $Data } | ConvertTo-Json -Compress -Depth 12) + "`n"
        $stream = [IO.File]::Open($path, [IO.FileMode]::Append, [IO.FileAccess]::Write, [IO.FileShare]::Read)
        try { $bytes = $script:SafUtf8.GetBytes($line); $stream.Write($bytes, 0, $bytes.Length) } finally { $stream.Dispose() }
    } catch { Write-Warning 'SAF operation log could not be written; the reported operation result is unchanged.' }
}

function Get-SafRuntimeIgnoreOperation([string]$Root) {
    $path = Get-SafTarget -Root $Root -Relative '.gitignore'
    $body = if ([IO.File]::Exists($path)) { [IO.File]::ReadAllText($path, $script:SafUtf8) } else { '' }
    $lines = @($body.Replace("`r`n", "`n").Split("`n"))
    $missing = @(@('/.agents/backup/', '/.agents/saf_log/', '/.agents/operation.lock') | Where-Object { $_ -cnotin $lines })
    if ($missing.Count -eq 0) { return $null }
    if ($body.Length -gt 0 -and -not $body.EndsWith("`n")) { $body += "`n" }
    $body += ($missing -join "`n") + "`n"
    return [pscustomobject]@{ relative = '.gitignore'; bytes = $script:SafUtf8.GetBytes($body); delete = $false; expected = Get-SafExistingHash $path -Raw }
}

function Read-SafBlob([string]$Source, [string]$Commit, [string]$Relative) {
    Assert-SafRelative $Relative
    $tree = Invoke-SafGit -Arguments @('-C', $Source, 'ls-tree', $Commit, '--', $Relative) -Text
    if ($tree -notmatch '^(100644|100755) blob [0-9a-f]{40}\t') { throw "Source is not a regular Git blob: $Relative" }
    return ,(Invoke-SafGit -Arguments @('-C', $Source, 'show', ($Commit + ':' + $Relative)))
}

function Get-SafRelease([string]$Root, [string]$Repository, [string]$Ref, [string]$SourcePath) {
    $temporary = $null
    try {
        Assert-SafRepository $Repository
        if (-not $Ref -or $Ref.StartsWith('-')) { throw 'Invalid source ref.' }
        if ($SourcePath) {
            $source = [IO.Path]::GetFullPath($SourcePath)
            if ($source.Equals($Root, [StringComparison]::OrdinalIgnoreCase)) { throw 'SourcePath cannot equal the target repository.' }
            $commit = Invoke-SafGit -Arguments @('-C', $source, 'rev-parse', ($Ref + '^{commit}')) -Text
        } else {
            $temporary = Join-Path ([IO.Path]::GetTempPath()) ('saf-source-' + [Guid]::NewGuid().ToString('N'))
            [void](Invoke-SafGit -Arguments @('init', '--bare', '--quiet', $temporary))
            [void](Invoke-SafGit -Arguments @('-C', $temporary, 'fetch', '--quiet', '--depth=1', '--no-tags', $Repository, $Ref))
            $source = $temporary; $commit = Invoke-SafGit -Arguments @('-C', $source, 'rev-parse', 'FETCH_HEAD^{commit}') -Text
        }
        if ($commit -notmatch '^[0-9a-f]{40}$') { throw 'Source must resolve to one Git commit.' }
        $manifestBytes = Read-SafBlob -Source $source -Commit $commit -Relative 'SAF.json'
        $manifest = Read-SafJson $manifestBytes
        if (($manifest.schemaVersion -isnot [int] -and $manifest.schemaVersion -isnot [long]) -or $manifest.schemaVersion -ne 1 -or
            ($manifest.handlerProtocolVersion -isnot [int] -and $manifest.handlerProtocolVersion -isnot [long]) -or $manifest.handlerProtocolVersion -ne 3 -or
            -not $manifest.PSObject.Properties['configSupportVersion'] -or
            ($manifest.configSupportVersion -isnot [int] -and $manifest.configSupportVersion -isnot [long]) -or $manifest.configSupportVersion -ne 2) { throw '[SAF_REFRESH] Unsupported SAF configuration contract. Migrate project config to schema 2 and rerun the trusted remote scripts/init.ps1 bootstrap.' }
        if ($manifest.frameworkVersion -notmatch '^\d+\.\d+\.\d+$' -or [version]$manifest.frameworkVersion -lt [version]'2.0.0') { throw 'Expected a SAF 2 release >= 2.0.0.' }
        $payload = @{}
        foreach ($entry in @($manifest.files)) {
            Assert-SafRelative $entry.source; Assert-SafRelative $entry.target
            if ($payload.ContainsKey($entry.target) -or $entry.target -in @('SAF.json', 'SAF.lock.json')) { throw 'Duplicate or reserved SAF target.' }
            if ($entry.mode -eq 'managed') { Assert-SafManaged $entry.target }
            elseif ($entry.mode -eq 'seed') {
                if ($entry.target -notin @('SAF.config.json', 'debug.bat', 'docs/.gitkeep') -and -not $entry.target.StartsWith('docs-ai/current/', [StringComparison]::Ordinal)) { throw 'Invalid seed target.' }
            } else { throw 'Unknown SAF file mode.' }
            if ($entry.sha256 -notmatch '^[0-9a-f]{64}$') { throw 'Invalid SAF payload hash.' }
            $bytes = Read-SafBlob -Source $source -Commit $commit -Relative $entry.source
            if ((Get-SafHash $bytes) -ne $entry.sha256) { throw "SAF payload hash mismatch: $($entry.source)" }
            $payload[$entry.target] = [pscustomobject]@{ mode = $entry.mode; hash = $entry.sha256; bytes = $bytes }
        }
        if (@($manifest.bootstrapFiles).Count -ne $script:SafHandlers.Count -or
            @(Compare-Object ($script:SafHandlers | Sort-Object) (@($manifest.bootstrapFiles) | Sort-Object)).Count -ne 0) { throw 'Invalid bootstrap handler set.' }
        foreach ($required in ($script:SafHandlers + @('AGENTS.md', 'docs-ai/common/rules.md', 'docs-ai/common/codedesign.md', 'docs-ai/common/UIUX.md'))) {
            if (-not $payload.ContainsKey($required) -or $payload[$required].mode -ne 'managed') { throw "Missing managed payload: $required" }
        }
        foreach ($required in @('SAF.config.json', 'debug.bat', 'docs/.gitkeep', 'docs-ai/current/1_architecture.md', 'docs-ai/current/1_dependency.md',
            'docs-ai/current/1_changelogs/.gitkeep', 'docs-ai/current/1_bugreports/.gitkeep', 'docs-ai/current/1_archived/.gitkeep')) {
            if (-not $payload.ContainsKey($required) -or $payload[$required].mode -ne 'seed') { throw "Missing project seed: $required" }
        }
        $seedEntry = @($manifest.files | Where-Object target -eq 'SAF.config.json')[0]
        $seed = Read-SafJson $payload['SAF.config.json'].bytes
        Assert-SafConfigObject $seed @('schemaVersion') 'config seed'
        if ($seedEntry.source -cne 'templates/SAF.config.json' -or
            ($seed.schemaVersion -isnot [int] -and $seed.schemaVersion -isnot [long]) -or $seed.schemaVersion -ne 2) { throw 'Invalid neutral configuration seed.' }
        return [pscustomobject]@{ manifest = $manifest; manifestBytes = $manifestBytes; manifestHash = Get-SafHash $manifestBytes
            commit = $commit; repository = $Repository; ref = $Ref; payload = $payload }
    } finally { if ($temporary) { Remove-SafTemporary -Path $temporary -Prefix 'saf-source-' } }
}

function Remove-SafTemporary([string]$Path, [string]$Prefix) {
    if (-not [IO.Directory]::Exists($Path)) { return }
    $absolute = [IO.Path]::GetFullPath($Path)
    $base = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd([char]92, [char]47)
    if (-not [IO.Path]::GetDirectoryName($absolute).Equals($base, [StringComparison]::OrdinalIgnoreCase) -or
        [IO.Path]::GetFileName($absolute) -notmatch ('^' + [regex]::Escape($Prefix) + '[0-9a-f]{32}$')) { throw 'Refusing unexpected temporary cleanup path.' }
    Remove-Item -LiteralPath $absolute -Recurse -Force
}
