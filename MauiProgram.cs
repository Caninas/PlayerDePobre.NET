using CommunityToolkit.Maui;
using PlayerDePobre.ViewModel;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using CommunityToolkit.Maui.Views;

namespace PlayerDePobre;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.UseMauiCommunityToolkitMediaElement()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddSingleton<MediaElement>();

        builder.Services.AddSingleton<MusicasVM>();

        builder.Services.AddSingleton<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
