using StudyRhythm.Core.Services;

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

        // Один екземпляр сервісу буде доступний майбутній ViewModel екрана «Сон».
        builder.Services.AddSingleton<SleepCalculatorService>();

        return builder.Build();
    }
}
