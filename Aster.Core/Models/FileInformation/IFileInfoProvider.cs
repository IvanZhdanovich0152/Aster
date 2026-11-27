using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Aster.Core.Models.FileInformation;

public readonly struct ExistsInfo
{
    public bool AsPath => AsFile || AsDir;
    
    public bool AsFile { get; }
    
    public bool AsDir { get; }

    private ExistsInfo(bool asFile, bool asDir)
    {
        AsFile = asFile;
        AsDir = asDir;
    }

    public static ExistsInfo ExistsAsFile() => new(true, false);
    
    public static ExistsInfo ExistsAsDirectory() => new(false, true);
    
    public static ExistsInfo DoesNotExist() => new(false, false);
}

public sealed class DirectoryContents
{
    public required IReadOnlyList<DirectoryEntryInfo> Dirs { get; init; }
    
    public required IReadOnlyList<FileEntryInfo> Files { get; init; }
}

public interface IFileInfoProvider
{
    public IPathTools PathTools { get; }

    public bool IsLinkedToDirectory(string linkPath, [NotNullWhen(true)] out string? directory);

    public ValueResult<DirectoryContents> GetPathEntries(
        string path,
        bool includeHidden,
        bool includeOs
    );

      public Task<ValueResult<DirectoryContents>> GetPathEntriesAsync(
        string path,
        bool includeHidden,
        bool includeOs,
        CancellationToken ct
    );
      
    public ValueResult<Stream> GetFileStream(string path);
    
    public DateTime? GetFileLastWriteUtc(string filePath);

    public Task<DateTime?> GetFileLastWriteUtcAsync(string filePath);

   public DateTime? GetDirLastWriteUtc(string dirPath);

   public Task<DateTime?> GetDirLastWriteUtcAsync(string dirPath);

    public bool IsHidden(string path, bool isDirectory);

    public string GetRoot();

    public ExistsInfo Exists(string path);

    public Task<ExistsInfo> ExistsAsync(string path);
}

public interface ILocalFileInfoProvider : IFileInfoProvider
{
    public DriveEntryInfo[] GetDrives();

    public IEnumerable<string> GetSpecialFolders();
}
