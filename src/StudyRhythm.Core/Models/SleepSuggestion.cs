namespace StudyRhythm.Core.Models;

/// <summary>Час для відходу до сну або пробудження за спрощеною моделлю.</summary>
public sealed record SleepSuggestion(
    int CycleCount,
    DateTimeOffset SuggestedAt,
    TimeSpan EstimatedSleep,
    TimeSpan TimeInBed);
