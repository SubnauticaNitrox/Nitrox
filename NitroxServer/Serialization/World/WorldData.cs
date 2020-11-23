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
        public const short VERSION = 1;

        [JsonProperty, ProtoMember(1)]
        public List<NitroxInt3> ParsedBatchCells { get; set; }

        [JsonProperty, ProtoMember(2)]
        public DateTime? ServerStartTime { get; set; }

        [JsonProperty, ProtoMember(3)]
        public VehicleData VehicleData { get; set; }

        [JsonProperty, ProtoMember(4)]
        public InventoryData InventoryData { get; set; }

        [JsonProperty, ProtoMember(5)]
        public GameData GameData { get; set; }

        [JsonProperty, ProtoMember(6)]
        public EscapePodData EscapePodData { get; set; }

        [JsonProperty, ProtoMember(7)]
        public StoryTimingData StoryTimingData { get; set; }

        public bool IsValid()
        {
            return (ParsedBatchCells != null) && // Always returns false on empty saves
                   (ServerStartTime.HasValue) &&
                   (VehicleData != null) &&
                   (InventoryData != null) &&
                   (GameData != null) &&
                   (EscapePodData != null) &&
                   (StoryTimingData != null);
        }
    }
}
