using System.Collections.Generic;
using NitroxModel.Platforms.OS.Shared;
using static NitroxModel.DisplayStatusCodes;
namespace NitroxModel.Platforms.OS.Unix
{
    public sealed class UnixFileSystem : FileSystem
    {
        bool throwNotImplementedException() {
            DisplayStatusCode(StatusCode.MISSING_FEATURE, "Tried to access a feature that has not yet been implemented");
            return false;
        }
        public override IEnumerable<string> GetDefaultPrograms(string file)
        {
            yield return "xdg-open";
        }

        public override bool SetFullAccessToCurrentUser(string directory)
        {
            DisplayStatusCode(StatusCode.MISSING_FEATURE, "Tried to access a feature that has not yet been implemented");
            return false;
        }

        public override bool IsTrustedFile(string file) => throwNotImplementedException();

    }

}
