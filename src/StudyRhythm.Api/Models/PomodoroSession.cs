namespace StudyRhythm.Api.Models;

public sealed class PomodoroSession
{
    public int Id { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime FinishedAtUtc { get; set; }
    public int FocusMinutes { get; set; }

    // Сесію можна пов’язати з предметом або конкретним ДЗ.
    public int? LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    public int? HomeworkId { get; set; }
    public Homework? Homework { get; set; }
}
