using Microsoft.EntityFrameworkCore;
using StudyRhythm.Api.Models;

namespace StudyRhythm.Api.Data;

public sealed class StudyRhythmDbContext(DbContextOptions<StudyRhythmDbContext> options)
    : DbContext(options)
{
    public DbSet<SleepEntry> SleepEntries => Set<SleepEntry>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<LessonNote> LessonNotes => Set<LessonNote>();
    public DbSet<Homework> HomeworkItems => Set<Homework>();
    public DbSet<PomodoroSession> PomodoroSessions => Set<PomodoroSession>();
    public DbSet<StreakDay> StreakDays => Set<StreakDay>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SleepEntry>(entity =>
        {
            entity.ToTable("sleep_entries");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.TimeZoneId).HasMaxLength(100).IsRequired();
            entity.HasIndex(item => item.WakeLocalDate);
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.ToTable("lessons");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Subject).HasMaxLength(120).IsRequired();
            entity.Property(item => item.Room).HasMaxLength(80);
            entity.Property(item => item.Teacher).HasMaxLength(120);
            entity.Property(item => item.ColorHex).HasMaxLength(7).IsRequired();
            entity.Property(item => item.DayOfWeek).HasConversion<int>();
            entity.Property(item => item.WeekParity).HasConversion<int>();
            entity.HasIndex(item => new { item.DayOfWeek, item.StartTime });
        });

        modelBuilder.Entity<LessonNote>(entity =>
        {
            entity.ToTable("lesson_notes");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Content).HasMaxLength(10_000).IsRequired();
            entity.HasOne(item => item.Lesson)
                .WithMany(lesson => lesson.Notes)
                .HasForeignKey(item => item.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Homework>(entity =>
        {
            entity.ToTable("homework_items");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Title).HasMaxLength(160).IsRequired();
            entity.Property(item => item.Description).HasMaxLength(4_000);
            entity.HasIndex(item => new { item.IsCompleted, item.DueAtUtc });
            entity.HasOne(item => item.Lesson)
                .WithMany(lesson => lesson.HomeworkItems)
                .HasForeignKey(item => item.LessonId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PomodoroSession>(entity =>
        {
            entity.ToTable("pomodoro_sessions");
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => item.FinishedAtUtc);
            entity.HasOne(item => item.Lesson)
                .WithMany(lesson => lesson.PomodoroSessions)
                .HasForeignKey(item => item.LessonId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(item => item.Homework)
                .WithMany(homework => homework.PomodoroSessions)
                .HasForeignKey(item => item.HomeworkId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<StreakDay>(entity =>
        {
            entity.ToTable("streak_days");
            entity.HasKey(item => item.LocalDate);
            entity.Property(item => item.GoalType).HasConversion<int>();
        });

        modelBuilder.Entity<AppSettings>(entity =>
        {
            entity.ToTable("app_settings");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).ValueGeneratedNever();
            entity.Property(item => item.ReferenceWeekParity).HasConversion<int>();
            entity.Property(item => item.StreakGoal).HasConversion<int>();
        });
    }
}
