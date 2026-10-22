using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StudyRhythm.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "app_settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ReferenceMonday = table.Column<DateOnly>(type: "date", nullable: false),
                    ReferenceWeekParity = table.Column<int>(type: "integer", nullable: false),
                    StreakGoal = table.Column<int>(type: "integer", nullable: false),
                    FocusMinutes = table.Column<int>(type: "integer", nullable: false),
                    ShortBreakMinutes = table.Column<int>(type: "integer", nullable: false),
                    LongBreakMinutes = table.Column<int>(type: "integer", nullable: false),
                    LongBreakEvery = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "lessons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Subject = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Room = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Teacher = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ColorHex = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    WeekParity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lessons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sleep_entries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SleepStartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WakeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WakeLocalDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TimeZoneId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sleep_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "streak_days",
                columns: table => new
                {
                    LocalDate = table.Column<DateOnly>(type: "date", nullable: false),
                    GoalType = table.Column<int>(type: "integer", nullable: false),
                    IsAchieved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_streak_days", x => x.LocalDate);
                });

            migrationBuilder.CreateTable(
                name: "homework_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LessonId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DueAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReminderMinutesBefore = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_homework_items_lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "lesson_notes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LessonId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lesson_notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lesson_notes_lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pomodoro_sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FocusMinutes = table.Column<int>(type: "integer", nullable: false),
                    LessonId = table.Column<int>(type: "integer", nullable: true),
                    HomeworkId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pomodoro_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pomodoro_sessions_homework_items_HomeworkId",
                        column: x => x.HomeworkId,
                        principalTable: "homework_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_pomodoro_sessions_lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_homework_items_IsCompleted_DueAtUtc",
                table: "homework_items",
                columns: new[] { "IsCompleted", "DueAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_homework_items_LessonId",
                table: "homework_items",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_lesson_notes_LessonId",
                table: "lesson_notes",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_lessons_DayOfWeek_StartTime",
                table: "lessons",
                columns: new[] { "DayOfWeek", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_pomodoro_sessions_FinishedAtUtc",
                table: "pomodoro_sessions",
                column: "FinishedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_pomodoro_sessions_HomeworkId",
                table: "pomodoro_sessions",
                column: "HomeworkId");

            migrationBuilder.CreateIndex(
                name: "IX_pomodoro_sessions_LessonId",
                table: "pomodoro_sessions",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_sleep_entries_WakeLocalDate",
                table: "sleep_entries",
                column: "WakeLocalDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_settings");

            migrationBuilder.DropTable(
                name: "lesson_notes");

            migrationBuilder.DropTable(
                name: "pomodoro_sessions");

            migrationBuilder.DropTable(
                name: "sleep_entries");

            migrationBuilder.DropTable(
                name: "streak_days");

            migrationBuilder.DropTable(
                name: "homework_items");

            migrationBuilder.DropTable(
                name: "lessons");
        }
    }
}
