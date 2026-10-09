using System.Runtime.InteropServices;

namespace XemuManager.Core.Xemu;

/// <summary>
/// Everything about xemu that differs between platforms, keyed by .NET runtime identifier.
/// </summary>
/// <param name="AssetPattern">Regex for the release asset to download.</param>
/// <param name="Executable">Path of the program to run, relative to the install directory.</param>
public record XemuPlatform(string AssetPattern, string Executable)
{
    private const string MacAsset = @"^xemu-[0-9.]+-macos-universal\.zip$";
    private const string MacExecutable = "xemu.app/Contents/MacOS/xemu";

    private static readonly Dictionary<string, XemuPlatform> Platforms = new()
    {
        ["win-x64"] = new(@"^xemu-[0-9.]+-windows-x86_64\.zip$", "xemu.exe"),
        ["win-arm64"] = new(@"^xemu-[0-9.]+-windows-arm64\.zip$", "xemu.exe"),
        ["linux-x64"] = new(@"^xemu-[0-9.]+-x86_64\.AppImage$", "xemu.AppImage"),
        ["linux-arm64"] = new(@"^xemu-[0-9.]+-aarch64\.AppImage$", "xemu.AppImage"),
        ["osx-x64"] = new(MacAsset, MacExecutable),
        ["osx-arm64"] = new(MacAsset, MacExecutable),
        ["maccatalyst-x64"] = new(MacAsset, MacExecutable),
        ["maccatalyst-arm64"] = new(MacAsset, MacExecutable),
    };

    public static XemuPlatform Current =>
        Platforms.TryGetValue(RuntimeInformation.RuntimeIdentifier, out var platform)
            ? platform
            : throw new PlatformNotSupportedException(
                $"xemu is not supported on {RuntimeInformation.RuntimeIdentifier}.");
}
