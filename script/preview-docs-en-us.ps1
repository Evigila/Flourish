[CmdletBinding()]
param(
    [ValidateNotNullOrEmpty()]
    [string]$Hostname = '127.0.0.1',

    [ValidateRange(1, 65535)]
    [int]$Port = 8098
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot

Push-Location -LiteralPath $repositoryRoot
try {
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to restore the local .NET tools (exit code $LASTEXITCODE)."
    }

    dotnet tool run docfx 'docs\docfx.en-us.json'
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build the English documentation (exit code $LASTEXITCODE)."
    }

    Write-Host "English documentation preview: http://${Hostname}:$Port/"
    Write-Host 'Press Ctrl+C to stop the preview server.'

    dotnet tool run docfx serve 'docs\_site\en-us' --hostname $Hostname --port $Port
    if ($LASTEXITCODE -ne 0) {
        throw "The English documentation preview server stopped with exit code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}
