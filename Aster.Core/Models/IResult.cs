using System.Collections.Generic;

namespace Aster.Core.Models;

public interface IResult
{
    bool IsSuccess { get; }
    
    IEnumerable<string> Errors { get; }
}