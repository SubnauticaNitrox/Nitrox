using System.Collections.Generic;
using Nitrox.Model.Platforms.OS.Shared;

namespace Nitrox.Model.Platforms.OS.Unix;

public sealed class UnixFileSystem : FileSystem
{
    public override IEnumerable<string> GetDefaultPrograms(string file)
    {
        yield return "xdg-open";
    }

    public override bool SetFullAccessToCurrentUser(string directory)
    {
        throw new System.NotImplementedException();
    }
}
