param(
    [Parameter(Mandatory=$true)][string]$Compiler,
    [Parameter(Mandatory=$true)][string]$Destination
)
$ErrorActionPreference='Stop'
if (!(Test-Path -LiteralPath $Compiler -PathType Leaf)) { throw 'Specify the Inno Setup ISCC.exe compiler.' }
$Destination=$ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Destination)
New-Item -ItemType Directory -Path $Destination -Force | Out-Null
$stage=Join-Path ([IO.Path]::GetTempPath()) ('soundspace-setup-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $stage | Out-Null
foreach ($name in @('Build-Launcher.ps1','Manage-Sounds.ps1','teamspeak-sounds.js','sound-assets.json','THIRD_PARTY_NOTICES.md','launcher','presets','sounds')) {
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination $stage -Recurse
}
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $stage 'Build-Launcher.ps1')
if ($LASTEXITCODE -ne 0) { throw 'Launcher build failed.' }
& $Compiler ('/DPayloadDir='+$stage) ('/DOutputDir='+$Destination) (Join-Path $PSScriptRoot 'installer\SoundSpace.iss')
if ($LASTEXITCODE -ne 0) { throw 'Installer build failed.' }
$setup=Join-Path $Destination 'SoundSpace-Setup-v3.0.1.exe'
$hash=(Get-FileHash -LiteralPath $setup -Algorithm SHA256).Hash.ToLowerInvariant()
[IO.File]::WriteAllText((Join-Path $Destination 'SoundSpace-Setup-v3.0.1.sha256'),($hash+'  '+[IO.Path]::GetFileName($setup)+[Environment]::NewLine),[Text.Encoding]::ASCII)
Write-Host $setup
