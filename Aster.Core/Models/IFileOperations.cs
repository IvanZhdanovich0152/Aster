using System;
using System.IO;

namespace Aster.Core.Models;

public interface IFileOperations
{
    SimpleResult CreateFile(string path);
    SimpleResult CreateDirectory(string path);
    SimpleResult Copy(string sourcePath, string destinationPath, bool overwrite);
    SimpleResult Move(string sourcePath, string destinationPath, bool overwrite);
    SimpleResult Rename(string sourcePath, string newName);
    SimpleResult Delete(string path, bool recursive);
    ValueResult<Stream> OpenRead(string path);
    ValueResult<Stream> OpenWrite(string path, bool overwrite);
}

public sealed class LocalFileOperations : IFileOperations
{
    private readonly IPathTools _pathTools;

    public LocalFileOperations(IPathTools pathTools)
    {
        _pathTools = pathTools;
    }

    public SimpleResult CreateFile(string path)
    {
        try
        {
            using FileStream stream = File.Create(path);
            return SimpleResult.Ok();
        }
        catch (IOException ex)
        {
            return SimpleResult.Error(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return SimpleResult.Error(ex.Message);
        }
    }

    public SimpleResult CreateDirectory(string path)
    {
        try
        {
            Directory.CreateDirectory(path);
            return SimpleResult.Ok();
        }
        catch (IOException ex)
        {
            return SimpleResult.Error(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return SimpleResult.Error(ex.Message);
        }
    }

    public SimpleResult Copy(string sourcePath, string destinationPath, bool overwrite)
    {
        try
        {
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, destinationPath, overwrite);
            }
            else if (Directory.Exists(sourcePath))
            {
                CopyDirectory(sourcePath, destinationPath, overwrite);
            }
            else
            {
                return SimpleResult.Error($"Источник не найден: {sourcePath}");
            }

            return SimpleResult.Ok();
        }
        catch (IOException ex)
        {
            return SimpleResult.Error(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return SimpleResult.Error(ex.Message);
        }
    }

    public SimpleResult Move(string sourcePath, string destinationPath, bool overwrite)
    {
        try
        {
            if (File.Exists(sourcePath))
            {
                if (overwrite && File.Exists(destinationPath))
                    File.Delete(destinationPath);
                File.Move(sourcePath, destinationPath);
            }
            else if (Directory.Exists(sourcePath))
            {
                if (overwrite && Directory.Exists(destinationPath))
                    Directory.Delete(destinationPath, recursive: true);
                Directory.Move(sourcePath, destinationPath);
            }
            else
            {
                return SimpleResult.Error($"Источник не найден: {sourcePath}");
            }

            return SimpleResult.Ok();
        }
        catch (IOException ex)
        {
            return SimpleResult.Error(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return SimpleResult.Error(ex.Message);
        }
    }

    public SimpleResult Rename(string sourcePath, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName) ||
            newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            return SimpleResult.Error("Некорректное имя файла или каталога.");

        string? parentPath = _pathTools.GetParentDir(sourcePath);
        if (string.IsNullOrEmpty(parentPath))
            return SimpleResult.Error("Невозможно определить родительский каталог.");

        return Move(sourcePath, _pathTools.Combine(parentPath, newName), overwrite: false);
    }

    public SimpleResult Delete(string path, bool recursive)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
            else if (Directory.Exists(path))
                Directory.Delete(path, recursive);
            else
                return SimpleResult.Error($"Путь не найден: {path}");

            return SimpleResult.Ok();
        }
        catch (IOException ex)
        {
            return SimpleResult.Error(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return SimpleResult.Error(ex.Message);
        }
    }

    public ValueResult<Stream> OpenRead(string path)
    {
        try
        {
            return ValueResult<Stream>.Ok(
                new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
        }
        catch (IOException ex)
        {
            return ValueResult<Stream>.Error(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ValueResult<Stream>.Error(ex.Message);
        }
    }

    public ValueResult<Stream> OpenWrite(string path, bool overwrite)
    {
        try
        {
            FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;
            return ValueResult<Stream>.Ok(
                new FileStream(path, mode, FileAccess.Write, FileShare.None));
        }
        catch (IOException ex)
        {
            return ValueResult<Stream>.Error(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ValueResult<Stream>.Error(ex.Message);
        }
    }

    private static void CopyDirectory(string sourcePath, string destinationPath, bool overwrite)
    {
        Directory.CreateDirectory(destinationPath);

        foreach (FileInfo file in new DirectoryInfo(sourcePath).EnumerateFiles())
            file.CopyTo(Path.Combine(destinationPath, file.Name), overwrite);

        foreach (DirectoryInfo directory in new DirectoryInfo(sourcePath).EnumerateDirectories())
            CopyDirectory(directory.FullName, Path.Combine(destinationPath, directory.Name), overwrite);
    }
}
