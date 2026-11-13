@echo off
setlocal DisableDelayedExpansion
set "SAF_PROJECT_ROOT=%~dp0."
set "SAF_PAUSE=0"
if "%~1"=="" set "SAF_PAUSE=1"
set "SAF_SHELL=powershell.exe"
where pwsh.exe >nul 2>&1
if not errorlevel 1 set "SAF_SHELL=pwsh.exe"
if not exist "%~dp0scripts\saf-core.ps1" (
    echo [SAF] Missing shared helper. Rerun the trusted remote scripts/init.ps1 bootstrap.
    if "%SAF_PAUSE%"=="1" pause
    exit /b 4
)
if not exist "%~dp0scripts\origin.ps1" (
    echo [SAF] Missing handler. Rerun the trusted remote scripts/init.ps1 bootstrap.
    if "%SAF_PAUSE%"=="1" pause
    exit /b 4
)
rem Parse all finishing instructions before a compatible update can replace this BAT.
(
    "%SAF_SHELL%" -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\origin.ps1" %*
    call set "SAF_RESULT=%%ERRORLEVEL%%"
    if "%SAF_PAUSE%"=="1" pause
    call exit /b %%SAF_RESULT%%
)
