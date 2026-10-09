namespace XemuManager.Core.LibraryManager;

/// <summary>
/// Finds candidate game files in the library folders.
/// </summary>
public interface ISearchClient
{
    /// <returns>Matching file paths, or null if the library folder does not exist.</returns>
    IEnumerable<string>? Search(string query, Func<string, bool> predicate);
}
