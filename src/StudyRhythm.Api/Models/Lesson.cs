namespace StudyRhythm.Api.Models;

public sealed class Lesson
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public StudyDayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Room { get; set; }
    public string? Teacher { get; set; }
    public string ColorHex { get; set; } = "#286555";
    public WeekParity WeekParity { get; set; } = WeekParity.EveryWeek;

    public ICollection<LessonNote> Notes { get; } = new List<LessonNote>();
    public ICollection<Homework> HomeworkItems { get; } = new List<Homework>();
    public ICollection<PomodoroSession> PomodoroSessions { get; } = new List<PomodoroSession>();
}
