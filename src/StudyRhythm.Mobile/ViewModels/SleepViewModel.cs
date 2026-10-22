using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using StudyRhythm.Core.Contracts;
using StudyRhythm.Core.Models;
using StudyRhythm.Core.Services;
using StudyRhythm.Mobile.Models;
using StudyRhythm.Mobile.Services;

namespace StudyRhythm.Mobile.ViewModels;

public partial class SleepViewModel : ObservableObject
{
    private const string ApiAddressPreference = "sleep_api_address";
    private static readonly CultureInfo Ukrainian = CultureInfo.GetCultureInfo("uk-UA");
    private readonly SleepCalculatorService calculator;
    private readonly SleepApiClient apiClient;

    public SleepViewModel(SleepCalculatorService calculator, SleepApiClient apiClient)
    {
        this.calculator = calculator;
        this.apiClient = apiClient;

        var now = DateTime.Now;
        var eightHoursAgo = now.AddHours(-8);
        bedtimeDate = eightHoursAgo.Date;
        bedtimeTime = eightHoursAgo.TimeOfDay;
        wakeDate = now.Date;
        wakeTime = now.TimeOfDay;
        targetWakeDate = now.Date.AddDays(1);
        targetWakeTime = new TimeSpan(7, 0, 0);
        apiAddress = Preferences.Default.Get(ApiAddressPreference, "http://10.0.2.2:5184");
    }

    [ObservableProperty] private DateTime bedtimeDate;
    [ObservableProperty] private TimeSpan bedtimeTime;
    [ObservableProperty] private DateTime wakeDate;
    [ObservableProperty] private TimeSpan wakeTime;
    [ObservableProperty] private DateTime targetWakeDate;
    [ObservableProperty] private TimeSpan targetWakeTime;
    [ObservableProperty] private string apiAddress;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private string averageText = "Ще немає записів цього тижня";
    [ObservableProperty] private string analysisSummary = string.Empty;
    [ObservableProperty] private string stageEstimate = string.Empty;
    [ObservableProperty] private string advice = string.Empty;
    [ObservableProperty] private bool hasAnalysis;
    [ObservableProperty] private bool isHistoryEmpty = true;

    public string ModelDisclaimer => SleepCalculatorService.ModelDisclaimer;

