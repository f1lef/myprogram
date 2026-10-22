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

## Частина 2 — моделі та база даних

Моделі містяться в `src/StudyRhythm.Api/Models`. Їхня відповідність таблицям,
довжини текстових полів, зв’язки та індекси описані в `StudyRhythmDbContext`.

| Таблиця | Дані |
| --- | --- |
| `sleep_entries` | Початок і кінець сну, місцева дата пробудження |
| `lessons` | Предмет, день, час, аудиторія, викладач, колір, парність |
| `lesson_notes` | Нотатки до пари |
| `homework_items` | ДЗ, дедлайн, стан виконання, час нагадування |
| `pomodoro_sessions` | Завершені навчальні сесії |
| `streak_days` | Чи виконано обрану ціль у певний день |
| `app_settings` | Опорний тиждень, ціль streak, тривалість таймера |

Часові мітки подій зберігаються у UTC. `DateOnly` зберігає місцеву календарну
дату для графіка сну та streak. Пара має необмежену кількість нотаток і ДЗ.
Після видалення пари її нотатки видаляються, але ДЗ та статистика Pomodoro
залишаються без прив’язки до пари.

Початкова міграція вже є в `src/StudyRhythm.Api/Data/Migrations`. Щоб створити
таблиці у своїй локальній базі, запустіть PostgreSQL, а потім у PowerShell
в корені репозиторію виконайте:

```powershell
dotnet tool restore
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet tool run dotnet-ef database update --project src/StudyRhythm.Api --startup-project src/StudyRhythm.Api
```

Ці команди застосовують міграцію з репозиторію; створювати нову не потрібно.
Для перевірки відкрийте `http://localhost:5184/api/health/schema` після запуску
API. Очікувана відповідь: `{"status":"ok","schema":"up-to-date"}`.

Робочі екрани та API для введення даних з’являться в наступних частинах.

## Як перевірити каркас

Потрібні .NET 8 SDK, компоненти .NET MAUI 8 для Android, Android SDK та
PostgreSQL. Файл `global.json` обирає встановлений SDK 8.x для цього репозиторію.
Перевірте `dotnet --version` у корені проєкту: число має починатися з `8.`.

Якщо встановлено Docker Desktop, запустіть базу командою:

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
