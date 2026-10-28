#ifndef PayloadDir
  #error PayloadDir must point to a built application directory
#endif
#ifndef OutputDir
  #error OutputDir is required
#endif

[Setup]
AppId={{795EBD06-51BA-48C2-9F52-C2C466478E82}
AppName=SoundSpace
AppVersion=3.0.0
AppPublisher=BrainMcLore
AppPublisherURL=https://github.com/BrainMcLore/discord-teamspeak-sounds
DefaultDirName={localappdata}\Programs\SoundSpace
DefaultGroupName=SoundSpace
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
MinVersion=10.0
WizardStyle=modern
SetupIconFile={#PayloadDir}\launcher\assets\soundspace.ico
UninstallDisplayIcon={app}\Discord-Sound-Launcher.exe
OutputDir={#OutputDir}
OutputBaseFilename=SoundSpace-Setup-v3.0.0
Compression=lzma2
SolidCompression=yes
CloseApplications=yes
RestartApplications=no
AppMutex=SoundSpaceSetup

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "ukrainian"; MessagesFile: "compiler:Languages\Ukrainian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#PayloadDir}\Discord-Sound-Launcher.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PayloadDir}\Manage-Sounds.ps1"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PayloadDir}\teamspeak-sounds.js"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PayloadDir}\sound-assets.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PayloadDir}\THIRD_PARTY_NOTICES.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PayloadDir}\launcher\Modern.xaml"; DestDir: "{app}\launcher"; Flags: ignoreversion
Source: "{#PayloadDir}\launcher\service.json"; DestDir: "{app}\launcher"; Flags: ignoreversion
Source: "{#PayloadDir}\launcher\assets\*"; DestDir: "{app}\launcher\assets"; Flags: ignoreversion
Source: "{#PayloadDir}\presets\*.dspack"; DestDir: "{app}\presets"; Flags: ignoreversion
Source: "{#PayloadDir}\sounds\*.wav"; DestDir: "{app}\sounds"; Flags: ignoreversion

[Icons]
Name: "{userprograms}\SoundSpace"; Filename: "{app}\Discord-Sound-Launcher.exe"; WorkingDir: "{app}"
Name: "{userdesktop}\SoundSpace"; Filename: "{app}\Discord-Sound-Launcher.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\Discord-Sound-Launcher.exe"; Description: "{cm:LaunchProgram,SoundSpace}"; Flags: nowait postinstall skipifsilent
