namespace XemuManager.Core.LibraryManager;

/// <summary>
/// What every game has, whatever the console. Console-specific readers
/// return a subtype with their extra fields (e.g. <see cref="XboxMetaData"/>).
/// </summary>
public abstract record GameMetaData(string FilePath, string Title);
