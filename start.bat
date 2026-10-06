@echo off
setlocal

rem Windows PowerShell is built into Windows; Visual Studio and PowerShell 7 are not required.
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\Start-Project.ps1" %*
exit /b %errorlevel%
