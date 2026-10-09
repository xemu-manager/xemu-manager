using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using XemuManager.Core.DownloadManager.Downloader;

namespace XemuManager.Core.Xemu;

public class XemuInstaller(IDownloader downloader, IOptions<XemuOptions> options)
{
    private readonly IDownloader _downloader = downloader;
    private readonly XemuOptions _options = options.Value;

    /// <returns>The folder xemu was installed into.</returns>
    public async Task<string> InstallAsync(CancellationToken cancellationToken = default)
    {
        var platform = XemuPlatform.Current;
        var assetRegex = new Regex(platform.AssetPattern, RegexOptions.IgnoreCase);

        var installDirectory = await _downloader.DownloadAsync(
            _options.Release.Owner,
            _options.Release.Repo,
            asset => assetRegex.IsMatch(asset.Name),
            _options.InstallDirectory,
            cancellationToken);

        RenameToExecutable(installDirectory, platform, assetRegex);
        return installDirectory;
    }

    /// <summary>
    /// A single-file asset (e.g. an AppImage) keeps its versioned name after download.
    /// Rename it to the fixed name the launcher looks for. Zip assets are already extracted
    /// and deleted, so nothing matches and nothing happens.
    /// </summary>
    private static void RenameToExecutable(string installDirectory, XemuPlatform platform, Regex assetRegex)
    {
        var downloaded = Directory.EnumerateFiles(installDirectory)
            .FirstOrDefault(path => assetRegex.IsMatch(Path.GetFileName(path)));
        if (downloaded is null)
            return;

        File.Move(downloaded, Path.Combine(installDirectory, platform.Executable), overwrite: true);
    }
}
