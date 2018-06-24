using NitroxModel.DataStructures.GameLogic;
using NitroxServer.GameLogic.Bases;
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
        public long version { get; set; } = 1;

        [ProtoMember(2)]
        public HashSet<Int3> ParsedBatchCells { get; set; }
        
        [ProtoMember(3)]
        public DateTime ServerStartTime { get; set; }
        
        [ProtoMember(4)]
        public EntityData EntityData { get; set; }

        [ProtoMember(5)]
        public BaseData BaseData { get; set; }
        
        public bool IsValid()
        {
            return (ParsedBatchCells != null) &&
                   (ServerStartTime != null) &&
                   (EntityData != null) &&
                   (BaseData != null);
        }
    }
}
