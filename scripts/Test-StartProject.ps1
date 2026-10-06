[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Start-Common.ps1')
$taskRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$catalog = Get-StartCatalog -RepositoryRoot $taskRoot
$script:checkCount = 0

function Assert-StartCheck {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { throw $Message }
    $script:checkCount++
}

function Assert-StartThrows {
    param([scriptblock]$Action, [string]$Message)
    $threw = $false
    try { & $Action | Out-Null }
    catch { $threw = $true }
    Assert-StartCheck $threw $Message
}

Assert-StartCheck ($catalog.projects.Count -eq 4) 'The allowlist must contain the three real Galleries and the Native diagnostic host.'
Assert-StartCheck ($catalog.defaultProject -eq 'blazor') 'The default selection must be Blazor.'
foreach ($entry in $catalog.projects) {
    Assert-StartCheck ((Resolve-StartProject $catalog $entry.id).name -eq $entry.name) 'Aliases must resolve only allowlisted projects.'
    Assert-StartCheck ((Resolve-StartProject $catalog $entry.name.ToUpperInvariant()).id -eq $entry.id) 'Project names must resolve case-insensitively.'
    Assert-StartCheck (Test-Path -LiteralPath $entry.FullPath -PathType Leaf) 'Allowlisted paths must point at real runnable project files.'
}
Assert-StartCheck ((Resolve-StartProject $catalog '1').id -eq 'blazor') 'Menu index 1 must select Blazor.'
Assert-StartCheck ((Resolve-StartProject $catalog ' 4 ').id -eq 'native') 'Trimmed menu selection must work.'
foreach ($invalid in @('0', '5', '-1', '../Flourish.Core', 'Flourish.Core', 'blazor;exit', 'C:\evil.csproj')) {
    Assert-StartThrows { Resolve-StartProject $catalog $invalid } "Unsafe/unknown selection was accepted: $invalid"
}

$blazor = Resolve-StartProject $catalog 'blazor'
$plan = @(Get-StartPlan $blazor)
Assert-StartCheck ($plan.Count -eq 3) 'Startup must perform explicit restore, build, run in sequence.'
Assert-StartCheck (($plan.Name -join ',') -eq 'restore,build,run') 'Startup step order changed.'
Assert-StartCheck ($plan[1].Arguments[3] -eq 'Release') 'Release must be the default build configuration.'
Assert-StartCheck ($plan[1].Arguments -contains '--no-restore') 'Build must reuse the checked restore.'
Assert-StartCheck ($plan[2].Arguments -contains '--no-build') 'Run must reuse the checked build.'
Assert-StartCheck ($plan[2].Arguments -contains '--no-restore') 'Run must not hide an additional restore.'
Assert-StartCheck (($plan[2].Arguments[-2..-1] -join ',') -eq '--launch-profile,http') 'Web startup must use the real HTTP profile.'
$native = @(Get-StartPlan (Resolve-StartProject $catalog 'native'))
Assert-StartCheck (($native[2].Arguments[-2..-1] -join ',') -eq '--launch-profile,http') 'Native sample must retain its own HTTP profile.'
$wpf = Resolve-StartProject $catalog 'wpf'
$wpfPlan = @(Get-StartPlan $wpf -WindowsHost $true)
Assert-StartCheck ($wpfPlan[2].Arguments -contains '--no-launch-profile') 'Desktop startup must not invent a launch profile.'
Assert-StartThrows { Get-StartPlan $wpf -WindowsHost $false } 'WPF must fail closed off Windows.'
Assert-StartThrows { Get-StartPlan $blazor -Platform x64 } 'A desktop platform must not affect Blazor builds.'
$winui = Resolve-StartProject $catalog 'winui'
Assert-StartThrows { Get-StartPlan $winui -WindowsHost $false } 'WinUI must fail closed off Windows.'
foreach ($architecture in @('x86', 'x64', 'ARM64')) {
    $desktopPlan = @(Get-StartPlan $winui -Platform $architecture -WindowsHost $true)
    foreach ($step in $desktopPlan) {
        Assert-StartCheck ($step.Arguments -contains "-p:Platform=$architecture") 'All WinUI steps must share the explicit platform.'
        Assert-StartCheck ($step.Arguments -contains ('win-' + $architecture.ToLowerInvariant())) 'All WinUI steps must share the runtime.'
    }
}
$armPlan = @(Get-StartPlan $winui -WindowsHost $true -MachineArchitecture ARM64)
Assert-StartCheck ($armPlan[0].Arguments -contains '-p:Platform=ARM64') 'Auto platform must recognize native ARM64.'
Assert-StartThrows { Get-StartPlan $winui -WindowsHost $true -MachineArchitecture unknown } 'Unknown native architecture must fail closed.'
$debugPlan = @(Get-StartPlan $blazor -Configuration Debug)
Assert-StartCheck ($debugPlan[1].Arguments -contains 'Debug') 'An explicit configuration must be honored (plan only; no Debug build occurs in these tests).'
$spacedProject = [pscustomobject]@{ name = 'Gallery.Space'; FullPath = 'C:\a repository with spaces\Gallery.Space.csproj'; kind = 'web'; launchProfile = 'http' }
$spacedPlan = @(Get-StartPlan $spacedProject)
Assert-StartCheck ($spacedPlan[0].Arguments[1] -eq $spacedProject.FullPath) 'A spaced project path must remain one argument.'
Assert-StartCheck ($spacedPlan[2].Arguments[2] -eq $spacedProject.FullPath) 'Run must not split or concatenate a spaced project path.'

foreach ($failureIndex in @(0, 1, 2, 3)) {
    $state = [pscustomobject]@{ Calls = (New-Object System.Collections.ArrayList); Index = 0 }
    $runner = {
        param([string]$Executable, [string[]]$CommandArguments)
        [void]$state.Calls.Add([pscustomobject]@{ Executable = $Executable; Arguments = $CommandArguments })
        $state.Index++
        if ($state.Index -eq ($failureIndex + 1)) { return 17 }
        return 0
    }.GetNewClosure()
    $exitCode = Invoke-StartSteps -DotnetPath 'C:\SDK location with spaces\dotnet.exe' -Steps $plan -CommandRunner $runner
    $expectedCalls = [Math]::Min($failureIndex + 1, 3)
    Assert-StartCheck ($state.Calls.Count -eq $expectedCalls) 'Failure must prevent all later launch steps.'
    Assert-StartCheck ($exitCode -eq $(if ($failureIndex -lt 3) { 17 } else { 0 })) 'Process exit codes must be propagated exactly.'
    Assert-StartCheck ($state.Calls[0].Executable -eq 'C:\SDK location with spaces\dotnet.exe') 'An executable path must remain a separate value.'
}

# Invoke only discovery/failure cases, never a project, SDK build, server or desktop window.
$taskPowerShell = if ([Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT) {
    Join-Path $env:SystemRoot 'System32/WindowsPowerShell/v1.0/powershell.exe'
} else { (Get-Command pwsh -CommandType Application).Source }
function Invoke-StartCliCheck {
    param([string[]]$Arguments, [bool]$ShouldPass, [string]$ExpectedOutput)
    $ErrorActionPreference = 'Continue'
    $output = & $taskPowerShell -NoLogo -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'Start-Project.ps1') @Arguments 2>&1
    $code = $LASTEXITCODE
    Assert-StartCheck (($code -eq 0) -eq $ShouldPass) "CLI returned unexpected exit code $code. $output"
    Assert-StartCheck (($output -join [Environment]::NewLine) -like "*$ExpectedOutput*") "CLI output did not contain $ExpectedOutput. $output"
}
Push-Location -LiteralPath ([System.IO.Path]::GetTempPath())
try {
    Invoke-StartCliCheck @('-Help') $true 'Visual Studio'
    Invoke-StartCliCheck @('-List') $true 'Tests.Flourish.Blazor.Native'
    Invoke-StartCliCheck @('-Project', 'Flourish.Core') $false 'Unknown startup project'
    Invoke-StartCliCheck @('-Project', 'blazor;exit') $false 'Unknown startup project'
    Invoke-StartCliCheck @('-Project', '../unsafe.csproj') $false 'Unknown startup project'
    Invoke-StartCliCheck @('-NonInteractive') $false 'Non-interactive startup requires -Project'
    Invoke-StartCliCheck @('-Project', 'blazor', '-Platform', 'x64') $false '-Platform applies only'
    Invoke-StartCliCheck @('-Project', 'blazor', '-Configuration', 'Invalid') $false 'ValidateSet'
    if ([Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT) {
        $taskOldPath = $env:PATH
        try {
            $env:PATH = Join-Path $env:SystemRoot 'System32'
            Invoke-StartCliCheck @('-List') $true 'Gallery.Flourish.Blazor'
            Invoke-StartCliCheck @('-Project', 'blazor') $false 'SDK is not on PATH'
        }
        finally { $env:PATH = $taskOldPath }
        function Invoke-StartBatchCheck {
            param([string]$BatchPath, [ValidateSet('-List', '-Project invalid')][string]$Arguments)
            $processInfo = New-Object System.Diagnostics.ProcessStartInfo
            $processInfo.FileName = Join-Path $env:SystemRoot 'System32/cmd.exe'
            # cmd /s /c requires outer quotes in addition to quotes around the batch path.
            $processInfo.Arguments = '/d /s /c ""' + $BatchPath + '" ' + $Arguments + '"'
            $processInfo.UseShellExecute = $false
            $processInfo.CreateNoWindow = $true
            $processInfo.RedirectStandardOutput = $true
            $processInfo.RedirectStandardError = $true
            $process = [System.Diagnostics.Process]::Start($processInfo)
            try {
                $output = $process.StandardOutput.ReadToEnd() + $process.StandardError.ReadToEnd()
                $process.WaitForExit()
                return [pscustomobject]@{ ExitCode = $process.ExitCode; Output = $output }
            }
            finally { $process.Dispose() }
        }
        $batchOutput = Invoke-StartBatchCheck (Join-Path $taskRoot 'start.bat') '-List'
        Assert-StartCheck ($batchOutput.ExitCode -eq 0) 'The root batch must list projects correctly from another working directory.'
        Assert-StartCheck ($batchOutput.Output -like '*Gallery.Flourish.Blazor*') 'The root batch did not call its own repository script.'
        $batchFailure = Invoke-StartBatchCheck (Join-Path $taskRoot 'start.bat') '-Project invalid'
        Assert-StartCheck ($batchFailure.ExitCode -ne 0) 'The root batch must preserve a failed selection exit code.'

        # Copy only launcher/project metadata into a disposable spaced-path fixture, never a build output.
        $fixtureName = 'flourish start tests ' + [Guid]::NewGuid().ToString('N')
        $fixtureRoot = [System.IO.Path]::GetFullPath((Join-Path ([System.IO.Path]::GetTempPath()) $fixtureName))
        [void](New-Item -ItemType Directory -Path $fixtureRoot)
        try {
            $fixtureFiles = @('start.bat', 'scripts/Start-Project.ps1', 'scripts/Start-Common.ps1', 'scripts/StartSettings.json')
            foreach ($entry in $catalog.projects) {
                $fixtureFiles += [string]$entry.path
                if ($entry.launchProfile) {
                    $fixtureFiles += Join-Path ([System.IO.Path]::GetDirectoryName($entry.path)) 'Properties/launchSettings.json'
                }
            }
            foreach ($relativePath in $fixtureFiles) {
                $destination = Join-Path $fixtureRoot $relativePath
                [void](New-Item -ItemType Directory -Path ([System.IO.Path]::GetDirectoryName($destination)) -Force)
                Copy-Item -LiteralPath (Join-Path $taskRoot $relativePath) -Destination $destination
            }
            $spacedBatch = Invoke-StartBatchCheck (Join-Path $fixtureRoot 'start.bat') '-List'
            Assert-StartCheck ($spacedBatch.ExitCode -eq 0) "A spaced repository path failed: $($spacedBatch.Output)"
            Assert-StartCheck ($spacedBatch.Output -like '*Gallery.Flourish.WPF*') 'Spaced-path batch must resolve its own project metadata.'
        }
        finally {
            $tempPrefix = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath()).TrimEnd([char[]]@('\', '/')) + [System.IO.Path]::DirectorySeparatorChar
            if (-not $fixtureRoot.StartsWith($tempPrefix, [StringComparison]::OrdinalIgnoreCase) -or
                [System.IO.Path]::GetFileName($fixtureRoot) -ne $fixtureName) { throw 'Refusing unsafe fixture cleanup.' }
            Remove-Item -LiteralPath $fixtureRoot -Recurse -Force
        }
    }
}
finally { Pop-Location }

Write-Host "PASS: $script:checkCount launcher checks (no project was built or started)."
