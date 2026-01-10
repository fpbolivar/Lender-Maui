using Microsoft.Extensions.Logging;
using Lender.Services;
using Lender.Services.Interfaces;
using Lender.ViewModels;

namespace Lender;

public static class MauiProgram
{
	/// <summary>
	/// Provides access to the MAUI service provider for dependency injection.
	/// </summary>
	public static IServiceProvider? ServiceProvider { get; private set; }

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			.ConfigureServices();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();
		ServiceProvider = app.Services;
		return app;
	}

	private static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
	{
		// Register authentication services
		builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
		
		// Register App Check service for Firebase Security
		builder.Services.AddSingleton<IAppCheckService, AppCheckService>();
		
		// Register view models
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<DashboardViewModel>();
		
		// Register pages
		builder.Services.AddTransient<LoginPage>();

		

		return builder;
	}
}

