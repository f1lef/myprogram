namespace StudyRhythm.Api.Models;

public sealed class Homework
{
    public int Id { get; set; }

    // ДЗ зберігається, навіть якщо користувач видалить пару.
    public int? LessonId { get; set; }
    public Lesson? Lesson { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueAtUtc { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    // null означає, що нагадування вимкнене.
    public int? ReminderMinutesBefore { get; set; }

    public ICollection<PomodoroSession> PomodoroSessions { get; } = new List<PomodoroSession>();
}
