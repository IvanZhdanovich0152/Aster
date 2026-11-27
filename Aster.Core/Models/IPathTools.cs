namespace Aster.Core.Models;

public interface IPathTools
{
    public char DirSeparator { get; }
    
    public string NormalizePath(string path);

    public string Combine(string pathBase, string pathSuffix);
    
    public string GetParentDir(string path);

    public string GetFileName(string path);

    public string GetExtension(string path);

    public bool NamesAreEqual(string? nameA, string? nameB);

    public bool PathsAreEqual(string? pathA, string? pathB);
}
