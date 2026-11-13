@echo off
setlocal DisableDelayedExpansion
set "SAF_LOCAL_SHELL=powershell.exe"
where pwsh.exe >nul 2>&1
if not errorlevel 1 set "SAF_LOCAL_SHELL=pwsh.exe"
where %SAF_LOCAL_SHELL% >nul 2>&1
if errorlevel 1 (
    echo [SAF] Windows PowerShell 5.1 or PowerShell 7 is required.
    if "%~1"=="" pause
    exit /b 1
)
if not exist "%~dp0scripts\local.ps1" (
    echo [SAF] Local controller is missing. Repair the SAF installation.
    if "%~1"=="" pause
    exit /b 1
)
if not exist "%~dp0scripts\saf-core.ps1" (
    echo [SAF] Shared helper is missing. Rerun trusted remote scripts/init.ps1 to repair the handlers.
    if "%~1"=="" pause
    exit /b 4
)
"%SAF_LOCAL_SHELL%" -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\local.ps1" %*
exit /b %ERRORLEVEL%
