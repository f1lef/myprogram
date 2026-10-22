namespace StudyRhythm.Mobile.Models;

public sealed record SleepHistoryRow(
    int Id,
    string DateText,
    string PeriodText,
    string DurationText);
