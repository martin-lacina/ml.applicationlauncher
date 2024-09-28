@echo off
echo Launching PowerShell Core publisher...

pwsh -Command "%~dp0Build\Scripts\Publish-Release.ps1" -Config 'Debug'
