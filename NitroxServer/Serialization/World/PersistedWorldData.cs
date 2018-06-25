using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Vehicles;
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
        public List<Int3> ParsedBatchCells { get; set; }
        
        [ProtoMember(3)]
        public DateTime ServerStartTime { get; set; }
        
        [ProtoMember(4)]
        public EntityData EntityData { get; set; }

        [ProtoMember(5)]
        public BaseData BaseData { get; set; }

        [ProtoMember(6)]
        public VehicleData VehicleData { get; set; }

        public bool IsValid()
        {
            return (ParsedBatchCells != null) &&
                   (ServerStartTime != null) &&
                   (EntityData != null) &&
                   (BaseData != null) &&
                   (VehicleData != null);
        }
    }
}
