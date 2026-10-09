using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XemuManager.Core.DownloadManager.Downloader;
using XemuManager.Core.DownloadManager.FileDownloader;
using XemuManager.Core.DownloadManager.ReleaseManager;
using XemuManager.Core.Xemu;

namespace XemuManager.Desktop;

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

		AddConfiguration(builder.Configuration);
		AddServices(builder.Services, builder.Configuration);

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	/// <summary>
	/// 1. appsettings.json embedded in the app (release info, defaults)
	/// 2. settings.json in the user's app data folder (overrides 1, optional)
	/// </summary>
	private static void AddConfiguration(ConfigurationManager configuration)
	{
		var appSettings = Assembly.GetExecutingAssembly()
			.GetManifestResourceStream("XemuManager.Desktop.appsettings.json");
		if (appSettings is not null)
			configuration.AddJsonStream(appSettings);

		var userSettingsPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"XemuManager",
			"settings.json");
		configuration.AddJsonFile(userSettingsPath, optional: true, reloadOnChange: false);
	}

	private static void AddServices(IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<XemuOptions>(configuration.GetSection(XemuOptions.SectionName));

		services.AddHttpClient<IGitHubReleaseManager, GitHubReleaseManager>();
		services.AddHttpClient<IFileDownloader, FileDownloader>();
		services.AddTransient<IDownloader, Downloader>();
		services.AddTransient<XemuInstaller>();
	}
}
