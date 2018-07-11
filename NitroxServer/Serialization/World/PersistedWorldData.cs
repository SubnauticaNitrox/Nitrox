using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Items;
using NitroxServer.GameLogic.Players;
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

        [ProtoMember(7)]
        public InventoryData InventoryData { get; set; }

        [ProtoMember(8)]
        public PlayerData PlayerData { get; set; }

        [ProtoMember(9)]
        public GameData GameData { get; set; }

        public bool IsValid()
        {
            return (ParsedBatchCells != null) &&
                   (ServerStartTime != null) &&
                   (EntityData != null) &&
                   (BaseData != null) &&
                   (VehicleData != null) &&
                   (InventoryData != null) &&
                   (GameData != null) &&
                   (PlayerData != null);
        }
    }
}
