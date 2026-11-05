using System.Collections.Generic;

namespace Aster.Core.Models;

public interface IFileSystem
{
    IEnumerable<string> GetFiles(string path);
}