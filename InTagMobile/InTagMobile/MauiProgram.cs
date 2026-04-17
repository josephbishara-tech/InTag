using InTagMobile.Services;
using InTagMobile.ViewModels;
using InTagMobile.Views;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui.Controls;

namespace InTagMobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseBarcodeReader()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif



        // Services
        builder.Services.AddSingleton<IApiService, ApiService>();

#if ANDROID
        builder.Services.AddSingleton<IZebraScannerService, ZebraScannerService>();
#else
        builder.Services.AddSingleton<IZebraScannerService, FallbackScannerService>();
#endif

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RequestListViewModel>();
        builder.Services.AddTransient<TrackingSessionViewModel>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RequestListPage>();
        builder.Services.AddTransient<TrackingSessionPage>();

        return builder.Build();
    }
}
