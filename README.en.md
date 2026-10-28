# TeamSpeak sounds for Discord

[Русский](README.md) | **English** | [Українська](README.uk.md)

A Windows script that replaces Discord's microphone and voice channel sounds with classic TeamSpeak 3 sounds. Apply it manually and run it again after Discord updates.

Vencord, BetterDiscord, and a separate background application are not required. A small patch runs inside Discord's existing process.

## Installation

1. Click **Code → Download ZIP** and extract the entire archive.
2. Fully quit Discord using its system tray icon: **Quit Discord**.
3. Double-click `Apply.cmd`.
4. Start Discord as usual.

After a Discord update, repeat steps 2–4. Keep the CMD files together with the other files and the `sounds` folder.

## Restore and check status

- **`Restore.cmd`** restores the original loader. Fully quit Discord first, run the restore script, then reopen Discord.
- **`Status.cmd`** shows whether the patch is installed and displays its latest log entries.

`TS-SOUNDS-HIT:mute`, `unmute`, `user_join`, `user_leave`, or `disconnect` means a sound request was intercepted and replaced. A `ready` entry confirms that the handler loaded, but does not by itself confirm playback.

## Sounds

| Discord event | Original TeamSpeak 3 file |
| --- | --- |
| Mute microphone | `mic_muted.wav` |
| Unmute microphone | `mic_activated.wav` |
| Connect / participant joins | `neutral_switched_tocurrentchannel.wav` |
| Participant leaves | `neutral_switched_awayfromcurrentchannel.wav` |
| Disconnect yourself | `disconnected.wav` |

Discord uses the same sound for your own connection and another participant joining. Standard, Halloween, and winter resource variants are supported: 15 addresses in total. The corresponding sounds must be enabled in Discord's notification settings.

## How it works

`Manage-Sounds.ps1` finds the latest installed Discord version in the standard user directory or `%ProgramData%\%USERNAME%\Discord`. Before changing `index.js`, it creates an `index.js.ts-original` backup. Reapplying the patch preserves that original backup. The script refuses to overwrite an unrecognized loader.

During application, Discord's public page and its `web.*.js` bundle are used to update audio filenames. If they cannot be retrieved, the bundled list is used instead. This download does not use authentication.

When Discord starts, the patch replaces requests for the specified audio resources with local WAV files. Other requests pass through to the original handler. The script does not access tokens or passwords.

The CMD files allow PowerShell script execution only for the launched process; they do not permanently change Windows execution policy. Administrator rights are usually unnecessary. Restoring disables the patch while leaving the backup and inactive files next to the loader.

## Compatibility and verification

Tested with Discord **1.0.9265** on Windows on October 28, 2026:

- starting the client with the patch;
- intercepting actual mute, unmute, user_join, and user_leave requests;
- reapplying the patch and restoring the loader;
- exact WAV byte matches for all 15 addresses;
- replacing `media.src` and `play`, while passing unrelated requests through unchanged;
- JavaScript and PowerShell syntax, and the structure of all five WAV files.

A real `disconnect` event and audible playback on a specific output device have not been separately confirmed. Future changes to Discord's playback mechanism may require updating the patch itself rather than simply rerunning `Apply.cmd`.

## Audio source

The original WAV files were extracted from the `sound/default` package of the official [TeamSpeak 3 Client 3.6.2](https://www.teamspeak.com/en/downloads/#ts3client), without installing or running TeamSpeak. See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
