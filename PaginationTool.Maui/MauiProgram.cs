using Microsoft.Extensions.Logging;
using PaginationTool.Shared.Services;

namespace PaginationTool.Maui;

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

		// Add HTTP client
		builder.Services.AddHttpClient();

		// Register custom services
		builder.Services.AddScoped<TokenService>();
		builder.Services.AddScoped<HttpService>();
		builder.Services.AddScoped<JsonService>();
		builder.Services.AddScoped<QueryService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
