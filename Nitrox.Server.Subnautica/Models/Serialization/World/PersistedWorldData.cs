using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Serialization.World;

internal class PersistedWorldData
{
    public WorldData? WorldData { get; set; }

    public GlobalRootData? GlobalRootData { get; set; }

    public EntityData? EntityData { get; set; }

    public bool IsValid()
    {
        return WorldData?.IsValid() == true &&
               GlobalRootData != null &&
               EntityData != null;
    }
}
