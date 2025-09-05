using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace AveTranslatorM
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddScoped(sp => new HttpClient { });

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<GameSelectionService>();
            builder.Services.AddSingleton<WorkingService>();

            return builder.Build();

        }
    }
}
