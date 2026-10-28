param([Parameter(Mandatory=$true)][string]$Fixture,[ValidateSet('Apply','Restore')][string]$Mode='Apply')
$ErrorActionPreference='Stop'
$env:LOCALAPPDATA=Join-Path $Fixture 'local'
$env:ProgramData=Join-Path $Fixture 'program'
function Get-Process {param($Name) @()}
function Invoke-WebRequest {throw 'Offline fixture: use bundled sound map'}
& (Join-Path (Split-Path $PSScriptRoot) 'Manage-Sounds.ps1') -Mode $Mode -PackPath (Join-Path $Fixture 'pack')
