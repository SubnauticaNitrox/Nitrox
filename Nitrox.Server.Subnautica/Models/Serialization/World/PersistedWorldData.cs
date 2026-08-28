using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Players;

namespace Nitrox.Server.Subnautica.Models.Serialization.World;

internal class PersistedWorldData
{
    public WorldData? WorldData { get; set; }

    public PlayerData? PlayerData { get; set; }

    public GlobalRootData? GlobalRootData { get; set; }

    public EntityData? EntityData { get; set; }

    public bool IsValid()
    {
        return WorldData?.IsValid() == true &&
               PlayerData != null &&
               GlobalRootData != null &&
               EntityData != null;
    }
}
