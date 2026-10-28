# Sound Pack Workshop

## Current workshop / Текущий воркшоп / Поточний воркшоп

**RU:** Откройте «Обзор» в лаунчере: это общий каталог пользователей с поиском, категориями, сортировкой, прослушиванием и выбором паков. Для публикации войдите через GitHub или подтверждённый email и нажмите «Поделиться паком». Удаление своих паков — в «Мои публикации», после подтверждения. Облако: 8 МБ на архив, 64 МБ на аккаунт. Локальные WAV/MP3 — до 10 МБ на файл.

**EN:** Discover shows the live shared catalog with search, categories, sorting, previews and downloads. Sign in with GitHub or verified email to Share a pack. Delete your own packs under My uploads after confirmation. Cloud limits: 8 MB per archive, 64 MB per account. Local WAV/MP3: up to 10 MB per file.

**UK:** «Огляд» показує спільний каталог із пошуком, категоріями, сортуванням, прослуховуванням і вибором. Для публікації увійдіть через GitHub або підтверджений email. Власні паки видаляються в «Мої публікації» після підтвердження. Хмара: 8 МБ на архів, 64 МБ на акаунт. Локальні WAV/MP3 — до 10 МБ на файл.

The GitHub catalog instructions below describe the archived v2 catalog and the pack format. They are not required for the current in-app workshop.

[Русский](#русский) · [English](#english) · [Українська](#українська)

## Русский

В лаунчере откройте **Воркшоп → Обновить каталог**, выберите пак и нажмите **Скачать выбранный пак**. SHA256 проверяется до импорта. Пак загрузится в редактор; применение к Discord выполняется отдельной кнопкой.

Создайте свою подборку на вкладке «Мои звуки» и нажмите «Экспорт пака». Полученный `.dspack` можно передать другому пользователю; он откроет его через «Импорт пака».

Чтобы предложить пак в общий каталог, откройте pull request с файлом в `workshop/packs/` и записью в `workshop/index.json`. Нужны название, автор, описание, HTTPS-ссылка на файл в этой папке и SHA256. Загружайте только звуки, которыми имеете право делиться. Публикация не происходит автоматически; аккаунты, рейтинги и сервер загрузок в эту версию не входят.

## English

Open **Workshop → Refresh catalog**, select a pack, and choose **Download selected pack**. Its SHA256 is checked before import. The editor loads the pack; applying it to Discord is a separate action.

Build a selection under My sounds and choose Export pack. Share the resulting `.dspack` file; another user can open it with Import pack.

To contribute a catalog entry, open a pull request adding the file to `workshop/packs/` and an entry to `workshop/index.json`. Include its name, author, description, HTTPS file URL in that directory, and SHA256. Share only audio you have permission to distribute. Publishing is manual; accounts, ratings, and an upload server are not included.

## Українська

Відкрийте **Воркшоп → Оновити каталог**, виберіть пак і натисніть **Завантажити вибраний пак**. SHA256 перевіряється до імпорту. Пак відкриється в редакторі; застосування до Discord виконується окремою кнопкою.

Створіть добірку на вкладці «Мої звуки» та натисніть «Експорт пака». Файл `.dspack` можна передати іншому користувачеві, який відкриє його через «Імпорт пака».

Щоб запропонувати пак до каталогу, відкрийте pull request із файлом у `workshop/packs/` і записом у `workshop/index.json`. Потрібні назва, автор, опис, HTTPS-посилання на файл у цій папці та SHA256. Поширюйте лише звуки, на які маєте відповідні права. Публікація не відбувається автоматично; облікові записи, рейтинги та сервер завантажень до цієї версії не входять.

## Pack format / Формат пака

`.dspack` is a ZIP archive with `pack.json` and up to seven audio files. Only WAV and MP3 are accepted, up to 10 MB each. Archives may not contain scripts or extra files.

```json
{
  "schemaVersion": 1,
  "name": "My pack",
  "author": "Your name",
  "description": "A short description",
  "sounds": {
    "mute": {"file": "mute.wav", "mime": "audio/wav"},
    "deafen": {"file": "deafen.mp3", "mime": "audio/mpeg"}
  }
}
```

Store referenced files as `sounds/mute.wav`, `sounds/deafen.mp3`, etc. Available keys: `mute`, `unmute`, `deafen`, `undeafen`, `user_join`, `user_leave`, `disconnect`. Omitted keys retain Discord's default sound. Filenames must match their key. No local paths are exported.

To rebuild the bundled presets from the official extracted TeamSpeak sound directory:

```text
python tools/build-presets.py PATH_TO_TEAMSPEAK_SOUND_DIRECTORY
```
