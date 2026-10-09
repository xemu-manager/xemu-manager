namespace XemuManager.Core.LibraryManager;

/// <summary>
/// Reads metadata from one game file. Each console has its own implementation
/// that returns its own subtype of <see cref="GameMetaData"/>.
/// </summary>
public interface IGameMetadataReader
{
    /// <returns>The metadata, or null if the file is not a game this reader understands.</returns>
    static abstract GameMetaData? Read(string filePath);
}
