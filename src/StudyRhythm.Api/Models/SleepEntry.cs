namespace StudyRhythm.Api.Models;

public sealed class SleepEntry
{
    public int Id { get; set; }

    // Часові мітки зберігаються у UTC. Їхню тривалість обчислюємо, а не дублюємо.
    public DateTime SleepStartUtc { get; set; }
    public DateTime WakeUtc { get; set; }

    // Дата пробудження за місцевим часом потрібна для тижневого графіка.
    public DateOnly WakeLocalDate { get; set; }
    public string TimeZoneId { get; set; } = string.Empty;
}
