using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace Prototype;

public static class MauiProgramExtensions
{
	public static MauiAppBuilder UseSharedMauiApp(this MauiAppBuilder builder)
	{
		builder
			.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("BebasNeue-Regular.ttf", "BebasNeue");
				fonts.AddFont("Roboto-Medium.ttf", "RobotoMedium");
				fonts.AddFont("LINESeedJP-Regular.ttf", "LineSeed");
				fonts.AddFont("LINESeedJP-thin.ttf", "LineSeedThin");
				fonts.AddFont("LINESeedJP-Bold.ttf", "LineSeedBold");
            });

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder;
	}
}
