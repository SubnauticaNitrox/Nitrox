using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Players;
using ProtoBufNet;

namespace NitroxServer.Serialization.World
{
    [ProtoContract]
    public class SaveFileVersions
    {
        [ProtoMember(1)]
        public long BaseDataVersion;

        [ProtoMember(2)]
        public long PlayerDataVersion;

        [ProtoMember(3)]
        public long WorldDataVersion;

        public SaveFileVersions()
        {
            BaseDataVersion = BaseData.VERSION;
            PlayerDataVersion = PlayerData.VERSION;
            WorldDataVersion = WorldData.VERSION;
        }
    }
}
