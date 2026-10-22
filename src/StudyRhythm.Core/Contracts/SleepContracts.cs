namespace StudyRhythm.Core.Contracts;

// Контракти спільні для Android-застосунку та Web API.
public sealed record CreateSleepEntryRequest(
    DateTimeOffset Bedtime,
    DateTimeOffset WakeAt,
    string TimeZoneId);

public sealed record SleepEntryResponse(
    int Id,
    DateTimeOffset Bedtime,
    DateTimeOffset WakeAt,
    DateOnly WakeLocalDate,
    string TimeZoneId);
