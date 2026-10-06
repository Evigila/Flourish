[CmdletBinding()]
param(
    [string]$Project,
    [ValidateSet('Release', 'Debug')][string]$Configuration = 'Release',
    [ValidateSet('Auto', 'x86', 'x64', 'ARM64')][string]$Platform = 'Auto',
    [switch]$List,
    [switch]$Help,
    [switch]$NonInteractive
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Start-Common.ps1')

try {
    if ($Help) {
        Write-Host @'
Flourish startup (no Visual Studio needed by the launcher)
  start.bat                                  Select a project; Enter selects the Blazor Gallery.
  start.bat -List                            List the fixed startup-project allowlist.
  start.bat -Project blazor                  Restore, build Release and run the Blazor Gallery.
  start.bat -Project Gallery.Flourish.WPF     Run the WPF Gallery on Windows.
  start.bat -Project native                  Run the Framework-only diagnostic Web sample.
  start.bat -Project winui -Platform x64      Run the existing WinUI3 placeholder on Windows.
  start.bat -Project blazor -Configuration Release -NonInteractive

PowerShell 5.1+ and the .NET SDK selected by global.json are required.
Only these aliases, project names or menu numbers are accepted, never arbitrary paths or commands.
Release is the default; -Configuration Debug is an explicit opt-in.
Web apps use their existing http profiles (Development), independently of the build configuration.
WinUI requires compatible existing Windows/XAML build tools; desktop startup is not certified here.
The script does not install SDKs/workloads, clear caches, publish packages, terminate other processes
or open a browser. Normal restore may download the projects' existing NuGet dependencies.
Stop the selected application with Ctrl+C; address/port conflicts are reported without stopping it.
Use PowerShell directly on other OSes: ./scripts/Start-Project.ps1 -Project blazor
'@
        exit 0
    }

    $taskRepositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
    $catalog = Get-StartCatalog -RepositoryRoot $taskRepositoryRoot
    if ($List -or [string]::IsNullOrWhiteSpace($Project)) {
        for ($index = 0; $index -lt $catalog.projects.Count; $index++) {
            $entry = $catalog.projects[$index]
            Write-Host ("{0}. {1} [{2}] - {3}" -f ($index + 1), $entry.name, $entry.id, $entry.description)
        }
        if ($List) { exit 0 }
        if ($NonInteractive -or [Console]::IsInputRedirected) { throw 'Non-interactive startup requires -Project. Use -List or -Help for discovery.' }
        $Project = Read-Host "Project (Enter = $($catalog.defaultProject); 0 = cancel)"
        if ($Project.Trim() -eq '0') { exit 0 }
        if ([string]::IsNullOrWhiteSpace($Project)) { $Project = $catalog.defaultProject }
    }
    $selectedProject = Resolve-StartProject -Catalog $catalog -Selection $Project
    $steps = @(Get-StartPlan -Project $selectedProject -Configuration $Configuration -Platform $Platform)
    Write-Host $selectedProject.description
    $sdkSettings = Get-Content -LiteralPath (Join-Path $taskRepositoryRoot 'global.json') -Raw | ConvertFrom-Json
    $dotnet = Get-Command dotnet -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($null -eq $dotnet) { throw "The .NET SDK is not on PATH. Install a compatible SDK for global.json ($($sdkSettings.sdk.version)); no Visual Studio is needed for Blazor/WPF." }

    Push-Location -LiteralPath $taskRepositoryRoot
    try {
        # Running at the repository root honors global.json even when the caller starts elsewhere.
        $taskPreviousErrorAction = $ErrorActionPreference
        try {
            $ErrorActionPreference = 'Continue'
            $sdkOutput = & $dotnet.Source --version 2>&1
            $sdkExitCode = $LASTEXITCODE
        }
        finally { $ErrorActionPreference = $taskPreviousErrorAction }
        if ($sdkExitCode -ne 0) {
            Write-Host ($sdkOutput -join [Environment]::NewLine)
            throw "No SDK compatible with global.json ($($sdkSettings.sdk.version), $($sdkSettings.sdk.rollForward)) is available. The launcher never installs it automatically."
        }
        Write-Host ("Using .NET SDK {0}; configuration {1}." -f ($sdkOutput -join ''), $Configuration)
        $taskExitCode = Invoke-StartSteps -DotnetPath $dotnet.Source -Steps $steps
    }
    finally { Pop-Location }
    exit $taskExitCode
}
catch {
    Write-Error $_ -ErrorAction Continue
    exit 1
}
