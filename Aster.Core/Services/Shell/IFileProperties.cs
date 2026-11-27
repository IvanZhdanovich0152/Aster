using Aster.Core.Models;
using Aster.Core.ViewModels;

namespace Aster.Core.Services.Shell;

public interface IFileProperties
{
    public IResult ShowFileProperties(FileSystemEntryViewModel entry);

    public bool SupportsOpenAs(IFileSystemEntry entry);

    public IResult ShowOpenAsDialog(IFileSystemEntry entry);
}
