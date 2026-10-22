using Microsoft.EntityFrameworkCore;
using StudyRhythm.Api.Data;
using StudyRhythm.Api.Models;
using StudyRhythm.Core.Contracts;
using StudyRhythm.Core.Services;

namespace StudyRhythm.Api.Endpoints;

public static class SleepEndpoints
{
    public static RouteGroupBuilder MapSleepEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sleep");

        group.MapGet("", async (StudyRhythmDbContext database, CancellationToken cancellationToken) =>
        {
            var entries = await database.SleepEntries
                .AsNoTracking()
                .OrderByDescending(entry => entry.WakeUtc)
                .ToListAsync(cancellationToken);

            return Results.Ok(entries.Select(ToResponse).ToArray());
        });

        group.MapGet("/{id:int}", async (
            int id,
            StudyRhythmDbContext database,
            CancellationToken cancellationToken) =>
        {
            var entry = await database.SleepEntries.AsNoTracking()
                .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
            return entry is null ? Results.NotFound() : Results.Ok(ToResponse(entry));
        });

        group.MapPost("", async (
            CreateSleepEntryRequest request,
            StudyRhythmDbContext database,
            SleepCalculatorService calculator,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.TimeZoneId) || request.TimeZoneId.Length > 100)
            {
                return Results.BadRequest(new { error = "Укажіть коректний часовий пояс." });
            }

            TimeZoneInfo timeZone;
            try
            {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById(request.TimeZoneId);
                calculator.Analyze(request.Bedtime, request.WakeAt);
            }
            catch (TimeZoneNotFoundException)
            {
                return Results.BadRequest(new { error = "Невідомий часовий пояс." });
            }
            catch (InvalidTimeZoneException)
            {
                return Results.BadRequest(new { error = "Некоректний часовий пояс." });
            }
            catch (ArgumentOutOfRangeException)
            {
                return Results.BadRequest(new { error = "Пробудження має бути після відходу до сну, а тривалість — не більше 24 годин." });
            }

            if (request.WakeAt > DateTimeOffset.UtcNow.AddMinutes(5))
            {
                return Results.BadRequest(new { error = "Час пробудження не може бути в майбутньому." });
            }

            var localWake = TimeZoneInfo.ConvertTime(request.WakeAt, timeZone);
            var entry = new SleepEntry
            {
                SleepStartUtc = request.Bedtime.UtcDateTime,
                WakeUtc = request.WakeAt.UtcDateTime,
                WakeLocalDate = DateOnly.FromDateTime(localWake.Date),
                TimeZoneId = timeZone.Id
            };

            database.SleepEntries.Add(entry);
            await database.SaveChangesAsync(cancellationToken);
            return Results.Created($"/api/sleep/{entry.Id}", ToResponse(entry));
        });

        group.MapDelete("/{id:int}", async (
            int id,
            StudyRhythmDbContext database,
            CancellationToken cancellationToken) =>
        {
            var entry = await database.SleepEntries
                .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
            if (entry is null)
            {
                return Results.NotFound();
            }

            database.SleepEntries.Remove(entry);
            await database.SaveChangesAsync(cancellationToken);
            return Results.NoContent();
        });

        return group;
    }

    private static SleepEntryResponse ToResponse(SleepEntry entry)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(entry.TimeZoneId);
        var bedtime = TimeZoneInfo.ConvertTime(new DateTimeOffset(entry.SleepStartUtc, TimeSpan.Zero), timeZone);
        var wakeAt = TimeZoneInfo.ConvertTime(new DateTimeOffset(entry.WakeUtc, TimeSpan.Zero), timeZone);
        return new SleepEntryResponse(entry.Id, bedtime, wakeAt, entry.WakeLocalDate, entry.TimeZoneId);
    }
}
