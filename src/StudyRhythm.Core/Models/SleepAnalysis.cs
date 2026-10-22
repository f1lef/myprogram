namespace StudyRhythm.Core.Models;

/// <summary>
/// Результат арифметичної моделі. Поля зі стадіями не є вимірюванням сну.
/// </summary>
public sealed record SleepAnalysis(
    TimeSpan TimeInBed,
    TimeSpan EstimatedSleep,
    int FullModelCycles,
    TimeSpan RemainingModelCycle,
    TimeSpan IllustrativeLightSleep,
    TimeSpan IllustrativeDeepSleep,
    TimeSpan IllustrativeRemSleep,
    string Advice,
    string Disclaimer);
