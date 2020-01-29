using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBufNet;

namespace NitroxServer.Serialization.World
{
    [ProtoContract]
    [ProtoInclude(200, typeof(WorldData))]
    public interface IWorldData
    {
        WorldData ToWorldData();
    }
}
