using System.Collections.Generic;
using NitroxModel.Platforms.OS.Shared;

namespace NitroxModel.Platforms.OS.MacOS
{
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

        public override bool IsTrustedFile(string file) => throw new System.NotImplementedException();
    }
}
