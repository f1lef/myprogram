namespace StudyRhythm.Api.Models;

public sealed class AppSettings
{
    // Один рядок налаштувань для цього навчального однокористувацького застосунку.
    public int Id { get; set; } = 1;

    public DateOnly ReferenceMonday { get; set; }
    public WeekParity ReferenceWeekParity { get; set; } = WeekParity.Odd;
    public StreakGoalType StreakGoal { get; set; } = StreakGoalType.OnePomodoro;

    public int FocusMinutes { get; set; } = 25;
    public int ShortBreakMinutes { get; set; } = 5;
    public int LongBreakMinutes { get; set; } = 15;
    public int LongBreakEvery { get; set; } = 4;
}
