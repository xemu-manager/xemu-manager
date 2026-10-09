namespace XemuManager.Core.LibraryManager;

public enum ImageType
{
    Xiso,
    RedumpXgd1,
    RedumpXgd2,
    RedumpXgd3,
}

[Flags]
public enum GameRegion : uint
{
    None = 0,
    NorthAmerica = 0x1,
    Japan = 0x2,
    RestOfWorld = 0x4,
    Manufacturing = 0x80000000,
}

public record XboxMetaData(
    string FilePath,
    ImageType ImageType,
    string TitleId,
    string Title,
    GameRegion Region,
    uint Version,
    int DiscNumber) : GameMetaData(FilePath, Title);
