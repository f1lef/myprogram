using Microsoft.EntityFrameworkCore;

namespace StudyRhythm.Api.Data;

public sealed class StudyRhythmDbContext(DbContextOptions<StudyRhythmDbContext> options)
    : DbContext(options)
{
    // Таблиці для сну, пар, ДЗ і Pomodoro додамо в частині 2.
}
