namespace Aster.Core.Models;

public interface IPathTools
{
    char DirSeparator { get; }
    string Combine(string left, string right);
    string GetParentDir(string path);
    string GetFullPath(string path);
    string GetFileName(string path);
    string GetExtension(string path);
    bool IsRoot(string path);
    bool IsAbsolute(string path);
}