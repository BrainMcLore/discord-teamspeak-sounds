$ErrorActionPreference='Stop'
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if(!(Test-Path -LiteralPath $compiler)){$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'}
if(!(Test-Path -LiteralPath $compiler)){throw '.NET Framework 4 compiler is required.'}
$wpf=Join-Path (Split-Path $compiler) 'WPF'
$icon=Join-Path $PSScriptRoot 'launcher\assets\soundspace.ico'
& $compiler /nologo /target:winexe /optimize+ /utf8output /reference:System.dll /reference:System.Core.dll /reference:System.Security.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /reference:System.Web.Extensions.dll /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll (('/reference:')+(Join-Path $wpf 'WindowsBase.dll')) (('/reference:')+(Join-Path $wpf 'PresentationCore.dll')) (('/reference:')+(Join-Path $wpf 'PresentationFramework.dll')) (('/reference:')+(Join-Path $wpf '..\System.Xaml.dll')) (('/win32icon:')+$icon) (('/out:')+(Join-Path $PSScriptRoot 'Discord-Sound-Launcher.exe')) (Join-Path $PSScriptRoot 'launcher\Launcher.cs') (Join-Path $PSScriptRoot 'launcher\ModernLauncher.cs') (Join-Path $PSScriptRoot 'launcher\AccountValidation.cs') (Join-Path $PSScriptRoot 'launcher\ModernAccount.cs')
if($LASTEXITCODE -ne 0){throw 'Launcher build failed.'}
Write-Host 'Built Discord-Sound-Launcher.exe'
