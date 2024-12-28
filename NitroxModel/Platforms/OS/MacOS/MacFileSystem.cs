using System.Collections.Generic;
using NitroxModel.Platforms.OS.Shared;

namespace NitroxModel.Platforms.OS.MacOS;

#if NET5_0_OR_GREATER
[System.Runtime.Versioning.SupportedOSPlatform("osx")]
#endif
public sealed class MacFileSystem : FileSystem
{
    public override IEnumerable<string> GetDefaultPrograms(string file)
    {
        yield return "open";
    }

    public override bool SetFullAccessToCurrentUser(string directory)
    {
        throw new System.NotImplementedException();
    }
}
