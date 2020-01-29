using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxServer.ConfigParser;
using ProtoBufNet;

namespace NitroxServer.Serialization.World
{
    [ProtoContract]
    [ProtoInclude(100, typeof(PersistedWorldData))]
    public interface IPersistedWorldData
    {
        bool IsValid();
        World ToWorld();
    }
}
