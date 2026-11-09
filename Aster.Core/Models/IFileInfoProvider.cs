using System.Reflection.Metadata;

namespace Aster.Core.Models;

public readonly struct ExistsInfo
{
    public bool AsFile { get; }
    public bool AsDir { get; }
    
    public bool AsPath => AsFile || AsDir;

    private ExistsInfo(bool asFile, bool asDir)
    {
        AsFile = asFile;
        AsDir = asDir;
    }

    public static ExistsInfo ExistsAsFile() => new(true, false);

    public static ExistsInfo ExistsAsDir() => new(false, true);

    public static ExistsInfo DoesNotExist() => new(false, false);
}



public interface IFileInfoProvider
{
    
}