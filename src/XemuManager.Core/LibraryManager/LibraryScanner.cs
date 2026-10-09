namespace XemuManager.Core.LibraryManager;

/// <summary>
/// Finds game files with <see cref="ISearchClient"/> and reads their metadata
/// with <typeparamref name="TReader"/>.
/// </summary>
public class LibraryScanner<TReader>(string path, ISearchClient searchClient)
    where TReader : IGameMetadataReader
{
    private readonly string _path = path;

    /// <returns>Metadata of every recognised game, keyed by file path.</returns>
    public IReadOnlyDictionary<string, GameMetaData> Scan()
    {
        var paths = searchClient.Search(_path,p => p.EndsWith(".iso",
            StringComparison.OrdinalIgnoreCase) || p.EndsWith(".xiso", StringComparison.OrdinalIgnoreCase));
        var metadatas = new Dictionary<string, GameMetaData>();
        if (paths is null)
            return metadatas;

        foreach (var filePath in paths)
        {
            if (TReader.Read(filePath) is { } metadata)
                metadatas[filePath] = metadata;
        }
        return metadatas;
    }
}
