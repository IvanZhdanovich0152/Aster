using System.Threading.Tasks;
using Aster.Core.Models;

namespace Aster.Core.Services.Shell;

public interface IShellHandler
{
    public IResult CreateFileLink(string filePath);

    public IResult CreateDirectoryLink(string dirPath);

    public ValueResult<string> ExecuteCommand(string programName, params string[] args);

    public Task<ValueResult<string>> ExecuteCommandAsync(string programName, params string[] args);
}

public interface ILocalShellHandler : IShellHandler
{
    public IResult OpenTerminalAt(string dirPath);

    public IResult OpenFile(string filePath);

    public IResult OpenInNotepad(string filePath);
}
