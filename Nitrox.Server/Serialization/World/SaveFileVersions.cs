using Newtonsoft.Json;
using Nitrox.Server.GameLogic.Bases;
using Nitrox.Server.GameLogic.Players;
using ProtoBufNet;

namespace Nitrox.Server.Serialization.World
{
    [ProtoContract, JsonObject(MemberSerialization.OptIn)]
    public class SaveFileVersions
    {
        [JsonProperty, ProtoMember(1)]
        public readonly short BaseDataVersion;

        [JsonProperty, ProtoMember(2)]
        public readonly short PlayerDataVersion;

        [JsonProperty, ProtoMember(3)]
        public readonly short WorldDataVersion;

        public SaveFileVersions()
        {
            BaseDataVersion = BaseData.VERSION;
            PlayerDataVersion = PlayerData.VERSION;
            WorldDataVersion = WorldData.VERSION;
        }

        public SaveFileVersions(short baseDataVersion, short playerDataVersion, short worldDataVersion)
        {
            BaseDataVersion = baseDataVersion;
            PlayerDataVersion = playerDataVersion;
            WorldDataVersion = worldDataVersion;
        }
    }
}
