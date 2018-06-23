using NitroxServer.GameLogic.Entities;
using ProtoBufNet;
using System;
using System.Collections.Generic;

namespace NitroxServer.Serialization.World
{
    [ProtoContract]
    public class PersistedWorldData
    {
        [ProtoMember(1)]
        public HashSet<Int3> ParsedBatchCells { get; set; }
        
        [ProtoMember(2)]
        public DateTime ServerStartTime { get; set; }
        
        [ProtoMember(3)]
        public EntityData EntityData { get; set; }
    }
}
