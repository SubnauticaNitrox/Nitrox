using System.Collections.Generic;
using NitroxModel.Platforms.OS.Shared;

namespace NitroxModel.Platforms.OS.Unix
{
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

        public override bool IsTrustedFile(string file) => throw new System.NotImplementedException();
    }
}
