using System.Collections.Generic;
using NitroxModel.Platforms.OS.Shared;
using static NitroxServer.Server;
using static NitroxModel.DisplayStatusCodes;
namespace NitroxModel.Platforms.OS.Unix
{
    public sealed class UnixFileSystem : FileSystem
    {
        bool throwNotImplementedException() {
            DisplayStatusCode(StatusCode.seven);
            throw new System.NotImplementedException();
        }
        public override IEnumerable<string> GetDefaultPrograms(string file)
        {
            yield return "xdg-open";
        }

        public override bool SetFullAccessToCurrentUser(string directory)
        {
            DisplayStatusCode(StatusCode.eight);
            throw new System.NotImplementedException();
        }

        public override bool IsTrustedFile(string file) => throwNotImplementedException();

    }

}
