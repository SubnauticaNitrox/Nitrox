using System.Runtime.Serialization;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Players;

namespace Nitrox.Server.Subnautica.Models.Serialization.World
{
    [DataContract]
    internal class PersistedWorldData
    {
        [DataMember(Order = 1)]
        public WorldData? WorldData { get; set; }

        [DataMember(Order = 2)]
        public PlayerData? PlayerData { get; set; }

        [DataMember(Order = 3)]
        public GlobalRootData? GlobalRootData { get; set; }

        [DataMember(Order = 4)]
        public EntityData? EntityData { get; set; }

        public bool IsValid()
        {
            return WorldData.IsValid() &&
                   PlayerData != null &&
                   GlobalRootData != null &&
                   EntityData != null;
        }
    }
}
