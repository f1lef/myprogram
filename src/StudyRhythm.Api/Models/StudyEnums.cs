namespace StudyRhythm.Api.Models;

// Значення 1–7 зручні для розкладу: понеділок — перший день.
public enum StudyDayOfWeek
{
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6,
    Sunday = 7
}

public enum WeekParity
{
    EveryWeek = 0,
    Odd = 1,
    Even = 2
}

public enum StreakGoalType
{
    AllDueHomework = 1,
    OnePomodoro = 2,
    SleepRecorded = 3
}
