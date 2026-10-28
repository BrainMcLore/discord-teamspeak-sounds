<div align="center">
  <img src="launcher/assets/soundspace.png" alt="SoundSpace" width="112" height="112">
  <h1>SoundSpace</h1>
  <p><strong>Your Discord. Your sounds.</strong></p>
  <p>TeamSpeak sounds · Custom WAV / MP3 · Community Workshop</p>
  <p>
    <a href="https://github.com/BrainMcLore/discord-teamspeak-sounds/releases/latest"><img src="https://img.shields.io/github/v/release/BrainMcLore/discord-teamspeak-sounds?style=flat-square&amp;color=ff6500&amp;label=SoundSpace" alt="Latest release"></a>
    <img src="https://img.shields.io/badge/Windows-10%20%2F%2011-0078D4?style=flat-square" alt="Windows 10 / 11">
    <img src="https://img.shields.io/badge/languages-RU%20%C2%B7%20EN%20%C2%B7%20UK-5865F2?style=flat-square" alt="RU / EN / UK">
  </p>
  <p><a href="https://github.com/BrainMcLore/discord-teamspeak-sounds/releases/latest"><strong>⬇️ Download SoundSpace</strong></a> · <a href="CHANGELOG.md">📋 Changelog</a> · <a href="https://donatello.to/loxire">🧡 Support the project</a></p>
</div>

---

[Русский](README.md) | **English** | [Українська](README.uk.md)

SoundSpace is a Windows app for customizing Discord sounds: classic TeamSpeak 3, your own WAV/MP3 files and community packs. Vencord and BetterDiscord are not required. Close the launcher after applying your sounds.

## 🚀 Installation

1. **[Download the v3.0.1 installer](https://github.com/BrainMcLore/discord-teamspeak-sounds/releases/download/v3.0.1/SoundSpace-Setup-v3.0.1.exe)**.
2. Choose a folder and optionally enable **Create a desktop shortcut**.
3. Open SoundSpace from the Start menu or desktop.

Requires Windows 10/11 and .NET Framework 4.8. Installs for the current user.

### One PowerShell command

```powershell
& ([scriptblock]::Create((irm 'https://raw.githubusercontent.com/BrainMcLore/discord-teamspeak-sounds/main/Install.ps1?setup=4')))
```

Downloads the installer, verifies SHA256 and opens the wizard with folder and shortcut options.

The **[portable ZIP v3.0.0](https://github.com/BrainMcLore/discord-teamspeak-sounds/releases/tag/v3.0.0)** is available separately. Extract everything and open `Discord-Sound-Launcher.exe`. Add `-Portable` to the PowerShell command to download it; `-Apply` applies classic sounds without the launcher.

## 🎧 Your sounds

- Choose **TeamSpeak Female**, **TeamSpeak Male**, **Classic Clicks**, or your own WAV/MP3 files.
- Preview before applying. Unchecked events keep their original Discord sound.
- Export a collection as `.dspack` or import an existing pack.
- Fully quit Discord from its tray icon, select **Apply to Discord**, then restart Discord.

Seven events: microphone mute/unmute, headphones deafen/undeafen, participant join/leave and your own disconnect. Enable the corresponding notification sounds in Discord. Local WAV/MP3 files: up to 10 MB each. Interface languages: RU / EN / UK.

## 🌍 Workshop

Use **Discover** to search titles or authors, filter categories, preview and download packs. The shared server is already configured.

Sign in with **GitHub** or **email + password** to publish. Email verification is required; password reset and verification resend are available on the sign-in screen. Build a collection and choose **Share a pack**. Delete your own publications under **My uploads**.

Cloud limits: **8 MB per pack**, **64 MB per account**, **512 MB for the shared catalog**. Only share audio you have permission to distribute.

## ↩️ Restore and uninstall

Fully quit Discord and select **Restore Discord sounds** in the launcher to restore defaults. Diagnostics are available in Settings. Reapply after Discord updates if needed.

Uninstall through **Windows Settings → Apps → SoundSpace**. Restore Discord sounds first if you also want to remove the patch. Your library is stored in `%LOCALAPPDATA%\DiscordTeamSpeakSounds\launcher`.

## ⚙️ Source code

`Build-Launcher.ps1` builds the launcher; `Build-Installer.ps1` builds the installer using Inno Setup. `cloud-workshop/` contains the backend; `presets/` contains bundled packs. The installer includes the compiled EXE.

The patch backs up Discord's loader and replaces sound-resource requests with local audio. Discord updates may require patch updates.

## 🎵 TeamSpeak audio

Bundled audio comes from the official TeamSpeak 3 Client 3.6.2. See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

## 🧡 Support the project

[Support development with a donation](https://donatello.to/loxire)
