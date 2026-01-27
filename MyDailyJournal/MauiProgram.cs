using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using MyDailyJournal.MauiBlazor.Services;
using MyDailyJournal.Services;

namespace MyDailyJournal;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Register Services
        builder.Services.AddSingleton<AuthenticationService>();
        builder.Services.AddSingleton<JournalService>();
        builder.Services.AddSingleton<StreakService>();
        builder.Services.AddMudServices();

        using var scope = builder.Services.BuildServiceProvider();
        using var journalDbContext = new JournalDbContext();
        journalDbContext.Database.Migrate();

        // Initialize Database
        using (var db = new JournalDbContext())
        {
            db.Database.EnsureCreated();
        }

        return builder.Build();
    }
}