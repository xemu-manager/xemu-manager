using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using XemuManager.Core.DownloadManager.Downloader;

namespace XemuManager.Core.Xemu;

public class XemuInstaller(IDownloader downloader, IOptions<XemuOptions> options)
{
    private readonly IDownloader _downloader = downloader;
    private readonly XemuOptions _options = options.Value;

    /// <returns>The folder xemu was installed into.</returns>
    public Task<string> InstallAsync(CancellationToken cancellationToken = default)
    {
        var release = _options.Release;
        if (!release.AssetPatterns.TryGetValue(CurrentPlatform(), out var pattern))
            throw new PlatformNotSupportedException($"No xemu asset configured for {CurrentPlatform()}.");

        var assetRegex = new Regex(pattern, RegexOptions.IgnoreCase);
        return _downloader.DownloadAsync(
            release.Owner,
            release.Repo,
            asset => assetRegex.IsMatch(asset.Name),
            _options.InstallDirectory,
            cancellationToken);
    }

    private static string CurrentPlatform()
    {
        if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
            return "macos";
        var arch = RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "arm64" : "x64";
        return $"windows-{arch}";
    }
}
