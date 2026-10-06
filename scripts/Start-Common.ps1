Set-StrictMode -Version Latest

function Get-StartCatalog {
    param([Parameter(Mandatory = $true)][string]$RepositoryRoot)

    $root = [System.IO.Path]::GetFullPath($RepositoryRoot)
    $settings = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'StartSettings.json') -Raw | ConvertFrom-Json
    if ($settings.schemaVersion -ne 1) { throw 'Unsupported launcher settings schema.' }
    $seen = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
    $rootPrefix = $root.TrimEnd([char[]]@('\', '/')) + [System.IO.Path]::DirectorySeparatorChar
    foreach ($entry in $settings.projects) {
        if ($entry.id -notmatch '^[a-z][a-z0-9-]*$' -or -not $seen.Add([string]$entry.id)) {
            throw 'Launcher project IDs must be unique, safe names.'
        }
        if ([System.IO.Path]::IsPathRooted($entry.path)) { throw 'Launcher paths must be repository-relative.' }
        $projectPath = [System.IO.Path]::GetFullPath((Join-Path $root $entry.path))
        if (-not $projectPath.StartsWith($rootPrefix, [StringComparison]::OrdinalIgnoreCase) -or
            [System.IO.Path]::GetExtension($projectPath) -ne '.csproj' -or
            -not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
            throw "Launcher project path is missing or outside the repository: $($entry.path)"
        }
        if ($entry.name -ne [System.IO.Path]::GetFileNameWithoutExtension($projectPath) -or
            -not $seen.Add([string]$entry.name)) { throw 'Launcher names must match unique project file names.' }
        [xml]$projectXml = Get-Content -LiteralPath $projectPath -Raw
        $sdk = [string]$projectXml.Project.GetAttribute('Sdk')
        $outputs = @($projectXml.SelectNodes('/Project/PropertyGroup/OutputType') | ForEach-Object { $_.InnerText })
        if ($sdk -ne 'Microsoft.NET.Sdk.Web' -and -not (@('Exe', 'WinExe') | Where-Object { $outputs -contains $_ })) {
            throw "A library cannot be a startup project: $($entry.name)"
        }
        if ($entry.kind -notin @('web', 'wpf', 'winui')) { throw 'Unknown startup project kind.' }
        if ($entry.launchProfile) {
            $profilePath = Join-Path ([System.IO.Path]::GetDirectoryName($projectPath)) 'Properties/launchSettings.json'
            $profiles = (Get-Content -LiteralPath $profilePath -Raw | ConvertFrom-Json).profiles
            $profile = $profiles.PSObject.Properties[[string]$entry.launchProfile]
            if ($null -eq $profile -or $profile.Value.commandName -ne 'Project') {
                throw "Missing project launch profile: $($entry.name) / $($entry.launchProfile)"
            }
        }
        $entry | Add-Member -NotePropertyName FullPath -NotePropertyValue $projectPath
    }
    if (-not (@($settings.projects | Where-Object { $_.id -eq $settings.defaultProject }).Count -eq 1)) {
        throw 'The default startup project must exist in the allowlist.'
    }
    return $settings
}

function Resolve-StartProject {
    param([Parameter(Mandatory = $true)]$Catalog, [Parameter(Mandatory = $true)][string]$Selection)

    $selectionText = $Selection.Trim()
    $number = 0
    if ([int]::TryParse($selectionText, [ref]$number) -and $number -ge 1 -and $number -le $Catalog.projects.Count) {
        return $Catalog.projects[$number - 1]
    }
    $matches = @($Catalog.projects | Where-Object { $_.id -eq $selectionText -or $_.name -eq $selectionText })
    if ($matches.Count -ne 1) { throw "Unknown startup project '$Selection'. Use -List to see the allowlist." }
    return $matches[0]
}

function Get-StartPlan {
    param(
        [Parameter(Mandatory = $true)]$Project,
        [ValidateSet('Release', 'Debug')][string]$Configuration = 'Release',
        [ValidateSet('Auto', 'x86', 'x64', 'ARM64')][string]$Platform = 'Auto',
        [bool]$WindowsHost = ([Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT),
        [string]$MachineArchitecture = $(if ($env:PROCESSOR_ARCHITEW6432) { $env:PROCESSOR_ARCHITEW6432 } else { $env:PROCESSOR_ARCHITECTURE })
    )

    if ($Project.kind -ne 'web' -and -not $WindowsHost) { throw "$($Project.name) can only run on Windows." }
    if ($Project.kind -ne 'winui' -and $Platform -ne 'Auto') { throw '-Platform applies only to the WinUI3 Gallery.' }
    $platformArguments = @()
    if ($Project.kind -eq 'winui') {
        if ($Platform -eq 'Auto') {
            $Platform = switch ($MachineArchitecture.ToUpperInvariant()) {
                'ARM64' { 'ARM64' }
                'AMD64' { 'x64' }
                'X64' { 'x64' }
                'X86' { 'x86' }
                default { throw 'Cannot determine the WinUI architecture. Supply -Platform x86, x64 or ARM64.' }
            }
        }
        $runtime = 'win-' + $Platform.ToLowerInvariant()
        $platformArguments = @('--runtime', $runtime, "-p:Platform=$Platform")
    }
    $runArguments = @('run', '--project', $Project.FullPath, '--configuration', $Configuration, '--no-restore', '--no-build') + $platformArguments
    if ($Project.launchProfile) { $runArguments += @('--launch-profile', [string]$Project.launchProfile) }
    else { $runArguments += '--no-launch-profile' }
    return @(
        [pscustomobject]@{ Name = 'restore'; Arguments = @('restore', $Project.FullPath) + $platformArguments },
        [pscustomobject]@{ Name = 'build'; Arguments = @('build', $Project.FullPath, '--configuration', $Configuration, '--no-restore') + $platformArguments },
        [pscustomobject]@{ Name = 'run'; Arguments = $runArguments }
    )
}

function Invoke-StartSteps {
    param(
        [Parameter(Mandatory = $true)][string]$DotnetPath,
        [Parameter(Mandatory = $true)][object[]]$Steps,
        [scriptblock]$CommandRunner = {
            param([string]$Executable, [string[]]$CommandArguments)
            # Native stderr must not become a terminating PowerShell 5.1 error before its exit code is read.
            $ErrorActionPreference = 'Continue'
            & $Executable @CommandArguments | Out-Host
            return $LASTEXITCODE
        }
    )

    foreach ($step in $Steps) {
        Write-Host ("[{0}] dotnet {1}" -f $step.Name, ($step.Arguments -join ' '))
        $commandExitCode = & $CommandRunner $DotnetPath ([string[]]$step.Arguments)
        if ($commandExitCode -isnot [int]) { throw 'The command runner did not return a single process exit code.' }
        if ($commandExitCode -ne 0) { return $commandExitCode }
    }
    return 0
}
