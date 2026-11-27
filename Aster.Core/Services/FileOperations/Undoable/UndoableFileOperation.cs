using System.Threading;
using System.Threading.Tasks;
using Aster.Core.Models;
using Aster.Core.Services.Dialogs;

namespace Aster.Core.Services.FileOperations.Undoable;

public interface IUndoableFileOperation : IFileOperation
{
    public Task<IResult> UndoAsync(ProgressReporter reporter, CancellationToken ct);
}

public abstract class UndoableFileOperation : FileOperation, IUndoableFileOperation
{
    protected abstract string UndoVerb { get; }

    protected UndoableFileOperation(IFileIoHandler fileIoHandler, IFileSystemEntry[] entries)
        : base(fileIoHandler, entries) { }

    public Task<IResult> UndoAsync(ProgressReporter reporter, CancellationToken ct) =>
        Task.Run(() => InvokeInternal(UndoAction, UndoVerb, reporter, ct), ct);

    protected abstract IResult UndoAction(IFileSystemEntry entry, int index);
}
