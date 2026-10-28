param(
 [string]$InstallRoot = (Join-Path $env:LOCALAPPDATA 'DiscordTeamSpeakSounds'),
 [switch]$DownloadOnly,
 [switch]$Launcher
)
$ErrorActionPreference = 'Stop'
if(!$DownloadOnly -and !$Launcher -and @(Get-Process Discord -ErrorAction SilentlyContinue).Count){
 throw 'Completely quit Discord using its tray icon, then run this command again.'
}
[Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12
$headers = @{'User-Agent'='Discord-TeamSpeak-Sounds-Installer'; Accept='application/vnd.github+json'}
$release = Invoke-RestMethod -Uri ('https://api.github.com/repos/BrainMcLore/discord-teamspeak-sounds/releases/latest?request='+[DateTime]::UtcNow.Ticks) -Headers $headers -TimeoutSec 30
$archives = @($release.assets | Where-Object name -Match '^Discord-TeamSpeak-Sounds-v[0-9.]+\.zip$')
$checksums = @($release.assets | Where-Object name -Match '^SHA256SUMS(?:-v[0-9.]+)?\.txt$')
if($archives.Count -ne 1 -or $checksums.Count -ne 1){throw 'The latest release does not contain a unique ZIP and checksum file.'}
$archive = $archives[0]
$packageId = $release.tag_name + '-' + [Guid]::NewGuid().ToString('N')
if($packageId -notmatch '^v[0-9.]+-[a-f0-9]+$'){throw 'Unexpected release version.'}
$destination = Join-Path ([IO.Path]::GetFullPath($InstallRoot)) ('packages\'+$packageId)
New-Item -ItemType Directory -Path $destination -Force | Out-Null
$zipPath = Join-Path $destination $archive.name
$checksumPath = Join-Path $destination $checksums[0].name
Write-Host ('Downloading '+$release.tag_name+'...')
foreach($asset in @($archive,$checksums[0])){
 if($asset.browser_download_url -notlike 'https://github.com/BrainMcLore/discord-teamspeak-sounds/releases/download/*'){throw 'Unexpected release download URL.'}
 $target = if($asset.name -eq $archive.name){$zipPath}else{$checksumPath}
 Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $target -UseBasicParsing -TimeoutSec 120
}
$expected = @()
foreach($line in (Get-Content -LiteralPath $checksumPath)){
 if($line -match '^([a-fA-F0-9]{64})\s+\*?(.+)$' -and $Matches[2] -eq $archive.name){$expected += $Matches[1].ToLowerInvariant()}
}
if($expected.Count -ne 1){throw 'Checksum entry missing or ambiguous.'}
$actual = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
if($actual -ne $expected[0]){throw 'ZIP checksum mismatch. Nothing was applied.'}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::OpenRead($zipPath)
try{
 foreach($entry in $zip.Entries){
  $parts = $entry.FullName -split '[/\\]'
  if($entry.FullName -match '^[/\\]|:' -or $parts -contains '..'){throw 'Unsafe archive entry. Nothing was applied.'}
 }
}finally{$zip.Dispose()}
$unpacked = Join-Path $destination 'unpacked'
Expand-Archive -LiteralPath $zipPath -DestinationPath $unpacked
$package = Join-Path $unpacked 'Discord-TeamSpeak-Sounds'
foreach($file in @('Manage-Sounds.ps1','teamspeak-sounds.js','sound-assets.json','Restore.cmd','sounds\mute.wav','sounds\unmute.wav','sounds\deafen.wav','sounds\undeafen.wav','sounds\user_join.wav','sounds\user_leave.wav','sounds\disconnect.wav')){
 if(!(Test-Path -LiteralPath (Join-Path $package $file) -PathType Leaf)){throw ('Incomplete release package: '+$file)}
}
Write-Host ('SHA256 verified. Package: '+$package)
if($DownloadOnly){Write-Host 'Download-only mode: Discord was not modified.'; return}
if($Launcher){
 $exe=Join-Path $package 'Discord-Sound-Launcher.exe'
 if(!(Test-Path -LiteralPath $exe)){throw 'This release does not contain the launcher.'}
 Start-Process -FilePath $exe -WorkingDirectory $package
 return
}
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $package 'Manage-Sounds.ps1') -Mode Apply
if($LASTEXITCODE -ne 0){throw ('Patch failed. Package retained at: '+$package)}
Write-Host 'Done. Start Discord as usual.'
Write-Host ('To restore, quit Discord and run: '+(Join-Path $package 'Restore.cmd'))
