namespace Aster.Core.Models;

public interface IPathTools
{
    public char DirSeparator { get; }
    public string GetParentDir(string path);
    public string GetFullPath(string path);
    public string GetFileName(string path);
    public string GetExtension(string path);
    
    
    
}