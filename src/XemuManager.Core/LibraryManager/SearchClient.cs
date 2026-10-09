using System.IO;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace XemuManager.Core.LibraryManager;

public class SearchClient(string libRootPath):ISearchClient
{
    private static readonly EnumerationOptions ScanOptions = 
        new EnumerationOptions
    {
        RecurseSubdirectories = true,
        MatchCasing = MatchCasing.CaseInsensitive,
    };

    public IEnumerable<string>? Search(string query, Func<string,bool> predicate)
    {
        if (!Directory.Exists(libRootPath))
            return null;
        var gameFilePaths = Directory.EnumerateFiles(libRootPath, "*", ScanOptions);
        gameFilePaths = gameFilePaths.Where(predicate);
        // gameFilePaths = gameFilePaths.Where(p => p.EndsWith(".iso", StringComparison.OrdinalIgnoreCase) || p.EndsWith(".xiso", StringComparison.OrdinalIgnoreCase));
        return gameFilePaths;
    }
}