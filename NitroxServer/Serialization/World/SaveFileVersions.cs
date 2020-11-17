using Newtonsoft.Json;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Players;
using ProtoBufNet;

namespace NitroxServer.Serialization.World
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
