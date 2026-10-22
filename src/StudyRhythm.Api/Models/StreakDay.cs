namespace StudyRhythm.Api.Models;

public sealed class StreakDay
{
    // Один запис на календарний день користувача.
    public DateOnly LocalDate { get; set; }
    public StreakGoalType GoalType { get; set; }
    public bool IsAchieved { get; set; }
}
