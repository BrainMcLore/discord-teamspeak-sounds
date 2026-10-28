# TeamSpeak audio

The WAV files in `sounds/` originate from the `sound/default` directory of the official TeamSpeak 3 Client 3.6.2 Windows installer:

https://files.teamspeak-services.com/releases/client/3.6.2/TeamSpeak3-Client-win64-3.6.2.exe

The files retain TeamSpeak's ownership and applicable terms. This repository does not grant an independent license to those audio recordings. This project is not affiliated with TeamSpeak or Discord.

The bundled `.dspack` presets and initial workshop packs also contain TeamSpeak audio. Female and Clicks use `sound/default`; Male uses `sound/default_male`. Classic Clicks substitutes `mic_click_off.wav` and `mic_click_on.wav` for microphone mute/unmute. Other events use the original files listed below. The preset selection and packaging do not change ownership of the recordings.

| Repository file | Original file |
| --- | --- |
| `sounds/mute.wav` | `mic_muted.wav` |
| `sounds/unmute.wav` | `mic_activated.wav` |
| `sounds/deafen.wav` | `sound_muted.wav` |
| `sounds/undeafen.wav` | `sound_resumed.wav` |
| `sounds/user_join.wav` | `neutral_switched_tocurrentchannel.wav` |
| `sounds/user_leave.wav` | `neutral_switched_awayfromcurrentchannel.wav` |
| `sounds/disconnect.wav` | `disconnected.wav` |
