# Настройка Firebase для SoundSpace

GitHub-вход уже настроен отдельно. Firebase здесь нужен только для email/пароля, подтверждения почты и восстановления пароля.

1. Откройте https://console.firebase.google.com/ и войдите в Google.
2. Нажмите **Create a project / Создать проект**. Название — **SoundSpace**. Можно использовать уже созданный проект.
3. Если предлагается Google Analytics, его можно отключить. Для авторизации он не нужен.
4. Создайте проект на бесплатном тарифе **Spark**. Подключать биллинг, карту или Blaze не требуется.
5. В меню проекта откройте **Authentication** (может находиться в разделе Build или Security) и нажмите **Get started**.
6. В **Sign-in method / Способы входа** выберите **Email/Password**. Включите обычный **Email/Password**, затем **Save**. Вход по ссылке без пароля включать не нужно.
7. Откройте шестерёнку возле Project Overview → **Project settings / Настройки проекта** → **General / Общие**.
8. В **Your apps / Ваши приложения** нажмите значок **</> Web**. Название — **SoundSpace Desktop**. Firebase Hosting не подключайте. Нажмите **Register app**.
9. В показанной конфигурации `firebaseConfig` найдите **apiKey** и **projectId**. Если окно закрыли, эти поля доступны в Project settings → General → Your apps → SDK setup and configuration → Config.

Передайте только эти две публичные настройки:

```text
projectId: идентификатор-вашего-проекта
apiKey: ключ-из-firebaseConfig
```

Нужен именно `projectId`, а не название проекта и не Project number. Нужен `apiKey` из конфигурации Web-приложения, а не сервисный аккаунт. Пароль Google, Client Secret, service-account JSON и private_key передавать не нужно.

После подключения:

1. В лаунчере откройте **Регистрация**, введите email, пароль и повтор пароля.
2. Придёт письмо подтверждения Firebase. Перейдите по ссылке; при необходимости проверьте «Спам».
3. Вернитесь на отдельный экран **Вход** и войдите с email и паролем. До подтверждения адреса сервер не выдаёт сессию SoundSpace.
4. **Забыли пароль?** отправляет письмо для сброса. Для повторного подтверждения есть отдельная кнопка на экране входа.

На тарифе Spark действуют лимиты Firebase. Для этих операций не нужны Firestore, Storage, Hosting или Cloud Functions.

Официальные инструкции:
- https://firebase.google.com/docs/auth/web/password-auth
- https://firebase.google.com/docs/web/setup
- https://firebase.google.com/docs/auth/limits
