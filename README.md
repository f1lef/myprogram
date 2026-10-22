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
- `src/StudyRhythm.Core` — розрахунки, які працюють на телефоні без мережі.
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

## Частина 3 — калькулятор сну

У `src/StudyRhythm.Core/Services/SleepCalculatorService.cs` є три методи:

| Метод | Що повертає |
| --- | --- |
| `GetBedtimeSuggestions` | Коли лягти, якщо відомий час пробудження |
| `GetWakeSuggestions` | Коли прокинутися, якщо відомий час відходу до сну |
| `Analyze` | Тривалість запису, умовні цикли та коротку пораду |

`StudyRhythm.Core` підключений до мобільного проєкту як `ProjectReference`.
Додаткових NuGet-пакетів для цього кроку немає. Обчислення виконуються на
пристрої, а зберігання історії буде доступне через API і PostgreSQL.

Формула для варіанта з `n` циклами: `15 хв + n × 90 хв`. Сервіс пропонує
`n = 4, 5, 6`. Наприклад, для пробудження о 07:00 він покаже 00:45,
23:15 і 21:45 попереднього вечора відповідно. Якщо лягти о 23:00,
підказки для пробудження: 05:15, 06:45 і 08:15. Для переходу через зміну
годинника передайте місцевий `TimeZoneInfo`, а час передавайте як
`DateTimeOffset` із правильним зсувом від UTC.

Ось короткий приклад виклику з C#:

```csharp
using StudyRhythm.Core.Services;

var calculator = new SleepCalculatorService();
var zone = TimeZoneInfo.Local;
var wakeAt = new DateTimeOffset(2026, 10, 23, 7, 0, 0, TimeSpan.FromHours(3));

foreach (var option in calculator.GetBedtimeSuggestions(wakeAt, zone))
{
    Console.WriteLine($"{option.CycleCount} циклів: {option.SuggestedAt:dd.MM HH:mm}");
}

var bedtime = new DateTimeOffset(2026, 10, 22, 23, 0, 0, TimeSpan.FromHours(3));
var analysis = calculator.Analyze(bedtime, wakeAt);
Console.WriteLine($"Час у ліжку: {analysis.TimeInBed.TotalHours:F1} год");
Console.WriteLine(analysis.Advice);
Console.WriteLine(analysis.Disclaimer);
```

`TimeInBed` означає час між двома введеними мітками. `EstimatedSleep`
віднімає умовні 15 хв на засинання. `FullModelCycles` — кількість повних
90-хвилинних відрізків. Значення `IllustrativeLightSleep`,
`IllustrativeDeepSleep` та `IllustrativeRemSleep` показують лише приклад
розподілу часу за загальним шаблоном для дорослих (55 %, 20 %, 25 %).
Це **не персональний аналіз фаз** і не медичний висновок. За часом, коли
людина лягла й прокинулася, неможливо дізнатися реальні фази сну або
гарантувати пробудження саме в легкій фазі. Згідно з
[NHLBI](https://www.nhlbi.nih.gov/health/sleep/stages-of-sleep), цикл може
тривати 80–100 хвилин, а стадії визначають за вимірюваннями під час
дослідження сну. Орієнтовні частки стадій наведено в
[огляді інтерпретації досліджень сну](https://pmc.ncbi.nlm.nih.gov/articles/PMC4246141/).

Сервіс перевіряє, що пробудження відбулося пізніше за відхід до сну, а
запис тривав не більше 24 годин. Для цієї бібліотеки достатньо .NET 8 SDK;
Android-емулятор і PostgreSQL для перевірки формул не потрібні:

```powershell
dotnet build src/StudyRhythm.Core/StudyRhythm.Core.csproj
```

## Частина 4 — екран «Сон»

Тепер вкладка «Сон» має робочі XAML та ViewModel:

- `src/StudyRhythm.Mobile/Views/SleepPage.xaml` — поля дат і часу, тижневий
  графік без додаткової бібліотеки, історія, аналіз і два калькулятори.
- `src/StudyRhythm.Mobile/ViewModels/SleepViewModel.cs` — команди кнопок,
  перевірка введених дат, середнє за поточний тиждень і дані для графіка.
- `src/StudyRhythm.Mobile/Services/SleepApiClient.cs` — HTTP-запити до API.
- `src/StudyRhythm.Api/Endpoints/SleepEndpoints.cs` — маршрути
  `GET /api/sleep`, `GET /api/sleep/{id}`, `POST /api/sleep` і
  `DELETE /api/sleep/{id}`.

Після натискання «Зберегти сон» застосунок надсилає час відходу до сну,
час пробудження та назву місцевого часового поясу. Сервер перевіряє
тривалість і зберігає часові мітки у UTC в PostgreSQL. Середнє обчислюється
за **записами, що закінчилися цього тижня**; якщо записів кілька за день,
графік показує їхню сумарну тривалість. Це час **між введеними мітками**,
тобто час у ліжку, а не виміряний час фактичного сну.

Щоб запустити на Android-емуляторі:

1. Запустіть PostgreSQL: `docker compose up -d`.
2. Застосуйте міграцію з частини 2, якщо ще не робили цього.
3. Запустіть API: `dotnet run --project src/StudyRhythm.Api --launch-profile http`.
4. Запустіть `StudyRhythm.Mobile` з Visual Studio на емуляторі.
5. На вкладці «Сон» залиште адресу `http://10.0.2.2:5184`, натисніть
   «Оновити історію», потім введіть нічний сон і натисніть «Зберегти сон».

`10.0.2.2` — спеціальна адреса, за якою Android-емулятор бачить комп’ютер.
Для **фізичного телефона** комп’ютер і телефон мають бути в одній мережі.
Запустіть сервер так, щоб він приймав запити з локальної мережі:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://0.0.0.0:5184"
dotnet run --project src/StudyRhythm.Api --no-launch-profile
```

Дізнайтеся локальну IPv4-адресу ПК через `ipconfig` і введіть у полі
«Адреса сервера», наприклад `http://192.168.1.20:5184`. Якщо телефон не
бачить API, перевірте правило брандмауера Windows для порту 5184.
Дозвіл на незашифровані HTTP-запити увімкнено **лише для Debug-збірки**
Android. Перед публічним розгортанням потрібні HTTPS та авторизація API.

Для цієї частини не потрібні нові NuGet-пакети та міграція бази даних.
Локальна адреса API зберігається в налаштуваннях застосунку, щоб її не
доводилося вводити щоразу.

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
