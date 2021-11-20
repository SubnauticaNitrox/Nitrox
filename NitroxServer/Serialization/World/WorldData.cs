using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroxModel.DataStructures;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Items;
using NitroxServer.GameLogic.Vehicles;
using ProtoBufNet;

namespace NitroxServer.Serialization.World
{
    [ProtoContract, JsonObject(MemberSerialization.OptIn)]
    public class WorldData
    {
        [JsonProperty, ProtoMember(1)]
        public List<NitroxInt3> ParsedBatchCells { get; set; }

        [JsonProperty, ProtoMember(2)]
        public VehicleData VehicleData { get; set; }

        [JsonProperty, ProtoMember(3)]
        public InventoryData InventoryData { get; set; }

        [JsonProperty, ProtoMember(4)]
        public GameData GameData { get; set; }

        [JsonProperty, ProtoMember(5)]
        public EscapePodData EscapePodData { get; set; }

        [JsonProperty, ProtoMember(6)]
        public string Seed { get; set; }

        public bool IsValid()
        {
            return ParsedBatchCells != null && // Always returns false on empty saves (sometimes also if never entered the ocean)
                   VehicleData != null &&
                   InventoryData != null &&
                   GameData != null &&
                   EscapePodData != null;
        }
    }
}
