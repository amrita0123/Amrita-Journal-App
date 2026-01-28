using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using MyDailyJournal.Data;
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

        builder.Services.AddSingleton<AuthenticationService>();
        builder.Services.AddSingleton<JournalService>();
        builder.Services.AddSingleton<StreakService>();
        builder.Services.AddMudServices();
        builder.Services.AddScoped<MoodService>();
        builder.Services.AddScoped<SecurityService>();
        builder.Services.AddSingleton<PdfExportService>();
        builder.Services.AddSingleton<IFileService, FileService>();


        builder.Services.AddDbContext<JournalDbContext>(options =>
        {
            var dbPath = DatabasePath.GetDatabasePath();
            options.UseSqlite($"Data Source={dbPath}");
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JournalDbContext>();
            db.Database.EnsureCreated(); 
        }

        return app;
    }
}
