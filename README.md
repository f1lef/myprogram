# StudyRhythm

Навчальний мобільний застосунок для студента: розклад пар, домашні завдання,
сон, щоденна ціль і Pomodoro.

## Архітектура

```text
Android-застосунок (.NET MAUI 8) → ASP.NET Core Web API (.NET 8) → PostgreSQL
```

Телефон звертається до API через HTTP(S). Реквізити бази даних залишаються
тільки на сервері. Підключати Android-застосунок безпосередньо до PostgreSQL
не потрібно.

## Частина 1 — каркас

- `src/StudyRhythm.Mobile` — Android-застосунок із п’ятьма вкладками Shell.
- `src/StudyRhythm.Api` — API та налаштування підключення до PostgreSQL.
- `compose.yaml` — PostgreSQL для локальної розробки. Пароль у цьому файлі
  призначений лише для локального запуску. Порт бази доступний тільки з цього ПК.

Моделі, таблиці, міграції та робочі екрани будуть у наступних частинах.

## Як перевірити каркас

Потрібні .NET 8 SDK, компоненти .NET MAUI 8 для Android, Android SDK та
PostgreSQL. Якщо встановлено Docker Desktop, запустіть базу командою:

Файл `global.json` обирає встановлений SDK 8.x для цього репозиторію.
Перевірте `dotnet --version` у корені проєкту: число має починатися з `8.`.

```powershell
docker compose up -d
```

Запустіть API:

```powershell
dotnet run --project src/StudyRhythm.Api --launch-profile http
```

Відкрийте `http://localhost:5184/api/health` та
`http://localhost:5184/api/health/database`. Обидва запити мають повернути
`status: ok`. Другий запит перевіряє з’єднання з PostgreSQL.

Відкрийте `src/StudyRhythm.Mobile/StudyRhythm.Mobile.csproj` у Visual Studio
та запустіть на Android-емуляторі. Мають з’явитися п’ять вкладок.

Для іншого пароля або віддаленої бази задайте серверну змінну середовища
`ConnectionStrings__StudyRhythm`. Не додавайте справжній пароль у Git.

> .NET MAUI 8 уже поза підтримкою Microsoft. Ціль `net8.0-android` збережено
> відповідно до навчальної вимоги; перед публікацією потрібне оновлення.
