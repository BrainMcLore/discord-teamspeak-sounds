param([Parameter(Mandatory=$true)][string]$Version,[string]$Destination=(Join-Path $PSScriptRoot 'dist'))
$ErrorActionPreference='Stop'
if($Version -notmatch '^v[0-9]+\.[0-9]+\.[0-9]+$'){throw 'Use a version such as v2.0.0.'}
$stage=Join-Path ([IO.Path]::GetTempPath()) ('discord-sounds-release-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $stage,$Destination -Force|Out-Null
$source=Join-Path $stage 'source.zip'
git -C $PSScriptRoot archive --format=zip --prefix=Discord-TeamSpeak-Sounds/ ("--output=$source") HEAD
if($LASTEXITCODE -ne 0){throw 'git archive failed.'}
Expand-Archive -LiteralPath $source -DestinationPath (Join-Path $stage 'source')
$package=Join-Path $stage 'source\Discord-TeamSpeak-Sounds'
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $package 'Build-Launcher.ps1')
if($LASTEXITCODE -ne 0){throw 'Launcher build failed.'}
$zip=Join-Path $Destination ('Discord-TeamSpeak-Sounds-'+$Version+'.zip')
Compress-Archive -LiteralPath $package -DestinationPath $zip -Force
$hash=(Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash.ToLowerInvariant()
[IO.File]::WriteAllText((Join-Path $Destination ('SHA256SUMS-'+$Version+'.txt')),($hash+'  '+[IO.Path]::GetFileName($zip)+[Environment]::NewLine),[Text.Encoding]::ASCII)
Write-Host $zip
Write-Host ('SHA256: '+$hash)
