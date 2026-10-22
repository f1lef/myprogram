using StudyRhythm.Core.Models;

namespace StudyRhythm.Core.Services;

/// <summary>
/// Навчальний калькулятор: 90 хв на цикл і 15 хв на засинання.
/// Реальні цикли різняться; за самими годинами стадії сну визначити не можна.
/// </summary>
public sealed class SleepCalculatorService
{
    public const int MinutesPerModelCycle = 90;
    public const int MinutesToFallAsleep = 15;

    public const string ModelDisclaimer =
        "Це приблизна модель, а не вимірювання. Цикли сну мають різну " +
        "тривалість; час засинання та фази сну за введеними годинами " +
        "визначити неможливо. Підказки не гарантують пробудження " +
        "у фазі легкого сну.";

    private static readonly int[] SuggestedCycleCounts = [6, 5, 4];

    /// <summary>Три варіанти часу, коли лягти для бажаного пробудження.</summary>
    public IReadOnlyList<SleepSuggestion> GetBedtimeSuggestions(
        DateTimeOffset desiredWakeAt,
        TimeZoneInfo timeZone)
    {
        ArgumentNullException.ThrowIfNull(timeZone);

        return SuggestedCycleCounts.Select(cycles =>
        {
            var estimatedSleep = TimeSpan.FromMinutes(cycles * MinutesPerModelCycle);
            var timeInBed = estimatedSleep + TimeSpan.FromMinutes(MinutesToFallAsleep);
            var suggestedAt = TimeZoneInfo.ConvertTime(desiredWakeAt - timeInBed, timeZone);
            return new SleepSuggestion(cycles, suggestedAt, estimatedSleep, timeInBed);
        }).ToArray();
    }

    /// <summary>Три варіанти часу пробудження, якщо лягти у вказаний час.</summary>
    public IReadOnlyList<SleepSuggestion> GetWakeSuggestions(
        DateTimeOffset bedtime,
        TimeZoneInfo timeZone)
    {
        ArgumentNullException.ThrowIfNull(timeZone);

        return SuggestedCycleCounts.Reverse().Select(cycles =>
        {
            var estimatedSleep = TimeSpan.FromMinutes(cycles * MinutesPerModelCycle);
            var timeInBed = estimatedSleep + TimeSpan.FromMinutes(MinutesToFallAsleep);
            var suggestedAt = TimeZoneInfo.ConvertTime(bedtime + timeInBed, timeZone);
            return new SleepSuggestion(cycles, suggestedAt, estimatedSleep, timeInBed);
        }).ToArray();
    }

    /// <summary>
    /// Аналіз ручного запису. DateTimeOffset зберігає зсув від UTC, тому
    /// перехід через північ або зміну годинника не спотворює тривалість.
    /// </summary>
    public SleepAnalysis Analyze(DateTimeOffset bedtime, DateTimeOffset wakeAt)
    {
        var timeInBed = wakeAt.ToUniversalTime() - bedtime.ToUniversalTime();
        if (timeInBed <= TimeSpan.Zero || timeInBed > TimeSpan.FromHours(24))
        {
            throw new ArgumentOutOfRangeException(
                nameof(wakeAt),
                "Час пробудження має бути пізнішим за час відходу до сну; запис не може тривати понад 24 години.");
        }

        var estimatedSleep = timeInBed - TimeSpan.FromMinutes(MinutesToFallAsleep);
        if (estimatedSleep < TimeSpan.Zero)
        {
            estimatedSleep = TimeSpan.Zero;
        }

        var modelCycle = TimeSpan.FromMinutes(MinutesPerModelCycle);
        var fullCycles = (int)(estimatedSleep.Ticks / modelCycle.Ticks);
        var remainder = TimeSpan.FromTicks(estimatedSleep.Ticks % modelCycle.Ticks);

        // Ілюстрація типового розподілу стадій у дорослих, а не персональний результат.
        // N1 + N2 ≈ 55 %, N3 ≈ 20 %, REM ≈ 25 %.
        var light = TimeSpan.FromTicks(estimatedSleep.Ticks * 55 / 100);
        var deep = TimeSpan.FromTicks(estimatedSleep.Ticks * 20 / 100);
        var rem = estimatedSleep - light - deep;

        string advice;
        if (estimatedSleep < modelCycle)
        {
            advice = "Запис закороткий для оцінки повного умовного циклу.";
        }
        else if (fullCycles < 5)
        {
            advice = "За цією моделлю вийшло менше п’яти повних циклів. Якщо можливо, виділи більше часу на сон.";
        }
        else if (remainder <= TimeSpan.FromMinutes(10) ||
                 remainder >= modelCycle - TimeSpan.FromMinutes(10))
        {
            advice = "Час пробудження близький до умовної межі циклу. Орієнтуйся також на самопочуття.";
        }
        else
        {
            var earlierBy = modelCycle - remainder;
            advice = $"За умовною моделлю до наступної межі циклу бракує приблизно {Math.Ceiling(earlierBy.TotalMinutes):0} хв. " +
                     "Можна спробувати лягти раніше, якщо це зручно.";
        }

        return new SleepAnalysis(
            timeInBed,
            estimatedSleep,
            fullCycles,
            remainder,
            light,
            deep,
            rem,
            advice,
            ModelDisclaimer);
    }
}
