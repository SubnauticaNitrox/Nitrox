using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Players;
using ProtoBufNet;

namespace NitroxServer.Serialization.World
{
    [ProtoContract]
    public class PersistedWorldData
    {
        [ProtoMember(1)]
        public WorldData WorldData { get; set; } = new WorldData();

        [ProtoMember(2)]
        public BaseData BaseData { get; set; }

        [ProtoMember(3)]
        public PlayerData PlayerData { get; set; }
        
        public bool IsValid()
        {
            return (WorldData.IsValid()) &&
                   (BaseData != null) &&
                   (PlayerData != null);
        }
    }
}
