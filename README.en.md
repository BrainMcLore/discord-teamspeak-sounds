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

A Windows script that replaces Discord's microphone and voice channel sounds with classic TeamSpeak 3 sounds. Apply it manually and run it again after Discord updates.

Vencord, BetterDiscord, and a separate background application are not required. A small patch runs inside Discord's existing process.

## 🎧 Launcher

**v3.0 SoundSpace:** a redesigned card-based interface and original app icon. The live shared workshop lets users publish packs directly from the launcher. Browse real user packs, search titles/authors, filter categories, preview and download.

Sign in with GitHub or email and password. Email accounts must verify their address through Firebase. Sign-in and registration have separate screens; password reset and resend verification are available. Legacy username sign-in preserves access to existing accounts and packs.

**My uploads** lets owners delete their packs after confirmation. Ownership is enforced by the server. Cloud limits: 8 MB per pack, 64 MB per account and 512 MB for the shared test catalog. The server address is already configured; users do not need their own Cloudflare account.

Download **[SoundSpace Setup.exe](https://github.com/BrainMcLore/discord-teamspeak-sounds/releases/latest/download/SoundSpace-Setup-v3.0.1.exe)** and run it. The wizard offers an installation folder and an optional desktop shortcut. Requires Windows with .NET Framework 4.8.

- **TeamSpeak Female**, **TeamSpeak Male**, and **Classic Clicks** presets.
- Choose your own WAV/MP3 for each of seven events and preview it before applying.
- Uncheck an event to keep Discord's original sound.
- Import/export `.dspack` files and retain your selections between sessions.
- Workshop: live user catalog, in-app publication and SHA256-verified downloads.
- Russian, English, and Ukrainian interface.

Download and launch with one PowerShell command:

```powershell
& ([scriptblock]::Create((irm 'https://raw.githubusercontent.com/BrainMcLore/discord-teamspeak-sounds/main/Install.ps1?setup=4')))
```

The launcher can stay open alongside Discord. Fully quit Discord before Apply or Restore, then start Discord again. The launcher can be closed afterward; it is not needed for sound playback.

Packs contain metadata and audio only, up to 10 MB per local sound. Cloud publication requires an archive of 8 MB or less. The shared workshop uses Cloudflare.

To build from source, run `Build-Launcher.ps1` or `Launcher.cmd`. Releases include the compiled EXE. Settings and the local library are stored in `%LOCALAPPDATA%\DiscordTeamSpeakSounds\launcher`.

## 🚀 Installation

### With PowerShell

```powershell
& ([scriptblock]::Create((irm 'https://raw.githubusercontent.com/BrainMcLore/discord-teamspeak-sounds/main/Install.ps1?setup=4')))
```

The command downloads the latest EXE, verifies SHA256 and opens the installation wizard. Choose a folder and optionally enable the desktop shortcut. Fully quit Discord before applying sounds; installing the launcher does not require closing Discord.

### Portable version

[ZIP without installation — v3.0.0](https://github.com/BrainMcLore/discord-teamspeak-sounds/releases/tag/v3.0.0): extract everything and open `Discord-Sound-Launcher.exe`. Add `-Portable` to the PowerShell command for this mode. Use `-Apply` to apply classic sounds without the launcher after fully quitting Discord.

## ↩️ Restore and check status

- **`Restore.cmd`** restores the original loader. Fully quit Discord first, run the restore script, then reopen Discord.
- **`Status.cmd`** shows whether the patch is installed and displays its latest log entries.

`TS-SOUNDS-HIT:mute`, `unmute`, `user_join`, `user_leave`, or `disconnect` means a sound request was intercepted and replaced. A `ready` entry confirms that the handler loaded, but does not by itself confirm playback.

## 🔊 Sounds

| Discord event | Original TeamSpeak 3 file |
| --- | --- |
| Mute microphone | `mic_muted.wav` |
| Unmute microphone | `mic_activated.wav` |
| Deafen using the headphones button | `sound_muted.wav` — Sound muted |
| Undeafen using the headphones button | `sound_resumed.wav` — Sound resumed |
| Connect / participant joins | `neutral_switched_tocurrentchannel.wav` |
| Participant leaves | `neutral_switched_awayfromcurrentchannel.wav` |
| Disconnect yourself | `disconnected.wav` |

Discord uses the same sound for your own connection and another participant joining. Standard, Halloween, and winter resource variants are supported: 21 addresses in total. The corresponding sounds must be enabled in Discord's notification settings.

## ⚙️ How it works

`Manage-Sounds.ps1` finds the latest installed Discord version in the standard user directory or `%ProgramData%\%USERNAME%\Discord`. Before changing `index.js`, it creates an `index.js.ts-original` backup. Reapplying the patch preserves that original backup. The script refuses to overwrite an unrecognized loader.

During application, Discord's public page and its `web.*.js` bundle are used to update audio filenames. If they cannot be retrieved, the bundled list is used instead. This download does not use authentication.

When Discord starts, the patch replaces requests for the specified audio resources with local WAV files. Other requests pass through to the original handler. The script does not access tokens or passwords.

The CMD files allow PowerShell script execution only for the launched process; they do not permanently change Windows execution policy. Administrator rights are usually unnecessary. Restoring disables the patch while leaving the backup and inactive files next to the loader.

## ✅ Compatibility and verification

For the headphones button, exact WAV replacement and standard and seasonal addresses have been verified. Handler checks cover all 21 addresses. A real button press with this new version has not been separately tested.

Tested with Discord **1.0.9265** on Windows on October 28, 2026:

- starting the client with the patch;
- intercepting actual mute, unmute, user_join, and user_leave requests;
- reapplying the patch and restoring the loader;
- exact WAV byte matches for all 15 addresses;
- replacing `media.src` and `play`, while passing unrelated requests through unchanged;
- JavaScript and PowerShell syntax, and the structure of all five WAV files.

A real `disconnect` event and audible playback on a specific output device have not been separately confirmed. Future changes to Discord's playback mechanism may require updating the patch itself rather than simply rerunning `Apply.cmd`.

## 🎵 Audio source

The original WAV files were extracted from the `sound/default` package of the official [TeamSpeak 3 Client 3.6.2](https://www.teamspeak.com/en/downloads/#ts3client), without installing or running TeamSpeak. See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

## 🧡 Support the project

[Support development with a donation](https://donatello.to/loxire)
