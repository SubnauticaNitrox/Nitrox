using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBufNet;
using NitroxModel.Logger;

namespace NitroxServer.Serialization.World
{
    [ProtoContract]
    public class WorldVersion
    {
        private const long VERSION = 12;

        [ProtoMember(1)]
        public long Version = VERSION;

        public bool IsValid()
        {
            if (Version != VERSION)
            {
                Log.Error("Version " + Version + " save file is no longer supported.  Creating world under version " + VERSION, new WorldVersionMismatchException());
            }
            return Version == VERSION;
        }
    }
}
