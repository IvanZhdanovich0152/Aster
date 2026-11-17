using System;
using System.IO;
using Aster.Core.Models;

namespace Aster.Core.Services;

public interface IDirectoryNavigationService
{
    ValueResult<DirectoryContents> Load(
        string path,
        bool includeHidden = false,
        bool includeOs = true);
}

public sealed class DirectoryNavigationService : IDirectoryNavigationService
{
    private readonly IFileSystem _fileSystem;

    public DirectoryNavigationService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public ValueResult<DirectoryContents> Load(
        string path,
        bool includeHidden = false,
        bool includeOs = true)
    {
        if (string.IsNullOrWhiteSpace(path))
            return ValueResult<DirectoryContents>.Error("Путь к каталогу не указан.");

        string fullPath;
        try
        {
            fullPath = _fileSystem.PathTools.GetFullPath(path);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException)
        {
            return ValueResult<DirectoryContents>.Error(ex.Message);
        }

        ExistsInfo exists = _fileSystem.FileInfoProvider.Exists(fullPath);
        if (!exists.AsPath)
            return ValueResult<DirectoryContents>.Error($"Путь не найден: {fullPath}");
        if (!exists.AsDir)
            return ValueResult<DirectoryContents>.Error($"Путь не является каталогом: {fullPath}");

        return _fileSystem.FileInfoProvider.GetPathEntries(
            fullPath,
            includeHidden,
            includeOs);
    }
}
