using Microsoft.Extensions.Logging;
using StudentCabinetMVVM.Data;
using StudentCabinetMVVM.Services;
using StudentCabinetMVVM.ViewModels;

namespace StudentCabinetMVVM;

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
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "students.db3");

        builder.Services.AddSingleton<IAppStateService, AppStateService>();
        builder.Services.AddSingleton(new AppDatabase(dbPath));
        builder.Services.AddSingleton<IEncryptionService, AesEncryptionService>();
        builder.Services.AddSingleton<IStudentRepository, StudentRepository>();
        builder.Services.AddSingleton<ISyncService, SyncService>();
        builder.Services.AddTransient<StudentViewModel>();
        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}