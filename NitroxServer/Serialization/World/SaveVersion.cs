using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBufNet;
using NitroxModel.Logger;

namespace NitroxServer.Serialization.World
{
    [ProtoContract]
    public class SaveVersion
    {
        [ProtoMember(1)]
        public long Version;

        public SaveVersion()
        {}

        public SaveVersion(long version)
        {
            Version = version;
        }
    }
}
