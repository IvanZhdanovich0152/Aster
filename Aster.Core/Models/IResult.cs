using System.Collections.Generic;

namespace Aster.Core.Models;

public interface IResult
{
    public bool IsOk { get; }
    
    public IEnumerable<string> Errors { get; }
}
