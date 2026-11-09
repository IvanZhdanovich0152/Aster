using System.Collections.Generic;

namespace Aster.Core.Models;

public interface IFileSystem
{
    IReadOnlyList<IFileSystemEntry> GetEntries(string path);
    void Open(IFileSystemEntry entry);
}