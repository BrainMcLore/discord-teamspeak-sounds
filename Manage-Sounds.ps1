param([ValidateSet('Apply','Restore','Status')][string]$Mode='Apply',[string]$PackPath=$PSScriptRoot)
$ErrorActionPreference='Stop'
$utf8=New-Object Text.UTF8Encoding($false)
try {
 $roots=@((Join-Path $env:LOCALAPPDATA 'Discord'),(Join-Path $env:ProgramData "$env:USERNAME\Discord"))
 $running=@(Get-Process Discord -ErrorAction SilentlyContinue)
 foreach($process in $running) {if($process.Path){$roots+=Split-Path (Split-Path $process.Path)}}
 $roots=@($roots|Select-Object -Unique)
 $versions=@(foreach($root in $roots){if(Test-Path -LiteralPath $root){Get-ChildItem -LiteralPath $root -Directory -Filter 'app-*'}})
 $versions=@($versions|Sort-Object {try{[version]($_.Name.Substring(4))}catch{[version]'0.0'}} -Descending)
 if(!$versions.Count){throw 'Discord installation not found.'}
 if($Mode -eq 'Status') {
  foreach($v in $versions){
   Get-ChildItem -Path (Join-Path $v.FullName 'modules\discord_desktop_core-*\discord_desktop_core\index.js') -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host $_.FullName
    Write-Host ('Patched: '+((Get-Content -LiteralPath $_.FullName -Raw).Contains('TS-SOUNDS-PATCH')))
    $log=Join-Path $_.DirectoryName 'ts-sounds\status.log'
    if(Test-Path -LiteralPath $log){Get-Content -LiteralPath $log -Tail 8}
   }
  }; exit 0
 }
 if($running.Count){throw 'Completely quit Discord using its tray icon, then run this file again.'}
 if($Mode -eq 'Restore') {
  $count=0
  foreach($v in $versions){
   Get-ChildItem -Path (Join-Path $v.FullName 'modules\discord_desktop_core-*\discord_desktop_core\index.js.ts-original') -ErrorAction SilentlyContinue | ForEach-Object {
    $target=$_.FullName.Substring(0,$_.FullName.Length-12)
    $current=[IO.File]::ReadAllText($target)
    if(!$current.Contains('TS-SOUNDS-PATCH')){Write-Host "Already original: $target"; return}
    [IO.File]::WriteAllBytes($target,[IO.File]::ReadAllBytes($_.FullName)); $count++
   }
  }
  Write-Host "Restored: $count. You can start Discord."; exit 0
 }
 $v=$versions[0]
 $entries=@(Get-ChildItem -Path (Join-Path $v.FullName 'modules\discord_desktop_core-*\discord_desktop_core\index.js') -ErrorAction SilentlyContinue)
 if($entries.Count -ne 1){throw 'Cannot uniquely find Discord core. No changes made.'}
 $entry=$entries[0].FullName
 $soundKeys=@('mute','unmute','deafen','undeafen','user_join','user_leave','disconnect')
 $soundManifest=@{}
 $manifestPath=Join-Path $PackPath 'sounds.json'
 if(Test-Path -LiteralPath $manifestPath){
  $manifest=Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8|ConvertFrom-Json
  foreach($property in $manifest.PSObject.Properties){
   $key=$property.Name; $spec=$property.Value
   if($key -notin $soundKeys -or $spec.file -notin @("$key.wav","$key.mp3")){throw 'Invalid sound pack manifest.'}
   $mime=if($spec.file.EndsWith('.mp3')){'audio/mpeg'}else{'audio/wav'}
   if($spec.mime -ne $mime){throw 'Invalid sound MIME type.'}
   $soundManifest[$key]=@{file=$spec.file;mime=$mime}
  }
 }else{foreach($key in $soundKeys){$soundManifest[$key]=@{file="$key.wav";mime='audio/wav'}}}
 foreach($key in $soundManifest.Keys){
  $soundFile=Get-Item -LiteralPath (Join-Path $PackPath ('sounds\'+$soundManifest[$key].file))
  if($soundFile.Length -gt 10MB -or $soundFile.Length -lt 12){throw "Invalid audio file size: $key"}
 }
 $assetMap=@{}
 $bundledMap=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'sound-assets.json') -Raw|ConvertFrom-Json
 foreach($property in $bundledMap.PSObject.Properties){$assetMap[$property.Name]=$property.Value}
 try {
  $html=(Invoke-WebRequest 'https://discord.com/app' -UseBasicParsing -TimeoutSec 20).Content
  $web=[regex]::Match($html,'src="(/assets/web\.[a-z0-9]+\.js)"')
  if(!$web.Success){throw 'Public web bundle not found'}
  $bundle=(Invoke-WebRequest ('https://discord.com'+$web.Groups[1].Value) -UseBasicParsing -TimeoutSec 30).Content
  $found=0
  foreach($name in @('mute','unmute','deafen','undeafen','user_join','user_leave','disconnect')){
   $baseFound=$false
   foreach($prefix in @('','halloween_','winter_')){
    $id=[regex]::Match($bundle,'"\./'+$prefix+$name+'\.mp3":"(\d+)"')
    if(!$id.Success){continue}
    $file=[regex]::Match($bundle,','+$id.Groups[1].Value+'\([^)]*\)\{[^}]*"(?:/assets/)?([a-f0-9]+\.mp3)"')
    if($file.Success){$assetMap[$file.Groups[1].Value]=$name; $found++;if($prefix -eq ''){$baseFound=$true}}
   }
   if(!$baseFound){throw "Could not resolve current sound: $name"}
  }
  Write-Host "Updated Discord sound filenames: $found."
 } catch {Write-Host ('Using bundled sound filenames. Update unavailable: '+$_.Exception.Message)}
 $original=[IO.File]::ReadAllText($entry)
 $expected="module.exports = require('./core.asar');"
 if(!$original.Contains('TS-SOUNDS-PATCH') -and $original.Trim() -ne $expected){throw 'Unknown or modified Discord loader. No changes made.'}
 $backup=$entry+'.ts-original'
 if(!(Test-Path -LiteralPath $backup)){
  if($original.Contains('TS-SOUNDS-PATCH')){throw 'Original backup missing. Reinstall Discord to restore its loader.'}
  Copy-Item -LiteralPath $entry -Destination $backup
 }
 if([IO.File]::ReadAllText($backup).Trim() -ne $expected){throw 'Unexpected backup. No changes made.'}
 $dest=Join-Path $entries[0].DirectoryName 'ts-sounds'
 New-Item -ItemType Directory -Path (Join-Path $dest 'sounds') -Force|Out-Null
 Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'teamspeak-sounds.js') -Destination (Join-Path $dest 'teamspeak-sounds.js') -Force
 [IO.File]::WriteAllText((Join-Path $dest 'sound-assets.json'),($assetMap|ConvertTo-Json),$utf8)
 foreach($name in $soundManifest.Keys){
  $file=$soundManifest[$name].file
  $source=Join-Path $PackPath "sounds\$file"
  Copy-Item -LiteralPath $source -Destination (Join-Path $dest "sounds\$file") -Force
 }
 [IO.File]::WriteAllText((Join-Path $dest 'sounds.json'),(ConvertTo-Json -InputObject $soundManifest -Depth 4),$utf8)
 $patch="// TS-SOUNDS-PATCH v1`r`ntry { require('./ts-sounds/teamspeak-sounds.js'); } catch (e) { console.error('TS-SOUNDS', e.message); }`r`nmodule.exports = require('./core.asar');`r`n"
 [IO.File]::WriteAllText($entry+'.ts-new',$patch,$utf8)
 Move-Item -LiteralPath ($entry+'.ts-new') -Destination $entry -Force
 Write-Host "Applied to $($v.Name). You can start Discord."
} catch {Write-Host ('ERROR: '+$_.Exception.Message) -ForegroundColor Red; exit 1}
