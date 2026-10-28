<div align="center">
  <img src="launcher/assets/soundspace.png" alt="SoundSpace" width="112" height="112">
  <h1>SoundSpace</h1>
  <p><strong>Твій Discord. Твої звуки.</strong></p>
  <p>TeamSpeak sounds · Custom WAV / MP3 · Community Workshop</p>
  <p>
    <a href="https://github.com/BrainMcLore/discord-teamspeak-sounds/releases/latest"><img src="https://img.shields.io/github/v/release/BrainMcLore/discord-teamspeak-sounds?style=flat-square&amp;color=ff6500&amp;label=SoundSpace" alt="Latest release"></a>
    <img src="https://img.shields.io/badge/Windows-10%20%2F%2011-0078D4?style=flat-square" alt="Windows 10 / 11">
    <img src="https://img.shields.io/badge/languages-RU%20%C2%B7%20EN%20%C2%B7%20UK-5865F2?style=flat-square" alt="RU / EN / UK">
  </p>
  <p><a href="https://github.com/BrainMcLore/discord-teamspeak-sounds/releases/latest"><strong>⬇️ Завантажити SoundSpace</strong></a> · <a href="CHANGELOG.md">📋 Що нового</a> · <a href="https://donatello.to/loxire">🧡 Підтримати проєкт</a></p>
</div>

---

[Русский](README.md) | [English](README.en.md) | **Українська**

SoundSpace — програма для Windows, яка змінює звуки Discord: класичний TeamSpeak 3, власні WAV/MP3 та паки спільноти. Vencord і BetterDiscord не потрібні. Після застосування лаунчер можна закрити.

## 🚀 Встановлення

1. **[Завантажте інсталятор v3.0.1](https://github.com/BrainMcLore/discord-teamspeak-sounds/releases/download/v3.0.1/SoundSpace-Setup-v3.0.1.exe)**.
2. Виберіть папку та за бажанням увімкніть **створення ярлика на робочому столі**.
3. Запустіть SoundSpace з меню «Пуск» або робочого столу.

Потрібні Windows 10/11 і .NET Framework 4.8. Встановлення виконується для поточного користувача.

### Одна команда PowerShell

```powershell
& ([scriptblock]::Create((irm 'https://raw.githubusercontent.com/BrainMcLore/discord-teamspeak-sounds/main/Install.ps1?setup=4')))
```

Команда завантажить інсталятор, перевірить SHA256 та відкриє майстер із вибором папки й ярлика.

**[Переносна ZIP-версія v3.0.0](https://github.com/BrainMcLore/discord-teamspeak-sounds/releases/tag/v3.0.0)** доступна окремо. Розпакуйте повністю та запустіть `Discord-Sound-Launcher.exe`. Для завантаження через PowerShell додайте `-Portable`; `-Apply` застосовує класичні звуки без лаунчера.

## 🎧 Власні звуки

- Виберіть **TeamSpeak Female**, **TeamSpeak Male**, **Classic Clicks** або свої WAV/MP3.
- Прослухайте звук перед застосуванням. Вимкнена галочка залишає оригінальний звук Discord для цієї події.
- Експортуйте добірку в `.dspack` або імпортуйте готовий пак.
- Повністю закрийте Discord через значок біля годинника, натисніть **«Застосувати в Discord»**, потім запустіть Discord знову.

Сім подій: вимкнення/увімкнення мікрофона, вимкнення/увімкнення звуку кнопкою навушників, вхід і вихід учасника та власне відключення. Відповідні звуки сповіщень мають бути ввімкнені в Discord. Локальний WAV/MP3 — до 10 МБ на файл. Мови інтерфейсу: RU / EN / UK.

## 🌍 Воркшоп

В **«Огляді»** шукайте паки за назвою або автором, вибирайте категорії, слухайте та завантажуйте. Спільний сервер уже підключено.

Для публікації увійдіть через **GitHub** або **email + пароль**. Email потрібно підтвердити; відновлення пароля та повторне надсилання листа доступні на екрані входу. Створіть добірку й натисніть **«Поділитися паком»**. Власні публікації можна видалити в **«Мої публікації»**.

Хмарні ліміти: **8 МБ на пак**, **64 МБ на акаунт**, **512 МБ на спільний каталог**. Поширюйте лише звуки, на розповсюдження яких маєте право.

## ↩️ Відновлення та видалення

Щоб повернути стандартні звуки, повністю закрийте Discord і натисніть **«Повернути звуки Discord»** у лаунчері. Діагностика доступна в налаштуваннях. Після оновлення Discord може знадобитися повторне застосування.

Лаунчер видаляється через **Параметри Windows → Програми → SoundSpace**. Якщо хочете прибрати й патч, спочатку відновіть стандартні звуки. Власна бібліотека зберігається в `%LOCALAPPDATA%\DiscordTeamSpeakSounds\launcher`.

## ⚙️ Вихідний код

`Build-Launcher.ps1` збирає лаунчер; `Build-Installer.ps1` — інсталятор за допомогою Inno Setup. `cloud-workshop/` містить сервер, `presets/` — вбудовані паки. Інсталятор уже містить готовий EXE.

Патч зберігає резервну копію завантажувача Discord і замінює запити звукових ресурсів локальним аудіо. Оновлення Discord можуть вимагати оновлення патча.

## 🎵 Звуки TeamSpeak

Вбудовані звуки взято з офіційного TeamSpeak 3 Client 3.6.2. Докладніше — [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

## 🧡 Підтримати проєкт

[Підтримати розробку донатом](https://donatello.to/loxire)