    public ObservableCollection<SleepHistoryRow> History { get; } = [];
    public ObservableCollection<SleepWeekBar> WeekBars { get; } = [];
    public ObservableCollection<string> BedtimeSuggestions { get; } = [];
    public ObservableCollection<string> WakeSuggestions { get; } = [];

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            await LoadEntriesCoreAsync();
            StatusMessage = string.Empty;
        }
        catch (Exception error) when (IsConnectionError(error))
        {
            StatusMessage = ConnectionMessage(error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy) return;

        if (!TryMakeLocalTime(BedtimeDate, BedtimeTime, out var bedtime) ||
            !TryMakeLocalTime(WakeDate, WakeTime, out var wakeAt))
        {
            return;
        }

        SleepAnalysis analysis;
        try
        {
            analysis = calculator.Analyze(bedtime, wakeAt);
        }
        catch (ArgumentOutOfRangeException error)
        {
            StatusMessage = error.Message;
            return;
        }

        if (wakeAt > DateTimeOffset.Now.AddMinutes(5))
        {
            StatusMessage = "Час пробудження не може бути в майбутньому.";
            return;
        }

        IsBusy = true;
        var wasSaved = false;
        try
        {
            var request = new CreateSleepEntryRequest(bedtime, wakeAt, TimeZoneInfo.Local.Id);
            await apiClient.CreateEntryAsync(ApiAddress, request);
            wasSaved = true;
            RememberApiAddress();
            await LoadEntriesCoreAsync();
            ShowAnalysis(analysis);
            StatusMessage = "Запис сну збережено.";
        }
        catch (Exception error) when (IsConnectionError(error))
        {
            StatusMessage = wasSaved
                ? "Запис збережено, але історію не вдалося оновити. Натисни «Оновити історію»."
                : ConnectionMessage(error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(SleepHistoryRow? row)
    {
        if (IsBusy || row is null) return;
        IsBusy = true;
        var wasDeleted = false;
        try
        {
            await apiClient.DeleteEntryAsync(ApiAddress, row.Id);
            wasDeleted = true;
            RememberApiAddress();
            await LoadEntriesCoreAsync();
            StatusMessage = "Запис видалено.";
        }
        catch (Exception error) when (IsConnectionError(error))
        {
            StatusMessage = wasDeleted
                ? "Запис видалено, але історію не вдалося оновити. Натисни «Оновити історію»."
                : ConnectionMessage(error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void CalculateBedtimes()
    {
        if (!TryMakeLocalTime(TargetWakeDate, TargetWakeTime, out var target)) return;

        BedtimeSuggestions.Clear();
        foreach (var option in calculator.GetBedtimeSuggestions(target, TimeZoneInfo.Local))
        {
            BedtimeSuggestions.Add(FormatSuggestion(option));
        }

        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private void CalculateWakeFromNow()
    {
        WakeSuggestions.Clear();
        foreach (var option in calculator.GetWakeSuggestions(DateTimeOffset.Now, TimeZoneInfo.Local))
        {
            WakeSuggestions.Add(FormatSuggestion(option));
        }

        StatusMessage = string.Empty;
    }

    private async Task LoadEntriesCoreAsync()
    {
        var entries = await apiClient.GetEntriesAsync(ApiAddress);
        RememberApiAddress();

        History.Clear();
        foreach (var entry in entries)
        {
            var duration = entry.WakeAt - entry.Bedtime;
            History.Add(new SleepHistoryRow(
                entry.Id,
                entry.WakeLocalDate.ToString("dd MMMM", Ukrainian),
                $"{entry.Bedtime:dd.MM HH:mm} → {entry.WakeAt:dd.MM HH:mm}",
                FormatDuration(duration)));
        }

        IsHistoryEmpty = History.Count == 0;
        BuildWeek(entries);

        if (entries.Count > 0)
        {
            ShowAnalysis(calculator.Analyze(entries[0].Bedtime, entries[0].WakeAt));
        }
        else
        {
            HasAnalysis = false;
        }
    }

    private void BuildWeek(IReadOnlyList<SleepEntryResponse> entries)
    {
        var today = DateTime.Today;
        var monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        var weekStart = DateOnly.FromDateTime(monday);
        var weekEnd = weekStart.AddDays(7);
        var thisWeek = entries
            .Where(entry => entry.WakeLocalDate >= weekStart && entry.WakeLocalDate < weekEnd)
            .ToArray();

        AverageText = thisWeek.Length == 0
            ? "Ще немає записів цього тижня"
            : $"Середнє за {thisWeek.Length} зап.: " +
              FormatDuration(TimeSpan.FromMinutes(
                  Math.Round(thisWeek.Average(entry => (entry.WakeAt - entry.Bedtime).TotalMinutes))));

        WeekBars.Clear();
        for (var day = 0; day < 7; day++)
        {
            var date = weekStart.AddDays(day);
            var hours = thisWeek
                .Where(entry => entry.WakeLocalDate == date)
                .Sum(entry => (entry.WakeAt - entry.Bedtime).TotalHours);
            WeekBars.Add(new SleepWeekBar(
                date.ToString("ddd", Ukrainian),
                hours > 0 ? $"{hours:0.#}" : "—",
                hours > 0 ? Math.Max(4, Math.Min(120, hours / 12 * 120)) : 2));
        }
    }

    private void ShowAnalysis(SleepAnalysis analysis)
    {
        AnalysisSummary = $"Час у ліжку: {FormatDuration(analysis.TimeInBed)} · " +
                          $"умовно {analysis.FullModelCycles} повних циклів";
        StageEstimate = $"Ілюстрація фаз: легкий {FormatDuration(analysis.IllustrativeLightSleep)}, " +
                        $"глибокий {FormatDuration(analysis.IllustrativeDeepSleep)}, " +
                        $"REM {FormatDuration(analysis.IllustrativeRemSleep)}";
        Advice = analysis.Advice;
        HasAnalysis = true;
    }

    private bool TryMakeLocalTime(DateTime date, TimeSpan time, out DateTimeOffset result)
    {
        var local = DateTime.SpecifyKind(date.Date + time, DateTimeKind.Unspecified);
        var zone = TimeZoneInfo.Local;
        if (zone.IsInvalidTime(local) || zone.IsAmbiguousTime(local))
        {
            StatusMessage = "Обраний час припадає на зміну годинника. Вибери інший час.";
            result = default;
            return false;
        }

        result = new DateTimeOffset(local, zone.GetUtcOffset(local));
        return true;
    }

    private void RememberApiAddress() =>
        Preferences.Default.Set(ApiAddressPreference, ApiAddress?.Trim() ?? string.Empty);

    private static string FormatSuggestion(SleepSuggestion option) =>
        $"{option.CycleCount} циклів · {option.SuggestedAt:dd.MM HH:mm} " +
        $"({FormatDuration(option.EstimatedSleep)} умовного сну)";

    private static string FormatDuration(TimeSpan duration) =>
        $"{(int)duration.TotalHours} год {duration.Minutes} хв";

    private static bool IsConnectionError(Exception error) =>
        error is HttpRequestException or TaskCanceledException or ArgumentException or
            InvalidOperationException or JsonException;

    private static string ConnectionMessage(Exception error) => error switch
    {
        ArgumentException => "Перевір адресу API: вона має починатися з http:// або https://.",
        TaskCanceledException => "Сервер не відповів вчасно. Перевір адресу API та мережу.",
        HttpRequestException { StatusCode: HttpStatusCode.BadRequest } => "Сервер відхилив запис. Перевір введені дати й часовий пояс.",
        HttpRequestException => "Не вдалося зв’язатися з API. Перевір, чи запущено сервер і чи доступна його адреса.",
        _ => "Сервер повернув неочікувану відповідь. Спробуй ще раз."
    };
}
