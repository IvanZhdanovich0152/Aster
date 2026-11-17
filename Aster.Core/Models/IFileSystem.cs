namespace Aster.Core.Models;

public interface IFileSystem
{
    IFileInfoProvider FileInfoProvider { get; }
    IFileOperations FileOperations { get; }
    IPathTools PathTools { get; }
    string GetLabel();
    bool IsLocal();
    bool IsReady();
}


public sealed class LocalFileSystem : IFileSystem
{
    private const string Label = "local";
    
    IFileInfoProvider IFileSystem.FileInfoProvider => LocalFileInfoProvider;
    IFileOperations IFileSystem.FileOperations => FileOperations;
    IPathTools IFileSystem.PathTools => LocalFileInfoProvider.PathTools;
    
    public required ILocalFileInfoProvider LocalFileInfoProvider { get; init; }
    public required IFileOperations FileOperations { get; init; }
    
    public bool IsLocal() => true;
    public bool IsReady() => true;
    public string GetLabel() => Label;
}