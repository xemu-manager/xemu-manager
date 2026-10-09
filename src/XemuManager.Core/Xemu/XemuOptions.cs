namespace XemuManager.Core.Xemu;

/// <summary>
/// Settings for installing xemu, bound from the "Xemu" configuration section.
/// appsettings.json ships the release info; the user's settings.json can override InstallDirectory.
/// </summary>
public class XemuOptions
{
    public const string SectionName = "Xemu";

    public string InstallDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "XemuManager",
        "xemu");

    public XemuReleaseOptions Release { get; set; } = new();
}

public class XemuReleaseOptions
{
    public string Owner { get; set; } = "";
    public string Repo { get; set; } = "";
}
