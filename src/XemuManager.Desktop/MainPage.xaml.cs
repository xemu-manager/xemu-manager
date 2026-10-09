using XemuManager.Core.LibraryManager;
using XemuManager.Core.Xemu;

namespace XemuManager.Desktop;

public partial class MainPage : ContentPage
{
	private readonly XemuLauncher _launcher;

	public MainPage(XemuLauncher launcher)
	{
		InitializeComponent();
		_launcher = launcher;
	}

	private void OnLaunchClicked(object? sender, EventArgs e)
	{
		var path = GamePathEntry.Text?.Trim().Trim('"');
		if (string.IsNullOrEmpty(path))
		{
			StatusLabel.Text = "Enter the path of a game file.";
			return;
		}

		try
		{
			if (XisoReader.Read(path) is not { } game)
			{
				StatusLabel.Text = "This file is not an Xbox game image.";
				return;
			}

			_launcher.Launch(game);
			StatusLabel.Text = $"Started {game.Title} ({game.TitleId}).";
		}
		catch (Exception ex)
		{
			StatusLabel.Text = ex.Message;
		}
	}
}
