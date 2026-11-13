@echo off

pwsh -NoLogo -NoProfile -File "%~dp0Publish-Helper.ps1" %*

exit /b %errorlevel%
