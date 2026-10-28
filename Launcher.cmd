@echo off
if exist "%~dp0Discord-Sound-Launcher.exe" (
 start "" "%~dp0Discord-Sound-Launcher.exe"
) else (
 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Build-Launcher.ps1"
 if errorlevel 1 pause
 if exist "%~dp0Discord-Sound-Launcher.exe" start "" "%~dp0Discord-Sound-Launcher.exe"
)
