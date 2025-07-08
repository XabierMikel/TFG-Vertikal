
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vertikal.Core.Interfaces;
using Vertikal.Core.Services;
using Vertikal.Helpers;
using Vertikal.Services;


namespace Vertikal
{
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

             // Carga la configuración de Firebase
             var firebaseConfig = ConfigManager.LoadFirebaseConfigFromFile();

            // Registro de servicios          
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<IApiClient, ApiClient>();           
            builder.Services.AddSingleton<IFirebaseUsersServices, FirebaseUsersServices>();
            builder.Services.AddSingleton<IFirebaseSummitService, FirebaseSummitService>();
            builder.Services.AddSingleton<IFirebaseAscentService, FirebaseAscentService>();
            builder.Services.AddSingleton<ISecureStorageService, MauiSecureStorageService>();
            builder.Services.AddSingleton<IAuthClient>(sp =>
            {
                return new MauiFirebaseAuthClient(firebaseConfig);
            });
            builder.Services.AddSingleton<IFirebaseAuthService>(sp =>
            {
                var apiClient = sp.GetRequiredService<IApiClient>();
                var authClient = sp.GetRequiredService<IAuthClient>();
                var secureStorage = sp.GetRequiredService<ISecureStorageService>();
                var firebaseApiKey = firebaseConfig.ApiKey;
                return new FirebaseAuthService(apiClient, authClient, secureStorage, firebaseApiKey);
             });

            // Registro de la AppShell
            builder.Services.AddTransient<Views.RegisterPage>();
            builder.Services.AddTransient<Views.MainPage>();
            builder.Services.AddTransient<Views.ForgotPasswordPage>();
            builder.Services.AddTransient<Views.HistorialPage>();
            builder.Services.AddTransient<Views.SummitsPage>();
            builder.Services.AddTransient<Views.UserProfilePage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
           
        }
    }
}
