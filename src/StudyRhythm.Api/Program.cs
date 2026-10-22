using Microsoft.EntityFrameworkCore;
using StudyRhythm.Api.Data;
using StudyRhythm.Api.Endpoints;
using StudyRhythm.Core.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("StudyRhythm")
    ?? throw new InvalidOperationException("Налаштуйте ConnectionStrings:StudyRhythm.");

builder.Services.AddDbContext<StudyRhythmDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddSingleton<SleepCalculatorService>();

var app = builder.Build();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/health/database", async (
    StudyRhythmDbContext database,
    CancellationToken cancellationToken) =>
{
    if (await database.Database.CanConnectAsync(cancellationToken))
    {
        return Results.Ok(new { status = "ok", database = "postgresql" });
    }

    return Results.Problem(
        title: "Немає з’єднання з PostgreSQL",
        statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.MapGet("/api/health/schema", async (
    StudyRhythmDbContext database,
    CancellationToken cancellationToken) =>
{
    if (!await database.Database.CanConnectAsync(cancellationToken))
    {
        return Results.Problem(
            title: "Немає з’єднання з PostgreSQL",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    var pendingMigrations = await database.Database
        .GetPendingMigrationsAsync(cancellationToken);

    if (pendingMigrations.Any())
    {
        return Results.Problem(
            title: "Потрібно застосувати міграцію бази даних",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    return Results.Ok(new { status = "ok", schema = "up-to-date" });
});

app.MapSleepEndpoints();

app.Run();
