using System;
using Aster.Core.Models.FileInformation;
using Aster.Core.Services.FileOperations;
using Aster.Core.Services.Shell;

namespace Aster.Core.Models;

public interface IFileSystem : IDisposable
{
    public IFileInfoProvider FileInfoProvider { get; }

    public IIconProvider IconProvider { get; }

    public IArchiveManager ArchiveManager { get; }
    
    public IFileIoHandler FileIoHandler { get; }

    public IBinInteraction BinInteraction { get; }
    
    public IFileProperties FileProperties { get; }

    public IShellHandler ShellHandler { get; }
    
    public bool IsReady();

    public bool IsLocal();
    
    public bool IsSame(IFileSystem? fileSystem) => ReferenceEquals(this, fileSystem);
    
    public string GetLabel();
}

public sealed class LocalFileSystem : IFileSystem
{
    private const string Label = "local";

    IFileInfoProvider IFileSystem.FileInfoProvider => LocalFileInfoProvider;
    IShellHandler IFileSystem.ShellHandler => LocalShellHandler;
    
    public required ILocalFileInfoProvider LocalFileInfoProvider { get; init; }
    
    public required ILocalShellHandler LocalShellHandler { get; init; }

    public required IIconProvider IconProvider { get; init; }

    public required IArchiveManager ArchiveManager { get; init; }

    public required IFileIoHandler FileIoHandler { get; init; }

    public required IBinInteraction BinInteraction { get; init; }

    public required IFileProperties FileProperties { get; init; }
    
    public bool IsReady() => true;

    public bool IsLocal() => true;

    public string GetLabel() => Label;

    public void Dispose()
    {
        IconProvider.Dispose();
        BinInteraction.Dispose();
    }
}
