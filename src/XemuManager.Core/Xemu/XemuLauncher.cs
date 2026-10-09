using System.Diagnostics;
using Microsoft.Extensions.Options;
using XemuManager.Core.LibraryManager;

namespace XemuManager.Core.Xemu;

public class XemuLauncher(IOptions<XemuOptions> options)
{
    private readonly XemuOptions _options = options.Value;

    /// <summary>
    /// Starts xemu with the game in the DVD drive.
    /// </summary>
    /// <param name="qmpPort">If set, xemu listens for QMP on 127.0.0.1 at this port.</param>
    /// <returns>The running xemu process.</returns>
    public Process Launch(GameMetaData game, bool fullScreen = false, int? qmpPort = null)
    {
        var xemuPath = ExecutablePath();
        if (!File.Exists(xemuPath))
            throw new FileNotFoundException("xemu is not installed.", xemuPath);
        if (!File.Exists(game.FilePath))
            throw new FileNotFoundException("Game file not found.", game.FilePath);

        var startInfo = new ProcessStartInfo
        {
            FileName = xemuPath,
            WorkingDirectory = _options.InstallDirectory,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add("-dvd_path");
        startInfo.ArgumentList.Add(game.FilePath);
        if (fullScreen)
            startInfo.ArgumentList.Add("-full-screen");
        if (qmpPort is { } port)
        {
            // server=on: xemu listens and we connect; wait=off: don't block boot until we connect
            startInfo.ArgumentList.Add("-qmp");
            startInfo.ArgumentList.Add($"tcp:127.0.0.1:{port},server=on,wait=off");
        }

        return Process.Start(startInfo)
               ?? throw new InvalidOperationException("xemu could not be started.");
    }

    /// <summary>Full path of the xemu program for this platform.</summary>
    private string ExecutablePath() =>
        Path.Combine(_options.InstallDirectory, XemuPlatform.Current.Executable);
}
