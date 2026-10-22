using StudyRhythm.Core.Services;
using StudyRhythm.Mobile.Services;
using StudyRhythm.Mobile.ViewModels;
using StudyRhythm.Mobile.Views;

namespace StudyRhythm.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>()
            .ConfigureFonts(font =>
            {
                font.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                font.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // DI передає сервіси у ViewModel, а ViewModel — у сторінку «Сон».
        builder.Services.AddSingleton<SleepCalculatorService>();
        builder.Services.AddSingleton(new HttpClient { Timeout = TimeSpan.FromSeconds(10) });
        builder.Services.AddSingleton<SleepApiClient>();
        builder.Services.AddTransient<SleepViewModel>();
        builder.Services.AddTransient<SleepPage>();

        return builder.Build();
    }
}
