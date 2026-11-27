using System.Threading.Tasks;

namespace Aster.Core.Models;


public sealed class Location
{
    public string Path { get; }

    public IFileSystem FileSystem { get; }
    
    public Location(IFileSystem fileSystem, string dirPath)
    {
        Path = dirPath;
        FileSystem = fileSystem;
    }

    public bool Exists() => FileSystem.IsReady() && FileSystem.FileInfoProvider.Exists(Path).AsDir;

    public async Task<bool> ExistsAsync()
    {
        if (!FileSystem.IsReady())
            return false;

        return (await FileSystem.FileInfoProvider.ExistsAsync(Path)).AsDir;
    }

    public bool IsSame(Location? other)
    {
        if (other is null)
            return false;

        if (FileSystem.IsLocal() && other.FileSystem.IsLocal())
            return LocalPathTools.PathsAreEqual(Path, other.Path);

        return FileSystem == other.FileSystem
            && FileSystem.FileInfoProvider.PathTools.PathsAreEqual(Path, other.Path);
    }

    public override string ToString() => $"{FileSystem.GetLabel()}:{Path}";
}
