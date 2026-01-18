using HouseApp.Services;
using HouseApp.ViewModels;
using HouseApp.Views;

namespace HouseApp;

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

        // HttpClient
        builder.Services.AddSingleton(sp => new HttpClient
        {
            BaseAddress = new Uri(Constants.ApiBaseUrl)
        });

        // Services
        builder.Services.AddSingleton<UserSession>();
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<ChatService>();
        builder.Services.AddSingleton<PaymentService>();
        builder.Services.AddSingleton<HouseService>();

        // ViewModels
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddTransient<RegistrationViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<ChatViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<LandlordDashboardViewModel>();
        builder.Services.AddTransient<HouseManagementViewModel>();
        builder.Services.AddTransient<PaymentViewModel>();
        builder.Services.AddTransient<HouseSearchViewModel>();

        // Views (Pages)
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<LandlordShell>();
        builder.Services.AddSingleton<StudentShell>();
        builder.Services.AddTransient<RegistrationPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<CalendarPage>();
        builder.Services.AddTransient<ChatPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<LandlordDashboardPage>();
        builder.Services.AddTransient<HouseManagementPage>();
        builder.Services.AddTransient<PaymentPage>();
        builder.Services.AddTransient<HouseSearchPage>();

        return builder.Build();
    }
}
