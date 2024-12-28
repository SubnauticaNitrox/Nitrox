using System.Collections.Generic;
using NitroxModel.Platforms.OS.Shared;

namespace NitroxModel.Platforms.OS.Unix;

#if NET5_0_OR_GREATER
[System.Runtime.Versioning.SupportedOSPlatform("linux")]
#endif
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
