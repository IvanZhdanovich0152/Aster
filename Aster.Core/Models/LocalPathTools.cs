using System;
using System.IO;

namespace Aster.Core.Models;

public class LocalPathTools : IPathTools
{
    public static readonly LocalPathTools Instance = new();

    char IPathTools.DirSeparator => DirSeparator;
    
    string IPathTools.GetParentDir(string path) => GetParentDir(path);
    string IPathTools.GetFileName(string path) => GetFileName(path);
    string IPathTools.GetExtension(string path) => Path.GetExtension(path);
    string IPathTools.GetFullPath(string path) => GetFullPath(path);
    
    
    public static char DirSeparator => Path.DirectorySeparatorChar;
    private LocalPathTools() { }

    public static string GetParentDir(string path)
    {
        ReadOnlySpan<char> trimmed = path.AsSpan().TrimEnd(DirSeparator);
        ReadOnlySpan<char> parent = Path.GetDirectoryName(trimmed);
        return parent.ToString();
    }

    public static string GetFileName(string path)
    {
        ReadOnlySpan<char> trimmed = path.AsSpan().TrimEnd(DirSeparator);
        ReadOnlySpan<char> name = Path.GetFileName(trimmed);
        return name.ToString();
    }

    public static string GetExtension(string path)
    {
        ReadOnlySpan<char> trimmed = path.AsSpan().TrimEnd(DirSeparator);
        ReadOnlySpan<char> extension = Path.GetExtension(trimmed);
        return extension.ToString();
    }

    public static string GetFullPath(string path)
    {
        return Path.GetFullPath(path);
    }
    
    
    
}