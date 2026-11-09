namespace Aster.Core.Models;

public interface IPathTool
{
    public string GetParentDir(string path);
    public string GetFullPath();
    public string GetFileName(string path);
    public string GetExtension(string path);
    
    
    
}