using Newtonsoft.Json;
using Nitrox.Server.GameLogic.Bases;
using Nitrox.Server.GameLogic.Entities;
using Nitrox.Server.GameLogic.Players;
using ProtoBufNet;

namespace Nitrox.Server.Serialization.World
{
    [ProtoContract, JsonObject(MemberSerialization.OptIn)]
    public class PersistedWorldData
    {
        [JsonProperty, ProtoMember(1)]
        public WorldData WorldData { get; set; } = new WorldData();

        [JsonProperty, ProtoMember(2)]
        public BaseData BaseData { get; set; }

        [JsonProperty, ProtoMember(3)]
        public PlayerData PlayerData { get; set; }

        [JsonProperty, ProtoMember(4)]
        public EntityData EntityData { get; set; }

        public bool IsValid()
        {
            return WorldData.IsValid() &&
                   BaseData != null &&
                   PlayerData != null &&
                   EntityData != null;
        }
    }
}
