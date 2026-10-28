<div align="center">
  <img src="launcher/assets/soundspace.png" alt="SoundSpace" width="112" height="112">
  <h1>SoundSpace</h1>
  <p><strong>Твой Discord. Твои звуки.</strong></p>
  <p>TeamSpeak sounds · Custom WAV / MP3 · Community Workshop</p>
  <p>
    <a href="https://github.com/BrainMcLore/discord-teamspeak-sounds/releases/latest"><img src="https://img.shields.io/github/v/release/BrainMcLore/discord-teamspeak-sounds?style=flat-square&amp;color=ff6500&amp;label=SoundSpace" alt="Latest release"></a>
    <img src="https://img.shields.io/badge/Windows-10%20%2F%2011-0078D4?style=flat-square" alt="Windows 10 / 11">
    <img src="https://img.shields.io/badge/languages-RU%20%C2%B7%20EN%20%C2%B7%20UK-5865F2?style=flat-square" alt="RU / EN / UK">
  </p>
  <p><a href="https://github.com/BrainMcLore/discord-teamspeak-sounds/releases/latest"><strong>⬇️ Скачать SoundSpace</strong></a> · <a href="CHANGELOG.md">📋 Что нового</a> · <a href="https://donatello.to/loxire">🧡 Поддержать проект</a></p>
</div>

---

**Русский** | [English](README.en.md) | [Українська](README.uk.md)

SoundSpace — приложение для Windows, которое меняет звуки Discord: классический TeamSpeak 3, свои WAV/MP3 и паки от сообщества. Vencord и BetterDiscord не нужны. После применения лаунчер можно закрыть.

## 🚀 Установка

1. **[Скачайте установщик v3.0.1](https://github.com/BrainMcLore/discord-teamspeak-sounds/releases/download/v3.0.1/SoundSpace-Setup-v3.0.1.exe)**.
2. Выберите папку и при желании отметьте **«Создать ярлык на рабочем столе»**.
3. Запустите SoundSpace из меню «Пуск» или с рабочего стола.

Нужны Windows 10/11 и .NET Framework 4.8. Установка выполняется для текущего пользователя.

### Одна команда PowerShell

```powershell
& ([scriptblock]::Create((irm 'https://raw.githubusercontent.com/BrainMcLore/discord-teamspeak-sounds/main/Install.ps1?setup=4')))
```

Команда скачает установщик, проверит SHA256 и откроет мастер с выбором папки и ярлыка.

[Переносная ZIP-версия v3.0.0](https://github.com/BrainMcLore/discord-teamspeak-sounds/releases/tag/v3.0.0) доступна отдельно. Распакуйте её целиком и запустите `Discord-Sound-Launcher.exe`. Для загрузки через PowerShell добавьте `-Portable`; `-Apply` применяет классические звуки без лаунчера.

## 🎧 Свои звуки

- Выберите **TeamSpeak Female**, **TeamSpeak Male**, **Classic Clicks** или свои WAV/MP3.
- Прослушайте звук перед применением. Выключенная галочка оставляет оригинальный звук Discord для этого события.
- Сохраните подборку в `.dspack` или импортируйте готовый пак.
- Полностью закройте Discord через значок возле часов, нажмите **«Применить»**, затем снова запустите Discord.

Поддерживаются семь событий: выключение/включение микрофона, отключение/включение звука кнопкой наушников, вход участника, выход участника и собственное отключение. Звуки уведомлений должны быть включены в Discord. Локальный WAV/MP3 — до 10 МБ на файл. Интерфейс: RU / EN / UK.

## 🌍 Воркшоп

В **«Обзоре»** ищите паки по названию или автору, выбирайте категории, слушайте и скачивайте. Общий сервер уже подключён.

Для публикации войдите через **GitHub** или **email + пароль**. Email нужно подтвердить; восстановление пароля и повторная отправка письма доступны на экране входа. Создайте подборку и нажмите **«Поделиться паком»**. Свои публикации можно удалить в **«Мои публикации»**.

Лимиты облака: **8 МБ на пак**, **64 МБ на аккаунт**, **512 МБ на общий каталог**. Делитесь только звуками, которые имеете право распространять.

## ↩️ Восстановление и удаление

Чтобы вернуть стандартные звуки, полностью закройте Discord и нажмите **«Вернуть звуки Discord»** в лаунчере. Диагностика доступна в настройках. После обновления Discord может потребоваться повторное применение.

Лаунчер удаляется через **Параметры Windows → Приложения → SoundSpace**. Если хотите убрать и патч, сначала восстановите стандартные звуки. Личная библиотека хранится в `%LOCALAPPDATA%\DiscordTeamSpeakSounds\launcher`.

## ⚙️ Исходный код

`Build-Launcher.ps1` собирает лаунчер; `Build-Installer.ps1` — установщик с помощью Inno Setup. `cloud-workshop/` содержит сервер, `presets/` — встроенные паки. Установщик уже включает готовый EXE.

Патч сохраняет резервную копию загрузчика Discord и заменяет обращения к звуковым ресурсам локальным аудио. Обновления Discord могут потребовать обновления патча.

## 🎵 Звуки TeamSpeak

Исходные звуки взяты из официального TeamSpeak 3 Client 3.6.2. Подробности — [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

## 🧡 Поддержать проект

[Поддержать разработку донатом](https://donatello.to/loxire)
