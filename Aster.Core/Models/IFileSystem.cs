using System;
using System.Collections.Generic;
using System.IO;

namespace Aster.Core.Models;

public interface IFileSystem
{
    public IFileInfoProvider FileInfoProvider { get; }
    public string GetLabel();
    public bool IsLocal();
}


public sealed class LocalFileSystem : IFileSystem
{
    private const string Label = "local";
    
    IFileInfoProvider IFileSystem.FileInfoProvider => LocalFileInfoProvider;

    public required ILocalFileInfoProvider LocalFileInfoProvider { get; init; }
    
    
    public bool IsLocal() => true;
    public string GetLabel() => Label;
}